using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;


namespace SkinnedModel
{
    /// <summary>All vertex data for animating and rendering is in here
    /// stored in Model.Tag of a animated model</summary>
    public class SkinningData
    {
        /// <summary>Default CTOR</summary>
        public SkinningData(Dictionary<string, 
            AnimationClip> animationClips,
            List<Matrix> bindPose,
            List<Matrix> inverseBindPose,
            List<int> skelHierarchy)
        {
            AnimationClips = animationClips;
            BindPose = bindPose;
            InverseBindPose = inverseBindPose;
            SkelHierarchy = skelHierarchy;
        }

        /// <summary>private CTOR used by XNB deserializer</summary>
        private SkinningData() { }

        /// <summary>Retrieves a list of animation clips, and stores into dictionary</summary>
        [ContentSerializer]
        public Dictionary<string, AnimationClip> AnimationClips { get; private set; }

        /// <summary>matrices for each bone in skel</summary>
        [ContentSerializer]
        public List<Matrix> BindPose { get; private set; }

        /// <summary>matrices to bonespace transformations</summary>
        [ContentSerializer]
        public List<Matrix> InverseBindPose { get; private set; }

        /// <summary>the bone hierarchy</summary>
        [ContentSerializer]
        public List<int> SkelHierarchy { get; private set; }

    }
}
