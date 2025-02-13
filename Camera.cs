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
        // Viewport dimensions
        float focalLength;
        float viewportHeight;
        float viewportWidth;

        public Vector3 origin; // Camera center

        // Vectors across horizontal and down vertical viewport edges
        Vector3 vpu; // Horizontal
        Vector3 vpv; // Vertical

        public Vector3 pdu; // Offset to pixel to the right
        public Vector3 pdv; // Offset to pixel below

        // Location of pixel (0, 0)
        Vector3 viewportOrigin;
        public Vector3 pixelOrigin;

        Random rng = new Random();

        public Camera(Vector3 origin, double aspectRatio, int width, int height)
        {
            // Calculate viewport dimensions
            focalLength = 1.0f;
            viewportHeight = 2.0f;
            viewportWidth = viewportHeight * (float)aspectRatio;

            this.origin = origin;

            // Calculate vectors across horizontal and down vertical viewport edges
            vpu = new Vector3(viewportWidth, 0, 0);
            vpv = new Vector3(0, -viewportHeight, 0);

            // Calculate horizontal and vertical delta vectors from pixel to pixel
            pdu = Vector3.Divide(vpu, width);
            pdv = Vector3.Divide(vpv, height);

            // Calculate location of upper left pixel
            viewportOrigin = origin - new Vector3(0, 0, focalLength) - Vector3.Divide(vpu, 2) - Vector3.Divide(vpv, 2);
            pixelOrigin = viewportOrigin + 0.5f * (pdu + pdv);
        }

        /// <summary>
        /// Gets the correct ray that goes through the 'camera', based on the (x, y) coordinates on the camera.
        /// </summary>
        /// <param name="x">The x-coordinate on the camera.</param>
        /// <param name="y">The y-coordinate on the camera.</param>
        /// <returns>A ray that goes through these coordinates in the camera.</returns>
        public Ray getRay(int x, int y)
        {
            Vector3 offset = sampleSquare();
            Vector3 sample = pixelOrigin + ((x + offset.X) * pdu) + ((y + offset.Y) * pdv);

            Vector3 rayOrigin = origin;
            Vector3 rayDirection = sample - rayOrigin;

            return new Ray(rayOrigin, rayDirection);
        }

        /// <summary>
        /// Randomly generates a 3D-vector.
        /// </summary>
        /// <returns>A random 3D-vector.</returns>
        public Vector3 sampleSquare()
        {
            return new Vector3((float) (rng.NextDouble() - 0.5), (float) (rng.NextDouble() - 0.5), 0);
        }
    }
}
