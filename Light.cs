using geometry;

namespace tinyraytracer
{
    public class Light
    {
        public Vec3f position;
        public float intensity;

        public Light( Vec3f position, float intensity )
        {
            this.position = position;
            this.intensity = intensity;
        }
    }
}
