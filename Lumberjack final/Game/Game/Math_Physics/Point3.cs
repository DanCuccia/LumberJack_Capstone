using System;

namespace Game.Math_Physics
{
    public struct Point3
    {
        public int x, y, z;

        public Point3(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public bool equals(Point3 other)
        {
            if (this.x == other.x &&
                this.y == other.y &&
                this.z == other.z)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    };
}
