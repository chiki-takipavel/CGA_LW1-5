using CGA_LW1.Interfaces;
using CGA_LW1.Models;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows.Media;

namespace CGA_LW1.Algorithms.Shaders
{
    public class PhongShading : FlatShading
    {
        private readonly bool texturesEnabled;
        private readonly bool bloom;
        private readonly double sigma;
        private readonly double exposure;
        private readonly Color[] bloomColors;
        private readonly Vector3 brightnessVector = new(0.2126f, 0.7152f, 0.0722f);

        public PhongShading(Bgr24Bitmap bitmap, Model model, ILighting lighting, bool d, bool n, bool s, bool e, bool bloom, double sigma, double exposure)
           : base(bitmap, model, lighting)
        {
            this.bloom = bloom;
            texturesEnabled = d || n || s || e;
            this.sigma = sigma;
            this.exposure = exposure;
            bloomColors = new Color[bitmap.PixelWidth * bitmap.PixelHeight];
        }

        public override void DrawModel()
        {
            base.DrawModel();
            GaussianSuperFastBlur gaussianSuperFastBlur = new(bloomColors, Bitmap.PixelWidth, Bitmap.PixelHeight);
            Color[] gaussianColors = gaussianSuperFastBlur.Process(sigma);

            if (bloom)
            {
                Parallel.For(0, Bitmap.PixelHeight, y =>
                {
                    for (int x = 0; x < Bitmap.PixelWidth; x++)
                    {
                        Color bitmapColor = Bitmap[x, y];
                        Color gaussianColor = gaussianColors[x + y * Bitmap.PixelWidth];
                        Vector4 bitmapVector = new(bitmapColor.ScA, bitmapColor.ScR, bitmapColor.ScG, bitmapColor.ScB);
                        Vector4 gaussianVector = new(gaussianColor.ScA, gaussianColor.ScR, gaussianColor.ScG, gaussianColor.ScB);
                        bitmapVector += gaussianVector;
                        bitmapVector.Y = (float)(1f - Math.Exp(-bitmapVector.Y * exposure));
                        bitmapVector.Z = (float)(1f - Math.Exp(-bitmapVector.Z * exposure));
                        bitmapVector.W = (float)(1f - Math.Exp(-bitmapVector.W * exposure));
                        Bitmap[x, y] = Color.FromScRgb(bitmapVector.X, bitmapVector.Y, bitmapVector.Z, bitmapVector.W);
                    }
                });
            }
        }

        // Отрисовывание ребра
        protected override void DrawSide(List<Vector3> face, int index1, int index2, Color color, List<Pixel> sidesPixels = null)
        {
            // Нормали задаются в вершинах  
            Vector3 normal1 = Model.Normals[(int)face[index1].Z];
            Vector3 normal2 = Model.Normals[(int)face[index2].Z];

            Vector3 texel1 = texturesEnabled ? Model.Textures[(int)face[index1].Y] : new Vector3();
            Vector3 texel2 = texturesEnabled ? Model.Textures[(int)face[index2].Y] : new Vector3();

            Pixel point1 = GetFacePoint(face, index1, normal1, texel1);
            Pixel point2 = GetFacePoint(face, index2, normal2, texel2);

            DrawLine(point1, point2, sidesPixels);
        }

        // Получение вершины грани
        protected Pixel GetFacePoint(List<Vector3> face, int i, Vector3 normal, Vector3 texel)
        {
            // индексы вершин в массиве Points - их x-координаты
            int indexPoint = (int)face[i].X;
            // получение самой вершины
            Vector4 point = Model.Points[indexPoint];

            if (!texturesEnabled)
            {
                point.W = 1;
            }

            return new Pixel((int)point.X, (int)point.Y, point.Z, 1 / point.W, Color, normal / point.W, texel / point.W);
        }

        // Целочисленный алгоритм Брезенхема для отрисовки ребра
        protected override void DrawLine(Pixel src, Pixel desc, List<Pixel> sidesPixels = null)
        {
            bool sameX = Math.Abs(src.X - desc.X) < Math.Abs(desc.Y - src.Y);

            // разница координат начальной и конечной точек
            int dx = Math.Abs(desc.X - src.X);
            int dy = Math.Abs(desc.Y - src.Y);
            float dz = Math.Abs(desc.Z - src.Z);

            // учитываем квадрант
            int signX = src.X < desc.X ? 1 : -1;
            int signY = src.Y < desc.Y ? 1 : -1;
            float signZ = src.Z < desc.Z ? 1 : -1;

            float curZ = src.Z;  // текущее z
            float deltaZ = dz / dy;  // при изменении y будем менять z

            Vector3 deltaNormal, curNormal;
            Vector3 deltaTexel, curTexel;
            float deltaNW, curNW;

            if (sameX)
            {
                deltaNormal = (desc.Normal - src.Normal) / dy;
                curNormal = src.Normal;

                deltaTexel = (desc.Texel - src.Texel) / dy;
                curTexel = src.Texel;

                deltaNW = (desc.NW - src.NW) / dy;
                curNW = src.NW;
            }
            else
            {
                deltaNormal = (desc.Normal - src.Normal) / dx;
                curNormal = src.Normal;

                deltaTexel = (desc.Texel - src.Texel) / dx;
                curTexel = src.Texel;

                deltaNW = (desc.NW - src.NW) / dx;
                curNW = src.NW;
            }

            int err = dx - dy;   // ошибка

            // текущий пиксель
            Pixel p = src;

            // пока не достигнем конца
            while (p.X != desc.X || p.Y != desc.Y)
            {
                // пиксель внутри окна
                DrawPixel(p.X, p.Y, curZ, curNW, curNormal, curTexel, Color, sidesPixels);

                int err2 = err * 2;      // модифицированное значение ошибки

                if (err2 > -dy)
                {
                    p.X += signX;        // изменияем x на единицу
                    err -= dy;           // корректируем ошибку

                    if (!sameX)
                    {
                        curNormal += deltaNormal;
                        curTexel += deltaTexel;
                        curNW += deltaNW;
                    }
                }

                if (err2 < dx)
                {
                    p.Y += signY;            // изменяем y на единицу
                    curZ += signZ * deltaZ;  // меняем z
                    err += dx;               // корректируем ошибку   

                    if (sameX)
                    {
                        curNormal += deltaNormal;
                        curTexel += deltaTexel;
                        curNW += deltaNW;
                    }
                }
            }

            // отрисовывем последний пиксель
            DrawPixel(desc.X, desc.Y, desc.Z, desc.NW, desc.Normal, desc.Texel, Color, sidesPixels);
        }

        protected virtual void DrawPixel(int x, int y, float z, float nw, Vector3 normal, Vector3 texel, Color color, List<Pixel> sidesPixels = null)
        {
            Color pixelColor = texturesEnabled ? Lighting.GetPointColor(Model, texel / nw, normal / nw) : Lighting.GetPointColor(normal, color);
            Vector3 pixelVector = new(pixelColor.ScR, pixelColor.ScG, pixelColor.ScB);
            float brightness = Vector3.Dot(pixelVector, brightnessVector);

            sidesPixels.Add(new Pixel(x, y, z, nw, pixelColor, normal, texel));   // добавляеи точку в список граничных точек грани

            if (x > 0 && x < Bitmap.PixelWidth &&
                y > 0 && y < Bitmap.PixelHeight &&
                z > 0 && z < 1 && z <= ZBuffer[x, y])
            {
                if (brightness > 1f)
                {
                    bloomColors[x + Bitmap.PixelWidth * y] = pixelColor;
                }
                else
                {
                    bloomColors[x + Bitmap.PixelWidth * y] = Color.FromScRgb(0f, 0f, 0f, 0f);
                }

                ZBuffer[x, y] = z;
                Bitmap[x, y] = pixelColor;
            }
        }

        // Отрисовка грани изнутри
        protected override void DrawPixelsInFace(List<Pixel> sidesPixels) // список всех точек ребер грани
        {
            (int? minY, int? maxY) = GetMinMaxY(sidesPixels);
            if (minY is null || maxY is null)
            {
                return;
            }

            for (int y = (int)minY; y < maxY; y++)
            {
                (Pixel? startPixel, Pixel? endPixel) = GetStartEndXForY(sidesPixels, y);
                if (startPixel is null || endPixel is null)
                {
                    continue;
                }

                Pixel start = (Pixel)startPixel;
                Pixel end = (Pixel)endPixel;

                float z = start.Z;
                float dz = (end.Z - start.Z) / Math.Abs((float)(end.X - start.X));

                Vector3 deltaNormal = (end.Normal - start.Normal) / (end.X - start.X);
                Vector3 curNormal = start.Normal;

                Vector3 deltaTexel = (end.Texel - start.Texel) / (end.X - start.X);
                Vector3 curTexel = start.Texel;

                float deltaNW = (end.NW - start.NW) / (end.X - start.X);
                float curNW = start.NW;

                for (int x = start.X; x < end.X; x++, z += dz)
                {
                    curNormal += deltaNormal;
                    curTexel += deltaTexel;
                    curNW += deltaNW;

                    if ((x > 0) && (x < ZBuffer.Width) &&
                        (y > 0) && (y < ZBuffer.Height) &&
                        (z <= ZBuffer[x, y]) && z > 0 && z < 1)
                    {
                        var pixelColor = texturesEnabled ? Lighting.GetPointColor(Model, curTexel / curNW, curNormal / curNW) : Lighting.GetPointColor(curNormal, Color);
                        Vector3 pixelVector = new(pixelColor.ScR, pixelColor.ScG, pixelColor.ScB);
                        float brightness = Vector3.Dot(pixelVector, brightnessVector);
                        if (brightness > 1f)
                        {
                            bloomColors[x + Bitmap.PixelWidth * y] = pixelColor;
                        }
                        else
                        {
                            bloomColors[x + Bitmap.PixelWidth * y] = Color.FromScRgb(0f, 0f, 0f, 0f);
                        }

                        ZBuffer[x, y] = z;
                        Bitmap[x, y] = pixelColor;
                    }
                }
            }
        }
    }
}
