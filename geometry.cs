using System;

namespace geometry
{
    public class Vec3f
    {
        public float x;
        public float y;
        public float z;

        public Vec3f( float x, float y, float z )
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Vec3f()
        {
        }

        public float this[ int i ]
        {
            get { return i <= 0 ? x : ( i == 1 ? y : z ); }
        }

        public float norm()
        {
            return ( float ) Math.Sqrt( x * x + y * y + z * z );
        }

        public Vec3f normalize() 
        {
            return this / norm();
        }

        public float[] ToArray()
        {
            return new[] { x, y, z };
        }

        public static Vec3f operator +( Vec3f self, Vec3f other )
        {
            return new Vec3f( self.x + other.x, self.y + other.y, self.z + other.z );
        }

        public static Vec3f operator -( Vec3f self, Vec3f other )
        {
            return new Vec3f( self.x - other.x, self.y - other.y, self.z - other.z );
        }

        public static float operator *( Vec3f self, Vec3f other )
        {
            return self.x * other.x + self.y * other.y + self.z * other.z;
        }

        public static Vec3f operator *( Vec3f self, float value )
        {
            return new Vec3f( self.x * value, self.y * value, self.z * value );
        }

        public static Vec3f operator /( Vec3f self, float value )
        {
            return self * ( 1f / value );
        }

        public static Vec3f operator -( Vec3f self )
        {
            return self * -1f;
        }
    }
}
