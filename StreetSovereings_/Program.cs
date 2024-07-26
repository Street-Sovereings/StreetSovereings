using System;
using OpenTK.Mathematics;

namespace StreetSovereings_
{
    class Program
    {
        [STAThread]
        public static void Main()
        {
            using (var game = new Renderer.Game())
            {
                game.AddCube(0.0f, 0.0f, 0.0f, new Vector4(1.0f, 0.0f, 0.0f, 1.0f), 1.0f);
                game.AddCube(1.0f, 1.0f, 0.0f, new Vector4(0.0f, 1.0f, 0.0f, 1.0f), 1.0f);
                game.Run();
            }
        }
    }
}
