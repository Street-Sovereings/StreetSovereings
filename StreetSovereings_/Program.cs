using OpenTK.Mathematics;
using StreetSovereings_.src;

namespace StreetSovereings_
{
    class Program
    {
        [STAThread]
        public static void Main()
        {
            Console.WriteLine("Loading...");
            using (var game = new Renderer.Game())
            {
                Console.WriteLine("Game runned!");
                game.AddCube(0.0f, 1.0f, 0.0f, new Vector4(1.0f, 0.0f, 0.0f, 1.0f), 1.0f);
                game.Run();
            }
        }
    }
}
