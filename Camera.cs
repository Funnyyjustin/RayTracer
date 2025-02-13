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
        float focalLength;
        float viewportHeight;
        float viewportWidth;
        public Vector3 origin;
        Vector3 vpu;
        Vector3 vpv;
        public Vector3 pdu;
        public Vector3 pdv;
        Vector3 viewportOrigin;
        public Vector3 pixelOrigin;

        Random rng = new Random();

        public Camera(Vector3 origin, double aspectRatio, int width, int height)
        {
            focalLength = 1.0f;
            viewportHeight = 2.0f;
            viewportWidth = viewportHeight * (float)aspectRatio;
            this.origin = origin;

            vpu = new Vector3(viewportWidth, 0, 0);
            vpv = new Vector3(0, -viewportHeight, 0);

            pdu = Vector3.Divide(vpu, width);
            pdv = Vector3.Divide(vpv, height);

            viewportOrigin = origin - new Vector3(0, 0, focalLength) - Vector3.Divide(vpu, 2) - Vector3.Divide(vpv, 2);
            pixelOrigin = viewportOrigin + 0.5f * (pdu + pdv);
        }

        public Ray getRay(int x, int y)
        {
            Vector3 offset = sampleSquare();
            Vector3 sample = pixelOrigin + ((x + offset.X) * pdu) + ((y + offset.Y) * pdv);

            Vector3 rayOrigin = origin;
            Vector3 rayDirection = sample - rayOrigin;

            return new Ray(rayOrigin, rayDirection);
        }

        public Vector3 sampleSquare()
        {
            return new Vector3((float) (rng.NextDouble() - 0.5), (float) (rng.NextDouble() - 0.5), 0);
        }
    }
}
