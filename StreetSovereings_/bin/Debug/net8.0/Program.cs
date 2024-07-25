using System;

namespace StreetSovereings_
{
    class Program
    {
        [STAThread]
        public static void Main()
        {
            using (var game = new Engine.Game())
            {
                game.Run();
            }
        }
    }
}

