using OpenTK.Mathematics;
using System.Collections.Generic;

namespace StreetSovereings_.src.objects
{
    public class Plane
    {
        public Vector3 Position { get; set; }
        public Vector3 Size { get; set; }
        public Vector4 Color { get; set; }
        public float Thickness { get; set; }

        public Plane(Vector3 position, Vector3 size, Vector4 color, float thickness)
        {
            Position = position;
            Size = size;
            Color = color;
            Thickness = thickness;
        }
    }

    public class PlaneManager
    {
        private List<Plane> _planes = new List<Plane>();

        public void AddPlane(float x, float y, float z, float sizeX, float sizeY, float sizeZ, Vector4 rgba, float thickness)
        {
            var plane = new Plane(new Vector3(x, y, z), new Vector3(sizeX, sizeY, sizeZ), rgba, thickness);
            _planes.Add(plane);
        }

        public List<Plane> GetPlanes()
        {
            return _planes;
        }
    }
}
