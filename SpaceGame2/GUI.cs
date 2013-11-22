using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace SpaceGame2
{
    class GUI
    {
        public Color BlendColor = Color.White;
        public Color TextColor = Color.Black;

        Dictionary<string, Texture2D> Texs = new Dictionary<string, Texture2D>();
        Dictionary<string, SpriteFont> Fonts = new Dictionary<string, SpriteFont>();

        public bool PlanetInfo { get; set; }
        private bool MouseReleased = true;

        private Rectangle GUIBar;
        private const float GUIBarScale = 0.5F;
        private float GUIBarHeight = (96 - 34) * GUIBarScale;

        private Rectangle GUIPlanetInfo;
        private const float GUIPlanetScale = 0.5F;

        private RenderTarget2D Text;
        private bool MouseClicked = false;
        private float PlanetScroll = 0;

        private int PrevMouseX = 0;
        private int PrevMouseY = 0;
        
        public void LoadContent(ContentManager Content)
        {
            Texs.Add("lowerBar", Content.Load<Texture2D>("GUI/gui"));
            Texs.Add("planetInfo", Content.Load<Texture2D>("GUI/guiPlanet"));
            Texs.Add("fastForwardArrow", Content.Load<Texture2D>("GUI/smallArrow"));

            Fonts.Add("infoFont", Content.Load<SpriteFont>("GUI/GUIFont"));
            Fonts["infoFont"] = Utils.FixFontSpacing(Fonts["infoFont"], 2F);

            GUIBar = new Rectangle(0, 0, 
                (int)(Texs["lowerBar"].Width * GUIBarScale), 
                (int)(Texs["lowerBar"].Height * GUIBarScale));

            GUIPlanetInfo = new Rectangle(0, 0, 
                (int)(Texs["planetInfo"].Width * GUIPlanetScale),
                (int)(Texs["planetInfo"].Height * GUIPlanetScale));
        }

        /// <summary>
        /// Updates the GUI
        /// </summary>
        /// <param name="spriteBatch">the Spritebatch being used to render</param>
        /// <param name="targetPlanet">The target planet</param>
        public void Update(SpriteBatch spriteBatch, Planet targetPlanet)
        {
            MouseState mouse = Mouse.GetState();

            #region Drag on Planet Info
            if (!MouseClicked && mouse.LeftButton == ButtonState.Pressed &&
                ((mouse.X > 0 && mouse.X < GUIPlanetInfo.Width) &
                (mouse.Y > GUIPlanetInfo.Y && mouse.X < GUIPlanetInfo.Y + GUIPlanetInfo.Height)))
            {
                MouseClicked = true;
                PrevMouseX = mouse.X;
                PrevMouseY = mouse.Y;
            }
            if (mouse.LeftButton == ButtonState.Released)
            {
                MouseClicked = false;
            }

            if (MouseClicked)
            {
                PlanetScroll -= mouse.Y - PrevMouseY;
                PlanetScroll = PlanetScroll < 0 ? 0 : PlanetScroll;

                PrevMouseX = mouse.X;
                PrevMouseY = mouse.Y;
            }
            #endregion

            #region Toggle Planet Info
            if (mouse.LeftButton == ButtonState.Pressed && MouseReleased)
            {
                if (mouse.X >= 0 & mouse.X < GUIPlanetInfo.Width)
                {
                    if (!PlanetInfo &&
                        (mouse.Y > GUIPlanetInfo.Y + (GUIPlanetInfo.Height - 20) &
                        mouse.Y < GUIPlanetInfo.Y + GUIPlanetInfo.Height))
                        PlanetInfo = !PlanetInfo;
                    else if (PlanetInfo &&
                        (mouse.Y > GUIPlanetInfo.Y&
                        mouse.Y < GUIPlanetInfo.Y + 20))
                        PlanetInfo = !PlanetInfo;
                }

                MouseReleased = false;
            }
            else if (mouse.LeftButton == ButtonState.Released)
            {
                MouseReleased = true;
            }
            #endregion

            #region Build Text
            StringBuilder text = new StringBuilder();
            text.AppendLine("Toxicity: " + targetPlanet.Settings.AtmosphereToxicity + "%");
            text.AppendLine("Atm. Density: " + targetPlanet.Settings.AtomosphereDensity);
            text.AppendLine("Type: " + targetPlanet.Settings.Type);
            if (targetPlanet.Settings.Type != Planet.PlanetType.Star)
            {
                text.AppendLine("Apoap. : " + targetPlanet.Settings.OrbitInfo.Apopsis);
                text.AppendLine("Perap. : " + targetPlanet.Settings.OrbitInfo.Perapsis);
                text.AppendLine("Alt. : " + targetPlanet.Settings.OrbitInfo.GetRadius(targetPlanet.State.OrbitAngle));
            }
            text.AppendLine("Temp: " + Math.Round(targetPlanet.Temp, 2) + "°");
            text.AppendLine(
                "Description: \n" +
                Utils.WrapText(Fonts["infoFont"], targetPlanet.Settings.Description,
                GUIPlanetInfo.Width - 10));

            GUIPlanetInfo = new Rectangle(
                0, spriteBatch.GraphicsDevice.Viewport.Height - GUIPlanetInfo.Height 
                - (int)GUIBarHeight, GUIPlanetInfo.Width, GUIPlanetInfo.Height);

            Text = new RenderTarget2D(spriteBatch.GraphicsDevice,
                GUIPlanetInfo.Width - 10, GUIPlanetInfo.Height - 40);

            spriteBatch.GraphicsDevice.SetRenderTarget(Text);
            spriteBatch.GraphicsDevice.Clear(Color.Transparent);

            spriteBatch.Begin();
            spriteBatch.DrawString(Fonts["infoFont"],
                text,
                new Vector2(
                    0,
                    -PlanetScroll),
                    TextColor);
            spriteBatch.End();

            spriteBatch.GraphicsDevice.SetRenderTarget(null);
            #endregion
        }

        /// <summary>
        /// Draws the GUI
        /// </summary>
        /// <param name="spriteBatch">The SpriteBatch to draw with</param>
        /// <param name="targetPlanet">The currently targeted planet</param>
        public void Draw(SpriteBatch spriteBatch, Planet targetPlanet)
        {
            spriteBatch.Draw(Texs["lowerBar"], new Rectangle(
                GUIBar.X, spriteBatch.GraphicsDevice.Viewport.Height - GUIBar.Height,
                GUIBar.Width, GUIBar.Height), BlendColor);

            if (PlanetInfo)
            {
                spriteBatch.Draw(Texs["planetInfo"], GUIPlanetInfo, BlendColor);

                spriteBatch.DrawString(Fonts["infoFont"], targetPlanet.ToString(),
                    new Vector2(
                        GUIPlanetInfo.X + 26,
                        spriteBatch.GraphicsDevice.Viewport.Height - GUIPlanetInfo.Height
                        - GUIBarHeight + 2.8F),
                        TextColor);

                spriteBatch.Draw(Text, new Rectangle(GUIPlanetInfo.X + 8, GUIPlanetInfo.Y + 30,
                    Text.Width, Text.Height), Color.White);
            }
            else
            {
                spriteBatch.Draw(Texs["planetInfo"], new Rectangle(
                    0, GUIPlanetInfo.Y + (GUIPlanetInfo.Height - 20),
                    GUIPlanetInfo.Width, 20),
                    new Rectangle(0, 0, Texs["planetInfo"].Width, (int)(20 / GUIPlanetScale)), 
                    BlendColor);

                spriteBatch.DrawString(Fonts["infoFont"], targetPlanet.ToString(),
                    new Vector2(
                        GUIPlanetInfo.X + 26,
                        GUIPlanetInfo.Y + (GUIPlanetInfo.Height - 20) + 2.8F),
                        TextColor);
            }
        }
    }
}
