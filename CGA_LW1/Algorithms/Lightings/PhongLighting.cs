using CGA_LW1.Interfaces;
using CGA_LW1.Models;
using System;
using System.Numerics;
using System.Windows.Media;

namespace CGA_LW1.Algorithms.Lightings
{
    public class PhongLighting : ILighting
    {
        private Vector3 lightVector;
        private Vector3 viewVector;
        private Vector3 koefA;
        private Vector3 koefD;
        private Vector3 koefS;
        private Vector3 ambientColor;
        private Vector3 reflectionColor;
        private readonly float shiness;
        private readonly bool d;
        private readonly bool n;
        private readonly bool s;

        public PhongLighting(Vector3 vector, Vector3 viewVector, Vector3 koefA, Vector3 koefD, Vector3 koefS,
            Vector3 ambientColor, Vector3 reflectionColor, float shiness, bool d, bool n, bool s)
        {
            lightVector = Vector3.Normalize(vector);   // вектор света
            this.viewVector = Vector3.Normalize(viewVector);  // направление взгляда
            this.koefA = koefA;  // коэфициент фонового освещения
            this.koefD = koefD;  // коэфициент рассеянного освещение
            this.koefS = koefS;  // коэфициент зеркального освещения
            this.ambientColor = ambientColor;        // цвет фонового света
            this.reflectionColor = reflectionColor;  // цвет отраженного света
            this.shiness = shiness;  // коэффициент блеска поверхности
            this.d = d;
            this.n = n;
            this.s = s;
        }

        public Color GetPointColor(Vector3 normal, Color color)
        {
            // фоновое освещение
            Vector3 iA = koefA * ambientColor;
            // рассеянное освещение
            Vector3 iD = new Vector3(color.R, color.G, color.B) * koefD * Math.Max(Vector3.Dot(normal, Vector3.Normalize(lightVector)), 0);
            // вектор отраженного луча света
            Vector3 reflectionVector = Vector3.Normalize(Vector3.Reflect(lightVector, normal));
            // зеркальное освещение
            Vector3 iS = reflectionColor * koefS * (float)Math.Pow(Math.Max(0, Vector3.Dot(reflectionVector, viewVector)), shiness);
            // совмещение компонентов освещения
            Vector3 light = iA + iD + iS;

            byte r = (byte)Math.Min(light.X, 255);
            byte g = (byte)Math.Min(light.Y, 255);
            byte b = (byte)Math.Min(light.Z, 255);

            return Color.FromArgb(255, r, g, b);
        }


        // Refactor
        public Color GetPointColor(Model model, Vector3 texel, Vector3 argNormal)
        {
            // в текстуре координатный центр в нижнем левом углу
            float x = texel.X * model.DiffuseTexture.PixelWidth;
            float y = (1 - texel.Y) * model.DiffuseTexture.PixelHeight;

            // повторение текстуры
            x %= model.DiffuseTexture.PixelWidth;
            y %= model.DiffuseTexture.PixelHeight;

            if (x < 0 || y < 0)
            {
                return Color.FromArgb(255, 0, 0, 0);
            }

            Vector3 normal = argNormal;
            if (model.NormalsTexture != null && n)
            {
                normal = model.NormalsTexture.GetRGBVector((int)(x + 0.5f), (int)(y + 0.5f));
                normal -= new Vector3(127.5f);
                normal = Vector3.Normalize(Vector3.TransformNormal(Vector3.Normalize(normal), Transformations.ModelWorldMatrix));
            }

            // фоновое освещение
            Vector3 iA = model.DiffuseTexture.GetRGBVector((int)(x + 0.5f), (int)(y + 0.5f)) * koefA;
            // рассеянное освещение
            Vector3 iD = model.DiffuseTexture.GetRGBVector((int)(x + 0.5f), (int)(y + 0.5f)) * koefD *
                                                Math.Max(Vector3.Dot(normal, lightVector), 0);
            // вектор отраженного луча света
            Vector3 reflectionVector = Vector3.Normalize(Vector3.Reflect(lightVector, normal));
            // зеркальное освещение
            Vector3 iS = (model.SpecularTexture == null || !s) ? new Vector3(0) :
                            model.SpecularTexture.GetRGBVector((int)(x + 0.5f), (int)(y + 0.5f)) * koefS
                            * (float)Math.Pow(Math.Max(0, Vector3.Dot(reflectionVector, viewVector)), shiness);

            // совмещение компонентов освещения
            Vector3 light = iA + iD + iS;

            byte r = (byte)Math.Min(light.X, 255);
            byte g = (byte)Math.Min(light.Y, 255);
            byte b = (byte)Math.Min(light.Z, 255);

            return Color.FromArgb(255, r, g, b);
        }
    }
}
