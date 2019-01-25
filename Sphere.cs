using System;
using geometry;

namespace tinyraytracer
{
    public class Sphere
    {
        public Vec3f Center;
        public float Radius;
        public Material Material;

        public Sphere( Vec3f center, float radius, Material material )
        {
            Center = center;
            Radius = radius;
            Material = material;
        }

        public bool RayIntersect( Vec3f orig, Vec3f dir, ref float t0 )
        {
            var L = Center.sub( orig );

            var tca = L.mul( dir );

            var d2 = L.mul(L) - tca * tca;

            if ( d2 > Radius * Radius ) return false;

            var thc = ( float ) Math.Sqrt( Radius * Radius - d2 );

            t0 = tca - thc;

            var t1 = tca + thc;

            if ( t0 < 0 ) t0 = t1;

            if ( t0 < 0 ) return false;

            return true;
        }
    }
}
