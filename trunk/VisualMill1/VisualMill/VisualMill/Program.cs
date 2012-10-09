using System;

namespace VisualMill
{
#if WINDOWS
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            //MainForm form = new MainForm();
            //form.Show();
            MainProgramm game = new MainProgramm();          
            game.Run();            
        }
    }
#endif
}

