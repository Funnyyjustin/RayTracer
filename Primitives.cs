using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Threading.Tasks;

namespace Ray_Tracer
{
    public class HitRecord
    {
        public Vector3 point { get; set; }
        public Vector3 normal { get; set; }
        public double t { get; set; }
        public bool hit { get; set; }

        public bool frontFace { get; set; }

        public HitRecord()
        {
            hit = false;
        }

        public HitRecord(Vector3 point, Vector3 normal, double t, bool hit)
        {
            this.point = point;
            this.normal = normal;
            this.t = t;
            this.hit = hit;
        }

        public void setFaceNormal(Ray r, Vector3 outwardNormal)
        {
            frontFace = Vector3.Dot(r.direction, outwardNormal) < 0;

            if (frontFace)
                normal = outwardNormal;
            else
                normal = -outwardNormal;
        }
    }

    public class World
    {
        List<Primitive> primitives;

        public World()
        {
            primitives = new List<Primitive>();
        }

        public void Add(Primitive p)
        {
            primitives.Add(p);
        }

        public HitRecord rayHit(Ray r, double tmin, double tmax)
        {
            HitRecord record = new HitRecord();
            double closest = tmax;

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

    public abstract class Primitive
    {
        public abstract HitRecord rayHit(Ray r, double tmin, double tmax);
    }

    public class Sphere : Primitive
    {
        public Vector3 center;
        public double radius;

        public Sphere(Vector3 center, double radius)
        {
            this.center = center;
            this.radius = radius;
        }

        public override HitRecord rayHit(Ray r, double tmin, double tmax)
        {
            Vector3 offset = center - r.origin;
            var a = Vector3.Dot(r.direction, r.direction);
            var b = -2.0 * Vector3.Dot(r.direction, offset);
            var c = Vector3.Dot(offset, offset) - radius * radius;
            var d = b * b - 4 * a * c;

            if (d < 0)
                return new HitRecord();

            double sd = Math.Sqrt(d);

            double root = (-b - sd) / (2.0 * a);

            if (root <= tmin || root >= tmax)
            {
                root = (-b + sd) / (2.0 * a);
                if (root <= tmin || root >= tmax)
                    return new HitRecord();
            }

            HitRecord record = new HitRecord();
            record.t = root;
            record.point = r.at(record.t);
            Vector3 outwardNormal = Vector3.Divide(record.point - center, (float) radius);
            record.setFaceNormal(r, outwardNormal);
            record.hit = true;

            return record;
        }
    }
}
