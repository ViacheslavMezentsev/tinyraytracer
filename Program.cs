using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

using geometry;


namespace tinyraytracer
{
    class Program
    {
        private static Vec3f Reflect( Vec3f I, Vec3f N ) 
        {
            return I - N * 2f * ( I * N );
        }

        // Snell's law
        private static Vec3f Refract( Vec3f I, Vec3f N, float eta_t, float eta_i = 1f ) 
        { 
            var cosi = -Math.Max( -1f, Math.Min( 1, I * N ) );

            // if the ray comes from the inside the object, swap the air and the media.
            if ( cosi < 0 ) return Refract( I, -N, eta_i, eta_t );

            var eta = eta_i / eta_t;

            var k = 1 - eta * eta * ( 1 - cosi * cosi );

            // k < 0 = total reflection, no ray to refract. 
            // I refract it anyways, this has no physical meaning.
            return k < 0 ? new Vec3f( 1, 0, 0 ) : I * eta + N * ( eta * cosi - ( float ) Math.Sqrt(k) );
        }


        private static bool SceneIntersect( Vec3f orig, Vec3f dir, List<Sphere> spheres, ref Vec3f hit, ref Vec3f N, ref Material material )
        {
            var spheresDist = float.MaxValue;

            foreach ( var sphere in spheres )
            {
                var disti = 0f;

                if ( sphere.RayIntersect( orig, dir, ref disti ) && disti < spheresDist )
                {
                    spheresDist = disti;

                    hit = orig + dir * disti;

                    N = ( hit - sphere.Center ).normalize();

                    material = sphere.Material;
                }
            }

            var checkerboardDist = float.MaxValue;

            if ( Math.Abs( dir.y ) > 1e-3 )
            {
                // The checkerboard plane has equation y = -4.
                var d = - ( orig.y + 4 ) / dir.y; 

                var pt = orig + dir * d;

                if ( d > 0 && Math.Abs( pt.x ) < 10 && pt.z < -10 && pt.z > -30 && d < spheresDist )
                {
                    checkerboardDist = d;
                    hit = pt;

                    N = new Vec3f( 0, 1, 0 );

                    var c1 = new Vec3f( .3f, .3f, .3f );
                    var c2 = new Vec3f( .3f, .2f, .1f );

                    material.DiffColor = ( ( ( int ) ( .5 * hit.x + 1000 ) + ( int ) ( .5 * hit.z ) ) & 1 ) == 1 ? c1 : c2;
                }
            }

            return Math.Min( spheresDist, checkerboardDist ) < 1000;
        }


        private static Vec3f cast_ray( Vec3f orig, Vec3f dir, List<Sphere> spheres, List<Light> lights, int depth = 0 )
        {
            var point = new Vec3f();
            var N = new Vec3f();
            var material = new Material();

            if ( depth > 4 || !SceneIntersect( orig, dir, spheres, ref point, ref N, ref material ) ) 
            {
                // Background color.
                return new Vec3f( .2f, .7f, .8f ); 
            }

            var reflectDir = Reflect( dir, N ).normalize();
            var refractDir = Refract( dir, N, material.RefIndex ).normalize();

            // Offset the original point to avoid occlusion by the object itself.
            var nscl = N * 1e-3f;
            
            var psub = point - nscl;
            var padd = point + nscl;

            var reflectOrig = reflectDir * N < 0 ? psub : padd; 
            var refractOrig = refractDir * N < 0 ? psub : padd;

            var reflectColor = cast_ray( reflectOrig, reflectDir, spheres, lights, depth + 1 );
            var refractColor = cast_ray( refractOrig, refractDir, spheres, lights, depth + 1 );

            float diffuseLightIntensity = 0, specularLightIntensity = 0;

            foreach ( var light in lights )
            {
                var lightDir = ( light.position - point ).normalize();
            
                var lightDistance = ( light.position - point ).norm();
            
                // Checking if the point lies in the shadow of the light.
                var shadowOrig = lightDir * N < 0 ? psub : padd;
            
                var shadow_pt = new Vec3f();
                var shadow_N = new Vec3f();
                var tmpMaterial = new Material();
            
                var sceneIntersect = SceneIntersect( shadowOrig, lightDir, spheres, ref shadow_pt, ref shadow_N, ref tmpMaterial );
            
                if ( sceneIntersect && ( shadow_pt- shadowOrig ).norm() < lightDistance ) continue;
            
                diffuseLightIntensity += light.intensity * Math.Max( 0, lightDir * N );
            
                specularLightIntensity += ( float ) Math.Pow( Math.Max( 0, -Reflect( -lightDir, N ) * dir ), material.SpecExp ) * light.intensity;
            }

            return material.DiffColor * diffuseLightIntensity * material.Albedo[0] 
                + new Vec3f( 1, 1, 1 ) * specularLightIntensity * material.Albedo[1] 
                + reflectColor * material.Albedo[2] 
                + refractColor * material.Albedo[3];
        }


        private static void Render( List<Sphere> spheres, List<Light> lights )
        {
            var width = 640;
            var height = 480;

            var fov = ( float ) ( Math.PI / 3f );

            var framebuffer = new Vec3f[ width * height ];

            // actual rendering loop
            for ( var j = 0; j < height; j++ )
            { 
                for ( var i = 0; i < width; i++ )
                {
                    // this flips the image at the same time
                    var dirx = i + .5f - width / 2f;

                    var diry = -( j + .5f ) + height / 2f;

                    var dirz = -height / ( 2f * ( float ) Math.Tan( fov / 2f ) );

                    var vcam = new Vec3f( 0, 0, 0 );

                    var vdir = new Vec3f( dirx, diry, dirz ).normalize();

                    framebuffer[ i + j * width ] = cast_ray( vcam, vdir, spheres, lights );
                }
            }

            // Save the framebuffer as image.

            // 32 bits per pixel.
            const int pixelSize = 4; 

            var bmp = new Bitmap( width, height, PixelFormat.Format32bppArgb );

            BitmapData bmpData = null;

            try
            {
                bmpData = bmp.LockBits( new Rectangle( 0, 0, width, height ), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb );

                for ( var y = 0; y < height; ++y )
                    unsafe
                    {
                        var targetRow = ( byte * ) bmpData.Scan0 + y * bmpData.Stride;

                        for ( var x = 0; x < width; ++x )
                        {
                            var i = y * width + x;

                            var c = framebuffer[i];

                            var max = Math.Max( c[0], Math.Max( c[1], c[2] ) );

                            if ( max > 1 ) c = c / max;

                            targetRow[ x * pixelSize + 0 ] = ( byte ) ( 255 * Math.Max( 0f, Math.Min( 1f, c[2] ) ) );
                            targetRow[ x * pixelSize + 1 ] = ( byte ) ( 255 * Math.Max( 0f, Math.Min( 1f, c[1] ) ) );
                            targetRow[ x * pixelSize + 2 ] = ( byte ) ( 255 * Math.Max( 0f, Math.Min( 1f, c[0] ) ) );
                            targetRow[ x * pixelSize + 3 ] = 255;
                        }
                    }
            }
            finally
            {
                if ( bmpData != null ) bmp.UnlockBits( bmpData );
            }

            bmp.Save( "out.png", ImageFormat.Png );
        }

        static void Main()
        {
            // Materials.
            var ivory = new Material( 1, new[] { .6f,  .3f, .1f, .0f }, new Vec3f( .4f, .4f, .3f ), 50 );

            var glass = new Material( 1.5f, new[] { .0f, .5f, .1f, .8f }, new Vec3f( .6f, .7f, .8f ), 125 );

            var red_rubber = new Material( 1, new[] { .9f, .1f, .0f, .0f }, new Vec3f( .3f, .1f, .1f), 10 );

            var mirror = new Material( 1, new[] { .0f, 10f, .8f, .0f }, new Vec3f( 1, 1, 1 ), 1425 );

            // Spheres.            
            var spheres = new List<Sphere>
            {
                new Sphere( new Vec3f( -3, 0, -16 ), 2, ivory ),
                new Sphere( new Vec3f( -1, -1.5f, -12 ), 2, glass ),
                new Sphere( new Vec3f( 1.5f, -0.5f, -18 ), 3, red_rubber ),
                new Sphere( new Vec3f( 7, 5, -18 ), 4, mirror )
            };

            // Lights.
            var lights = new List<Light>
            {
                new Light( new Vec3f( -20, 20, 20 ), 1.5f ),
                new Light( new Vec3f( 30, 50, -25 ), 1.8f ),
                new Light( new Vec3f( 30, 20, 30 ), 1.7f )
            };

            Render( spheres, lights );
        }
    }
}
