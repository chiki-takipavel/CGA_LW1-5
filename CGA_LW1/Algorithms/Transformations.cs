using CGA_LW1.Models;
using System.Numerics;

namespace CGA_LW1.Algorithms
{
    public static class Transformations
    {
        public static Matrix4x4 ModelWorldMatrix { get; set; }

        public static void TransformFromWorldToView(Model model, ModelParams modelParams)
        {
            Matrix4x4 worldProjctionMatix = GetMVP(modelParams);
            float[] w = new float[model.Points.Count];
            for (int i = 0; i < model.Points.Count; i++)
            {
                model.Points[i] = Vector4.Transform(model.Points[i], worldProjctionMatix);

                w[i] = model.Points[i].W;
                model.Points[i] /= model.Points[i].W;
            }

            TransformNormal(model, modelParams);
            TransformToViewPort(model, modelParams, w);
        }

        private static Matrix4x4 GetWorldMatrix(ModelParams modelParams)
        {
            ModelWorldMatrix = Matrix4x4.CreateScale(modelParams.Scaling) * Matrix4x4.CreateFromYawPitchRoll(modelParams.ModelYaw, modelParams.ModelPitch, modelParams.ModelRoll)
                * Matrix4x4.CreateTranslation(modelParams.TranslationX, modelParams.TranslationY, modelParams.TranslationZ);
            return ModelWorldMatrix;
        }


        private static Matrix4x4 GetViewerMatrix(ModelParams modelParams)
        {
            return
                 Matrix4x4.CreateTranslation(-new Vector3(modelParams.CameraPositionX, modelParams.CameraPositionY, modelParams.CameraPositionZ))
                 * Matrix4x4.Transpose(Matrix4x4.CreateFromYawPitchRoll(modelParams.CameraYaw, modelParams.CameraPitch, modelParams.CameraRoll));
        }

        private static Matrix4x4 GetWorldProjectionMatrix(ModelParams modelParams)
        {
            return Matrix4x4.CreatePerspectiveFieldOfView(modelParams.FieldOfView, modelParams.AspectRatio, modelParams.NearPlaneDistance, modelParams.FarPlaneDistance);
        }

        private static void TransformNormal(Model model, ModelParams modelParams)
        {
            for (int i = 0; i < model.Normals.Count; i++)
            {
                model.Normals[i] = Vector3.Normalize(Vector3.TransformNormal(model.Normals[i], GetWorldMatrix(modelParams)));
            }
        }

        private static Matrix4x4 GetViewPortMetrix(ModelParams modelParams)
        {
            return GetViewPortMatrix(modelParams.XMin, modelParams.YMin, modelParams.Width, modelParams.Height);
        }

        public static Matrix4x4 GetViewPortMatrix(int minX, int minY, int width, int height)
        {
            return new Matrix4x4(width / 2, 0, 0, 0,
                                 0, -1 * height / 2, 0, 0,
                                 0, 0, 1, 0,
                                 minX + (width / 2), minY + (height / 2), 0, 1);
        }

        private static Matrix4x4 GetMVP(ModelParams modelParams)
        {
            return GetWorldMatrix(modelParams) * GetViewerMatrix(modelParams) * GetWorldProjectionMatrix(modelParams);
        }

        private static void TransformToViewPort(Model model, ModelParams modelParams, float[] w)
        {
            for (int i = 0; i < model.Points.Count; i++)
            {
                model.Points[i] = Vector4.Transform(model.Points[i], GetViewPortMetrix(modelParams));
                model.Points[i] = new Vector4(model.Points[i].X, model.Points[i].Y, model.Points[i].Z, w[i]);
            }
        }
    }
}
