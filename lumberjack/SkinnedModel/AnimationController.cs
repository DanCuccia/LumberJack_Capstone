using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;


namespace SkinnedModel
{
    /// <summary>
    /// This decodes bone position matrices from an animation clip
    /// </summary>
    public class AnimationController
    {
        
        //The Current playing animation clip
        AnimationClip   m_curClip;
        TimeSpan        m_curTime;
        int             m_curKeyframe;

        //The Current transorms
        Matrix[]        m_boneTransforms;
        Matrix[]        m_worldTransforms;
        Matrix[]        m_skinTransforms;

        //link to skeleton heirarchy data
        SkinningData    m_skinnningData;

        //default used CTOR
        public AnimationController(SkinningData skinData)
        {
            if (skinData == null)
                throw new ArgumentException("AnimationController CTOR: skinData");

            m_skinnningData = skinData;

            m_boneTransforms = new Matrix[m_skinnningData.BindPose.Count];
            m_worldTransforms = new Matrix[m_skinnningData.BindPose.Count];
            m_skinTransforms = new Matrix[m_skinnningData.BindPose.Count];
        }


        //Start playing the specified clip
        public void StartClip(AnimationClip clip)
        {
            if (clip == null)
                throw new ArgumentException("StartClip(): null Clip");

            m_curClip = clip;
            m_curTime = TimeSpan.Zero;
            m_curKeyframe = 0;

            //Init Bone transforms
            m_skinnningData.BindPose.CopyTo(m_boneTransforms, 0);
        }

        //Advance current Animation
        public void Update(TimeSpan time, bool relativeToCurrentTime,
                           Matrix rootTransform)
        {
            UpdateBoneTransforms(time, relativeToCurrentTime);
            UpdateWorldTransforms(rootTransform);
            UpdateSkinTransforms();
        }

        //Refresh bone transform data
        public void UpdateBoneTransforms(TimeSpan time, bool relativeTime)
        {
            if (m_curClip == null)
                throw new InvalidOperationException("UpdateBoneTransforms(): Update() was called before StartClip");

            //upate anim position
            if (relativeTime)
            {
                time += m_curTime;
                //loop back to start
                while (time >= m_curClip.Duration)
                    time -= m_curClip.Duration;
            }

            if ((time < TimeSpan.Zero) || (time >= m_curClip.Duration))
                throw new ArgumentOutOfRangeException("UpdateBoneTransforms(): invalid time");

            if (time < m_curTime)
            {
                m_curKeyframe = 0;
                m_skinnningData.BindPose.CopyTo(m_boneTransforms, 0);
            }

            m_curTime = time;

            //read keyframe matrices
            IList<Keyframe> keyframes = m_curClip.Keyframes;

            while (m_curKeyframe < keyframes.Count)
            {
                Keyframe keyf = keyframes[m_curKeyframe];

                //read up to current time
                if (keyf.Time > m_curTime)
                    break;

                m_boneTransforms[keyf.Bone] = keyf.Transform;

                m_curKeyframe++;
            }
        }

        /// <summary>
        /// Refresh the world transform data
        /// </summary>
        /// <param name="root"></param>
        public void UpdateWorldTransforms(Matrix root)
        {
            m_worldTransforms[0] = m_boneTransforms[0] * root;

            int parent;
            for (int bone = 1; bone < m_worldTransforms.Length; bone++)
            {
                parent = m_skinnningData.SkelHierarchy[bone];
                m_worldTransforms[bone] = m_boneTransforms[bone] * m_worldTransforms[parent];
            }
        }

        /// <summary>
        /// Refresh the Skin transform data
        /// </summary>
        public void UpdateSkinTransforms()
        {
            for(int bone = 0; bone < m_skinTransforms.Length; bone++)
                m_skinTransforms[bone] = m_skinnningData.InverseBindPose[bone] * m_worldTransforms[bone];
        }


        //Mutators
        public Matrix[] getBoneTransforms()
        {
            return m_boneTransforms;
        }

        public Matrix[] getSkinTransforms()
        {
            return m_skinTransforms;
        }

        public AnimationClip CurrentClip
        {
            get { return m_curClip; }
        }

        public TimeSpan CurrentTime
        {
            get { return m_curTime; }
        }
    }
}
