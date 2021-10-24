using CGA_LW1.Interfaces;
using CGA_LW1.Models;
using System;
using System.Numerics;
using System.Windows.Media;

namespace CGA_LW1.Algorithms
{
    public class LambertLighting : ILighting
    {
        private Vector3 lightVector;

        public LambertLighting(Vector3 vector)
        {
            lightVector = vector;
        }

        public Color GetPointColor(Vector3 normal, Color color)
        {
            double coef = Math.Max(Vector3.Dot(normal, Vector3.Normalize(lightVector)), 0);
            byte r = (byte)Math.Round(color.R * coef);
            byte g = (byte)Math.Round(color.G * coef);
            byte b = (byte)Math.Round(color.B * coef);

            return Color.FromArgb(255, r, g, b);
        }

        public Color GetPointColor(Model model, Vector3 texel, Vector3 argNormal)
        {
            throw new NotImplementedException();
        }
    }
}
