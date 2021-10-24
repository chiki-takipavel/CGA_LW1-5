using System;
using System.Numerics;

namespace CGA_LW1.Models
{
    public struct Matrix
    {
        public float M11 { get; set; }
        public float M12 { get; set; }
        public float M13 { get; set; }
        public float M14 { get; set; }
        public float M21 { get; set; }
        public float M22 { get; set; }
        public float M23 { get; set; }
        public float M24 { get; set; }
        public float M31 { get; set; }
        public float M32 { get; set; }
        public float M33 { get; set; }
        public float M34 { get; set; }
        public float M41 { get; set; }
        public float M42 { get; set; }
        public float M43 { get; set; }
        public float M44 { get; set; }

        public Matrix(float m11, float m12, float m13, float m14,
                        float m21, float m22, float m23, float m24,
                        float m31, float m32, float m33, float m34,
                        float m41, float m42, float m43, float m44)
        {
            M11 = m11;
            M12 = m12;
            M13 = m13;
            M14 = m14;

            M21 = m21;
            M22 = m22;
            M23 = m23;
            M24 = m24;

            M31 = m31;
            M32 = m32;
            M33 = m33;
            M34 = m34;

            M41 = m41;
            M42 = m42;
            M43 = m43;
            M44 = m44;
        }

        public static Matrix GetTranslationMatrix(float xTranslation, float yTranslation, float zTranslation)
        {
            Matrix result = new()
            {
                M11 = 1.0f,
                M12 = 0.0f,
                M13 = 0.0f,
                M14 = xTranslation,
                M21 = 0.0f,
                M22 = 1.0f,
                M23 = 0.0f,
                M24 = yTranslation,
                M31 = 0.0f,
                M32 = 0.0f,
                M33 = 1.0f,
                M34 = zTranslation,
                M41 = 0.0f,
                M42 = 0.0f,
                M43 = 0.0f,
                M44 = 1.0f
            };

            return result;
        }

        public static Matrix GetScaleMatrix(float xScale, float yScale, float zScale)
        {
            Matrix result = new()
            {
                M11 = xScale,
                M12 = 0.0f,
                M13 = 0.0f,
                M14 = 0.0f,
                M21 = 0.0f,
                M22 = yScale,
                M23 = 0.0f,
                M24 = 0.0f,
                M31 = 0.0f,
                M32 = 0.0f,
                M33 = zScale,
                M34 = 0.0f,
                M41 = 0.0f,
                M42 = 0.0f,
                M43 = 0.0f,
                M44 = 1.0f
            };

            return result;
        }

        public static Matrix GetRotationXMatrix(float radians)
        {
            Matrix result = new();

            float cos = (float)Math.Cos(radians);
            float sin = (float)Math.Sin(radians);

            result.M11 = 1.0f;
            result.M12 = 0.0f;
            result.M13 = 0.0f;
            result.M14 = 0.0f;
            result.M21 = 0.0f;
            result.M22 = cos;
            result.M23 = -sin;
            result.M24 = 0.0f;
            result.M31 = 0.0f;
            result.M32 = sin;
            result.M33 = cos;
            result.M34 = 0.0f;
            result.M41 = 0.0f;
            result.M42 = 0.0f;
            result.M43 = 0.0f;
            result.M44 = 1.0f;

            return result;
        }

        public static Matrix GetRotationYMatrix(float radians)
        {
            Matrix result = new();

            float cos = (float)Math.Cos(radians);
            float sin = (float)Math.Sin(radians);

            result.M11 = cos;
            result.M12 = 0.0f;
            result.M13 = sin;
            result.M14 = 0.0f;
            result.M21 = 0.0f;
            result.M22 = 1.0f;
            result.M23 = 0.0f;
            result.M24 = 0.0f;
            result.M31 = -sin;
            result.M32 = 0.0f;
            result.M33 = cos;
            result.M34 = 0.0f;
            result.M41 = 0.0f;
            result.M42 = 0.0f;
            result.M43 = 0.0f;
            result.M44 = 1.0f;

            return result;
        }
        public static Matrix GetRotationZMatrix(float radians)
        {
            Matrix result = new();

            float cos = (float)Math.Cos(radians);
            float sin = (float)Math.Sin(radians);

            result.M11 = cos;
            result.M12 = -sin;
            result.M13 = 0.0f;
            result.M14 = 0.0f;
            result.M21 = sin;
            result.M22 = cos;
            result.M23 = 0.0f;
            result.M24 = 0.0f;
            result.M31 = 0.0f;
            result.M32 = 0.0f;
            result.M33 = 1.0f;
            result.M34 = 0.0f;
            result.M41 = 0.0f;
            result.M42 = 0.0f;
            result.M43 = 0.0f;
            result.M44 = 1.0f;

            return result;
        }

        public override string ToString()
        {
            return $"{{ {{M11:{M11} M12:{M12} M13:{M13} M14:{M14}}} {{M21:{M21} M22:{M22} M23:{M23} M24:{M24}}} {{M31:{M31} M32:{M32} M33:{M33} M34:{M34}}} {{M41:{M41} M42:{M42} M43:{M43} M44:{M44}}} }}";
        }

        public static Matrix GetPerspectiveFieldOfView(float fieldOfView, float aspectRatio, float nearPlaneDistance, float farPlaneDistance)
        {
            if (fieldOfView is <= 0.0f or >= (float)Math.PI)
            {
                throw new ArgumentOutOfRangeException(nameof(fieldOfView));
            }

            if (nearPlaneDistance <= 0.0f)
            {
                throw new ArgumentOutOfRangeException(nameof(nearPlaneDistance));
            }

            if (farPlaneDistance <= 0.0f)
            {
                throw new ArgumentOutOfRangeException(nameof(farPlaneDistance));
            }

            if (nearPlaneDistance >= farPlaneDistance)
            {
                throw new ArgumentOutOfRangeException(nameof(nearPlaneDistance));
            }

            float yScale = 1.0f / (float)Math.Tan(fieldOfView * 0.5f);
            float xScale = yScale / aspectRatio;

            Matrix result = new();

            result.M11 = xScale;
            result.M12 = result.M13 = result.M14 = 0.0f;

            result.M22 = yScale;
            result.M21 = result.M23 = result.M24 = 0.0f;

            result.M31 = result.M32 = 0.0f;
            result.M33 = farPlaneDistance / (nearPlaneDistance - farPlaneDistance);
            result.M34 = -1.0f;

            result.M41 = result.M42 = result.M44 = 0.0f;
            result.M43 = nearPlaneDistance * farPlaneDistance / (nearPlaneDistance - farPlaneDistance);

            return result;
        }

        public static Matrix4x4 GetViewPortMatrix(int minX, int minY, int width, int height, int x)
        {
            return new Matrix4x4(width / 2, 0, 0, 0,
                                 0, -1 * height / 2, 0, 0,
                                 0, 0, 1, 0,
                                 minX + (width / 2), minY + (height / 2), 0, 1);
        }

        public static Matrix Transpose(Matrix matrix)
        {
            Matrix result = new();

            result.M11 = matrix.M11;
            result.M12 = matrix.M21;
            result.M13 = matrix.M31;
            result.M14 = matrix.M41;
            result.M21 = matrix.M12;
            result.M22 = matrix.M22;
            result.M23 = matrix.M32;
            result.M24 = matrix.M42;
            result.M31 = matrix.M13;
            result.M32 = matrix.M23;
            result.M33 = matrix.M33;
            result.M34 = matrix.M43;
            result.M41 = matrix.M14;
            result.M42 = matrix.M24;
            result.M43 = matrix.M34;
            result.M44 = matrix.M44;

            return result;
        }

        public static Matrix operator *(Matrix m1, Matrix m2)
        {
            Matrix m = new();

            m.M11 = (m1.M11 * m2.M11) + (m1.M12 * m2.M21) + (m1.M13 * m2.M31) + (m1.M14 * m2.M41);
            m.M12 = (m1.M11 * m2.M12) + (m1.M12 * m2.M22) + (m1.M13 * m2.M32) + (m1.M14 * m2.M42);
            m.M13 = (m1.M11 * m2.M13) + (m1.M12 * m2.M23) + (m1.M13 * m2.M33) + (m1.M14 * m2.M43);
            m.M14 = (m1.M11 * m2.M14) + (m1.M12 * m2.M24) + (m1.M13 * m2.M34) + (m1.M14 * m2.M44);

            m.M21 = (m1.M21 * m2.M11) + (m1.M22 * m2.M21) + (m1.M23 * m2.M31) + (m1.M24 * m2.M41);
            m.M22 = (m1.M21 * m2.M12) + (m1.M22 * m2.M22) + (m1.M23 * m2.M32) + (m1.M24 * m2.M42);
            m.M23 = (m1.M21 * m2.M13) + (m1.M22 * m2.M23) + (m1.M23 * m2.M33) + (m1.M24 * m2.M43);
            m.M24 = (m1.M21 * m2.M14) + (m1.M22 * m2.M24) + (m1.M23 * m2.M34) + (m1.M24 * m2.M44);

            m.M31 = (m1.M31 * m2.M11) + (m1.M32 * m2.M21) + (m1.M33 * m2.M31) + (m1.M34 * m2.M41);
            m.M32 = (m1.M31 * m2.M12) + (m1.M32 * m2.M22) + (m1.M33 * m2.M32) + (m1.M34 * m2.M42);
            m.M33 = (m1.M31 * m2.M13) + (m1.M32 * m2.M23) + (m1.M33 * m2.M33) + (m1.M34 * m2.M43);
            m.M34 = (m1.M31 * m2.M14) + (m1.M32 * m2.M24) + (m1.M33 * m2.M34) + m1.M34 * m2.M44;

            m.M41 = (m1.M41 * m2.M11) + (m1.M42 * m2.M21) + (m1.M43 * m2.M31) + (m1.M44 * m2.M41);
            m.M42 = (m1.M41 * m2.M12) + (m1.M42 * m2.M22) + (m1.M43 * m2.M32) + (m1.M44 * m2.M42);
            m.M43 = (m1.M41 * m2.M13) + (m1.M42 * m2.M23) + (m1.M43 * m2.M33) + (m1.M44 * m2.M43);
            m.M44 = (m1.M41 * m2.M14) + (m1.M42 * m2.M24) + (m1.M43 * m2.M34) + (m1.M44 * m2.M44);

            return m;
        }
    }
}
