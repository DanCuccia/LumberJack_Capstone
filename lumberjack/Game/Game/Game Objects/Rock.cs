using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

using Game.Drawing_Objects;

namespace Game.Game_Objects
{
    class Rock : Prop
    {
        StaticMesh m_model = new StaticMesh();

        public Rock(){ }

        public override void Draw()
        {
            if (m_model != null)
                m_model.Draw();
        }

        public void Initialize(ContentManager content, string modelFilename, string textureFilename)
        {
            m_model.Initialize(content, modelFilename, textureFilename);
            base.PScale = 1.0f;
        }


        #region Mutators

        public Vector3 Position
        {
            set
            {
                base.PPosition = value;
                m_model.Position = value;
            }
            get { return base.PPosition; }
        }
        public float PositionX
        {
            set
            {
                base.PPositionX = value;
                m_model.PositionX = value;
            }
            get { return base.PPositionX; }
        }
        public float PositionY
        {
            set
            {
                base.PPositionY = value;
                m_model.PositionY = value;
            }
            get { return base.PPositionY; }
        }
        public float PositionZ
        {
            set
            {
                base.PPositionZ = value;
                m_model.PositionZ = value;
            }
            get { return base.PPositionZ; }
        }
        public Vector3 Rotation
        {
            set
            {
                base.PRotation = value;
                m_model.Rotation = value;
            }
            get { return base.PRotation; }
        }
        public float RotationY
        {
            get { return base.PRotationY; }
            set
            {
                base.PRotationY = value;
                m_model.RotationY = value;
            }
        }
        public float RotationX
        {
            get { return base.PRotationX; }
            set
            {
                base.PRotationX = value;
                m_model.RotationX = value;
            }
        }
        public float RotationZ
        {
            get { return base.PRotationZ; }
            set
            {
                base.PRotationZ = value;
                m_model.RotationZ = value;
            }
        }
        public float Scale
        {
            get { return base.PScale; }
            set
            {
                base.PScale = value;
                m_model.Scale = value;
            }
        }

        #endregion Mutators
    }
}
