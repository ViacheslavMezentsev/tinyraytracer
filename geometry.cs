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
            var v = scale( 1.0f / norm() );

            x = v.x;
            y = v.y;
            z = v.z;

            return this;
        }

        public float[] ToArray()
        {
            return new[] { x, y, z };
        }

        public Vec3f add( Vec3f other )
        {
            return new Vec3f( x + other.x, y + other.y, z + other.z );
        }

        public Vec3f sub( Vec3f other )
        {
            return new Vec3f( x - other.x, y - other.y, z - other.z );
        }

        public float mul( Vec3f other )
        {
            return x * other.x + y * other.y + z * other.z;
        }

        public Vec3f scale( float rhs )
        {
            return new Vec3f( x * rhs, y * rhs, z * rhs );
        }

        public Vec3f reverse()
        {
            return new Vec3f( -x, -y, -z );
        }
    }
}
