using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using XNAnimation;

using Game.Drawing_Objects;

namespace Game.Drawing_Objects
{
    class BoneControlledModel : StaticMesh
    {
        AnimatedMesh animModel; //ref
        int boneIndex = -1;
        Matrix boneBindPose;

        public void InitializeBoneModel(ContentManager content, string modelFilepath, string texFilepath, AnimatedMesh animModel, string boneIndex)
        {
            base.Initialize(content, modelFilepath, texFilepath);
            this.animModel = animModel;

            int i = 0;
            foreach (SkinnedModelBone bone in animModel.SkinnedModel.SkeletonBones)
            {
                if (bone.Name == boneIndex)
                {
                    this.boneIndex = i;
                    boneBindPose = Matrix.CreateScale(bone.BindPose.Scale) *
                        Matrix.CreateFromQuaternion(bone.BindPose.Orientation) *
                        Matrix.CreateTranslation(bone.BindPose.Translation);
                    break;
                }
                i++;
            }
        }

        protected override void UpdateMatrix()
        {
            if (this.boneIndex == -1)
            {
                base.m_isDirty = true;
                return;
            }

            Matrix[] skeleton = animModel.AnimationController.SkinnedBoneTransforms;
            Matrix bone = skeleton[this.boneIndex];

            Vector3 rotFix = new Vector3(90, 270, 180);

            base.m_modelMatrix = Matrix.Identity *
                //Matrix.CreateRotationX(MathHelper.ToRadians(rotFix.X)) *
                //Matrix.CreateRotationY(MathHelper.ToRadians(rotFix.Y)) *
                //Matrix.CreateRotationZ(MathHelper.ToRadians(rotFix.Z)) *
                //Matrix.CreateTranslation(bone.Translation) *

                Matrix.CreateScale(base.Scale) *
                //bone *
                //Matrix.CreateTranslation(bone.Translation) *
                
                Matrix.CreateRotationX(MathHelper.ToRadians(base.RotationX)) *
                Matrix.CreateRotationY(MathHelper.ToRadians(base.RotationY)) *
                Matrix.CreateRotationZ(MathHelper.ToRadians(base.RotationZ)) *
                Matrix.CreateTranslation(base.Position);

            m_modelMatrix = boneBindPose * bone * m_modelMatrix;

            base.m_isDirty = false;
        }
    }
}
