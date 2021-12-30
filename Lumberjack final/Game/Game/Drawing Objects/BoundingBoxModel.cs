using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using Game.Math_Physics;

namespace Game.Drawing_Objects
{
    class BoundingBoxModel
    {
        #region Member Variables

        Line            line;
        Vector3[]       vertices = new Vector3[8];

        #endregion Member Variables

        #region Initialization

        public BoundingBoxModel(GraphicsDevice device)
        {
            line = new Line(device);
        }

        #endregion Initialization

        #region API

        public void Draw(BoundingBox boundingbox, Vector4 color1, Vector4 color2)
        {
            if (boundingbox == null)
                return;

            updateVertices(boundingbox);
            line.Draw(vertices[0], vertices[6], color1, color2);
            line.Draw(vertices[6], vertices[4], color1, color2);
            line.Draw(vertices[4], vertices[2], color1, color2);
            line.Draw(vertices[2], vertices[0], color1, color2);
            line.Draw(vertices[1], vertices[3], color1, color2);
            line.Draw(vertices[3], vertices[5], color1, color2);
            line.Draw(vertices[5], vertices[7], color1, color2);
            line.Draw(vertices[7], vertices[1], color1, color2);
            line.Draw(vertices[0], vertices[5], color1, color2);
            line.Draw(vertices[3], vertices[6], color1, color2);
            line.Draw(vertices[4], vertices[1], color1, color2);
            line.Draw(vertices[7], vertices[2], color1, color2);
        }

        public void Draw(OrientedBoundingBox obb, Vector4 color1, Vector4 color2)
        {
            if (obb == null)
                return;

            updateVertices(obb);
            line.Draw(vertices[0], vertices[1], color1, color2);
            line.Draw(vertices[1], vertices[2], color1, color2);
            line.Draw(vertices[2], vertices[3], color1, color2);
            line.Draw(vertices[3], vertices[0], color1, color2);
            line.Draw(vertices[0], vertices[4], color1, color2);
            line.Draw(vertices[4], vertices[5], color1, color2);
            line.Draw(vertices[5], vertices[6], color1, color2);
            line.Draw(vertices[6], vertices[7], color1, color2);
            line.Draw(vertices[7], vertices[4], color1, color2);

        }

        private void updateVertices(BoundingBox bb)
        {
            vertices[0] = bb.Min;
            vertices[1] = bb.Max;
            vertices[2] = new Vector3(bb.Max.X, bb.Min.Y, bb.Min.Z);
            vertices[3] = new Vector3(bb.Min.X, bb.Max.Y, bb.Max.Z);
            vertices[4] = new Vector3(bb.Max.X, bb.Max.Y, bb.Min.Z);
            vertices[5] = new Vector3(bb.Min.X, bb.Min.Y, bb.Max.Z);
            vertices[6] = new Vector3(bb.Min.X, bb.Max.Y, bb.Min.Z);
            vertices[7] = new Vector3(bb.Max.X, bb.Min.Y, bb.Max.Z);
        }

        private void updateVertices(OrientedBoundingBox obb)
        {
            vertices[0] = new Vector3(obb.Min.X, obb.Min.Y, obb.Min.Z);
            vertices[1] = new Vector3(obb.Max.X, obb.Min.Y, obb.Min.Z);
            vertices[2] = new Vector3(obb.Max.X, obb.Max.Y, obb.Min.Z);
            vertices[3] = new Vector3(obb.Min.X, obb.Max.Y, obb.Min.Z);
            vertices[4] = new Vector3(obb.Min.X, obb.Min.Y, obb.Max.Z);
            vertices[5] = new Vector3(obb.Max.X, obb.Min.Y, obb.Max.Z);
            vertices[6] = new Vector3(obb.Max.X, obb.Max.Y, obb.Max.Z);
            vertices[7] = new Vector3(obb.Min.X, obb.Max.Y, obb.Max.Z);

            Matrix world = obb.World;
            Vector3.Transform(vertices, ref world, vertices);
        }

        #endregion API
    }
}
