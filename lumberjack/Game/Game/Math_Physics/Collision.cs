using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Game.Drawing_Objects;

namespace Game.Math_Physics
{
    public class Collision
    {
        /// <summary>
        /// Use basic Intersects() to check for collision
        /// </summary>
        /// <param name="one">sprite 1 to be tested against</param>
        /// <param name="two">sprite two</param>
        /// <returns>true if the two sprites are colliding</returns>
        public static bool RectangleCollision2D(Sprite one, Sprite two)
        {
            return one.getBB().Intersects(two.getBB());
        }

        /// <summary>
        /// Distance type collision
        /// </summary>
        /// <param name="one">sprite 1 to be tested with</param>
        /// <param name="two">sprite 2</param>
        /// <param name="offset">pixel offest for the distance</param>
        /// <returns>true if the 2 radius's are colliding</returns>
        public static bool DistanceCollision2D(Sprite one, Sprite two, double offset = 0.0)
        {
            double radiusOne, radiusTwo;

            if (one.FrameSize.X > one.FrameSize.Y)
                radiusOne = (one.FrameSize.X * one.Scale) / 2;
            else
                radiusOne = (one.FrameSize.Y * one.Scale) / 2;

            double xOne = one.Position.X + radiusOne;
            double yOne = one.Position.Y + radiusOne;
            Vector3 v1 = new Vector3((float)xOne, (float)yOne, 0.0f);

            if (two.FrameSize.X > two.FrameSize.Y)
                radiusTwo = (two.FrameSize.X * two.Scale) / 2;
            else
                radiusTwo = (two.FrameSize.Y * two.Scale) / 2;

            double xTwo = two.Position.X + radiusTwo;
            double yTwo = two.Position.Y + radiusTwo;
            Vector3 v2 = new Vector3((float)xTwo, (float)yTwo, 0.0f);

            double dist = Vector3.Distance(v1, v2);

            return (dist < (radiusOne - offset) + (radiusTwo - offset));

        }

        public static Vector2 PixelCollision2D(Color[,] texture1, Matrix mat1, Color[,] texture2, Matrix mat2)
        {
            //W & H of texture  in pixels
            int width1 = texture1.GetLength(0);
            int height1 = texture1.GetLength(1);
            int width2 = texture2.GetLength(0);
            int height2 = texture2.GetLength(1);

            //loop each column
            for (int x1 = 0; x1 < width1; x1++)
            {
                for (int y1 = 0; y1 < height1; y1++)
                {
                    if (texture1[x1, y1].A > 0)
                    {
                        //find screen coordinates
                        Vector2 pos1 = new Vector2(x1, y1);
                        Vector2 screenCoord = Vector2.Transform(pos1, mat1);

                        //get x&y of second texture
                        Matrix inverseMat2 = Matrix.Invert(mat2);
                        Vector2 pos2 = Vector2.Transform(screenCoord, inverseMat2);
                        int x2 = (int)pos2.X;
                        int y2 = (int)pos2.Y;

                        //test the two points, and check alphas
                        if ((x2 >= 0) && (x2 < width2))
                            if ((y2 >= 0) && (y2 < height2))
                                if ((texture1[x1, y1].A > 0) && (texture2[x2, y2].A > 0))
                                {
                                    Vector2 screenPos = Vector2.Transform(pos1, mat1);
                                    return screenPos;
                                }
                    }
                }
            }
            //no colision found
            return new Vector2(-1, -1);
        }
        
    }
}
