using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Threading.Tasks;

namespace Ray_Tracer
{
    public struct ScatteredRecord
    {
        public Vector3 attenuation { get; set; }
        public Ray scatteredRay { get; set; }
        public bool scatter { get; set; }

        public ScatteredRecord(Vector3 attenuation, Ray scatteredRay, bool scatter)
        {
            this.attenuation = attenuation;
            this.scatteredRay = scatteredRay;
            this.scatter = scatter;
        }
    }

    public abstract class Material
    {
        protected Random rng = new Random();

        public abstract ScatteredRecord scatter(Ray r, HitRecord h);

        protected Vector3 randomUnitVector()
        {
            Vector3 vec = Vector3.Normalize(new Vector3((float)rng.NextDouble(), (float)rng.NextDouble(), (float)rng.NextDouble()));
            return vec;
        }
    }

    class Lambartian : Material
    {
        private Vector3 color;

        public Lambartian(Vector3 color)
        {
            this.color = color;
        }

        public override ScatteredRecord scatter(Ray r, HitRecord h)
        {
            ScatteredRecord sc = new ScatteredRecord();

            Vector3 scatterDirection = h.normal + randomUnitVector();
            sc.scatteredRay = new Ray(h.point, scatterDirection);
            sc.attenuation = color;
            sc.scatter = true;

            return sc;
        }
    }

    class Metal : Material
    {
        private Vector3 color;
        double fuzz;

        public Metal(Vector3 color, double fuzz)
        {
            this.color = color;
            if (fuzz < 1.0)
            {
                if (fuzz > 0.0)
                    this.fuzz = fuzz;
                else this.fuzz = 0.0;
            }
            else this.fuzz = 1.0;
        }

        public override ScatteredRecord scatter(Ray r, HitRecord h)
        {
            ScatteredRecord sc = new ScatteredRecord();

            Vector3 reflected = reflect(Vector3.Normalize(r.direction), h.normal);
            reflected = Vector3.Normalize(reflected) + ((float) fuzz * randomUnitVector());
            sc.scatteredRay = new Ray(h.point, reflected);
            sc.attenuation = color;
            sc.scatter = Vector3.Dot(sc.scatteredRay.direction, h.normal) > 0.0;

            return sc;
        }

        private Vector3 reflect(Vector3 v, Vector3 n)
        {
            return v - 2 * Vector3.Dot(v, n) * n;
        }
    }

    public class Dielectric : Material
    {
        private float refractionIndex;

        public Dielectric(float refractionIndex)
        {
            this.refractionIndex = refractionIndex;
        }

        public override ScatteredRecord scatter(Ray r, HitRecord h)
        {
            ScatteredRecord sc = new ScatteredRecord();
            sc.attenuation = new Vector3(1.0f, 1.0f, 1.0f);

            double ri;

            if (h.frontFace)
                ri = 1.0f / refractionIndex;
            else ri = refractionIndex;

            Vector3 dir = Vector3.Normalize(r.direction);
            float cosTheta = Math.Min(Vector3.Dot(-dir, h.normal), 1.0f);
            float sinTheta = (float)Math.Sqrt(1.0 - cosTheta * cosTheta);

            bool cannotRefract = ri * sinTheta > 1.0f;
            Vector3 direction;

            if (cannotRefract || reflectance(cosTheta, ri) > rng.NextDouble())
                direction = reflect(dir, h.normal);
            else
                direction = refract(dir, h.normal, ri);

            sc.scatteredRay = new Ray(h.point, direction);
            sc.scatter = true;

            return sc;
        }

        private Vector3 refract(Vector3 uv, Vector3 n, double thing)
        {
            float cosTheta = Math.Min(Vector3.Dot(-uv, n), 1.0f);
            Vector3 r1 = (float)thing * (uv + cosTheta * n);
            Vector3 r2 = (float)-Math.Sqrt(1 - r1.LengthSquared()) * n;
            return r1 + r2;
        }

        private Vector3 reflect(Vector3 v, Vector3 n)
        {
            return v - 2 * Vector3.Dot(v, n) * n;
        }

        private static double reflectance(double cosine, double ri)
        {
            var r0 = (1 - ri) / (1 + ri);
            r0 = r0 * r0;
            return r0 + (1 - r0) * Math.Pow((1 - cosine), 5);
        }
    }
}
