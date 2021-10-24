using System;
using System.Numerics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using System.Globalization;
using CGA_LW1.Models;
using CGA_LW1.Readers;
using CGA_LW1.Parser;
using CGA_LW1.Algorithms;

namespace CGA_LW1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Model model;
        private readonly int width, height;

        public MainWindow()
        {
            InitializeComponent();
            width = (int)screenPictureBox.Width;
            height = (int)screenPictureBox.Height;
            sbError.Message.ActionClick += (_, _) => sbError.IsActive = false;
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
            }
            catch (Exception ex)
            {
                sbError.Message.Content = $"Ошибка! {ex.Message}";
                sbError.IsActive = true;
            }
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

        private void DrawButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TextureEnabled(false);
                if (model is not null)
                {
                    WriteableBitmap source = new(width, height, 96, 96, PixelFormats.Bgra32, null);
                    Bgr24Bitmap bitmap = new(source);

                    ModelParams modelParams = GetModelsParams();
                    Model modelMain = model.Clone() as Model;

                    Transformations.TransformFromWorldToView(modelMain, modelParams);

                    if (modelMain.CheckSize(width, height))
                    {

                        Color color = Color.FromRgb(byte.Parse(colorRTextBox.Text), byte.Parse(colorGTextBox.Text), byte.Parse(colorBTextBox.Text));
                        Vector3 lighting = new(int.Parse(lightVectorXTextBox.Text), int.Parse(lightVectorYTextBox.Text), -int.Parse(lightVectorZTextBox.Text));

                        if (bresenhamRadioButton.IsChecked == true)
                        {
                            Bresenham bresenham = new(bitmap, modelMain);
                            bresenham.DrawModel(color);
                        }
                        else if (plainShadingRadioButton.IsChecked == true)
                        {
                            PlaneShading shader = new(bitmap, modelMain, new LambertLighting(lighting));
                            shader.DrawModel(color);
                        }
                        else if (phongShadingRadioButton.IsChecked == true)
                        {
                            TextureEnabled(true);
                            // затенение фонга
                            Vector3 viewVector = new(0, 0, -1);
                            Vector3 koefA = new(float.Parse(colorRTextBox_A.Text), float.Parse(colorGTextBox_A.Text), float.Parse(colorBTextBox_A.Text));
                            Vector3 koefD = new(float.Parse(colorRTextBox_D.Text), float.Parse(colorGTextBox_D.Text), float.Parse(colorBTextBox_D.Text));
                            Vector3 koefS = new(float.Parse(colorRTextBox_S.Text), float.Parse(colorGTextBox_S.Text), float.Parse(colorBTextBox_S.Text));
                            Vector3 ambientColor = new(int.Parse(colorRTextBox_Ambient.Text), int.Parse(colorGTextBox_Ambient.Text), int.Parse(colorBTextBox_Ambient.Text));
                            Vector3 reflectionColor = new(int.Parse(colorRTextBox_Reflection.Text), int.Parse(colorGTextBox_Reflecion.Text), int.Parse(colorBTextBox_Reflection.Text));
                            float shiness = float.Parse(shinessBox.Text, CultureInfo.InvariantCulture);
                            bool d = false, n = false, s = false;
                            if (diffuseCheckBox != null && (bool)diffuseCheckBox.IsChecked)
                            {
                                d = true;
                            }
                            if (normalCheckBox != null && (bool)normalCheckBox.IsChecked)
                            {
                                n = true;
                            }
                            if (mirrorCheckBox != null && (bool)mirrorCheckBox.IsChecked)
                            {
                                s = true;
                            }

                            /*var light = new PhongLighting(lighting, viewVector, koef_a, koef_d, koef_s, ambientColor, reflectionColor, shiness, d, n, s);
                            var light = new LambertLighting(lighting);
                            PhongShading shader = new PhongShading(bitmap, modelMain, light, d, n, s);
                            shader.DrawModel(color);*/
                        }

                        screenPictureBox.Source = bitmap.Source;
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

        private void TextureEnabled(bool flag)
        {
            //diffuseCheckBox.IsEnabled = flag;
            //normalCheckBox.IsEnabled = flag;
           // mirrorCheckBox.IsEnabled = flag;
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
            float fieldOfView = (float)(45 * Math.PI / 180);
            float aspectRatio = (float)width / height;
            float nearPlaneDistance = 2f;
            float farPlaneDistance = 50f;
            int xMin = 0;
            int yMin = 0;

            return new ModelParams(scaling, modelYaw, modelPitch, modelRoll, translationX, translationY, translationZ,
                 cameraPositionX, cameraPositionY, cameraPositionZ, cameraYaw, cameraPitch, cameraRoll, fieldOfView, aspectRatio, nearPlaneDistance,
                 farPlaneDistance, xMin, yMin, width, height);
        }
    }
}
