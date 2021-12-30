using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Game.Math_Physics
{
    public class MyMath
    {
        public static float GetRandomLerpFloat(float min, float max)
        {
            return MathHelper.Lerp(min, max, (float) Game1.random.NextDouble());
        }

        public static float GetRandomFloat(float min, float max)
        {
            return min + ((max - min) * (float) Game1.random.NextDouble());
        }
    }
}
