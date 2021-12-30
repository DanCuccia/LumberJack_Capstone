using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Game.Drawing_Objects
{
    /// <summary>
    /// This class handles drawing text within 3D space, Takes care of all 
    /// rotation so the billboard will always face the camera, and be scaled
    /// according, position is based on the bottom middle of the text
    /// </summary>
    public class BillboardText
    {

        Vector3 m_position = Vector3.Zero;
        string m_text = "";
        float m_size = .25f;
        Color m_color = Color.White;


        /// <summary>
        /// Default CTOR -- must call Initialize()
        /// </summary>
        /// <param name="ID">base class: OnScreenObject ID</param>
        public BillboardText(int ID)
        {        }

        public bool Initialize(string text)
        {
            m_text = text;
            return true;
        }


        #region Mutators


        public Color Color
        {
            get { return m_color; }
            set { m_color = value; }
        }
        public string Text
        {
            get { return m_text; }
        }
        public float Size
        {
            get { return m_size; }
            set { m_size = value; }
        }
        public Vector3 Position
        {
            get { return m_position; }
            set { m_position = value; }
        }
        public float PositionX
        {
            get { return m_position.X; }
            set { m_position.X = value; }
        }
        public float PositionY
        {
            get { return m_position.Y; }
            set { m_position.Y = value; }
        }
        public float PositionZ
        {
            get { return m_position.Z; }
            set { m_position.Z = value; }
        }

        #endregion Mutators
    }
}
