using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using Game.Math_Physics;

namespace Game.Drawing_Objects
{
    class Point3D
    {
        #region Member Varialbes

        Line                line;
        Vector3[]           vertices    = new Vector3[7];
        bool                isDirty     = true;
        Vector3             position    = Vector3.Zero;
        float               radius      = 30;

        Vector4             color1      = MyColors.HotRed;
        Vector4             color2      = MyColors.BlankWhite;

        #endregion Member Variables


        #region Initialization

        /// <summary>default ctor</summary>
        public Point3D(GraphicsDevice device)
        {
            line = new Line(device);
        }

        #endregion Initialization


        #region API

        /// <summary>draw the point</summary>
        public void Draw()
        {
            updateVerts();
            line.Draw(vertices[0], vertices[2], color1, color2);
            line.Draw(vertices[0], vertices[1], color1, color2);
            line.Draw(vertices[0], vertices[1], color1, color2);
            line.Draw(vertices[0], vertices[2], color1, color2);
            line.Draw(vertices[0], vertices[3], color1, color2);
            line.Draw(vertices[0], vertices[4], color1, color2);
            line.Draw(vertices[0], vertices[5], color1, color2);
            line.Draw(vertices[0], vertices[6], color1, color2);
        }

        /// <summary>update the vertices according current position</summary>
        private void updateVerts()
        {
            if (isDirty == false)
                return;

            vertices[0] = position;
            vertices[1] = position + new Vector3(0, radius, 0);
            vertices[2] = position - new Vector3(0, radius, 0);
            vertices[3] = position + new Vector3(radius, 0, 0);
            vertices[4] = position - new Vector3(radius, 0, 0);
            vertices[5] = position + new Vector3(0, 0, radius);
            vertices[6] = position - new Vector3(0, 0, radius);
            isDirty = false;
        }

        #endregion API


        #region Mutators

        public Vector3 Position
        {
            get { return position; }
            set
            {
                isDirty = true;
                position = value;
            }
        }
        public Vector4 ColorCenter
        {
            get { return color1; }
            set { color1 = value; }
        }
        public Vector4 ColorOutside
        {
            get { return color2; }
            set { color2 = value; }
        }

        #endregion Mutators
    }
}
