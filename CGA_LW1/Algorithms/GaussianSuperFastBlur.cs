using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media;

namespace CGA_LW1.Algorithms
{
    public class GaussianSuperFastBlur
    {
        private readonly float[] alpha;
        private readonly float[] red;
        private readonly float[] green;
        private readonly float[] blue;

        private readonly int width;
        private readonly int height;

        public GaussianSuperFastBlur(Color[] source, int width, int height)
        {
            this.width = width;
            this.height = height;

            alpha = new float[width * height];
            red = new float[width * height];
            green = new float[width * height];
            blue = new float[width * height];

            Parallel.For(0, source.Length, i =>
            {
                alpha[i] = source[i].ScA;
                red[i] = source[i].ScR;
                green[i] = source[i].ScG;
                blue[i] = source[i].ScB;
            });
        }

        public Color[] Process(double radial)
        {
            var newAlpha = new float[width * height];
            var newRed = new float[width * height];
            var newGreen = new float[width * height];
            var newBlue = new float[width * height];
            var dest = new Color[width * height];

            Parallel.Invoke(
                () => GaussBlur(alpha, newAlpha, radial),
                () => GaussBlur(red, newRed, radial),
                () => GaussBlur(green, newGreen, radial),
                () => GaussBlur(blue, newBlue, radial)
            );

            Parallel.For(0, dest.Length, i =>
            {
                if (newAlpha[i] < 0) newAlpha[i] = 0;
                if (newRed[i] < 0) newRed[i] = 0;
                if (newGreen[i] < 0) newGreen[i] = 0;
                if (newBlue[i] < 0) newBlue[i] = 0;

                dest[i] = Color.FromScRgb(newAlpha[i], newRed[i], newGreen[i], newBlue[i]);
            });

            return dest;
        }

        private void GaussBlur(float[] source, float[] dest, double r)
        {
            var boxes = BoxesForGauss(r, 3);
            BoxBlur(source, dest, width, height, (boxes[0] - 1) / 2);
            BoxBlur(dest, source, width, height, (boxes[1] - 1) / 2);
            BoxBlur(source, dest, width, height, (boxes[2] - 1) / 2);
        }

        private static int[] BoxesForGauss(double sigma, int n)
        {
            var wIdeal = Math.Sqrt((12 * sigma * sigma / n) + 1);
            var wl = (int)Math.Floor(wIdeal);
            if (wl % 2 == 0) wl--;
            var wu = wl + 2;

            var mIdeal = ((12 * sigma * sigma) - n * wl * wl - 4 * n * wl - 3 * n) / (-4 * wl - 4);
            var m = Math.Round(mIdeal);

            var sizes = new List<int>();
            for (var i = 0; i < n; i++) sizes.Add(i < m ? wl : wu);

            return sizes.ToArray();
        }

        private static void BoxBlur(float[] source, float[] dest, int w, int h, int r)
        {
            for (var i = 0; i < source.Length; i++)
            {
                dest[i] = source[i];
            }

            BoxBlurHorizontal(dest, source, w, h, r);
            BoxBlurTotal(source, dest, w, h, r);
        }

        private static void BoxBlurHorizontal(float[] source, float[] dest, int width, int height, int filterOffset)
        {
            int tempSize = Math.Min(width, height) / 2 - 1;
            filterOffset = filterOffset > tempSize ? tempSize : filterOffset;
            var factor = 1f / (filterOffset + filterOffset + 1);
            Parallel.For(0, height, i =>
            {
                var temp = i * width;
                var left = temp;
                var right = temp + filterOffset + 1;
                var firstValue = source[temp];
                var lastValue = source[temp + width - 1];
                var value = firstValue * filterOffset;
                for (var j = 0; j <= filterOffset; j++)
                {
                    value += source[temp + j];
                }
                dest[temp++] = value * factor;

                for (var j = 0; j < filterOffset; j++)
                {
                    value += source[right++] - firstValue;
                    dest[temp++] = value * factor;
                }

                for (var j = filterOffset + 1; j < width - filterOffset; j++)
                {
                    value += source[right++] - source[left++];
                    dest[temp++] = value * factor;
                }

                for (var j = width - filterOffset; j < width; j++)
                {
                    value += lastValue - source[left++];
                    dest[temp++] = value * factor;
                }
            });
        }

        private static void BoxBlurTotal(float[] source, float[] dest, int width, int height, int filterOffset)
        {
            int tempSize = Math.Min(width, height) / 2 - 1;
            filterOffset = filterOffset > tempSize ? tempSize : filterOffset;
            var factor = 1f / (filterOffset + filterOffset + 1);
            Parallel.For(0, width, i =>
            {
                var temp = i;
                var left = temp;
                var right = temp + (filterOffset + 1) * width;
                var firstValue = source[temp];
                var lastValue = source[temp + width * (height - 1)];
                var value = firstValue * filterOffset;
                for (var j = 0; j <= filterOffset; j++)
                {
                    value += source[temp + j * width];
                }
                dest[temp] = value * factor;
                temp += width;

                for (var j = 0; j < filterOffset; j++)
                {
                    value += source[right] - firstValue;
                    dest[temp] = value * factor;
                    right += width;
                    temp += width;
                }

                for (var j = filterOffset + 1; j < height - filterOffset; j++)
                {
                    value += source[right] - source[left];
                    dest[temp] = value * factor;
                    left += width;
                    right += width;
                    temp += width;
                }

                for (var j = height - filterOffset; j < height; j++)
                {
                    value += lastValue - source[left];
                    dest[temp] = value * factor;
                    left += width;
                    temp += width;
                }
            });
        }
    }
}