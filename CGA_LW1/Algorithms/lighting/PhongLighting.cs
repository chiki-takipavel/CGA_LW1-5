using CGA_LW1.Interfaces;
using CGA_LW1.Models;
using System;
using System.Numerics;
using System.Windows.Media;

namespace CGA_LW1.Algorithms
{
    public class PhongLighting : ILighting
    {
        Vector3 _lightVector;
        Vector3 _viewVector;
        Vector3 _koef_a;
        Vector3 _koef_d;
        Vector3 _koef_s;
        Vector3 _ambientColor;
        Vector3 _reflectionColor;
        readonly float _shiness;
        private readonly bool _d;
        private readonly bool _n;
        private readonly bool _s;

        public PhongLighting(Vector3 vector, Vector3 viewVector, Vector3 koef_a, Vector3 koef_d, Vector3 koef_s,
            Vector3 ambientColor, Vector3 reflectionColor, float shiness, bool d, bool n, bool s)
        {
            _lightVector = Vector3.Normalize(vector);   // вектор света
            _viewVector = Vector3.Normalize(viewVector);  // направление взгляда
            _koef_a = koef_a;  // коэфициент фонового освещения
            _koef_d = koef_d;  // коэфициент рассеянного освещение
            _koef_s = koef_s;  // коэфициент зеркального освещения
            _ambientColor = ambientColor;        // цвет фонового света
            _reflectionColor = reflectionColor;  // цвет отраженного света
            _shiness = shiness;  // коэффициент блеска поверхности
            _d = d;
            _n = n;
            _s = s;
        }

        public Color GetPointColor(Vector3 normal, Color color)
        {
            // фоновое освещение
            var Ia = _koef_a * _ambientColor;
            // рассеянное освещение
            var Id = new Vector3(color.R, color.G, color.B) * _koef_d * Math.Max(Vector3.Dot(normal, Vector3.Normalize(_lightVector)), 0);
            // вектор отраженного луча света
            var reflectionVector = Vector3.Normalize(Vector3.Reflect(_lightVector, normal));
            // зеркальное освещение
            var Is = _reflectionColor * _koef_s * (float)Math.Pow(Math.Max(0, Vector3.Dot(reflectionVector, _viewVector)), _shiness);
            // совмещение компонентов освещения
            var light = Ia + Id + Is;

            byte r = (byte)Math.Min(light.X, 255);
            byte g = (byte)Math.Min(light.Y, 255);
            byte b = (byte)Math.Min(light.Z, 255);

            return Color.FromArgb(255, r, g, b);
        }


        // Refactor
        public Color GetPointColor(Model model, Vector3 texel, Vector3 argNormal)
        {
            // в текстуре координатный центр в нижнем левом углу
            var x = texel.X * model.DiffuseTexture.PixelWidth;
            var y = (1 - texel.Y) * model.DiffuseTexture.PixelHeight;

            // повторение текстуры
            x %= model.DiffuseTexture.PixelWidth;
            y %= model.DiffuseTexture.PixelHeight;

            if (x < 0 || y < 0)
            {
                return Color.FromArgb(255, 0, 0, 0);
            }

            Vector3 normal = argNormal;
            if (model.NormalsTexture != null && _n)
            {
                normal = model.NormalsTexture.GetRGBVector((int)(x + 0.5f), (int)(y + 0.5f));
                normal -= new Vector3(127.5f);
                normal = Vector3.Normalize(Vector3.TransformNormal(Vector3.Normalize(normal), Transformations.ModelWorldMatrix));
            }

            // фоновое освещение
            var Ia = model.DiffuseTexture.GetRGBVector((int)(x + 0.5f), (int)(y + 0.5f)) * _koef_a;
            // рассеянное освещение
            var Id = model.DiffuseTexture.GetRGBVector((int)(x + 0.5f), (int)(y + 0.5f)) * _koef_d *
                                                Math.Max(Vector3.Dot(normal, _lightVector), 0);
            // вектор отраженного луча света
            var reflectionVector = Vector3.Normalize(Vector3.Reflect(_lightVector, normal));
            // зеркальное освещение
            Vector3 Is = (model.SpecularTexture == null || !_s) ? new Vector3(0) :
                            model.SpecularTexture.GetRGBVector((int)(x + 0.5f), (int)(y + 0.5f)) * _koef_s
                            * (float)Math.Pow(Math.Max(0, Vector3.Dot(reflectionVector, _viewVector)), _shiness);

            // совмещение компонентов освещения
            var light = Ia + Id + Is;

            byte r = (byte)Math.Min(light.X, 255);
            byte g = (byte)Math.Min(light.Y, 255);
            byte b = (byte)Math.Min(light.Z, 255);

            return Color.FromArgb(255, r, g, b);
        }
    }
}
