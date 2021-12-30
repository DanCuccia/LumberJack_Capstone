using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

using Game.Drawing_Objects;

namespace Game.Game_Objects
{

    public class LogicPropXmlMedium
    {
        public int id;
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;
        public bool enabled;
    }

    /// <summary>
    /// Special Prop is used for objects like the damn, the house, the boat, etc...
    /// Any prop-like object that requires actual game logic will enherit from this
    /// </summary>
    public class LogicProp : Prop
    {
        #region Member Variables

        protected StaticMesh    model;
        protected bool          enabled;

        #endregion Member Variables


        #region Initialization

        /// <summary>default CTOR</summary>
        public LogicProp()
            : base()
        {
            enabled = true;
            model = new StaticMesh();
        }

        #endregion Initialization


        #region API

        /// <summary>main draw call</summary>
        public override void Draw()
        {
            if (model != null)
            {
                model.Draw();
                if (Game1.drawDevelopment)
                {
                    model.DrawOBB();
                }
            }
        }

        /// <summary>get the correct model to test collision with</summary>
        /// <returns>the model programmed to test collision with, may be null</returns>
        public override StaticMesh GetBoundingModel()
        {
            return model;
        }

        /// <summary>Get the xml struct that will be serializaed</summary>
        public LogicPropXmlMedium GetLogicPropXmlMedium()
        {
            LogicPropXmlMedium output = new LogicPropXmlMedium();

            output.enabled = this.enabled;
            output.id = base.ID;
            output.position = this.GetBoundingModel().Position;
            output.rotation = this.GetBoundingModel().Rotation;
            output.scale = this.GetBoundingModel().Scale;

            return output;
        }

        #endregion API


        #region Mutators

        public bool Enabled
        {
            get { return this.enabled; }
            set 
            {
                this.enabled = value;
                this.model.IsVisible = value;
            }
        }
        virtual public Vector3 Position
        {
            get { return base.PPosition; }
            set
            {
                base.PPosition = value;
                model.Position = value;
            }
        }
        virtual public float PositionX
        {
            get { return base.PPositionX; }
            set
            {
                base.PPositionX = value;
                model.PositionX = value;
            }
        }
        virtual public float PositionY
        {
            get { return base.PPositionY; }
            set
            {
                base.PPositionY = value;
                model.PositionY = value;
            }
        }
        virtual public float PositionZ
        {
            get { return base.PPositionZ; }
            set
            {
                base.PPositionZ = value;
                model.PositionZ = value;
            }
        }
        virtual public Vector3 Rotation
        {
            get { return base.PRotation; }
            set
            {
                base.PRotation = value;
                model.Rotation = value;
            }
        }
        virtual public float RotationX
        {
            get { return base.PRotationX; }
            set
            {
                base.PRotationX = value;
                model.RotationX = value;
            }
        }
        virtual public float RotationY
        {
            get { return base.PRotationY; }
            set
            {
                base.PRotationY = value;
                model.RotationY = value;
            }
        }
        virtual public float RotationZ
        {
            get { return base.PRotationZ; }
            set
            {
                base.PRotationZ = value;
                model.RotationZ = value;
            }
        }
        virtual public Vector3 Scale
        {
            get { return base.PScale; }
            set
            {
                base.PScale = value;
                model.Scale = value;
            }
        }

        #endregion Mutators
    }
}
