using System;

namespace CGA_LW1.Models
{
    public class ZBuffer
    {
        private readonly double[,] buffer;

        public int Width { get; set; }
        public int Height { get; set; }

        public ZBuffer(int width, int height)
        {
            Width = width;
            Height = height;
            buffer = new double[Width, Height];

            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    buffer[i, j] = double.MaxValue;
                }
            }
        }

        private bool IsValidPixel(int x, int y)
        {
            return x >= 0 && x <= Width && y >= 0 && y <= Height;
        }

        public double this[int x, int y]
        {
            get => IsValidPixel(x, y) ? buffer[x, y] : throw new ArgumentOutOfRangeException(nameof(x));
            set => buffer[x, y] = IsValidPixel(x, y) ? value : throw new ArgumentOutOfRangeException(nameof(x));
        }
    }
}
