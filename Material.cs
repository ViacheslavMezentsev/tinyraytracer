using geometry;

namespace tinyraytracer
{
    public class Material
    {
        public float RefIndex;        
        public Vec3f DiffColor;
        public float SpecExp;
        public float[] Albedo;

        public Material( float refIndex, float[] albedo, Vec3f diffColor, float specExp )
        {
            RefIndex = refIndex;
            Albedo = albedo;
            DiffColor = diffColor;
            SpecExp = specExp;
        }

        public Material()
        {
            RefIndex = 1;
            SpecExp = 0;
            Albedo = new[] { 1f, 0f, 0f, 0f };

            DiffColor = new Vec3f();            
        }
    }
}
