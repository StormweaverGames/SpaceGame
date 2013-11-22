using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaceGame2
{
    public static class StaticVars
    {
        /// <summary>
        /// Gets or set the common RNG
        /// </summary>
        public static Random Random;

        private static float gameSpeed = 1;
        /// <summary>
        /// Gets or sets this game's speed, clamped above 0
        /// </summary>
        public static float GameSpeed
        {
            get { return gameSpeed; }
            set
            {
                gameSpeed = value > 0 ? value : 0;
            }
        }

        public static void Initialize()
        {
            Random = new Random();
        }
    }
}
