using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Game.Game_Objects.Build_System
{
    public class BuildCamera
    {
        public Vector3 position;
        public Vector3 target;
        Vector3 up;
        
        public Matrix view;
        public Matrix projection;

        float nearPlane;
        float farPlane;

        public BuildCamera(Vector3 position, Vector3 target, float nearPlane = 1.0f, float farPlane = 1000.0f)
        {
            this.position = position;
            this.target = target;
            this.nearPlane = nearPlane;
            this.farPlane = farPlane;

            this.up = new Vector3(0.0f, 1.0f, 0.0f);

            this.projection = Matrix.CreatePerspectiveFieldOfView((float)Math.PI / 4.0f, 1.333f, nearPlane, farPlane);
        }

        public void movePosition(Vector3 speed)
        {
            position += speed;
        }

        public void moveTarget(Vector3 speed)
        {
            target += speed;
        }

        public void move(Vector3 speed)
        {
            position += speed;
            target += speed;
        }

        public void moveTowardsPoint(Vector3 point, Vector3 speed)
        {
            if (this.position.X < point.X)
                this.position.X += speed.X;
            if (this.position.X > point.X)
                this.position.X -= speed.X;

            if (this.position.Y < point.Y)
                this.position.Y += speed.Y;
            if (this.position.Y > point.Y)
                this.position.Y -= speed.Y;

            if (this.position.Z < point.Z)
                this.position.Z += speed.Z;
            if (this.position.Z > point.Z)
                this.position.Z -= speed.Z;
        }

        public virtual void update()
        {
            this.view = Matrix.CreateLookAt(this.position, this.target, this.up);
        }
    }
}
