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

        static int samples = 100; // How many rays we shoot through each pixel in the scene.
        double pixelSamplesScale = 1.0 / samples; // Color scale factor for a sum of pixel samples.
        int maxDepth = 25; // How often a ray can bounce before we terminate the recursion.

        public Viewer()
        {
            // Calculate height
            height = (int)(width / aspectRatio);

            InitializeComponent(width, height);
        }

        /// <summary>
        /// Creates a new bitmap.
        /// </summary>
        /// <returns>A bitmap with the raytraced scene.</returns>
        public Bitmap Render()
        {
            // Set up new bitmap
            Bitmap img = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            // Set up camera
            Vector3 origin = new Vector3(0, 0, 0);

            Camera cam = new Camera(origin, aspectRatio, width, height);

            // Set up world
            World world = new World();

            // Add materials
            Material m1 = new Lambartian(new Vector3(0.8f, 0.8f, 0.0f));
            Material m2 = new Lambartian(new Vector3(0.1f, 0.2f, 0.5f));
            Material m3 = new Dielectric(1.50f);
            Material m5 = new Dielectric(1.00f / 1.50f);
            Material m4 = new Metal(new Vector3(0.8f, 0.6f, 0.2f), 1.0f);

            world.Add(new Sphere(new Vector3(0.0f, -100.5f, -1.0f), 100.0f, m1));
            world.Add(new Sphere(new Vector3(0.0f, 0.0f, -1.2f), 0.5f, m2));
            world.Add(new Sphere(new Vector3(-1.0f, 0.0f, -1.0f), 0.5f, m3));
            world.Add(new Sphere(new Vector3(-1.0f, 0.0f, -1.0f), 0.4f, m5)); // Smaller sphere inside the previous sphere
            world.Add(new Sphere(new Vector3(1.0f, 0.0f, -1.0f), 0.5f, m4));

            // Color each pixel of the bitmap
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Vector3 color = new Vector3(0, 0, 0);

                    // Shoot a number of rays through the camera
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

        /// <summary>
        /// Calculates which color the pixel should receive, based on the world.
        /// </summary>
        /// <param name="r">The ray we are shooting into the world.</param>
        /// <param name="depth">How many times the ray is still allowed to bounce if it hits something.</param>
        /// <param name="w">The world, a collection of primitives.</param>
        /// <returns>A 3D-vector with the correct RGB values in each scalar.</returns>
        private Vector3 rayColor(Ray r, int depth, World w)
        {
            // Limit how much the ray can bounce around before we stop the (possibly) infinite recursion
            if (depth <= 0)
                return new Vector3(0, 0, 0);

            // Shoot the ray, and see if it hits something
            HitRecord record = w.rayHit(r, 0.001f, double.MaxValue);

            if (record.hit)
            {
                // If it hits something, we want to know the material it hit, and if it will scatter
                ScatteredRecord sc = record.mat.scatter(r, record);

                // If it will scatter, update the color of the pixel
                if (sc.scatter)
                    return sc.attenuation * rayColor(sc.scatteredRay, depth - 1, w);

                // Otherwise, we did not scatter, and the pixel should be black
                return new Vector3(0);
            }

            // Ambient lighting of the 'sky'
            Vector3 unitdir = Vector3.Normalize(r.direction);
            float a = 0.5f * (unitdir.Y + 1.0f);
            return ((1.0f - a) * new Vector3(1, 1, 1)) + (a * new Vector3(0.5f, 0.7f, 1));
        }

        /// <summary>
        /// Gets a color based on a 3D-vector with RGB values as its scalars.
        /// </summary>
        /// <param name="color">The 3D-vector containing the RGB values. These should be between 0 and 1.</param>
        /// <returns>A color, based on the 3D-vector.</returns>
        private Color getColor(Vector3 color)
        {
            // Scale the color correctly
            color = (float) pixelSamplesScale * color;

            float min = 0;
            float max = 0.999f;

            // Convert to gamma
            float r = linearToGamma(color.X);
            float g = linearToGamma(color.Y);
            float b = linearToGamma(color.Z);

            // Convert from 0..1 values to 0..255 values
            r = (float) (256 * Math.Sqrt(Clamp(r, min, max)));
            g = (float) (256 * Math.Sqrt(Clamp(g, min, max)));
            b = (float) (256 * Math.Sqrt(Clamp(b, min, max)));

            // Return a color based on the 0..255 values
            return Color.FromArgb((int)r, (int)g, (int)b);
        }

        /// <summary>
        /// Converts a float from linear to gamma.
        /// </summary>
        /// <param name="x">The initial linear float value.</param>
        /// <returns>The value converted to gamma.</returns>
        private float linearToGamma(float x)
        {
            if (x > 0)
                return (float) Math.Sqrt(x);

            return 0;
        }

        /// <summary>
        /// Clamps a given value inside a given range.
        /// </summary>
        /// <param name="value">The value that is supposed to be clamped.</param>
        /// <param name="min">The minimum value it can have.</param>
        /// <param name="max">The maximum value it can have.</param>
        /// <returns>The original value, clamped between min and max.</returns>
        private float Clamp(float value, float min, float max)
        {
            if (value < min) value = min;
            if (value > max) value = max;
            return value;
        }

        /// <summary>
        /// Draws the bitmap to WinForms.
        /// </summary>
        private void Viewer_Paint(object sender, PaintEventArgs e)
        {
            Bitmap img = Render();
            e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            e.Graphics.DrawImage(img, new Rectangle(0, 0, width, height), 0, 0, width, height, GraphicsUnit.Pixel);
        }
    }
}
