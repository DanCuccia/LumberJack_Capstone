using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Game.Drawing_Objects;

namespace Game.Math_Physics
{
    public class MyMath
    {
        /// <summary>get random float</summary>
        /// <param name="min">min value</param>
        /// <param name="max">max value</param>
        /// <returns>uniform random float</returns>
        public static float GetRandomFloat(float min, float max)
        {
            return min + ((max - min) * (float) Game1.random.NextDouble());
        }

        public static float AngleBetweenVectors(Vector3 one, Vector3 two)
        {
            double oneMag = Math.Sqrt( (one.X * one.X) + (one.Y * one.Y) + (one.Z * one.Z) );
            double twoMag = Math.Sqrt( (two.X * two.X) + (two.Y * two.Y) + (two.Z * two.Z) );
            double theta = Vector3.Dot(one, two) / (oneMag * twoMag);
            return (float)Math.Acos(theta);
        }

        public static Vector3[] GenerateVertexPositionArray(VertexPositionNormalTexture[] vertices)
        {
            Vector3[] output = new Vector3[vertices.Length];

            int index = 0;
            foreach (VertexPositionNormalTexture vert in vertices)
            {
                output[index++] = vert.Position;
            }

            return output;
        }

        public static Vector3[] GenerateVertexPositionArray(VertexPositionNormal[] vertices)
        {
            Vector3[] output = new Vector3[vertices.Length];

            int index = 0;
            foreach (VertexPositionNormal vert in vertices)
            {
                output[index++] = vert.Position;
            }

            return output;
        }

        public static Vector3 Multiply(Vector3 vector, Matrix mat)
        {
            return new Vector3(vector.X * mat.M11 + vector.Y * mat.M21 + vector.Z * mat.M31 + mat.M41,
                            vector.X * mat.M12 + vector.Y * mat.M22 + vector.Z * mat.M32 + mat.M42,
                            vector.X * mat.M13 + vector.Y * mat.M23 + vector.Z * mat.M33 + mat.M43);
        }

        public static Matrix3 Abs(Matrix3 m3)
        {
            Matrix3 absMatrix = new Matrix3(0);

            for (int r = 0; r < 3; r++)
                for (int c = 0; c < 3; c++)
                    absMatrix[r, c] = Math.Abs(m3[r, c]);
                
            return absMatrix;
        }
    }
}
