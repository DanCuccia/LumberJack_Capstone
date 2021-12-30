using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

using Game.Drawing_Objects;
using Game.Managers;
using Game.Math_Physics;

namespace Game.Game_Objects
{
    class WorldBorder : Prop
    {
        StaticMesh m_model = new StaticMesh();

        public WorldBorder(ContentManager content) 
        {
            this.Initialize(content);
            base.ID = GameIDList.Prop_Border;
        }

        public void Initialize(ContentManager content)
        {
            m_model.Initialize(content, "models\\border", MyColors.HotRed);
            base.PScale = Vector3.One;
            m_model.GenerateBoundingBox();
        }

        public override void Draw()
        {
            if (m_model != null && Game1.drawDevelopment == true)
                m_model.Draw();
        }

        public override StaticMesh GetBoundingModel()
        {
            if (m_model != null)
                return m_model;
            else return null;
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
        public Vector3 Scale
        {
            get { return base.PScale; }
            set
            {
                base.PScale = value;
                m_model.Scale = value;
            }
        }
        public float ScaleX
        {
            get { return base.PScale.X; }
            set
            {
                base.PScaleX = value;
                m_model.ScaleX = value;
            }
        }
        public float ScaleY
        {
            get { return base.PScale.Y; }
            set
            {
                base.PScaleY = value;
                m_model.ScaleY = value;
            }
        }
        public float ScaleZ
        {
            get { return base.PScale.Z; }
            set
            {
                base.PScaleZ = value;
                m_model.ScaleZ = value;
            }
        }

        #endregion Mutators
    }
}
