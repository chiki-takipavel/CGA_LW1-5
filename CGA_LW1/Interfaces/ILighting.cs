using CGA_LW1.Models;
using System.Numerics;
using System.Windows.Media;

namespace CGA_LW1.Interfaces
{
    public interface ILighting
    {
        Color GetPointColor(Vector3 normal, Color color);
        Color GetPointColor(Model model, Vector3 texel, Vector3 argNormal);
    }
}
