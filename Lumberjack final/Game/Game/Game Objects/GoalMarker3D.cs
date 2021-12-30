using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

using Game.Drawing_Objects;
using Game.Math_Physics;

namespace Game.Game_Objects
{
    /// <summary>
    /// 3D Goal Marker, that spins along Y
    /// </summary>
    class GoalMarker3D
    {
        StaticMesh goalStar = new StaticMesh();
        StaticMesh goalText = new StaticMesh();

        float rotY = 0;
        const float rotSpeed = 1f;

        /// <summary>default ctor</summary>
        public GoalMarker3D()
        {        }

        /// <summary>
        /// init member variables
        /// </summary>
        /// <param name="content"></param>
        public void Initialize(ContentManager content)
        {
            goalStar.Initialize(content, "models\\goalStar", RenderTechnique.RT_WOOD);
            goalText.Initialize(content, "models\\goalText", MyColors.GoalText);

            Vector3 position = new Vector3(-3277.736f, -1848.879f, -53058.21f);
            goalStar.Position = position;
            goalText.Position = position;

            goalStar.Scale = goalText.Scale = new Vector3(2f);
        }

        /// <summary>rotates the marker along Y-axis</summary>
        public void Update(GameTime gameTime)
        {
            rotY += GoalMarker3D.rotSpeed;
            if (rotY >= 360)
                rotY -= 360;

            goalStar.RotationY = rotY;
            goalText.RotationY = rotY;
        }

        /// <summary> main draw call</summary>
        public void Draw()
        {
            if (goalStar != null)
                goalStar.Draw();
            if (goalText != null)
                goalText.Draw();
        }

        /// <summary>get distance from goal marker</summary>
        /// <param name="testPosition">what your testing against</param>
        /// <returns>world space distance</returns>
        public float getDistanceFrom(Vector3 testPosition)
        {
            return Math.Abs(Vector3.Distance(goalStar.Position, testPosition));
        }

        public Vector3 Position
        {
            get { return this.goalStar.Position; }
        }
    }
}
