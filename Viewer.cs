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
        int width = 800;
        int height;

        static int samples = 100;
        double pixelSamplesScale = 1.0 / samples;
        int maxDepth = 50;

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

            Camera cam = new Camera(origin, aspectRatio, width, height);

            // Set up world
            World world = new World();
            world.Add(new Sphere(new Vector3(0, 0, -1), 0.5));
            world.Add(new Sphere(new Vector3(0, -100.5f, -1), 100));

            // Color each pixel of the bitmap
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Vector3 color = new Vector3(0, 0, 0);

                    for (int s = 0; s < samples; s++)
                    {
                        Ray r = cam.getRay(x, y);
                        color += rayColor(r, maxDepth, world);
                    }

                    img.SetPixel(x, y, getColor(color));
                }
            }

            return img;
        }

        private Vector3 rayColor(Ray r, int depth, World w)
        {
            if (depth <= 0)
                return new Vector3(0, 0, 0);

            HitRecord record = w.rayHit(r, 0.001f, int.MaxValue);

            if (record.hit)
            {
                Vector3 dir = record.normal + randomUnitVector();
                return 0.5f * rayColor(new Ray(record.point, dir), depth - 1, w);
            }

            Vector3 unitdir = Vector3.Normalize(r.direction);
            float a = 0.5f * (unitdir.Y + 1.0f);
            return ((1.0f - a) * new Vector3(1, 1, 1)) + (a * new Vector3(0.5f, 0.7f, 1));
        }

        private Color getColor(Vector3 color)
        {
            color = (float) pixelSamplesScale * color;

            float min = 0;
            float max = 0.999f;

            int r = (int)(255 * Clamp(color.X, min, max));
            int g = (int)(255 * Clamp(color.Y, min, max));
            int b = (int)(255 * Clamp(color.Z, min, max));

            return Color.FromArgb(r, g, b);
        }

        private float Clamp(float value, float min, float max)
        {
            if (value < min) value = min;
            if (value > max) value = max;
            return value;
        }

        private Vector3 randomUnitVector()
        {
            Vector3 vec = Vector3.Normalize(new Vector3((float) rng.NextDouble(), (float) rng.NextDouble(), (float) rng.NextDouble()));
            return vec;
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
