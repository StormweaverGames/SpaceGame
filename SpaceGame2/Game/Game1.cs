using System;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.IO;
using EmberEngine.Tools;

namespace SpaceGame2
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class SpaceGame : Microsoft.Xna.Framework.Game
    {
        /// <summary>
        /// The graphics device manager
        /// </summary>
        GraphicsDeviceManager graphics;
        /// <summary>
        /// The spritebatch used for drawing basic 2D shapes and such
        /// </summary>
        SpriteBatch spriteBatch;
        
        /// <summary>
        /// The list of all gamescreens in this game
        /// </summary>
        List<GameScreen> Screens = new List<GameScreen>();
        /// <summary>
        /// The screen ID to render and update
        /// </summary>
        public int ScreenID { get; set; }

        /// <summary>
        /// Creates a new version of this game
        /// </summary>
        public SpaceGame()
        {
            StaticVars.Initialize();
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            new MainGame(this);

            base.Initialize();
        }

        /// <summary>
        /// Adds a new gamescreen to this game
        /// </summary>
        /// <param name="Screen">The screen to add</param>
        /// <returns>The ID of the screen</returns>
        public int AddGameScreen(GameScreen Screen)
        {
            Screens.Add(Screen);
            return Screens.Count - 1;
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (ScreenID >= 0 & ScreenID < Screens.Count)
                Screens[ScreenID].Update(gameTime);
                
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            FramerateCounter.OnDraw(gameTime);

            if (ScreenID >= 0 & ScreenID < Screens.Count)
                Screens[ScreenID].Draw();

            base.Draw(gameTime);
        }
    }
}
