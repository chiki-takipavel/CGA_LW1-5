using CGA_LW1.Interfaces;
using CGA_LW1.Models;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Windows.Media;

namespace CGA_LW1.Algorithms
{
    public class GouraudShading : PlaneShading
    {
        public GouraudShading(Bgr24Bitmap bitmap, Model model, ILighting lighting)
            : base(bitmap, model, lighting) { }

        // Отрисовывание ребра
        protected override void DrawSide(List<Vector3> face, int index1, int index2, Color color, List<Pixel> sidesPixels = null)
        {
            // Нормали задаются в вершинах  
            var normal1 = Model.Normals[(int)face[index1].Z];
            var normal2 = Model.Normals[(int)face[index2].Z];

            var color1 = Lighting.GetPointColor(normal1, color);
            var color2 = Lighting.GetPointColor(normal2, color);

            var point1 = GetFacePoint(face, index1, color1);
            var point2 = GetFacePoint(face, index2, color2);

            DrawLine(point1, point2, sidesPixels);
        }

        // Целочисленный алгоритм Брезенхема для отрисовки ребра
        protected override void DrawLine(Pixel src, Pixel desc, List<Pixel> sidesPixels = null)
        {
            // разница координат начальной и конечной точек
            int dx = Math.Abs(desc.X - src.X);
            int dy = Math.Abs(desc.Y - src.Y);
            float dz = Math.Abs(desc.Z - src.Z);

            // разница цветов начальной и конечной точек
            float dR = Math.Abs(desc.Color.R - src.Color.R);
            float dG = Math.Abs(desc.Color.G - src.Color.G);
            float dB = Math.Abs(desc.Color.B - src.Color.B);
            float dA = Math.Abs(desc.Color.A - src.Color.A);

            // учитываем квадрант
            int signX = src.X < desc.X ? 1 : -1;
            int signY = src.Y < desc.Y ? 1 : -1;
            float signZ = src.Z < desc.Z ? 1 : -1;

            int signR = src.Color.R < desc.Color.R ? 1 : -1;
            int signG = src.Color.G < desc.Color.G ? 1 : -1;
            int signB = src.Color.B < desc.Color.B ? 1 : -1;
            int signA = src.Color.A < desc.Color.A ? 1 : -1;

            // текущий пиксель
            Pixel p = src;

            // текущие z и цвет
            float curZ = src.Z;
            float curR = src.Color.R;
            float curG = src.Color.G;
            float curB = src.Color.B;
            float curA = src.Color.A;

            // при изменении y будем менять z и цвет
            float deltaZ = dz / dy;
            float deltaR = dR / dy;
            float deltaG = dG / dy;
            float deltaB = dB / dy;
            float deltaA = dA / dy;

            int err = dx - dy;   // ошибка

            // пока не достигнем конца
            while (p.X != desc.X || p.Y != desc.Y)
            {
                // пиксель внутри окна
                var pixelColor = Color.FromArgb((byte)Math.Round(curA), (byte)Math.Round(curR), (byte)Math.Round(curG), (byte)Math.Round(curB));
                DrawPixel(p.X, p.Y, curZ, pixelColor, sidesPixels);

                int err2 = err * 2;      // модифицированное значение ошибки

                if (err2 > -dy)          // dx > dy / 2
                {
                    p.X += signX;        // изменияем x на единицу
                    err -= dy;           // корректируем ошибку
                }

                if (err2 < dx)           // dy > dx / 2
                {
                    p.Y += signY;            // изменяем y на единицу
                    err += dx;               // корректируем ошибку   

                    // меняем z и цвета
                    curZ += signZ * deltaZ;
                    curA += signA * deltaA;
                    curR += signR * deltaR;
                    curG += signG * deltaG;
                    curB += signB * deltaB;
                }
            }

            // отрисовывем последний пиксель
            DrawPixel(desc.X, desc.Y, desc.Z, desc.Color, sidesPixels);
        }


        // Отрисовка грани изнутри
        protected override void DrawPixelsInFace(List<Pixel> sidesPixels) // список всех точек ребер грани
        {
            (int? minY, int? maxY) = GetMinMaxY(sidesPixels);
            if (minY == null || maxY == null) return;

            for (int y = (int)minY; y < maxY; y++)      // по очереди отрисовываем линии для каждой y-координаты
            {
                (Pixel? startPixel, Pixel? endPixel) = GetStartEndXForY(sidesPixels, y);
                if (startPixel == null || endPixel == null) continue;

                Pixel start = (Pixel)startPixel;
                Pixel end = (Pixel)endPixel;

                float z = start.Z;                                       // в какую сторону приращение z
                float dz = (end.Z - start.Z) / Math.Abs((float)(end.X - start.X));  // z += dz при изменении x

                float A = start.Color.A;
                float dA = (end.Color.A - start.Color.A) / Math.Abs((float)(end.Color.A - start.Color.A));
                float R = start.Color.R;
                float dR = (end.Color.R - start.Color.R) / Math.Abs((float)(end.Color.R - start.Color.R));
                float G = start.Color.G;
                float dG = (end.Color.G - start.Color.G) / Math.Abs((float)(end.Color.G - start.Color.G));
                float B = start.Color.B;
                float dB = (end.Color.B - start.Color.B) / Math.Abs((float)(end.Color.B - start.Color.B));

                // отрисовываем линию
                for (int x = start.X; x < end.X; x++, z += dz, A += dA, R += dR, G += dG, B += dB)
                {
                    if ((x > 0) && (x < ZBuffer.Width) &&           // x попал в область экрана
                        (y > 0) && (y < ZBuffer.Height) &&          // y попал в область экрана
                        (z <= ZBuffer[x, y]) && (z > 0 && z < 1))   // z координата отображаемая
                    {
                        var pixelColor = Color.FromArgb((byte)Math.Round(A), (byte)Math.Round(R), (byte)Math.Round(G), (byte)Math.Round(B));
                        Bitmap[x, y] = pixelColor;
                        ZBuffer[x, y] = z;
                    }
                }
            }
        }
    }
}
