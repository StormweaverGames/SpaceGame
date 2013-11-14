using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace SpaceGame2
{
    class GUI
    {
        public Color BlendColor = Color.White;
        public Color TextColor = Color.Black;

        Dictionary<string, Texture2D> Texs = new Dictionary<string, Texture2D>();
        Dictionary<string, SpriteFont> Fonts = new Dictionary<string, SpriteFont>();
        bool PlanetInfo;

        private Rectangle GUIBar;
        private const float GUIBarScale = 0.5F;
        private float GUIBarHeight = (96 - 34) * GUIBarScale;

        private Rectangle GUIPlanetInfo;
        private const float GUIPlanetScale = 0.5F;

        public void LoadContent(ContentManager Content)
        {
            Texs.Add("lowerBar", Content.Load<Texture2D>("GUI/gui"));
            Texs.Add("planetInfo", Content.Load<Texture2D>("GUI/guiPlanet"));
            Texs.Add("fastForwardArrow", Content.Load<Texture2D>("GUI/smallArrow"));

            Fonts.Add("infoFont", Content.Load<SpriteFont>("GUI/GUIFont"));

            GUIBar = new Rectangle(0, 0, 
                (int)(Texs["lowerBar"].Width * GUIBarScale), 
                (int)(Texs["lowerBar"].Height * GUIBarScale));

            GUIPlanetInfo = new Rectangle(0, 0, 
                (int)(Texs["planetInfo"].Width * GUIPlanetScale),
                (int)(Texs["planetInfo"].Height * GUIPlanetScale));
        }

        public void Draw(SpriteBatch spriteBatch, GraphicsDevice graphics, Planet targetPlanet)
        {
            spriteBatch.Draw(Texs["lowerBar"], new Rectangle(
                GUIBar.X, graphics.Viewport.Height - GUIBar.Height,
                GUIBar.Width, GUIBar.Height), BlendColor);

            spriteBatch.Draw(Texs["planetInfo"], new Rectangle(
                GUIPlanetInfo.X, graphics.Viewport.Height - GUIPlanetInfo.Height - (int)GUIBarHeight,
                GUIPlanetInfo.Width, GUIPlanetInfo.Height), BlendColor);

            spriteBatch.DrawString(Fonts["infoFont"], targetPlanet.ToString(),
                new Vector2(
                    GUIPlanetInfo.X + 26,
                    graphics.Viewport.Height - GUIPlanetInfo.Height - GUIBarHeight + 2.8F),
                    TextColor);

            spriteBatch.DrawString(Fonts["infoFont"], 
                "Toxicity: " + targetPlanet.Settings.AtmosphereToxicity + "%",
                new Vector2(
                    GUIPlanetInfo.X + 8,
                    graphics.Viewport.Height - GUIPlanetInfo.Height - GUIBarHeight + 26F),
                    TextColor);

            spriteBatch.DrawString(Fonts["infoFont"],
                "Atm. Density: " + targetPlanet.Settings.AtomosphereDensity,
                new Vector2(
                    GUIPlanetInfo.X + 8,
                    graphics.Viewport.Height - GUIPlanetInfo.Height - GUIBarHeight + 40F),
                    TextColor);
            
            spriteBatch.DrawString(Fonts["infoFont"],
                "Type: " + targetPlanet.Settings.Type,
                new Vector2(
                    GUIPlanetInfo.X + 8,
                    graphics.Viewport.Height - GUIPlanetInfo.Height - GUIBarHeight + 54F),
                    TextColor);

            spriteBatch.DrawString(Fonts["infoFont"],
                "Orbit: " + 
                Math.Abs(targetPlanet.Settings.OrbitAngleSpeed * targetPlanet.Settings.OrbitDistance) + 
                " km/s",
                new Vector2(
                    GUIPlanetInfo.X + 8,
                    graphics.Viewport.Height - GUIPlanetInfo.Height - GUIBarHeight + 68F),
                    TextColor);

            spriteBatch.DrawString(Fonts["infoFont"],
                "Orbit Time: " +
                (targetPlanet.Settings.Type != Planet.PlanetType.Star ?
                "" + (int)Math.Abs(targetPlanet.Settings.OrbitAngleSpeed / targetPlanet.Settings.PlanetAngleSpeed):
                "0") +
                " days",
                new Vector2(
                    GUIPlanetInfo.X + 8,
                    graphics.Viewport.Height - GUIPlanetInfo.Height - GUIBarHeight + 82F),
                    TextColor);

            spriteBatch.DrawString(Fonts["infoFont"],
                "Temp: " +
                Math.Round(targetPlanet.Temp, 2) +
                "°",
                new Vector2(
                    GUIPlanetInfo.X + 8,
                    graphics.Viewport.Height - GUIPlanetInfo.Height - GUIBarHeight + 96F),
                    TextColor);
        }
    }
}
