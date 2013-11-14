using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using EmberEngine.Tools.Input;


namespace SpaceGame2
{
    public abstract class GameScreen
    {
        /// <summary>
        /// The ID refrence of this screen
        /// </summary>
        int ID;
        private SpaceGame game;
        public SpaceGame Game { get { return game; } }

        /// <summary>
        /// The dictionairy that contains all the control keys
        /// </summary>
        public Dictionary<string, KeyWatcher> KeyControls = new Dictionary<string, KeyWatcher>();

        public SpriteBatch spriteBatch;        

        public GameScreen(SpaceGame game)
        {
            spriteBatch = new SpriteBatch(game.GraphicsDevice);
            this.ID = game.AddGameScreen(this);
            this.game = game;   
        }

        public virtual void Update(GameTime gameTime)
        {
            foreach (KeyWatcher w in KeyControls.Values)
            {
                w.Update();
            }
        }

        public virtual void Draw()
        {
            spriteBatch.Begin();

            spriteBatch.End();
        }
    }
}
