using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;


namespace Game.Math_Physics
{
    public class OrientedBoundingBox
    {
        private Vector3     min;
        private Vector3     max;
        private Vector3     center;
        private Vector3     extents;

        private Matrix      world       = Matrix.Identity;

        public OrientedBoundingBox(Vector3 min, Vector3 max)
        {
            this.min = min;
            this.max = max;

            updateFromMinMax();
        }

        private void updateFromMinMax()
        {
            this.center = (min + max) * .5f;
            this.extents = (max - min) * .5f;
        }

        public bool Intersects(OrientedBoundingBox other)
        {
            // Matrix to transform other OBB into my reference to allow me to be treated as an AABB
            Matrix toMe = other.World * Matrix.Invert(world);

            Vector3 centerOther = MyMath.Multiply(other.Center, toMe);
            Vector3 extentsOther = other.Extents;
            Vector3 separation = centerOther - center;

            Matrix3 rotations = new Matrix3(toMe);
            Matrix3 absRotations = MyMath.Abs(rotations);

            float r, r0, r1, r01;

            //--- Test case 1 - X axis

            r = Math.Abs(separation.X);
            r1 = Vector3.Dot(extentsOther, absRotations.Column(0));
            r01 = extents.X + r1;

            if (r > r01) return false;

            //--- Test case 1 - Y axis

            r = Math.Abs(separation.Y);
            r1 = Vector3.Dot(extentsOther, absRotations.Column(1));
            r01 = extents.Y + r1;

            if (r > r01) return false;

            //--- Test case 1 - Z axis

            r = Math.Abs(separation.Z);
            r1 = Vector3.Dot(extentsOther, absRotations.Column(2));
            r01 = extents.Z + r1;

            if (r > r01) return false;

            //--- Test case 2 - X axis

            r = Math.Abs(Vector3.Dot(rotations.Row(0), separation));
            r0 = Vector3.Dot(extents, absRotations.Row(0));
            r01 = r0 + extentsOther.X;

            if (r > r01) return false;

            //--- Test case 2 - Y axis

            r = Math.Abs(Vector3.Dot(rotations.Row(1), separation));
            r0 = Vector3.Dot(extents, absRotations.Row(1));
            r01 = r0 + extentsOther.Y;

            if (r > r01) return false;

            //--- Test case 2 - Z axis

            r = Math.Abs(Vector3.Dot(rotations.Row(2), separation));
            r0 = Vector3.Dot(extents, absRotations.Row(2));
            r01 = r0 + extentsOther.Z;

            if (r > r01) return false;

            //--- Test case 3 # 1

            r = Math.Abs(separation.Z * rotations[0, 1] - separation.Y * rotations[0, 2]);
            r0 = extents.Y * absRotations[0, 2] + extents.Z * absRotations[0, 1];
            r1 = extentsOther.Y * absRotations[2, 0] + extentsOther.Z * absRotations[1, 0];
            r01 = r0 + r1;

            if (r > r01) return false;

            //--- Test case 3 # 2

            r = Math.Abs(separation.Z * rotations[1, 1] - separation.Y * rotations[1, 2]);
            r0 = extents.Y * absRotations[1, 2] + extents.Z * absRotations[1, 1];
            r1 = extentsOther.X * absRotations[2, 0] + extentsOther.Z * absRotations[0, 0];
            r01 = r0 + r1;

            if (r > r01) return false;

            //--- Test case 3 # 3

            r = Math.Abs(separation.Z * rotations[2, 1] - separation.Y * rotations[2, 2]);
            r0 = extents.Y * absRotations[2, 2] + extents.Z * absRotations[2, 1];
            r1 = extentsOther.X * absRotations[1, 0] + extentsOther.Y * absRotations[0, 0];
            r01 = r0 + r1;

            if (r > r01) return false;

            //--- Test case 3 # 4

            r = Math.Abs(separation.X * rotations[0, 2] - separation.Z * rotations[0, 0]);
            r0 = extents.X * absRotations[0, 2] + extents.Z * absRotations[0, 0];
            r1 = extentsOther.Y * absRotations[2, 1] + extentsOther.Z * absRotations[1, 1];
            r01 = r0 + r1;

            if (r > r01) return false;

            //--- Test case 3 # 5

            r = Math.Abs(separation.X * rotations[1, 2] - separation.Z * rotations[1, 0]);
            r0 = extents.X * absRotations[1, 2] + extents.Z * absRotations[1, 0];
            r1 = extentsOther.X * absRotations[2, 1] + extentsOther.Z * absRotations[0, 1];
            r01 = r0 + r1;

            if (r > r01) return false;

            //--- Test case 3 # 6

            r = Math.Abs(separation.X * rotations[2, 2] - separation.Z * rotations[2, 0]);
            r0 = extents.X * absRotations[2, 2] + extents.Z * absRotations[2, 0];
            r1 = extentsOther.X * absRotations[1, 1] + extentsOther.Y * absRotations[0, 1];
            r01 = r0 + r1;

            if (r > r01) return false;

            //--- Test case 3 # 7

            r = Math.Abs(separation.Y * rotations[0, 0] - separation.X * rotations[0, 1]);
            r0 = extents.X * absRotations[0, 1] + extents.Y * absRotations[0, 0];
            r1 = extentsOther.Y * absRotations[2, 2] + extentsOther.Z * absRotations[1, 2];
            r01 = r0 + r1;

            if (r > r01) return false;

            //--- Test case 3 # 8

            r = Math.Abs(separation.Y * rotations[1, 0] - separation.X * rotations[1, 1]);
            r0 = extents.X * absRotations[1, 1] + extents.Y * absRotations[1, 0];
            r1 = extentsOther.X * absRotations[2, 2] + extentsOther.Z * absRotations[0, 2];
            r01 = r0 + r1;

            if (r > r01) return false;

            //--- Test case 3 # 9

            r = Math.Abs(separation.Y * rotations[2, 0] - separation.X * rotations[2, 1]);
            r0 = extents.X * absRotations[2, 1] + extents.Y * absRotations[2, 0];
            r1 = extentsOther.X * absRotations[1, 2] + extentsOther.Y * absRotations[0, 2];
            r01 = r0 + r1;

            if (r > r01) return false;

            return true;  // No separating axis, then we have intersection
        }

        public int Intersects(Ray ray, out float nearCollision, out float farCollision)
        {

            Vector3 rayOrigin = ray.Position;
            Vector3 rayDirection = ray.Direction;
            Matrix inverseWorld;
            Matrix.Invert(ref world, out inverseWorld);

            Vector3.Transform(ref rayOrigin, ref inverseWorld, out rayOrigin);
            Vector3.TransformNormal(ref rayDirection, ref inverseWorld, out rayDirection);

            Vector3 min = this.min;
            Vector3 max = this.max;

            float t, t1, t2;

            nearCollision = float.MinValue;
            farCollision = float.MaxValue;

            int face, i = -1, j = -1;

            if (rayDirection.X > -0.00001f && rayDirection.X < -0.00001f)
            {
                if (rayOrigin.X < min.X || rayOrigin.X > max.X)
                    return -1;
            }
            else
            {
                t = 1.0f / rayDirection.X;
                t1 = (min.X - rayOrigin.X) * t;
                t2 = (max.X - rayOrigin.X) * t;

                if (t1 > t2)
                {
                    t = t1; t1 = t2; t2 = t;
                    face = 0;
                }
                else
                    face = 3;

                if (t1 > nearCollision)
                {
                    nearCollision = t1;
                    i = face;
                }
                if (t2 < farCollision)
                {
                    farCollision = t2;
                    if (face > 2)
                        j = face - 3;
                    else
                        j = face + 3;
                }

                if (nearCollision > farCollision || farCollision < 0.00001f)
                    return -1;
            }

            // intersect in Y 
            if (rayDirection.Y > -0.00001f && rayDirection.Y < -0.00001f)
            {
                if (rayOrigin.Y < min.Y || rayOrigin.Y > max.Y)
                    return -1;
            }
            else
            {
                t = 1.0f / rayDirection.Y;
                t1 = (min.Y - rayOrigin.Y) * t;
                t2 = (max.Y - rayOrigin.Y) * t;

                if (t1 > t2)
                {
                    t = t1; t1 = t2; t2 = t;
                    face = 1;
                }
                else
                    face = 4;

                if (t1 > nearCollision)
                {
                    nearCollision = t1;
                    i = face;
                }
                if (t2 < farCollision)
                {
                    farCollision = t2;
                    if (face > 2)
                        j = face - 3;
                    else
                        j = face + 3;
                }

                if (nearCollision > farCollision || farCollision < 0.00001f)
                    return -1;
            }

            // intersect in Z 
            if (rayDirection.Z > -0.00001f && rayDirection.Z < -0.00001f)
            {
                if (rayOrigin.Z < min.Z || rayOrigin.Z > max.Z)
                    return -1;
            }
            else
            {
                t = 1.0f / rayDirection.Z;
                t1 = (min.Z - rayOrigin.Z) * t;
                t2 = (max.Z - rayOrigin.Z) * t;

                if (t1 > t2)
                {
                    t = t1; t1 = t2; t2 = t;
                    face = 2;
                }
                else
                    face = 5;

                if (t1 > nearCollision)
                {
                    nearCollision = t1;
                    i = face;
                }
                if (t2 < farCollision)
                {
                    farCollision = t2;
                    if (face > 2)
                        j = face - 3;
                    else
                        j = face + 3;
                }
            }

            if (nearCollision > farCollision || farCollision < 0.00001f)
                return -1;

            if (nearCollision < 0.0f)
                return j;
            else
                return i;
        }

        #region Mutators
        public Vector3 Max
        {
            get { return this.max; }
            set { this.max = value; this.updateFromMinMax(); }
        }
        public Vector3 Min
        {
            get { return this.min; }
            set { this.min = value; this.updateFromMinMax(); }
        }
        public Vector3 Center
        {
            get { return this.center; }
        }
        public Vector3 Extents
        {
            get { return this.extents; }
        }
        public Matrix World
        {
            get { return this.world; }
            set { this.world = value; }
        }
        #endregion Mutators
    }
}
