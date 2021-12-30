using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace SkinnedModel
{
    /// <summary>
    /// This is the position of a single bone at a single point in Time
    /// </summary>
    public class Keyframe
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="bone"></param>
        /// <param name="time"></param>
        /// <param name="transform"></param>
        public Keyframe(int bone, TimeSpan time, Matrix transform)
        {
            Bone = bone;
            Time = time;
            Transform = transform;
        }

        /// <summary>
        /// private Construtor used by XNB deserializer
        /// </summary>
        private Keyframe() { }


        [ContentSerializer]
        public int Bone { get; private set; }

        [ContentSerializer]
        public TimeSpan Time { get; private set; }

        [ContentSerializer]
        public Matrix Transform { get; private set; }

    }
}
