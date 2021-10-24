using CGA_LW1.Models;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows.Media;

namespace CGA_LW1.Algorithms
{
    public class Bresenham
    {
        protected Bgr24Bitmap Bitmap { get; set; }
        protected Model Model { get; set; }
        protected Color Color { get; set; } = Color.FromRgb(0, 0, 0);

        public Bresenham(Bgr24Bitmap bitmap, Model model)
        {
            Bitmap = bitmap;
            Model = model;
        }

        public void DrawModel(Color color)
        {
            Color = color;
            DrawModel();
        }

        public virtual void DrawModel()
        {
            _ = Parallel.ForEach(Model.Faces, face =>
            {
                if (IsFaceVisible(face))
                {
                    DrawFace(face);
                }
            });
        }

        private void DrawFace(List<Vector3> faces)
        {
            for (int i = 0; i < faces.Count - 1; i++)
            {
                DrawSide(faces, i, i + 1, Color);
            }

            DrawSide(faces, 0, faces.Count - 1, Color);
        }

        // Отрисовывание ребра
        protected virtual void DrawSide(List<Vector3> face, int index1, int index2, Color color, List<Pixel> sidesPixels = null)
        {
            Pixel point1 = GetFacePoint(face, index1, color);
            Pixel point2 = GetFacePoint(face, index2, color);

            DrawLine(point1, point2, sidesPixels);
        }

        // Определяем, видима ли грань
        protected bool IsFaceVisible(List<Vector3> face)
        {
            Vector3 normal = GetFaceNormal(face);
            return normal.Z < 0;
        }

        // Получение нормали к грани
        private Vector3 GetFaceNormal(List<Vector3> face)
        {
            // получение вершин
            Pixel point1 = GetFacePoint(face, 0, Color);
            Pixel point2 = GetFacePoint(face, 1, Color);
            Pixel point3 = GetFacePoint(face, 2, Color);

            return GetNormal(point1, point2, point3);
        }

        // Получение нормали
        private static Vector3 GetNormal(Pixel point1, Pixel point2, Pixel point3)
        {
            // вектора
            Vector3 vector1 = new(point2.X - point1.X,
                                          point2.Y - point1.Y,
                                          point2.Z - point1.Z);

            Vector3 vector2 = new(point3.X - point1.X,
                                          point3.Y - point1.Y,
                                          point3.Z - point1.Z);

            Vector3 cross = Vector3.Cross(vector1, vector2);

            return Vector3.Normalize(cross);
        }

        // Целочисленный алгоритм Брезенхема для отрисовки ребра
        protected virtual void DrawLine(Pixel src, Pixel desc, List<Pixel> sidesPixels = null)
        {
            Color color = src.Color;

            // разница координат начальной и конечной точек
            int dx = Math.Abs(desc.X - src.X);
            int dy = Math.Abs(desc.Y - src.Y);
            float dz = Math.Abs(desc.Z - src.Z);

            // учитываем квадрант
            int signX = src.X < desc.X ? 1 : -1;
            int signY = src.Y < desc.Y ? 1 : -1;
            float signZ = src.Z < desc.Z ? 1 : -1;

            // текущий пиксель
            Pixel p = src;

            float curZ = src.Z;  // текущее z
            float deltaZ = dz / dy;  // при изменении y будем менять z

            int err = dx - dy;   // ошибка

            // пока не достигнем конца
            while (p.X != desc.X || p.Y != desc.Y)
            {
                // пиксель внутри окна
                DrawPixel(p.X, p.Y, curZ, color, sidesPixels);

                int err2 = err * 2;      // модифицированное значение ошибки

                if (err2 > -dy)
                {
                    p.X += signX;        // изменияем x на единицу
                    err -= dy;           // корректируем ошибку
                }

                if (err2 < dx)
                {
                    p.Y += signY;            // изменяем y на единицу
                    curZ += signZ * deltaZ;  // меняем z
                    err += dx;               // корректируем ошибку   
                }
            }

            // отрисовывем последний пиксель
            DrawPixel(desc.X, desc.Y, desc.Z, color, sidesPixels);
        }

        // Получение вершины грани
        protected virtual Pixel GetFacePoint(List<Vector3> face, int i, Color color)
        {
            // индексы вершин в массиве Points - их x-координаты
            int indexPoint = (int)face[i].X;
            // получение самой вершины
            Vector4 point = Model.Points[indexPoint];

            return new Pixel((int)point.X, (int)point.Y, point.Z, color);
        }

        protected virtual void DrawPixel(int x, int y, float z, Color color, List<Pixel> sidesPixels = null)
        {
            if (x > 0 && x < Bitmap.PixelWidth &&
                y > 0 && y < Bitmap.PixelHeight &&
                z > 0 && z < 1)
            {
                Bitmap[x, y] = color;
            }
        }
    }
}
