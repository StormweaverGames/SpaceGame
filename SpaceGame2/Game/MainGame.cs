using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using System.Xml;
using EmberEngine.Tools.AdvancedMath;
using EmberEngine.Tools.Input;
using OpenTK.Input;
using EmberEngine.Tools;

namespace SpaceGame2
{
    class MainGame : GameScreen
    {        
        /// <summary>
        /// A basic font used for drawing text
        /// </summary>
        SpriteFont basicFont;
        /// <summary>
        /// The basic drawing effect (mostly for background)
        /// </summary>
        BasicEffect effect;
        RasterizerState wireframe;

        /// <summary>
        /// The game's view
        /// </summary>
        ViewParamaters view = new ViewParamaters();
        /// <summary>
        /// The game's background view
        /// </summary>
        ViewParamaters BackGroundView = new ViewParamaters();

        /// <summary>
        /// A list of all the game screens
        /// </summary>
        List<GameScreen> screens = new List<GameScreen>();

        /// <summary>
        /// The height of the camera (used for zooming)
        /// </summary>
        float CameraZoom = 1F;
        /// <summary>
        /// The camera's X and Z components
        /// </summary>
        Vector2 CameraPos;
        /// <summary>
        /// The farthest distance that the camera can be from the render plane
        /// </summary>
        const int MaxCameraZoom = 90;
        /// <summary>
        /// The nearest distance that the camera can be from the render plane
        /// </summary>
        const float MinCameraZoom = 0.01F;
        /// <summary>
        /// The camera tracking relative to the planet's surface tracking position
        /// </summary>
        float CameraTrack = 0;

        Quad BackQuad;
        /// <summary>
        /// The orgin planet of the solar system
        /// </summary>
        SolarSystem solarSystem;
        /// <summary>
        /// The planet that is the view's target
        /// </summary>
        Planet TargetPlanet;
        /// <summary>
        /// The ID refrence of the currently targeted planet
        /// </summary>
        int PlanetRef = 0;
        /// <summary>
        /// True if the view should rotate with the planet
        /// </summary>
        bool TrackSurface = false;
        /// <summary>
        /// The amount to vertically track the planet
        /// </summary>
        double TrackOffset;

        DebugState DebugState = DebugState.Full;
        GUI gui;

        public MainGame(SpaceGame game) : base(game)
        {
            Initialize();
            LoadContent();
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        private void Initialize()
        {
            this.Game.IsMouseVisible = true;

            wireframe = new RasterizerState();
            wireframe.FillMode = FillMode.WireFrame;
            wireframe.CullMode = CullMode.None;

            view.View = Matrix.CreateLookAt(new Vector3(0, 1F, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 1));
            view.Projection = Matrix.CreatePerspective(800 * 0.01F, 600 * 0.01F, 0.01F, MaxCameraZoom + 2);
            view.World = Matrix.CreateTranslation(Vector3.Zero);


            BackGroundView.View = Matrix.CreateLookAt(new Vector3(400, 1F, 300), new Vector3(400, 0, 300), new Vector3(0, 0, 1));
            BackGroundView.Projection = 
                Matrix.CreatePerspective(800 * 0.01F, 600 * 0.01F, 0.01F, MaxCameraZoom + 2);
            
            InitializePlanets();
            InitializeKeys();
        }

        /// <summary>
        /// Initializes all the planets
        /// </summary>
        private void InitializePlanets()
        {
            #region Kuma
            Planet.PlanetSetting kuma = new Planet.PlanetSetting();
            kuma.Type = Planet.PlanetType.Planet;
            kuma.PlanetName = "Kuma";
            kuma.GroundColor = Color.DarkGreen;
            kuma.InnerGroundColor = Color.Brown;
            kuma.RenderStyle = Planet.RenderStyle.Advanced;
            kuma.InitialOrbitAngle = 0;
            kuma.PlanetRadius = 100;
            kuma.OrbitInfo = 2500;
            kuma.PlanetAngleSpeed = 0.01;
            kuma.AirColor = Color.Aqua;
            kuma.AtomosphereDensity = 50.0F;
            kuma.AtomsphereDepth = 100;
            kuma.WaterColor = Color.LightSkyBlue;
            kuma.WaterDepth = 100;

            #region Luna
            Planet.PlanetSetting luna = new Planet.PlanetSetting();
            luna.Type = Planet.PlanetType.Moon;
            luna.PlanetName = "Luna";
            luna.GroundColor = Color.Gray;
            luna.InnerGroundColor = Color.LightGray;
            luna.InitialOrbitAngle = 75;
            luna.PlanetRadius = 50;
            luna.OrbitInfo = 600;
            luna.PlanetAngleSpeed = 0.001;
            #endregion
            #endregion

            #region Altos
            Planet.PlanetSetting altos = new Planet.PlanetSetting();
            altos.Roughness = 64;
            altos.PlanetName = "Altos";
            altos.GroundColor = Color.Maroon;
            altos.InnerGroundColor = Color.Magenta;
            altos.PlanetRadius = 200;
            altos.InitialOrbitAngle = 127;
            altos.OrbitInfo = 7000;
            altos.PlanetAngleSpeed = -0.001;
            altos.AirColor = Color.Blue;
            altos.AtmosphereToxicity = 25.0F;
            altos.AtomosphereDensity = 50.0F;
            altos.AtomsphereDepth = 300;
            altos.WaterColor = Color.Lerp(Color.LightPink, Color.Transparent, 0.1F);
            altos.WaterDepth = 200;

            #region Litha
            Planet.PlanetSetting litha = new Planet.PlanetSetting();
            litha.Type = Planet.PlanetType.Moon;
            litha.Roughness = 8;
            litha.PlanetName = "Litha";
            litha.GroundColor = Color.DarkGreen;
            litha.InnerGroundColor = Color.LightGray;
            litha.PlanetRadius = 75;
            litha.InitialOrbitAngle = 127;
            litha.OrbitInfo = 900;
            litha.PlanetAngleSpeed = -0.01;
            litha.AirColor = Color.Teal;
            litha.AtmosphereToxicity = 15.0F;
            litha.AtomosphereDensity = 50.0F;
            litha.AtomsphereDepth = 50;
            litha.WaterColor = Color.Lerp(Color.LightPink, Color.Transparent, 0.1F);
            litha.WaterDepth = 0;
            #endregion

            #region Nora
            Planet.PlanetSetting nora = new Planet.PlanetSetting();
            nora.Type = Planet.PlanetType.Moon;
            nora.Roughness = 8;
            nora.PlanetName = "Nora";
            nora.GroundColor = Color.Gray;
            nora.InnerGroundColor = Color.Maroon;
            nora.PlanetRadius = 50;
            nora.InitialOrbitAngle = 23;
            nora.OrbitInfo = 2000;
            nora.PlanetAngleSpeed = -0.001;
            nora.AirColor = Color.Red;
            nora.AtmosphereToxicity = 0;
            nora.AtomosphereDensity = 0;
            nora.AtomsphereDepth = 0;
            nora.WaterDepth = 0;
            #endregion
            #endregion

            #region Ceriese
            Planet.PlanetSetting ceriese = new Planet.PlanetSetting();
            ceriese.Type = Planet.PlanetType.Gas;
            ceriese.PlanetName = "Ceriese";
            ceriese.GroundColor = Color.Red;
            ceriese.InnerGroundColor = Color.Maroon;
            ceriese.PlanetRadius = 500;
            ceriese.InitialOrbitAngle = 202;
            ceriese.OrbitInfo = 30000;
            ceriese.PlanetAngleSpeed = -0.001;
            ceriese.AirColor = Color.Purple;
            ceriese.AtmosphereToxicity = 100.0F;
            ceriese.AtomosphereDensity = 100.0F;
            ceriese.AtomsphereDepth = 300;
            ceriese.WaterDepth = 0;

            #region Kratos
            Planet.PlanetSetting kratos = new Planet.PlanetSetting();
            kratos.PlanetName = "Kratos";
            kratos.Description =
                "A diamond in the rough, this rare planet is hospitable due to " +
                "it's molten core and high-density atmosphere. With a thick crust, " +
                "it makes the perfect place to start a new outpost.";
            kratos.Type = Planet.PlanetType.Moon;
            kratos.PlanetRadius = 100;
            kratos.CrustDepth = 0.3;
            kratos.Roughness = 8;
            kratos.WaterDepth = 100;
            kratos.WaterColor = Color.Lerp(Color.DarkGreen, Color.Transparent, 0.1F);
            kratos.GroundColor = Color.Red;
            kratos.InnerGroundColor = Color.DarkOrange;
            kratos.InitialOrbitAngle = 202;
            kratos.OrbitInfo = 5000;
            kratos.PlanetAngleSpeed = -0.001;
            kratos.AirColor = Color.LightGreen;
            kratos.AtomsphereDepth = 200;
            kratos.AtomosphereDensity = 300.0F;
            kratos.AtmosphereToxicity = 0.0F;
            kratos.InternalTemp = 2000;
            #endregion
            #endregion

            #region Jodon
            Planet.PlanetSetting jodon = new Planet.PlanetSetting();
            jodon.Roughness = 32;
            jodon.PlanetName = "Jodon";
            jodon.GroundColor = Color.Purple;
            jodon.InnerGroundColor = Color.DarkGray;
            jodon.PlanetRadius = 100;
            jodon.InitialOrbitAngle = 127;
            jodon.OrbitInfo = 15000;
            jodon.OrbitAngleSpeed = 1.5 * 10e-3;
            jodon.AirColor = Color.Green;
            jodon.AtmosphereToxicity = 100F;
            jodon.AtomosphereDensity = 100F;
            jodon.AtomsphereDepth = 100;
            jodon.WaterColor = Color.Lerp(Color.LightPink, Color.Transparent, 0.1F);
            jodon.WaterDepth = 100;
            #endregion

            #region Asteroids
            int AsteroidCount = StaticVars.Random.Next(0, 100);
            Planet.PlanetSetting[] Asteroids = new Planet.PlanetSetting[AsteroidCount];

            double sliceSize = (360.0 / AsteroidCount);

            for (int i = 0; i < AsteroidCount; i++)
            {
                Asteroids[i] = new Planet.PlanetSetting();

                Asteroids[i].InitialOrbitAngle =
                    (sliceSize * i) +
                    (StaticVars.Random.Next( -(int)(sliceSize / 2), (int)(sliceSize / 2)));

                Asteroids[i].HeightmapStepping = 16;
                Asteroids[i].RenderStyle = Planet.RenderStyle.Simple;
                Asteroids[i].PlanetName = "Asteroid " + i;
                Asteroids[i].InnerGroundColor = Color.DarkGray;
                byte Shade = (byte)StaticVars.Random.Next(20, 200);
                Asteroids[i].GroundColor = Color.FromNonPremultiplied(Shade, Shade, Shade, 255);
                Asteroids[i].OrbitInfo = 12000 + StaticVars.Random.Next(-500, 500);
                Asteroids[i].OrbitInfo.Apopsis = 12000 + StaticVars.Random.Next(-500, 500);
                Asteroids[i].OrbitInfo.Perapsis = 12000 + StaticVars.Random.Next(-500, 500);
            }
            #endregion

            #region Star
            Planet.PlanetSetting Star = new Planet.PlanetSetting();
            Star.Type = Planet.PlanetType.Star;
            Star.PlanetAngleSpeed = 0;
            Star.PlanetRadius = 700;
            Star.StarTemp = 5000;
            Star.PlanetName = "Akycha";
            Star.GroundColor = Color.DarkOrange;
            Star.InnerGroundColor = Color.Yellow;
            Star.RenderStyle = Planet.RenderStyle.Simple;
            Star.AirColor = Color.White;
            Star.AtomsphereDepth = 100;
            #endregion

            solarSystem = new SolarSystem(this.Game, Color.Yellow, 200F, Star);

            solarSystem.AddPlanet(kuma);
            solarSystem.AddPlanet("Kuma", luna);
            solarSystem.AddPlanet(altos);
            solarSystem.AddPlanet("Altos", litha);
            solarSystem.AddPlanet("Altos", nora);
            for (int i = 0; i < AsteroidCount; i++)
            {
                solarSystem.AddPlanet(Asteroids[i]);
            }
            solarSystem.AddPlanet(jodon);
            solarSystem.AddPlanet(ceriese);
            solarSystem.AddPlanet("Ceriese", kratos);

                TargetPlanet = solarSystem.GetStar();
        }

        /// <summary>
        /// Initializes all the control keys
        /// </summary>
        private void InitializeKeys()
        {
            KeyControls.Add("view_ZoomIn", new KeyWatcher(Key.Up));
            KeyControls["view_ZoomIn"].KeyDown += ZoomIn;
            KeyControls.Add("view_ZoomOut", new KeyWatcher(Key.Down));
            KeyControls["view_ZoomOut"].KeyDown += ZoomOut;

            KeyControls.Add("view_SurfaceTrackLeft", new KeyWatcher(Key.A));
            KeyControls["view_SurfaceTrackLeft"].KeyDown += TrackLeft;
            KeyControls.Add("view_SurfaceTrackRight", new KeyWatcher(Key.D));
            KeyControls["view_SurfaceTrackRight"].KeyDown += TrackRight;

            KeyControls.Add("view_SurfaceTrackUp", new KeyWatcher(Key.W));
            KeyControls["view_SurfaceTrackUp"].KeyDown += TrackUp;
            KeyControls.Add("view_SurfaceTrackDown", new KeyWatcher(Key.S));
            KeyControls["view_SurfaceTrackDown"].KeyDown += TrackDown;

            KeyControls.Add("view_PlanetUp", new KeyWatcher(Key.PageUp));
            KeyControls["view_PlanetUp"].KeyPressed += PlanetUp;
            KeyControls.Add("view_PlanetDown", new KeyWatcher(Key.PageDown));
            KeyControls["view_PlanetDown"].KeyPressed += PlanetDown;

            KeyControls.Add("game_SpeedDown", new KeyWatcher(Key.KeypadMinus));
            KeyControls["game_SpeedDown"].KeyPressed += SpeedDown;
            KeyControls.Add("game_SpeedUp", new KeyWatcher(Key.KeypadPlus));
            KeyControls["game_SpeedUp"].KeyPressed += SpeedUp;

            KeyControls.Add("view_SurfaceTrack", new KeyWatcher(Key.T));
            KeyControls["view_SurfaceTrack"].KeyPressed += ToggleTracking;

            KeyControls.Add("debug_SwitchState", new KeyWatcher(Key.F1));
            KeyControls["debug_SwitchState"].KeyPressed += SwitchDebug;
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        private void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            this.spriteBatch = new SpriteBatch(Game.GraphicsDevice);
            effect = new BasicEffect(Game.GraphicsDevice);
            //load a font
            basicFont = Game.Content.Load<SpriteFont>("BasicFont");
            basicFont.LineSpacing = (int)(basicFont.MeasureString(" ").Y * 0.7F);

            gui = new GUI();
            gui.LoadContent(Game.Content);
            gui.PlanetInfo = true;

            BackQuad = new Quad(Game.GraphicsDevice, new Vector2(800, 600), 
                Game.Content.Load<Texture2D>("space"), Color.White);

            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        private void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            solarSystem.Update(gameTime);

            // Allows the game to exit
            if (Keyboard.GetState().IsKeyDown(Key.Escape))
                Game.Exit();

            //Update control keys
            foreach (KeyWatcher w in KeyControls.Values)
            {
                w.Update();
            }

            //Update target planet
            TargetPlanet = solarSystem.GetPlanetFromID(PlanetRef);

            if (TrackSurface)
                view.Rotation -= (float)TargetPlanet.Settings.PlanetAngleSpeed * StaticVars.GameSpeed;

            //Update camera position
            CameraPos = TargetPlanet.Position +
                new Vector2(
                (float)Math2.LengthdirX(view.Rotation + CameraTrack, TrackOffset),
                (float)Math2.LengthdirY(view.Rotation + CameraTrack, TrackOffset));

            view.World = Matrix.Identity;

            view.View =
            Matrix.CreateLookAt(
            new Vector3(CameraPos.X, CameraZoom, CameraPos.Y),
                new Vector3(CameraPos.X, 0, CameraPos.Y), 
                new Vector3((float)Math2.LengthdirX(
            view.Rotation + CameraTrack, 1), 0, 
            (float)Math2.LengthdirY(
            view.Rotation + CameraTrack, 1)));
            ;

            gui.Update(spriteBatch, TargetPlanet);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Draw()
        {
            Game.GraphicsDevice.Clear(Color.Black);
            Game.GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

            if(DebugState == DebugState.Full)
                Game.GraphicsDevice.RasterizerState = wireframe;

            BackQuad.Render(BackGroundView);
            solarSystem.Draw(Game.GraphicsDevice, view);

            spriteBatch.Begin();

            if ((int)DebugState >= (int)DebugState.Partial)
            {
                spriteBatch.DrawString(basicFont, "FPS: " + FramerateCounter.FPS, new Vector2(10, 10), Color.White);
                spriteBatch.DrawString(basicFont, "Planet: " + TargetPlanet, new Vector2(10, 25), Color.White);
                spriteBatch.DrawString(basicFont, "Camera Y: " + CameraZoom, new Vector2(10, 40), Color.White);
                spriteBatch.DrawString(basicFont, "CameraRot: " + view.Rotation, new Vector2(10, 55), Color.White);
                spriteBatch.DrawString(basicFont, "CameraTrack: " + CameraTrack, new Vector2(10, 70), Color.White);
                spriteBatch.DrawString(basicFont, "Planet Rot: " + TargetPlanet.State.PlanetAngle, new Vector2(10, 85), Color.White);
                spriteBatch.DrawString(basicFont, "Orbit Angle: " + TargetPlanet.State.OrbitAngle, new Vector2(10, 100), Color.White);
                spriteBatch.DrawString(basicFont, "Pos: " + TargetPlanet.Position, new Vector2(10, 115), Color.White);
                spriteBatch.DrawString(basicFont, "Game Speed: " + StaticVars.GameSpeed, new Vector2(10, 130), Color.White);
                spriteBatch.DrawString(basicFont, "Debug: " + DebugState, new Vector2(10, 145), Color.White);
                spriteBatch.DrawString(basicFont, 
                    Utils.WrapText(basicFont, 
                    "Planet Description: " + TargetPlanet.Settings.Description, 400),//TargetPlanet.Settings.Description, 400), 
                    new Vector2(10, 160), Color.White);
            }

            gui.Draw(spriteBatch, TargetPlanet);

            spriteBatch.End();
        }

        #region Controls
        /// <summary>
        /// Zooms the Camera in
        /// </summary>
        /// <param name="o">The sender</param>
        /// <param name="e">A blank EventArgs</param>
        public void ZoomIn(KeyDownEventArgs e)
        {
            CameraZoom -= 0.2F * CameraZoom;

            CameraZoom = CameraZoom < MinCameraZoom ? MinCameraZoom : CameraZoom;
        }

        /// <summary>
        /// Zooms the Camera out
        /// </summary>
        /// <param name="o">The sender</param>
        /// <param name="e">A blank EventArgs</param>
        public void ZoomOut(KeyDownEventArgs e)
        {
            CameraZoom += 0.2F * CameraZoom;

            CameraZoom = CameraZoom > MaxCameraZoom ? MaxCameraZoom : CameraZoom;
        }

        /// <summary>
        /// Tracks the Camera left
        /// </summary>
        /// <param name="o">The sender</param>
        /// <param name="e">A blank EventArgs</param>
        public void TrackLeft(KeyDownEventArgs e)
        {
            view.Rotation -= 2F * CameraZoom;
            //SurfaceOffset -= 0.3F;
        }

        /// <summary>
        /// Tracks the Camera right
        /// </summary>
        /// <param name="o">The sender</param>
        /// <param name="e">A blank EventArgs</param>
        public void TrackRight(KeyDownEventArgs e)
        {
            view.Rotation += 2F * CameraZoom;
        }

        /// <summary>
        /// Tracks the Camera up
        /// </summary>
        /// <param name="o">The sender</param>
        /// <param name="e">A blank EventArgs</param>
        public void TrackUp(KeyDownEventArgs e)
        {
            TrackOffset += 2 * CameraZoom;
        }

        /// <summary>
        /// Tracks the Camera down
        /// </summary>
        /// <param name="o">The sender</param>
        /// <param name="e">A blank EventArgs</param>
        public void TrackDown(KeyDownEventArgs e)
        {
            TrackOffset -= 2 * CameraZoom;
            TrackOffset = TrackOffset > 0 ? TrackOffset : 0;
        }

        /// <summary>
        /// Scrolls to the next planet
        /// </summary>
        /// <param name="o">The sender</param>
        /// <param name="e">A blank EventArgs</param>
        public void PlanetUp(KeyDownEventArgs e)
        {
            PlanetRef++;
            PlanetRef = PlanetRef >= solarSystem.GetPlanetCount() ? 0 : PlanetRef;
        }

        /// <summary>
        /// Scrolls to the previous planet
        /// </summary>
        /// <param name="o">The sender</param>
        /// <param name="e">A blank EventArgs</param>
        public void PlanetDown(KeyDownEventArgs e)
        {
            PlanetRef--;
            PlanetRef = PlanetRef <  0 ? solarSystem.GetPlanetCount() - 1 : PlanetRef;
        }

        /// <summary>
        /// Speeds up the game
        /// </summary>
        /// <param name="o">The sender</param>
        /// <param name="e">A blank EventArgs</param>
        public void SpeedUp(KeyDownEventArgs e)
        {
            if (StaticVars.GameSpeed < 128)
                StaticVars.GameSpeed *= 2;
        }

        /// <summary>
        /// Slows down the game
        /// </summary>
        /// <param name="o">The sender</param>
        /// <param name="e">A blank EventArgs</param>
        public void SpeedDown(KeyDownEventArgs e)
        {
            if (StaticVars.GameSpeed > 0.5F)
                StaticVars.GameSpeed /= 2;
        }

        /// <summary>
        /// Toggles surface tracking
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        public void ToggleTracking(KeyDownEventArgs e)
        {
            TrackSurface = !TrackSurface;
        }

        /// <summary>
        /// Switches debug mode
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        public void SwitchDebug(KeyDownEventArgs e)
        {
            DebugState = (DebugState)(Utils.Wrap((int)(DebugState + 1), 1, 3));
        }
        #endregion
    }
}
