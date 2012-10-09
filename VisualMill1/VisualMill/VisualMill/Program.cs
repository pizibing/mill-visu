using System;

namespace VisualMill
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (MainProgramm game = new MainProgramm())
            {
                game.Run();
            }
        }
    }
#endif
}

