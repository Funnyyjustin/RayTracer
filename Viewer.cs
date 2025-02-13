using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ray_Tracer
{
    public partial class Viewer : Form
    {
        static Random rng = new Random();

        double aspectRatio = 16.0 / 9.0;
        int width = 1280;
        int height;

        public Viewer()
        {
            // Calculate height
            height = (int)(width / aspectRatio);

            InitializeComponent(width, height);
        }

        /// <summary>
        /// Creates a new bitmap.
        /// </summary>
        /// <returns>A bitmap with all pixels having the correct color.</returns>
        public Bitmap Render()
        {
            // Set up new bitmap
            Bitmap img = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            // Set up camera
            Vector3 origin = new Vector3(0, 0, 0);
            Vector3 direction = new Vector3(0, 0, 1);
            Vector3 normal = new Vector3(0, 1, 0);
            double focusDistance = 10;

            Camera cam = new Camera(origin, direction, normal, 20, aspectRatio, 0.1, focusDistance);

            int numRays = 1;

            // Color each pixel of the bitmap
            for (int y = 0; y < height; ++y)
            {
                for (int x = 0; x < width; ++x)
                {
                    for (int s = 0; s < numRays; ++s)
                    {
                        double flippedY = height - y - 1;
                        double u = (double)(x + rng.NextDouble()) / (width - 1);
                        double v = (flippedY + rng.NextDouble()) / (height - 1);

                        Ray r = cam.getRay(u, v);
                        img.SetPixel(x, y, getColor(rayColor(r)));
                    }
                }
            }

            return img;
        }

        private Vector3 rayColor(Ray r)
        {
            Vector3 direction = Vector3.Normalize(r.direction);
            var a = 0.5 * (direction.Y + 1.0);

            return (float) (1.0 - a) * new Vector3(1.0f) + (float) a * new Vector3(0.5f, 0.7f, 1.0f);
        }

        private Color getColor(Vector3 color)
        {
            color = color * 255f;
            return Color.FromArgb((int) (color.X), (int) (color.Y), (int) (color.Z));
        }

        /// <summary>
        /// Draws the bitmap to the forms
        /// </summary>
        private void Viewer_Paint(object sender, PaintEventArgs e)
        {
            Bitmap img = Render();
            e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            e.Graphics.DrawImage(img, new Rectangle(0, 0, width, height), 0, 0, width, height, GraphicsUnit.Pixel);
        }
    }
}
