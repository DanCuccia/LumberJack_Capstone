using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using SkinnedModel;

namespace SkinnedModelPipeline
{
    /// <summary>
    /// This processor extrends the builtin framework ModelProcessor class,
    /// by adding Animation List functionality
    /// </summary>
    [ContentProcessor]
    public class SkinnedModelProcessor : ModelProcessor
    {
        /// <summary>
        /// The main function that takes the content pipeline node tree,
        /// and creates our runtime objects with embedded anim data
        /// </summary>
        /// <param name="input">the nodeConent tree</param>
        /// <param name="context">the processor utility</param>
        /// <returns>our runtime model object</returns>
        public override ModelContent Process(NodeContent input, ContentProcessorContext context)
        {
            ValidateMesh(input, context, null);

            BoneContent skeleton = MeshHelper.FindSkeleton(input);

            if (skeleton == null)
                throw new Exception("The input skeleton was not found");

            //bake all the models parts to 1 coordinate system to be used run-time
            FlattenTransforms(input, skeleton);

            IList<BoneContent> bones = MeshHelper.FlattenSkeleton(skeleton);

            if (bones.Count > SkinnedEffect.MaxBones)
                throw new InvalidContentException(string.Format(
                    "Skeleton has too many bones, max supported is {1}.",
                    bones.Count, SkinnedEffect.MaxBones));

            //done testing, lets do this...
            List<Matrix> bindPose = new List<Matrix>();
            List<Matrix> invBindPose = new List<Matrix>();
            List<int> skelHierarchy = new List<int>();

            foreach (BoneContent bone in bones)
            {
                bindPose.Add(bone.Transform);
                invBindPose.Add(Matrix.Invert(bone.AbsoluteTransform));
                skelHierarchy.Add(bones.IndexOf(bone.Parent as BoneContent));
            }

            //convert to our runtime obj
            Dictionary<string, AnimationClip> animClips;
            animClips = ProcessAnimations(skeleton.Animations, bones);

            ModelContent model = base.Process(input, context);

            model.Tag = new SkinningData(animClips, bindPose, invBindPose, skelHierarchy);

            return model;

        }

        /// <summary>
        /// converts the content pipeline AnimationContentDictionary to runtime usable object
        /// </summary>
        /// <param name="animations">anims to process</param>
        /// <param name="bones">the bones to process</param>
        /// <returns>a dictionary of our runtime usable animations</returns>
        static Dictionary<string, AnimationClip> ProcessAnimations(
            AnimationContentDictionary animations, IList<BoneContent> bones)
        {
            Dictionary<string, int> boneMap = new Dictionary<string, int>();

            for (int i = 0; i < bones.Count; i++)
            {
                string boneName = bones[i].Name;
                if (!string.IsNullOrEmpty(boneName))
                    boneMap.Add(boneName, i);
            }

            //convert each anim
            Dictionary<string, AnimationClip> animClips = new Dictionary<string, AnimationClip>();
            foreach (KeyValuePair<string, AnimationContent> animation in animations)
            {
                AnimationClip processed = ProcessAnimation(animation.Value, boneMap);
                animClips.Add(animation.Key, processed);
            }

            if (animClips.Count == 0)
                throw new InvalidContentException("input file does not contain any animations!");

            return animClips;
        }

        /// <summary>
        /// converts the content pipeline animationContent into our runtime object
        /// </summary>
        /// <param name="animation">the anim to process</param>
        /// <param name="boneMap">the list of bones to process</param>
        /// <returns></returns>
        static AnimationClip ProcessAnimation(AnimationContent animation,
            Dictionary<string, int> boneMap)
        {
            List<Keyframe> keyframes = new List<Keyframe>();

            //input animation channels
            foreach (KeyValuePair<string, AnimationChannel> channel in animation.Channels)
            {
                int boneIndex;

                if (!boneMap.TryGetValue(channel.Key, out boneIndex))
                    throw new InvalidContentException(string.Format(
                        "Found animation for root bone!", channel.Key));

                //convert keyframe data to our type
                foreach (AnimationKeyframe keyframe in channel.Value)
                    keyframes.Add(new Keyframe(boneIndex, keyframe.Time, keyframe.Transform));
            }

            //sort the merged keyframes
            keyframes.Sort(CompareKeyframeTimes);

            //error checks
            if (keyframes.Count == 0)
                throw new Exception("Animation has no keyframes!");
            if (animation.Duration <= TimeSpan.Zero)
                throw new Exception("Animation has zero duration!");

            return new AnimationClip(animation.Duration, keyframes);
        }

        /// <summary>
        /// used to sort keyframes
        /// </summary>
        /// <param name="one">first keyframe</param>
        /// <param name="two">second keyframe to compare to</param>
        /// <returns>the different of the keyframe times</returns>
        static int CompareKeyframeTimes(Keyframe one, Keyframe two)
        {
            return one.Time.CompareTo(two.Time);
        }

        /// <summary>
        /// Test mesh for skinning data needed for animation
        /// </summary>
        /// <param name="node">the bone(node)</param>
        /// <param name="context">processor util</param>
        /// <param name="parentBoneName">parent bone</param>
        static void ValidateMesh(NodeContent node,
            ContentProcessorContext context,
            string parentBoneName)
        {
            MeshContent mesh = node as MeshContent;

            if (mesh != null)
            {
                if (parentBoneName != null)
                    context.Logger.LogWarning(null, null,
                        "Mesh {0} is a child of bone {1}, SkinnedModelProcessor does not " +
                        "correctly handl meshes that are children of bones.",
                        mesh.Name, parentBoneName);
                if (!MeshHasSkinning(mesh))
                {
                    context.Logger.LogWarning(null, null,
                        "Mesh{0} has no skinning information, so it has been deleted", mesh.Name);
                    mesh.Parent.Children.Remove(mesh);
                    return;
                }
            }
            else if (node is BoneContent)
            {
                parentBoneName = node.Name;
            }

            //recursively keep checking children
            foreach (NodeContent child in new List<NodeContent>(node.Children))
                ValidateMesh(child, context, parentBoneName);
        }

        /// <summary>
        /// Check if a a mesh has skinning information
        /// </summary>
        /// <param name="mesh">the mesh to test</param>
        /// <returns></returns>
        static bool MeshHasSkinning(MeshContent mesh)
        {
            foreach (GeometryContent geometry in mesh.Geometry)
                if (!geometry.Vertices.Channels.Contains(VertexChannelNames.Weights()))
                    return false;
            return true;
        }

        /// <summary>
        /// Bakes transformations into geometry,
        /// creating model into 1 coordinate system
        /// </summary>
        /// <param name="node">the bone</param>
        /// <param name="skeleton">the skeleton</param>
        static void FlattenTransforms(NodeContent node, BoneContent skeleton)
        {
            foreach (NodeContent child in node.Children)
            {
                if (child == skeleton)
                    continue;

                //bake transforms into geometry
                MeshHelper.TransformScene(child, child.Transform);

                //set local coords back to identity
                child.Transform = Matrix.Identity;

                //recurse through skeleton
                FlattenTransforms(child, skeleton);
            }
        }

        /// <summary>
        /// Force all materials to use skinned model effect
        /// NOTE: Probably temporary until a learn more about this
        /// </summary>
        [DefaultValue(MaterialProcessorDefaultEffect.SkinnedEffect)]
        public override MaterialProcessorDefaultEffect DefaultEffect
        {
            get { return MaterialProcessorDefaultEffect.SkinnedEffect; }
            set { }
        }
    }
}
