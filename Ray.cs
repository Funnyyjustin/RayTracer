using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Threading.Tasks;

namespace Ray_Tracer
{
    public class Ray
    {
        public Vector3 origin;
        public Vector3 direction;

        public Ray(Vector3 origin, Vector3 direction)
        {
            this.origin = origin;
            this.direction = direction;
        }

        /// <summary>
        /// Calculates the location on a ray, given some t-value.
        /// </summary>
        /// <param name="t">The given t-value.</param>
        /// <returns>The location on the ray.</returns>
        public Vector3 at(double t)
        {
            return origin + (direction * (float) t);
        }
    }
}
