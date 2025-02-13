using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Threading.Tasks;

namespace Ray_Tracer
{
    class Camera
    {
        private Vector3 origin;
        private Vector3 corner;
        private Vector3 horizontal;
        private Vector3 vertical;
        private Vector3 w;
        private Vector3 u;
        private Vector3 v;
        double lensRadius;

        public Camera(Vector3 origin, Vector3 direction, Vector3 normal, int fieldOfView, double aspectRatio, double radius, double focusDistance)
        {
            float theta = (float) (fieldOfView * Math.PI / 180.0);
            float viewportHeight = (float) (2.0 * Math.Tan(theta / 2.0));
            float viewportWidth = (float) (viewportHeight * aspectRatio);

            w = Vector3.Normalize(Vector3.Subtract(origin, direction));
            u = Vector3.Normalize(Vector3.Cross(normal, w));
            v = Vector3.Cross(w, u);

            this.origin = origin;
            horizontal = Vector3.Multiply((float) focusDistance, Vector3.Multiply(new Vector3(viewportWidth), u));
            vertical = Vector3.Multiply((float)focusDistance, Vector3.Multiply(new Vector3(viewportHeight), v));
            corner = Vector3.Subtract(Vector3.Subtract(Vector3.Subtract(origin, Vector3.Divide(horizontal, new Vector3(2))), Vector3.Divide(vertical, new Vector3(2))), Vector3.Multiply(w, new Vector3((float) focusDistance)));

            lensRadius = radius / 2.0;
        }

        public Ray getRay(double s, double t)
        {
            Vector3 dir = Vector3.Multiply(randomInUnitSphere(-1.0, 1.0), new Vector3((float) lensRadius));
            Vector3 offset = Vector3.Add(Vector3.Multiply(u, new Vector3(dir.X)), Vector3.Multiply(v, new Vector3(dir.Y)));

            return new Ray(Vector3.Add(origin, offset), Vector3.Add(corner, Vector3.Subtract(Vector3.Subtract(Vector3.Add(Vector3.Multiply(horizontal, new Vector3((float) s)), Vector3.Multiply(vertical, new Vector3((float) t))), origin), offset)));
        }

        public Vector3 randomInUnitSphere(double min, double max)
        {
            Random rng = new Random();

            double range = max - min;

            while (true)
            {
                Vector3 vec = new Vector3((float)(range * rng.NextDouble() + min), (float)(range * rng.NextDouble() + min), (float)(range * rng.NextDouble() + min));
                if (vec.LengthSquared() < 1.0)
                    return vec;
            }
        }

    }
}
