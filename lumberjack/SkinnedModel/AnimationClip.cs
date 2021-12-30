using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace SkinnedModel
{
    /// <summary>
    /// Runtime object holding all keyframes needed 
    /// to describe a single animation
    /// </summary>
    public class AnimationClip
    {
        /// <summary>
        /// default constructor
        /// </summary>
        /// <param name="duration"></param>
        /// <param name="keyframes"></param>
        public AnimationClip(TimeSpan duration, List<Keyframe> keyframes)
        {
            Duration = duration;
            Keyframes = keyframes;
        }

        /// <summary>
        /// private constructor for XNB deserializer
        /// </summary>
        private AnimationClip() { }

        [ContentSerializer]
        public TimeSpan Duration { get; private set; }

        [ContentSerializer]
        public List<Keyframe> Keyframes { get; private set; }
    }
}
