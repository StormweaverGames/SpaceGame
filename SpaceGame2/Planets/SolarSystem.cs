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
using System.Xml;
using System.Xml.Serialization;


namespace SpaceGame2
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class SolarSystem
    {
        /// <summary>
        /// The base planet in this system
        /// </summary>
        public Planet Star;
        /// <summary>
        /// A list of all planet names
        /// </summary>
        List<string> PlanetNames = new List<string>();
        Game game;
        public Game Game
        {
            get { return game; }
        }

        public SolarSystem() { }
                    
        /// <summary>
        /// Creates a new blank solar system
        /// </summary>
        /// <param name="game">The game that the solar system exists in</param>
        public SolarSystem(Game game, Color StarColor, float StarSize, Planet.PlanetSetting StarSetting)
        {
            this.game = game;
            
            Star = new Planet(game, StarSetting, this);
            PlanetNames.Add(Star.Settings.PlanetName);
        }

        /// <summary>
        /// Gets the planet from the ID
        /// </summary>
        /// <param name="ID">The integer ID of the planet</param>
        /// <returns>The planet at ID</returns>
        public Planet GetPlanetFromID(int ID)
        {
            if (ID >= 0 & ID < PlanetNames.Count)
                return GetPlanet(PlanetNames[ID]);
            else
                return null;
        }

        /// <summary>
        /// Adds a new planet to the Solar system
        /// </summary>
        /// <param name="game">The game that the system exists in</param>
        /// <param name="TargetPlanet">The name of the target to add the planet to</param>
        /// <param name="NewPlanet">The settings of the planet to add</param>
        /// <returns></returns>
        public bool AddPlanet(Planet.PlanetSetting NewPlanet)
        {
            if (!Star.HasMoon(NewPlanet.PlanetName))
            {
                Star.AddMoon(new Planet(this.Game, NewPlanet, this, Star));
                PlanetNames.Add(NewPlanet.PlanetName);
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Adds a new planet to the Solar system
        /// </summary>
        /// <param name="game">The game that the system exists in</param>
        /// <param name="TargetPlanet">The name of the target to add the planet to</param>
        /// <param name="NewPlanet">The settings of the planet to add</param>
        /// <returns></returns>
        public bool AddPlanet(string TargetPlanet, Planet.PlanetSetting NewPlanet)
        {
            Planet target = GetPlanet(TargetPlanet);

            if (target != null && !target.HasMoon(NewPlanet.PlanetName))
            {
                target.AddMoon(new Planet(this.Game, NewPlanet, this, target));
                PlanetNames.Add(NewPlanet.PlanetName);
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Checks if a planet exists in this solar system
        /// </summary>
        /// <param name="name">The name of the planet to search for</param>
        /// <returns>True if the planet exists in the system</returns>
        public bool PlanetExists(string name)
        {
            if (Star.Settings.PlanetName == name)
                return true;
            if (Star.HasMoon(name))
                return true;

            return false;
        }

        /// <summary>
        /// Checks if a planet exists in this solar system
        /// </summary>
        /// <param name="name">The name of the planet to search for</param>
        /// <returns>True if the planet exists in the system</returns>
        public Planet GetPlanet(string name)
        {
            if (Star.Settings.PlanetName == name)
                return Star;
            if (Star.GetMoon(name) != null)
                return Star.GetMoon(name);

            return null;
        }

        /// <summary>
        /// Gets the Solar System's Star
        /// </summary>
        /// <returns>The Star</returns>
        public Planet GetStar()
        {
            return Star;
        }

        /// <summary>
        /// Returns the total number of bodies in the system
        /// </summary>
        /// <returns>total number of bodies in the system</returns>
        public int GetPlanetCount()
        {
            return 1 + Star.getSubPlanetsCount();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void Update(GameTime gameTime)
        {
            Star.Update(gameTime);
        }

        /// <summary>
        /// Renders this Solar System
        /// </summary>
        /// <param name="graphicsDevice">The graphics device to draw with</param>
        /// <param name="view">The view parameters to use</param>
        public void Draw(GraphicsDevice graphicsDevice, ViewParamaters view)
        {
            Star.Draw(graphicsDevice, view);
        }

        /// <summary>
        /// Writes this Solar System to an XML file
        /// </summary>
        /// <param name="writer">The XML writer to use</param>
        public void WriteToXML(XmlWriter writer)
        {
            Star.WriteToXML(writer);
        }

        public static SolarSystem ReadFromXML(SpaceGame game, XmlReader reader)
        {
            SolarSystem temp = new SolarSystem(game, Color.Purple, 10, null);
            temp.Star =  Planet.ReadFromXML(temp, reader);
            return temp;
        }
    }
}
