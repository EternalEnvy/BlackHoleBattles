using System;

namespace BlackholeBattle
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (BlackholeBattle game = new BlackholeBattle())
            {
                game.Run();
            }
        }
    }
#endif
}

