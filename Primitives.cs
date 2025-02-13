using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Threading.Tasks;

namespace Ray_Tracer
{
    /// <summary>
    /// A class that contains all information in the situation a ray hits a primitive in the scene.
    /// </summary>
    public class HitRecord
    {
        public Vector3 point { get; set; } // The point of intersection between the ray and the primitive.
        public Vector3 normal { get; set; } // The normal (based on the surface of the primitive) of the intersection location.
        public double t { get; set; } // The t-value of the intersection, based on the equation of a ray.
        public bool hit { get; set; } // Whether the ray hit a primitive or not.
        public Material mat { get; set; } // The material of the hit primitive.

        public bool frontFace { get; set; } // Whether the ray is inside the primitive (3D) or not.

        public HitRecord()
        {
            hit = false;
        }

        public HitRecord(Vector3 point, Vector3 normal, Material mat, double t, bool hit)
        {
            this.point = point;
            this.normal = normal;
            this.t = t;
            this.hit = hit;
            this.mat = mat;
        }

        /// <summary>
        /// Sets the normal based on if the ray is inside the primitive (3D) or not.
        /// </summary>
        /// <param name="r">The given ray.</param>
        /// <param name="outwardNormal">The given normal.</param>
        public void setFaceNormal(Ray r, Vector3 outwardNormal)
        {
            frontFace = Vector3.Dot(r.direction, outwardNormal) < 0;

            // Ray is outside the primitive
            if (frontFace)
                normal = outwardNormal;

            // Ray is inside the primitive
            else
                normal = -outwardNormal;
        }
    }

    /// <summary>
    /// The world, containing all primitives.
    /// </summary>
    public class World
    {
        // A list of all primitives in the scene.
        List<Primitive> primitives;

        public World()
        {
            primitives = new List<Primitive>();
        }

        /// <summary>
        /// Adds a primitive to the world.
        /// </summary>
        /// <param name="p">The primitive that should be added to the world.</param>
        public void Add(Primitive p)
        {
            primitives.Add(p);
        }

        /// <summary>
        /// Checks if a ray hits any primitive in the world.
        /// </summary>
        /// <param name="r">The incoming ray.</param>
        /// <param name="tmin">The minimum t-value.</param>
        /// <param name="tmax">The maximum t-value.</param>
        /// <returns>A hitrecord, containing information about the possible hit.</returns>
        public HitRecord rayHit(Ray r, double tmin, double tmax)
        {
            HitRecord record = new HitRecord();
            double closest = tmax;

            // Checks each primitive in the world, and checks what the closest object is.
            foreach (Primitive p in primitives)
            {
                HitRecord h = p.rayHit(r, tmin, closest);
                if (h.hit)
                {
                    closest = h.t;
                    record = h;
                }
            }

            return record;
        }
    }

    /// <summary>
    /// An abstract class, making sure each primitive (subclass) has a rayHit method.
    /// </summary>
    public abstract class Primitive
    {
        /// <summary>
        /// Checks if a ray hits the primitive.
        /// </summary>
        /// <param name="r">The incoming ray.</param>
        /// <param name="tmin">The minimum t-value.</param>
        /// <param name="tmax">The maximum t-value.</param>
        /// <returns>A hitrecord, containing information about the possible hit.</returns>
        public abstract HitRecord rayHit(Ray r, double tmin, double tmax);
    }

    public class Sphere : Primitive
    {
        public Vector3 center;
        public double radius;
        public Material mat;

        public Sphere(Vector3 center, double radius, Material mat)
        {
            this.center = center;
            this.radius = radius;
            this.mat = mat;
        }

        /// <summary>
        /// Checks if a ray hits the sphere.
        /// </summary>
        /// <param name="r">The incoming ray.</param>
        /// <param name="tmin">The minimum t-value.</param>
        /// <param name="tmax">The maximum t-value.</param>
        /// <returns>A hitrecord, containing information about the possible hit.</returns>
        public override HitRecord rayHit(Ray r, double tmin, double tmax)
        {
            // Calculate the discriminant
            Vector3 offset = center - r.origin;
            var a = Vector3.Dot(r.direction, r.direction);
            var b = -2.0 * Vector3.Dot(r.direction, offset);
            var c = Vector3.Dot(offset, offset) - radius * radius;
            var d = b * b - 4 * a * c;

            // If the discriminant is negative, there's no intersection between our ray and the sphere.
            if (d < 0)
                return new HitRecord();

            // Calculate root
            double sd = Math.Sqrt(d);

            double root = (-b - sd) / (2.0 * a);

            // If the root is outside the boundaries, calculate the other root
            if (root <= tmin || root >= tmax)
            {
                root = (-b + sd) / (2.0 * a);
                // If the other root is also outside the boundaries, there's no hit.
                if (root <= tmin || root >= tmax)
                    return new HitRecord();
            }

            // Otherwise, create hitrecord with a positive hit.
            HitRecord record = new HitRecord();
            record.t = root;
            record.point = r.at(record.t);
            Vector3 outwardNormal = Vector3.Divide(record.point - center, (float) radius);
            record.setFaceNormal(r, outwardNormal);
            record.mat = mat;
            record.hit = true;

            return record;
        }
    }
}
