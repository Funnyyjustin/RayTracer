using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Threading.Tasks;

namespace Ray_Tracer
{
    /// <summary>
    /// Contains information about scattered rays, given some material it should scatter on.
    /// </summary>
    public struct ScatteredRecord
    {
        public Vector3 attenuation { get; set; } // The intensity of the light / ray / color.
        public Ray scatteredRay { get; set; } // The scattered ray.
        public bool scatter { get; set; } // Whether or not the ray should scatter.

        public ScatteredRecord(Vector3 attenuation, Ray scatteredRay, bool scatter)
        {
            this.attenuation = attenuation;
            this.scatteredRay = scatteredRay;
            this.scatter = scatter;
        }
    }

    /// <summary>
    /// An abstract class for materials, containing some useful methods, as well as maintaining that all materials have a scatter function.
    /// </summary>
    public abstract class Material
    {
        // Random number generator
        protected Random rng = new Random();

        /// <summary>
        /// Given some ray and a hitrecord, this function should calculate information about the scattered ray.
        /// </summary>
        /// <param name="r">The incoming ray.</param>
        /// <param name="h">The hitrecord of the ray.</param>
        /// <returns>A scatterrecord, containing information about the scattered ray.</returns>
        public abstract ScatteredRecord scatter(Ray r, HitRecord h);

        /// <summary>
        /// Gets a random vector that is normalized.
        /// </summary>
        /// <returns>A random, normalized vector.</returns>
        protected Vector3 randomUnitVector()
        {
            Vector3 vec = Vector3.Normalize(new Vector3((float)rng.NextDouble(), (float)rng.NextDouble(), (float)rng.NextDouble()));
            return vec;
        }
    }

    /// <summary>
    /// Lambertian reflectance is the property that defines an ideal "matte" or diffusely reflecting surface. It is similar to a regular diffuse material property.
    /// </summary>
    class Lambartian : Material
    {
        private Vector3 color;

        public Lambartian(Vector3 color)
        {
            this.color = color;
        }

        /// <summary>
        /// Given some ray and a hitrecord, this function calculates information about the scattered ray.
        /// </summary>
        /// <param name="r">The incoming ray.</param>
        /// <param name="h">The hitrecord of the ray.</param>
        /// <returns>A scatterrecord, containing information about the scattered ray.</returns>
        public override ScatteredRecord scatter(Ray r, HitRecord h)
        {
            ScatteredRecord sc = new ScatteredRecord();

            Vector3 scatterDirection = h.normal + randomUnitVector(); // Randomly scatter the light
            sc.scatteredRay = new Ray(h.point, scatterDirection);
            sc.attenuation = color;
            sc.scatter = true;

            return sc;
        }
    }

    /// <summary>
    /// A metal material.
    /// </summary>
    class Metal : Material
    {
        private Vector3 color;
        double fuzz; // How 'clear' the metal is.

        public Metal(Vector3 color, double fuzz)
        {
            this.color = color;

            // Clamp fuzz between 0 and 1.
            if (fuzz < 1.0)
            {
                if (fuzz > 0.0)
                    this.fuzz = fuzz;
                else this.fuzz = 0.0;
            }
            else this.fuzz = 1.0;
        }

        /// <summary>
        /// Given some ray and a hitrecord, this function calculates information about the scattered ray.
        /// </summary>
        /// <param name="r">The incoming ray.</param>
        /// <param name="h">The hitrecord of the ray.</param>
        /// <returns>A scatterrecord, containing information about the scattered ray.</returns>
        public override ScatteredRecord scatter(Ray r, HitRecord h)
        {
            ScatteredRecord sc = new ScatteredRecord();

            Vector3 reflected = reflect(Vector3.Normalize(r.direction), h.normal);
            reflected = Vector3.Normalize(reflected) + ((float) fuzz * randomUnitVector()); // Reflect in a random direction
            sc.scatteredRay = new Ray(h.point, reflected);
            sc.attenuation = color;
            sc.scatter = Vector3.Dot(sc.scatteredRay.direction, h.normal) > 0.0;

            return sc;
        }

        /// <summary>
        /// Calculates a reflected ray, given some (direction) ray and the normal of a surface.
        /// </summary>
        /// <param name="v">The given (direction) ray.</param>
        /// <param name="n">The normal of the surface.</param>
        /// <returns>The outgoing, reflected direction ray.</returns>
        private Vector3 reflect(Vector3 v, Vector3 n)
        {
            return v - 2 * Vector3.Dot(v, n) * n;
        }
    }

    /// <summary>
    /// Dielectric materials are materials such as water, glass and diamond, where the incoming ray is split into a reflected ray and a refracted (transmitted) ray.
    /// </summary>
    public class Dielectric : Material
    {
        private float refractionIndex; // The refraction index of the material.

        public Dielectric(float refractionIndex)
        {
            this.refractionIndex = refractionIndex;
        }

        /// <summary>
        /// Given some ray and a hitrecord, this function calculates information about the scattered ray.
        /// </summary>
        /// <param name="r">The incoming ray.</param>
        /// <param name="h">The hitrecord of the ray.</param>
        /// <returns>A scatterrecord, containing information about the scattered ray.</returns>
        public override ScatteredRecord scatter(Ray r, HitRecord h)
        {
            ScatteredRecord sc = new ScatteredRecord();
            sc.attenuation = new Vector3(1.0f, 1.0f, 1.0f);

            double ri;

            // Refraction index depends on where the ray is ('outside' or 'inside'?).
            if (h.frontFace)
                ri = 1.0f / refractionIndex;
            else ri = refractionIndex;

            Vector3 dir = Vector3.Normalize(r.direction);

            // Calculate angle between incoming ray and the normal
            float cosTheta = Math.Min(Vector3.Dot(-dir, h.normal), 1.0f);

            // Calculate angle of outgoing ray and normal, based on angle from incoming ray and normal
            float sinTheta = (float)Math.Sqrt(1.0 - cosTheta * cosTheta);

            // We can only refract in certain cases.
            bool cannotRefract = ri * sinTheta > 1.0f;
            Vector3 direction;

            // If we cannot refract, we reflect
            if (cannotRefract || reflectance(cosTheta, ri) > rng.NextDouble())
                direction = reflect(dir, h.normal);
            // Otherwise, we refract
            else
                direction = refract(dir, h.normal, ri);

            sc.scatteredRay = new Ray(h.point, direction);
            sc.scatter = true;

            return sc;
        }

        /// <summary>
        /// Given the incoming (direction) ray, and the normal of the surface, as well as the refraction index, this function calculates the refracted ray.
        /// </summary>
        /// <param name="uv">The incoming (direction) ray.</param>
        /// <param name="n">The normal of the surface.</param>
        /// <param name="ri">The refraction index.</param>
        /// <returns>A refracted (direction) ray.</returns>
        private Vector3 refract(Vector3 uv, Vector3 n, double ri)
        {
            float cosTheta = Math.Min(Vector3.Dot(-uv, n), 1.0f);
            Vector3 r1 = (float)ri * (uv + cosTheta * n);
            Vector3 r2 = (float)-Math.Sqrt(1 - r1.LengthSquared()) * n;
            return r1 + r2;
        }

        /// <summary>
        /// Calculates a reflected ray, given some (direction) ray and the normal of a surface.
        /// </summary>
        /// <param name="v">The given (direction) ray.</param>
        /// <param name="n">The normal of the surface.</param>
        /// <returns>The outgoing, reflected direction ray.</returns>
        private Vector3 reflect(Vector3 v, Vector3 n)
        {
            return v - 2 * Vector3.Dot(v, n) * n;
        }

        /// <summary>
        /// Calculates some reflectance value, based on Schlick's approximation for reflectance.
        /// </summary>
        /// <param name="cosine">The angle with which the direction ray is coming into the primitive.</param>
        /// <param name="ri">The refraction index.</param>
        /// <returns>Some reflectance value, stating how well the ray should be reflected.</returns>
        private static double reflectance(double cosine, double ri)
        {
            var r0 = (1 - ri) / (1 + ri);
            r0 = r0 * r0;
            return r0 + (1 - r0) * Math.Pow((1 - cosine), 5);
        }
    }
}
