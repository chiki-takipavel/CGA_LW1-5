using System;
using System.Numerics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Globalization;
using CGA_LW1.Models;
using CGA_LW1.Readers;
using CGA_LW1.Parser;
using CGA_LW1.Algorithms;
using CGA_LW1.Algorithms.Shaders;
using CGA_LW1.Algorithms.Lightings;
using System.IO;

namespace CGA_LW1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Model model;
        private readonly CultureInfo culture = CultureInfo.InvariantCulture;
        private int width, height;

        public MainWindow()
        {
            InitializeComponent();
            sbError.Message.ActionClick += (_, _) => sbError.IsActive = false;
        }

        private static Bgr24Bitmap GetBgr24BitmapDiffuse()
        {
            string path = ObjFileReader.GetDiffusePath();

            if (File.Exists(path))
            {
                BitmapImage imgDiffuse = new(new Uri(path, UriKind.Relative));
                imgDiffuse.CreateOptions = BitmapCreateOptions.None;
                WriteableBitmap initialWriteableBitmapDiffuse = new(imgDiffuse);
                return new Bgr24Bitmap(initialWriteableBitmapDiffuse);
            }
            else
            {
                throw new FileNotFoundException();
            }
        }

        private static Bgr24Bitmap GetBgr24BitmapNormals()
        {
            string path = ObjFileReader.GetNormalsPath();

            if (File.Exists(path))
            {
                BitmapImage imgNormals = new(new Uri(path, UriKind.Relative));
                imgNormals.CreateOptions = BitmapCreateOptions.None;
                WriteableBitmap initialWriteableBitmapNormals = new(imgNormals);
                return new Bgr24Bitmap(initialWriteableBitmapNormals);
            }
            else
            {
                throw new FileNotFoundException();
            }
        }

        private static Bgr24Bitmap GetBgr24BitmapSpecular()
        {
            string path = ObjFileReader.GetSpecularPath();

            if (File.Exists(path))
            {
                BitmapImage imgSpecular = new(new Uri(path, UriKind.Relative));
                imgSpecular.CreateOptions = BitmapCreateOptions.None;
                WriteableBitmap initialWriteableBitmapSpecular = new(imgSpecular);
                return new Bgr24Bitmap(initialWriteableBitmapSpecular);
            }
            else
            {
                throw new FileNotFoundException();
            }
        }

        private static Bgr24Bitmap GetBgr24BitmapEmission()
        {
            string path = ObjFileReader.GetEmissionPath();

            if (File.Exists(path))
            {
                BitmapImage imgEmission = new(new Uri(path, UriKind.Relative));
                imgEmission.CreateOptions = BitmapCreateOptions.None;
                WriteableBitmap initialWriteableBitmapEmission = new(imgEmission);
                return new Bgr24Bitmap(initialWriteableBitmapEmission);
            }
            else
            {
                throw new FileNotFoundException();
            }
        }

        private void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string[] fileLines = ObjFileReader.Execute();
                model = ObjParser.Parse(fileLines);
                model.DiffuseTexture = GetBgr24BitmapDiffuse();
                model.NormalsTexture = GetBgr24BitmapNormals();
                model.SpecularTexture = GetBgr24BitmapSpecular();
                model.EmissionTexture = GetBgr24BitmapEmission();
            }
            catch (Exception ex)
            {
                sbError.Message.Content = $"Ошибка! {ex.Message}";
                sbError.IsActive = true;
            }
        }

        private void DrawButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                texturesCheckBox.IsEnabled = false;
                normalsCheckBox.IsEnabled = false;
                bloomCheckBox.IsEnabled = false;
                if (model is not null)
                {
                    width = (int)gridPicture.ActualWidth;
                    height = (int)gridPicture.ActualHeight;
                    WriteableBitmap source = new(width, height, 96, 96, PixelFormats.Bgra32, null);
                    Bgr24Bitmap bitmap = new(source);

                    ModelParams modelParams = GetModelsParams();
                    Model modelMain = model.Clone() as Model;

                    Transformations.TransformFromWorldToView(modelMain, modelParams);

                    if (modelMain.CheckSize(width, height))
                    {
                        Color color = Color.FromRgb(byte.Parse(colorRTextBox.Text, culture), byte.Parse(colorGTextBox.Text, culture), byte.Parse(colorBTextBox.Text, culture));
                        Vector3 lighting = new(int.Parse(lightVectorXTextBox.Text, culture), int.Parse(lightVectorYTextBox.Text, culture), int.Parse(lightVectorZTextBox.Text, culture));
                        if (bresenhamRadioButton.IsChecked is true)
                        {
                            Bresenham bresenham = new(bitmap, modelMain);
                            bresenham.DrawModel(color);
                        }
                        else if (planeShadingRadioButton.IsChecked is true)
                        {
                            PlaneShading shader = new(bitmap, modelMain, new LambertLighting(lighting));
                            shader.DrawModel(color);
                        }
                        else if (phongShadingRadioButton.IsChecked == true)
                        {
                            texturesCheckBox.IsEnabled = true;
                            normalsCheckBox.IsEnabled = true;
                            bloomCheckBox.IsEnabled = true;
                            // затенение фонга
                            Vector3 viewVector = Vector3.Normalize(new Vector3(0, 0, 0) - new Vector3(modelParams.CameraPositionX, modelParams.CameraPositionY, modelParams.CameraPositionZ));
                            Vector3 koefA = new(float.Parse(ambientXTextBox.Text, culture), float.Parse(ambientYTextBox.Text, culture), float.Parse(ambientZTextBox.Text, culture));
                            Vector3 koefD = new(float.Parse(diffuseXTextBox.Text, culture), float.Parse(diffuseYTextBox.Text, culture), float.Parse(diffuseZTextBox.Text, culture));
                            Vector3 koefS = new(float.Parse(specularXTextBox.Text, culture), float.Parse(specularYTextBox.Text, culture), float.Parse(specularZTextBox.Text, culture));
                            Vector3 koefE = new(float.Parse(emissionXTextBox.Text, culture), float.Parse(emissionYTextBox.Text, culture), float.Parse(emissionZTextBox.Text, culture));
                            Vector3 ambientColor = new(int.Parse(ambientRTextBox.Text, culture), int.Parse(ambientGTextBox.Text, culture), int.Parse(ambientBTextBox.Text, culture));
                            Vector3 reflectionColor = new(int.Parse(reflectionRTextBox.Text, culture), int.Parse(reflectionGTextBox.Text), int.Parse(reflectionBTextBox.Text));
                            float shiness = float.Parse(shinessTextBox.Text, CultureInfo.InvariantCulture);
                            float sigma = float.Parse(sigmaTextBox.Text, CultureInfo.InvariantCulture);
                            float exposure = float.Parse(exposureTextBox.Text, CultureInfo.InvariantCulture);
                            bool d = false, n = false, s = false, em = false;
                            if (normalsCheckBox is not null && (bool)normalsCheckBox.IsChecked)
                            {
                                n = true;
                            }
                            if (texturesCheckBox is not null && (bool)texturesCheckBox.IsChecked)
                            {
                                d = true;
                                s = true;
                                em = true;
                            }

                            bool bloom = false;
                            if (bloomCheckBox is not null && (bool)bloomCheckBox.IsChecked)
                            {
                                bloom = true;
                            }

                            PhongLighting light = new(lighting, viewVector, koefA, koefD, koefS, koefE, ambientColor, reflectionColor, shiness, d, n, s, em);
                            PhongShading shader = new(bitmap, modelMain, light, d, n, s, em, bloom, sigma, exposure);
                            shader.DrawModel(color);
                        }

                        picture.Source = bitmap.Source;
                    }
                }
                else
                {
                    sbError.Message.Content = "Загрузите .obj файл!";
                    sbError.IsActive = true;
                }
            }
            catch (Exception ex)
            {
                sbError.Message.Content = $"Ошибка! {ex.Message}";
                sbError.IsActive = true;
            }
        }

        private ModelParams GetModelsParams()
        {
            float scaling = (float)scaleSlider.Value;
            float modelYaw = (float)(modelYawSlider.Value * Math.PI / 180);
            float modelPitch = (float)(modelPitchSlider.Value * Math.PI / 180);
            float modelRoll = (float)(modelRollSlider.Value * Math.PI / 180);
            float translationX = (float)translationXSlider.Value;
            float translationY = (float)translationYSlider.Value;
            float translationZ = (float)translationZSlider.Value;
            float cameraPositionX = (float)CameraPositionXSlider.Value;
            float cameraPositionY = (float)CameraPositionYSlider.Value;
            float cameraPositionZ = (float)CameraPositionZSlider.Value;
            float cameraYaw = (float)(CameraYawSlider.Value * Math.PI / 180);
            float cameraPitch = (float)(CameraPitchSlider.Value * Math.PI / 180);
            float cameraRoll = (float)(CameraRollSlider.Value * Math.PI / 180);
            float fieldOfView = (float)(60 * Math.PI / 180);
            float aspectRatio = (float)width / height;
            float nearPlaneDistance = 0.1f;
            float farPlaneDistance = 100f;
            int xMin = 0;
            int yMin = 0;

            return new ModelParams(scaling, modelYaw, modelPitch, modelRoll, translationX, translationY, translationZ,
                 cameraPositionX, cameraPositionY, cameraPositionZ, cameraYaw, cameraPitch, cameraRoll, fieldOfView, aspectRatio, nearPlaneDistance,
                 farPlaneDistance, xMin, yMin, width, height);
        }
    }
}
