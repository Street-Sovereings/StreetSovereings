using OpenTK.Mathematics;

namespace StreetSovereings_.src.objects
{
    public class Plane
    {
        public Vector3 Position { get; set; }
        public Vector3 Size { get; set; }
        public Vector4 Color { get; set; }

        public Plane(Vector3 position, Vector3 size, Vector4 color)
        {
            Position = position;
            Size = size;
            Color = color;
        }
    }

    public class PlaneManager
    {
        private List<Plane> _planes = new List<Plane>();

        public void AddPlane(float x, float y, float z, float sizeX, float sizeY, float sizeZ, Vector4 rgba)
        {
            var plane = new Plane(new Vector3(x, y, z), new Vector3(sizeX, sizeY, sizeZ), rgba);
            _planes.Add(plane);
        }

        public List<Plane> GetPlanes()
        {
            return _planes;
        }
    }
}
