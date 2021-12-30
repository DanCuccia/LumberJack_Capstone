using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

using Game.Drawing_Objects;
using Game.Managers;
using Game.Math_Physics;

namespace Game.Game_Objects
{
    class BoatA : LogicProp
    {
        StaticMesh sailModel;

        public BoatA(ContentManager content)
            : base()
        {
            sailModel = new StaticMesh();
            sailModel.Initialize(content, "models\\boatSail", MyColors.BlankWhite);
            sailModel.GenerateBoundingBox(false);

            base.model.Initialize(content, "models\\boat", RenderTechnique.RT_DARKWOOD);
            base.model.GenerateBoundingBox();
            base.ID = GameIDList.LogicProp_BoatA;
        }

        public override void Draw()
        {
            if (base.enabled == false)
                return;

            base.Draw();
            sailModel.Draw();
            if (Game1.drawDevelopment)
                sailModel.DrawOBB();
        }

        #region Mutators

        public override Vector3 Position
        {
            get
            {
                return base.Position;
            }
            set
            {
                base.Position = value;
                sailModel.Position = value;
            }
        }
        public override float PositionX
        {
            get
            {
                return base.PositionX;
            }
            set
            {
                base.PositionX = value;
                sailModel.PositionX = value;
            }
        }
        public override float PositionY
        {
            get
            {
                return base.PositionY;
            }
            set
            {
                base.PositionY = value;
                sailModel.PositionY = value;
            }
        }
        public override float PositionZ
        {
            get
            {
                return base.PositionZ;
            }
            set
            {
                base.PositionZ = value;
                sailModel.PositionZ = value;
            }
        }
        public override Vector3 Rotation
        {
            get
            {
                return base.Rotation;
            }
            set
            {
                base.Rotation = value;
                sailModel.Rotation = value;
            }
        }
        public override float RotationX
        {
            get
            {
                return base.RotationX;
            }
            set
            {
                base.RotationX = value;
                sailModel.RotationX = value;
            }
        }
        public override float RotationY
        {
            get
            {
                return base.RotationY;
            }
            set
            {
                base.RotationY = value;
                sailModel.RotationY = value;
            }
        }
        public override float RotationZ
        {
            get
            {
                return base.RotationZ;
            }
            set
            {
                base.RotationZ = value;
                sailModel.RotationZ = value;
            }
        }
        public override Vector3 Scale
        {
            get
            {
                return base.Scale;
            }
            set
            {
                base.Scale = value;
                sailModel.Scale = value;
            }
        }

        #endregion Mutators
    }
}
