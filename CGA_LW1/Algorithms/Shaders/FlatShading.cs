using CGA_LW1.Interfaces;
using CGA_LW1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows.Media;

namespace CGA_LW1.Algorithms.Shaders
{
    public class FlatShading : Bresenham
    {
        protected ZBuffer ZBuffer { get; set; }
        protected ILighting Lighting { get; set; }

        public FlatShading(Bgr24Bitmap bitmap, Model model, ILighting lighting)
            : base(bitmap, model)
        {
            Lighting = lighting;
            ZBuffer = new ZBuffer(Bitmap.PixelWidth, Bitmap.PixelHeight);
        }

        public override void DrawModel()
        {
            _ = Parallel.ForEach(Model.TriangleFaces, face =>
            {
                if (IsFaceVisible(face))
                {
                    DrawFace(face);
                }
            });
        }

        protected void DrawFace(List<Vector3> faces)
        {
            Color color = GetFaceColor(faces, Color);
            List<Pixel> sidesPixels = new();

            for (int i = 0; i < faces.Count - 1; i++)
            {
                DrawSide(faces, i, i + 1, color, sidesPixels);
            }

            DrawSide(faces, 0, faces.Count - 1, color, sidesPixels);

            DrawPixelsInFace(sidesPixels);
        }

        protected override void DrawPixel(int x, int y, float z, Color color, List<Pixel> sidesPixels = null)
        {
            sidesPixels.Add(new Pixel(x, y, z, color));   // добавляеи точку в список граничных точек грани

            if (x > 0 && x < Bitmap.PixelWidth &&
                y > 0 && y < Bitmap.PixelHeight &&
                z > 0 && z < 1 && z <= ZBuffer[x, y])
            {
                ZBuffer[x, y] = z;                            // помечаем новую координату в z-буффере
                Bitmap[x, y] = color;                         // красим пиксель
            }
        }

        protected Color GetFaceColor(List<Vector3> face, Color color)
        {
            Vector3 normal1 = Model.Normals[(int)face[0].Z];
            Vector3 normal2 = Model.Normals[(int)face[1].Z];
            Vector3 normal3 = Model.Normals[(int)face[2].Z];

            Color color1 = Lighting.GetPointColor(normal1, color);
            Color color2 = Lighting.GetPointColor(normal2, color);
            Color color3 = Lighting.GetPointColor(normal3, color);

            return GetAverageColor(color1, color2, color3);
        }

        public static Color GetAverageColor(Color color1, Color color2, Color color3)
        {
            int sumR = color1.R + color2.R + color3.R;
            int sumG = color1.G + color2.G + color3.G;
            int sumB = color1.B + color2.B + color3.B;
            int sumA = color1.A + color2.A + color3.A;

            byte r = (byte)Math.Round((double)sumR / 3);
            byte g = (byte)Math.Round((double)sumG / 3);
            byte b = (byte)Math.Round((double)sumB / 3);
            byte a = (byte)Math.Round((double)sumA / 3);

            return Color.FromArgb(a, r, g, b);
        }

        // Отрисовка грани изнутри
        protected virtual void DrawPixelsInFace(List<Pixel> sidesPixels) // список всех точек ребер грани
        {
            (int? minY, int? maxY) = GetMinMaxY(sidesPixels);
            if (minY is null || maxY is null)
            {
                return;
            }

            Color color = sidesPixels[0].Color;  // цвет одинаковый
            for (int y = (int)minY; y < maxY; y++)      // по очереди отрисовываем линии для каждой y-координаты
            {
                (Pixel? startPixel, Pixel? endPixel) = GetStartEndXForY(sidesPixels, y);
                if (startPixel is null || endPixel is null)
                {
                    continue;
                }

                Pixel start = (Pixel)startPixel;
                Pixel end = (Pixel)endPixel;

                float z = start.Z;                                       // в какую сторону приращение z
                float dz = (end.Z - start.Z) / Math.Abs((float)(end.X - start.X));  // z += dz при изменении x

                // отрисовываем линию
                for (int x = start.X; x < end.X; x++, z += dz)
                {
                    if ((x > 0) && (x < ZBuffer.Width) &&           // x попал в область экрана
                        (y > 0) && (y < ZBuffer.Height) &&          // y попал в область экрана
                        (z <= ZBuffer[x, y]) && z > 0 && z < 1)   // z координата отображаемая
                    {
                        ZBuffer[x, y] = z;
                        Bitmap[x, y] = color;
                    }
                }
            }
        }

        // Сортируем точки по Y-координате и находим min & max
        protected static (int? min, int? max) GetMinMaxY(List<Pixel> pixels)
        {
            List<Pixel> sorted = pixels.OrderBy(x => x.Y).ToList();
            return sorted.Count == 0 ? (min: null, max: null) : (min: sorted.First().Y, max: sorted.Last().Y);
        }

        // Находим стартовый и конечный X для определенного Y 
        protected static (Pixel? start, Pixel? end) GetStartEndXForY(List<Pixel> pixels, int y)
        {
            // Фильтруем пиксели с нужным Y и сортируем по X
            List<Pixel> filtered = pixels.Where(pixel => pixel.Y == y).OrderBy(pixel => pixel.X).ToList();
            return filtered.Count == 0 ? (start: null, end: null) : (start: filtered.First(), end: filtered.Last());
        }
    }
}
