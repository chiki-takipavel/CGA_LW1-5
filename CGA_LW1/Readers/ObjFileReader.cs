using Microsoft.Win32;
using System.IO;
using System.Text;

namespace CGA_LW1.Readers
{
    public static class ObjFileReader
    {
        private const string InitialDirectory = @"../../../objTest";
        private const string ObjFilter = "Obj files (*.obj) | *.obj";
        private static string pathFile;

        public static string[] Execute()
        {
            OpenFileDialog openFileDialog = new();
            string[] fileLines = null;
            openFileDialog.InitialDirectory = Path.GetFullPath(InitialDirectory);
            openFileDialog.Filter = ObjFilter;

            if (openFileDialog.ShowDialog() is not null)
            {
                string path = openFileDialog.FileName;
                pathFile = path;
                fileLines = File.ReadAllLines(path, Encoding.UTF8);
            }

            return fileLines;
        }

        public static string GetFileName()
        {
            return Path.GetFileNameWithoutExtension(pathFile);
        }

        public static string GetDiffusePath()
        {
            return $@"{pathFile.Substring(0, pathFile.LastIndexOf(@"\"))}\Diffuse.png";
        }

        public static string GetNormalsPath()
        {
            return $@"{pathFile.Substring(0, pathFile.LastIndexOf(@"\"))}\Normal.png";
        }

        public static string GetSpecularPath()
        {
            return $@"{pathFile.Substring(0, pathFile.LastIndexOf(@"\"))}\Specular.png";
        }

        public static string GetEmissionPath()
        {
            return $@"{pathFile.Substring(0, pathFile.LastIndexOf(@"\"))}\Emission.png";
        }
    }
}
