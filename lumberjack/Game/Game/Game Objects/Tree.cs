using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using Game.Drawing_Objects;
using Game.Math_Physics;
using Game.Managers;


namespace Game.Game_Objects
{
    class Tree : Prop
    {
        protected StaticMesh  trunk = new StaticMesh();
        protected StaticMesh  top = new StaticMesh();

        public Tree() { }

        /// <summary>Initialize an entire tree</summary>
        /// <param name="trunkFilename">filepath of the trunk model</param>
        /// <param name="treeTopFilename">filepath of the tree top model</param>
        public void Initialize(ContentManager content, string trunkFilename, string treeTopFilename)
        {
            trunk.Initialize(content, trunkFilename, RenderTechnique.RT_DARKWOOD);
            top.Initialize(content, treeTopFilename, MyColors.TreeTopGreen);
            base.PScale = 1.0f;
        }

        public override void Draw()
        {
            trunk.Draw();
            top.Draw();
        }

        #region Mutators

        public Vector3 Position
        {
            set
            {
                base.PPosition = value;
                trunk.Position = value;
                top.Position = value;
            }
            get { return base.PPosition; }
        }
        public float PositionX
        {
            set
            {
                base.PPositionX = value;
                trunk.PositionX = value;
                top.PositionX = value;
            }
            get { return base.PPositionX; }
        }
        public float PositionY
        {
            set
            {
                base.PPositionY = value;
                trunk.PositionY = value;
                top.PositionY = value;
            }
            get { return base.PPositionY; }
        }
        public float PositionZ
        {
            set
            {
                base.PPositionZ = value;
                trunk.PositionZ = value;
                top.PositionZ = value;
            }
            get { return base.PPositionZ; }
        }
        public Vector3 Rotation
        {
            set
            {
                base.PRotation = value;
                trunk.Rotation = value;
                top.Rotation = value;
            }
            get { return base.PRotation; }
        }
        public float RotationY
        {
            get { return base.PRotationY; }
            set
            {
                base.PRotationY = value;
                trunk.RotationY = value;
                top.RotationY = value;
            }
        }
        public float RotationX
        {
            get { return base.PRotationX; }
            set
            {
                base.PRotationX = value;
                trunk.RotationX = value;
                top.RotationX = value;
            }
        }
        public float RotationZ
        {
            get { return base.PRotationZ; }
            set
            {
                base.PRotationZ = value;
                trunk.RotationZ = value;
                top.RotationZ = value;
            }
        }
        public float Scale
        {
            get { return base.PScale; }
            set
            {
                base.PScale = value;
                trunk.Scale = value;
                top.Scale = value;
            }
        }

        #endregion Mutators

    }
}
