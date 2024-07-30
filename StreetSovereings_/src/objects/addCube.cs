using OpenTK.Mathematics;

namespace StreetSovereings_.src.objects
{
    public class Cube
    {
        public Vector3 Position { get; set; }
        public Vector4 Color { get; set; }
        public float Mass { get; set; }

        public Cube(Vector3 position, Vector4 color, float mass)
        {
            Position = position;
            Color = color;
            Mass = mass;
        }
    }

    public class CubeManager
    {
        private List<Cube> _cubes = new List<Cube>();

        public void AddCube(float x, float y, float z, Vector4 rgba, float mass)
        {
            var cube = new Cube(new Vector3(x, y, z), rgba, mass);
            _cubes.Add(cube);
        }

        public List<Cube> GetCubes()
        {
            return _cubes;
        }
    }
}
