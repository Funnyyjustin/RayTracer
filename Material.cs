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
        Random rng = new Random();

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
}
