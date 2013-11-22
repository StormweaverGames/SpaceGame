///<summary>
///</summary>


using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.IO;
using System.Xml.Serialization;
using EmberEngine.Tools.AdvancedMath;
using EmberEngine.Tools.AdvancedMath.Noise;

namespace SpaceGame2
{
    /// <summary>
    /// This is a game component that represents a planet. Can be updated and drawn.
    /// </summary>
    public class Planet
    {
        /// <summary>
        /// The length of any planet's heightmap
        /// </summary>
        public int HeightMapStepping;
        /// <summary>
        /// The amount of triangles generated for any planet's air
        /// </summary>
        public int AirStepping = 72;
        /// <summary>
        /// The amount of triangles generated for any planet's water
        /// </summary>
        public int WaterStepping = 72;

        /// <summary>
        /// Holds the settings for this planet
        /// </summary>
        PlanetSetting settings;
        public PlanetSetting Settings
        {
            get { return settings; }
        }

        /// <summary>
        /// The PlanetState of this planet
        /// </summary>
        PlanetState state = new PlanetState();
        /// <summary>
        /// The PlanetState of this planet
        /// </summary>
        public PlanetState State
        {
            get { return state; }
        }

        /// <summary>
        /// The Verticies representing the land
        /// </summary>
        VertexPositionColorNormal[] landVerts;
        short[] LandIndices;
        /// <summary>
        /// The Verticies representing the water
        /// </summary>
        VertexPositionColorNormal[] waterVerts;
        /// <summary>
        /// The Verticies representing the air
        /// </summary>
        VertexPositionColorNormal[] airVerts;
        /// <summary>
        /// Gets the effect that this planet uses for drawing
        /// </summary>
        BasicEffect effect;
        /// <summary>
        /// The world transformation matrix for this planet
        /// </summary>
        Matrix transform;
        /// <summary>
        /// The rotation matrix for this planet
        /// </summary>
        Matrix rotation;
        public Matrix Transform
        {
            get { return transform; }
        }
        public Matrix Rotation
        {
            get { return rotation;  }
        }
        /// <summary>
        /// Gets the total transformation matrix of this planet
        /// </summary>
        public Matrix TotalTransform
        {
            get
            {
                return rotation * transform;
            }
        }
        Vector2 pos;
        public Vector2 Position
        {
            get { return pos; }
        }

        SolarSystem parentSystem;
        Planet parentPlanet = null;

        private float temp = -100F;
        public float Temp
        {
            get 
            {
                if (settings.Type == PlanetType.Moon)
                {
                    return GetTemp(Vector2.Distance(Position, parentSystem.Star.Position),
                        parentSystem.Star.settings.StarTemp, settings.AtomosphereDensity,
                        PlanetType.Moon);
                }
                else if (settings.Type == PlanetType.Star)
                {
                    return settings.StarTemp;
                }
                else
                {
                    return temp;
                }
            }
        }

        private float GetTemp(float OrbitDistance, float StarTemp, float AtomosphereDensity, PlanetType Type)
            {
                if (Type == PlanetType.Star)
                {
                    return StarTemp;
                }
                else if (Type == PlanetType.Gas)
                {
                    return float.NaN;
                }
                else
                {
                    float  t = 
                        ((((StarTemp / OrbitDistance) *
                        (AtomosphereDensity /
                        (2 * (100 / AtomosphereDensity))))
                        - (OrbitDistance / 1000)) *
                        ((500 - settings.AtomsphereDepth) / 500)) + 
                        settings.InternalTemp / settings.PlanetRadius;
                    return t;
                }
            }

        public Planet()
        {
        }

        /// <summary>
        /// Creates a new planet
        /// </summary>
        /// <param name="game">The game that this planet exists in</param>
        /// <param name="settings">The planetSettings for this planet</param>
        public Planet(Game game, PlanetSetting settings, SolarSystem parent, Planet parentPlanet)
        {
            Init(game, settings, parent, parentPlanet);
        }

        /// <summary>
        /// Creates a new planet
        /// </summary>
        /// <param name="game">The game that this planet exists in</param>
        /// <param name="settings">The planetSettings for this planet</param>
        public Planet(Game game, PlanetSetting settings, SolarSystem parent)
        {
            Init(game, settings, parent, null);
        }

        /// <summary>
        /// Initializes this planet
        /// </summary>
        /// <param name="game">The game to initialize to</param>
        /// <param name="settings">The planetSettings to use</param>
        /// <param name="parent">The host solar system</param>
        /// <param name="parentPlanet">The host planet, or null if it is a star</param>
        private void Init(Game game, PlanetSetting settings, SolarSystem parent, Planet parentPlanet)
        {
            this.settings = settings;
            this.parentSystem = parent;
            this.parentPlanet = parentPlanet;

            this.HeightMapStepping = settings.HeightmapStepping;
            this.AirStepping = settings.AirStepping;
            this.WaterStepping = settings.WaterStepping;

            effect = new BasicEffect(game.GraphicsDevice);
            effect.VertexColorEnabled = true;

            if (parent.GetStar() != null)
            {
                effect.DiffuseColor =
                    Color.Lerp(parent.GetStar().Settings.AirColor, Color.White,
                    settings.AtomosphereDensity / 50F).ToVector3();
            }

            if (settings.Type != PlanetType.Star & settings.OrbitAngleSpeed == 0)
                this.settings.OrbitAngleSpeed = 100 / settings.OrbitInfo.Apopsis;

            state.OrbitAngle = settings.InitialOrbitAngle;
            state.PlanetAngle = settings.InitialPlanetAngle;

            if (settings.Type == PlanetType.Planet | settings.Type == PlanetType.Moon)
            {
                state.HeightMap = MidpointDisplacement.MidpointDisplacement2D(settings.Roughness, (int)settings.PlanetRadius, HeightMapStepping);
                Utils.Roughen(state.HeightMap, 0.1F);
            }
            else
            {
                state.HeightMap = new float[HeightMapStepping + 1];

                for (int i = 0; i < HeightMapStepping + 1; i++)
                    state.HeightMap[i] = settings.PlanetRadius;
            }

            RebuildGeometry(0, HeightMapStepping);

            if (settings.AtomsphereDepth != 0)
                BuildAirVerts();

            if (settings.WaterDepth != 0)
                BuildWaterVerts();

            if (settings.Type != PlanetType.Star)
            {
                temp = GetTemp(Settings.OrbitInfo.GetRadius(State.OrbitAngle), 
                    parent.Star.Settings.StarTemp,
                    Settings.AtomosphereDensity, Settings.Type);
            }

            UpdateMatrix();

            StreamWriter w = new StreamWriter(File.OpenWrite(settings.PlanetName + ".plnt"));
            for (int i = 0; i < state.HeightMap.Length; i++)
            {
                w.WriteLine("hPos_" + i + ": " + state.HeightMap[i]);
            }
            for (int i = 0; i < landVerts.Length; i++)
            {
                w.WriteLine("vPos_" + i + ": " + landVerts[i].Position);
                w.WriteLine("vCol_" + i + ": " + landVerts[i].Color);
            }

            w.Close();
        }

        /// <summary>
        /// Rebuilds the land verts between the min and max
        /// </summary>
        /// <param name="minX">The minimum x co-ord</param>
        /// <param name="maxX">The maximum x co-ord</param>
        public void RebuildGeometry(int minx = 0, int maxx = 0)
        {
            if (minx == 0 & maxx == 0)
            {
                minx = 0;
                maxx = HeightMapStepping;
            }

            minx = (int)MathHelper.Clamp(minx, 0, HeightMapStepping);
            maxx = (int)MathHelper.Clamp(maxx, 0, HeightMapStepping);

            int minX = minx < maxx ? minx : maxx;
            int maxX = maxx > minx ? maxx : minx;

            switch (Settings.RenderStyle)
            {
                case RenderStyle.Advanced:
                    BuildGeometryAdvanced(minX, maxX);
                    break;
                case RenderStyle.Simple:
                    BuildGeometrySimple(minX, maxX);
                    break;
            }
        }

        /// <summary>
        /// Gets the position for the given x, angle, and scale
        /// </summary>
        /// <param name="x">The heighmap co-ord to check</param>
        /// <param name="angle">The angle to check</param>
        /// <param name="scale">The scale to generate at</param>
        /// <returns></returns>
        private Vector3 GetPos(int x, double angle, double scale)
        {
            return new Vector3(
                (float)Math2.LengthdirX(angle, state.HeightMap[x] * scale),
                0,
                (float)Math2.LengthdirY(angle, state.HeightMap[x] * scale));

        }

        private void BuildGeometryAdvanced(int minX, int maxX)
        {
            double increment = 360.0 / (HeightMapStepping);
            double angle = increment * minX;

            float max = state.HeightMap.Max();

            if (landVerts == null)
            {
                landVerts = new VertexPositionColorNormal[(HeightMapStepping * 2) + 1];
            }
            else if (landVerts.Length != (HeightMapStepping * 2) + 1)
            {
                landVerts = new VertexPositionColorNormal[(HeightMapStepping * 2) + 1];
            }

            if (LandIndices == null)
            {
                LandIndices = new short[HeightMapStepping * 9];
            }
            else if (LandIndices.Length != HeightMapStepping * 9)
            {
                LandIndices = new short[HeightMapStepping * 9];
            }

            landVerts[HeightMapStepping * 2] = new VertexPositionColorNormal(Vector3.Zero,
                settings.InnerGroundColor,
                new Vector3(0, 1, 0));

            for (int x = minX; x < maxX; x++)
            {
                landVerts[x] = new VertexPositionColorNormal(
                    GetPos(x, angle, 1),
                    Settings.GroundColor,
                    new Vector3(
                        (float)Math2.LengthdirX(angle, 1),
                        0,
                        (float)Math2.LengthdirY(angle, 1)));

                landVerts[x + HeightMapStepping] = new VertexPositionColorNormal(
                    GetPos(x, angle, 1 - Settings.CrustDepth),
                    Settings.InnerGroundColor,
                    new Vector3(
                        (float)Math2.LengthdirX(angle, 1),
                        0,
                        (float)Math2.LengthdirY(angle, 1)));

                angle += increment;
            }

            int n;

            for (int x = minX; x < maxX; x++)
            {
                n = (x * 9);

                LandIndices[n] = (short)(HeightMapStepping * 2);
                LandIndices[n + 1] = GetIndexInner((short)(x));
                LandIndices[n + 2] = GetIndexInner((short)(x + 1));

                LandIndices[n + 3] = GetIndexInner((short)(x));
                LandIndices[n + 4] = GetIndexOuter((short)(x));
                LandIndices[n + 5] = GetIndexOuter((short)(x + 1));

                LandIndices[n + 6] = GetIndexInner((short)(x + 1));
                LandIndices[n + 7] = GetIndexInner((short)(x));
                LandIndices[n + 8] = GetIndexOuter((short)(x + 1));
            }
        }

        /// <summary>
        /// Builds this planet's geometry using the simpler method
        /// </summary>
        /// <param name="minX">The minimum x to start rendering from</param>
        /// <param name="maxX">The maximum x to render to</param>
        private void BuildGeometrySimple(int minX, int maxX)
        {
            double increment = 360.0 / (HeightMapStepping);
            double angle = increment * minX;

            float max = state.HeightMap.Max();

            if (landVerts == null)
            {
                landVerts = new VertexPositionColorNormal[HeightMapStepping + 1];
            }
            else if (landVerts.Length != HeightMapStepping + 1)
            {
                landVerts = new VertexPositionColorNormal[HeightMapStepping + 1];
            }

            if (LandIndices == null)
            {
                LandIndices = new short[HeightMapStepping * 3];
            }
            else if (LandIndices.Length != HeightMapStepping * 3)
            {
                LandIndices = new short[HeightMapStepping * 3];
            }

            landVerts[HeightMapStepping] = new VertexPositionColorNormal(
                Vector3.Zero,
                settings.InnerGroundColor,
                new Vector3(0, 1, 0));

            for (int x = minX; x < maxX; x++)
            {
                landVerts[x] = new VertexPositionColorNormal(
                    GetPos(x, angle, 1),
                    Settings.GroundColor,
                    new Vector3(
                        (float)Math2.LengthdirX(angle, 1),
                        0,
                        (float)Math2.LengthdirY(angle, 1)));

                angle += increment;
            }

            int n = 0;
            for (int x = minX; x < maxX; x++)
            {
                n = (x * 3);

                LandIndices[n] = (short)(HeightMapStepping);
                LandIndices[n + 1] = GetIndexOuter((short)(x));
                LandIndices[n + 2] = GetIndexOuter((short)(x + 1));
            }
        }

        /// <summary>
        /// Gets the vertex index from the specified index
        /// </summary>
        /// <param name="ID">The index to check</param>
        /// <returns></returns>
        private short GetIndexInner(short ID)
        {
            ID = (short)(
                (ID > (HeightMapStepping * 2) - 1) ? ID - (HeightMapStepping) : ID);
            ID = (short)(
                ID < HeightMapStepping ? ID + HeightMapStepping : ID);

            return ID;
        }

        /// <summary>
        /// Gets the vertex index from the specified index
        /// </summary>
        /// <param name="ID">The index to check</param>
        /// <returns></returns>
        private short GetIndexOuter(short ID)
        {
            ID = (short)(
                ID > HeightMapStepping - 1 ? ID - (HeightMapStepping) : ID);

            return ID;
        }
        
        /// <summary>
        /// Updates the transformation matrix
        /// </summary>
        private void BuildAirVerts()
        {
            airVerts = new VertexPositionColorNormal[AirStepping * 3];

            double increment = 360.0 / (double)AirStepping;
            double angle = 0;

            for (int x = 0; x < (AirStepping) * 3; x+=3)
            {
                airVerts[x] = new VertexPositionColorNormal(Vector3.Zero, 
                    settings.AirColor, 
                    new Vector3(0, 1, 0));

                airVerts[x + 1] = new VertexPositionColorNormal(
                    new Vector3(
                        (float)Math2.LengthdirX(angle, settings.AtomsphereDepth + settings.PlanetRadius), 
                        0,
                        (float)Math2.LengthdirY(angle, settings.AtomsphereDepth + settings.PlanetRadius)), 
                        Color.Transparent, 
                        new Vector3(0, 1, 0));

                airVerts[x + 2] = new VertexPositionColorNormal(
                    new Vector3(
                        (float)Math2.LengthdirX(angle + increment, settings.AtomsphereDepth + settings.PlanetRadius),
                        0,
                        (float)Math2.LengthdirY(angle + increment, settings.AtomsphereDepth + settings.PlanetRadius)),
                        Color.Transparent,
                        new Vector3(0, 1, 0));
                angle += increment;
            }
        }

        /// <summary>
        /// Updates the transformation matrix
        /// </summary>
        private void BuildWaterVerts()
        {
            waterVerts = new VertexPositionColorNormal[WaterStepping * 3];

            double increment = 360.0 / WaterStepping;
            double angle = 0;

            for (int x = 0; x < (WaterStepping) * 3; x += 3)
            {
                waterVerts[x] = new VertexPositionColorNormal(Vector3.Zero,
                    settings.WaterColor,
                    new Vector3(0, 1, 0));

                waterVerts[x + 1] = new VertexPositionColorNormal(
                    new Vector3(
                        (float)Math2.LengthdirX(angle, settings.WaterDepth),
                        0,
                        (float)Math2.LengthdirY(angle, settings.WaterDepth)),
                        settings.WaterColor,
                        new Vector3(0, 1, 0));

                waterVerts[x + 2] = new VertexPositionColorNormal(
                    new Vector3(
                        (float)Math2.LengthdirX(angle + increment, settings.WaterDepth),
                        0,
                        (float)Math2.LengthdirY(angle + increment, settings.WaterDepth)),
                        settings.WaterColor,
                        new Vector3(0, 1, 0));
                angle += increment;
            }
        }

        /// <summary>
        /// Rebuilds the the whole planet's geometry
        /// </summary>
        public void RebuildAllGeometry()
        {
            RebuildGeometry();
            BuildAirVerts();
            BuildWaterVerts();
        }

        /// <summary>
        /// Rebuilds this planet's heightmap
        /// </summary>
        public void RebuildHeightmap()
        {
            state.HeightMap =
                MidpointDisplacement.MidpointDisplacement2D(
                settings.Roughness, 
                (int)settings.PlanetRadius, 
                HeightMapStepping);

            RebuildGeometry();
        }

        /// <summary>
        /// Sets this planet's height map
        /// </summary>
        /// <param name="heightMap">The new heightmap to use</param>
        public void SetHeightmap(float[] heightMap)
        {
            state.HeightMap = heightMap;
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void Update(GameTime gameTime)
        {
            state.OrbitAngle += settings.OrbitAngleSpeed * StaticVars.GameSpeed;
            state.PlanetAngle += settings.PlanetAngleSpeed * StaticVars.GameSpeed;

            //Make sure angles wrap
            state.OrbitAngle = Utils.Wrap(state.OrbitAngle, 0, 360);
            state.PlanetAngle = Utils.Wrap(state.PlanetAngle, 0, 360);

            UpdateMatrix();

            foreach (Planet p in this.Settings.Moons)
                p.Update(gameTime);
        }

        /// <summary>
        /// Updates the transformation matrix
        /// </summary>
        private void UpdateMatrix()
        {
            //state.PlanetAngle is planet's rotation around it's centre
            //state.OrbitAngle is the planet's rotation around it's parent

            //lengthdir - Calculates change in x or y with the given angle and length

            if (settings.OrbitPlanet == null) //orbit planet is the planet it's orbiting
            {
                pos = new Vector2(0,0);
            }
            else
            {
                //Settings.OrbitInfo.Centre = Settings.OrbitPlanet.pos;
                this.pos = Settings.OrbitPlanet.Position + 
                    new Vector2(
                        (float)Math2.LengthdirX(State.OrbitAngle, Settings.OrbitInfo.Apopsis),
                        (float)Math2.LengthdirY(State.OrbitAngle, Settings.OrbitInfo.Apopsis)
                        );
            }

            rotation = Matrix.CreateRotationY((float)state.PlanetAngle.ToRad()); 

            transform = Matrix.CreateTranslation(new Vector3(pos.X, 0, pos.Y));
        }

        /// <summary>
        /// Draws this planet
        /// </summary>
        /// <param name="device">The graphics device to draw with</param>
        /// <param name="view">The ViewParameters to use</param>
        public void Draw(GraphicsDevice device, ViewParamaters view)
        {
            effect.View = view.View;
            effect.Projection = view.Projection;
            effect.World = TotalTransform * view.World;

            RenderAir(device);
            RenderWater(device);
            RenderLand(device);

            if (settings.Moons != null)
            {
                foreach (Planet p in settings.Moons)
                {
                    p.Draw(device, view);
                }
            }
        }

        /// <summary>
        /// Renders the air for this planet
        /// </summary>
        /// <param name="device">The graphics device to use</param>
        private void RenderAir(GraphicsDevice device)
        {
            if (airVerts != null && airVerts.Length >= 3)
            {
                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();

                    device.DrawUserPrimitives<VertexPositionColorNormal>(PrimitiveType.TriangleList, airVerts, 0, airVerts.Length / 3);
                }
            }
        }

        /// <summary>
        /// Renders the water for this planet
        /// </summary>
        /// <param name="device">The graphics device to use</param>
        private void RenderWater(GraphicsDevice device)
        {
            if (waterVerts != null && waterVerts.Length >= 3)
            {
                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();

                    device.DrawUserPrimitives<VertexPositionColorNormal>(PrimitiveType.TriangleList, waterVerts, 0, waterVerts.Length / 3);
                }
            }
        }

        /// <summary>
        /// Renders the land for this planet
        /// </summary>
        /// <param name="device">The graphics device to use</param>
        private void RenderLand(GraphicsDevice device)
        {
            if (landVerts.Length >= 3)
            {
                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();

                    device.DrawUserIndexedPrimitives<VertexPositionColorNormal>(
                        PrimitiveType.TriangleList, landVerts, 0, landVerts.Length,
                        LandIndices, 0, LandIndices.Length / 3);
                }
            }
        }

        /// <summary>
        /// Adds a planet that orbits this planet
        /// </summary>
        /// <param name="planet">The planet to add</param>
        public void AddMoon(Planet planet)
        {
            Planet[] temp = new Planet[settings.Moons.Length + 1]; //make a temp array

            settings.Moons.CopyTo(temp, 0); //copy moons to temp
            temp[temp.Length - 1] = planet; //add new moon to end of temp

            settings.Moons = temp; //set the moons to temp

            planet.settings.OrbitPlanet = this; //set the moon's parent to this
        }

        /// <summary>
        /// Returns true if this planet has a moon with a specific
        /// name
        /// </summary>
        /// <param name="name">The name of the planet to search for</param>
        /// <returns>True if one of this planet's moons</returns>
        public bool HasMoon(string name)
        {
            foreach (Planet p in settings.Moons)
            {
                if (p.settings.PlanetName == name)
                    return true;
                if (p.HasMoon(name))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the amount of moons, and moons of moons of this planet
        /// </summary>
        /// <returns>The count of planets that inherit from this planet</returns>
        public int getSubPlanetsCount()
        {
            int c = 0;

            foreach (Planet p in settings.Moons)
            {
                c++;
                c += p.getSubPlanetsCount();
            }

            return c;
        }

        /// <summary>
        /// Returns true if this planet has a moon with a specific
        /// name
        /// </summary>
        /// <param name="name">The name of the planet to search for</param>
        /// <returns>The planet's moon, or null</returns>
        public Planet GetMoon(string name)
        {
            foreach (Planet p in settings.Moons)
            {
                if (p.settings.PlanetName == name)
                    return p;
                if (p.GetMoon(name) != null)
                    return p.GetMoon(name);
            }
            return null;
        }

        /// <summary>
        /// Writes the Planet to the XML stream
        /// </summary>
        /// <param name="writer">The XML writer</param>
        public void WriteToXML(XmlWriter writer)
        {
            XmlSerializer s = new XmlSerializer(typeof(PlanetSetting));
            s.Serialize(writer, settings);
        }

        public static Planet ReadFromXML(SolarSystem system, XmlReader reader)
        {
            PlanetSetting p = PlanetSetting.ReadFromXML(system, reader);
            return new Planet(system.Game, p, system);
        }

        /// <summary>
        /// Converts the planet to a string
        /// </summary>
        /// <returns>The name of the planet</returns>
        public override string ToString()
        {
            return settings.PlanetName;
        }

        public class PlanetObject
        {
            Vector2 landPos;

            public void Draw()
            {

            }
        }

        /// <summary>
        /// An enumeration for the different types of planets
        /// </summary>
        public enum PlanetType
        {
            Star = 0,
            Planet = 1,
            Moon = 2,
            Gas = 3
        }

        /// <summary>
        /// Represents the style to render a planet with
        /// </summary>
        public enum RenderStyle
        {
            Advanced = 0,
            Simple = 1
        }

        /// <summary>
        /// Holds a planets settings
        /// </summary>
        [Serializable]
        public class PlanetSetting
        {
            /// <summary>
            /// The name of the planet
            /// </summary>
            public string PlanetName = "Unknown";
            /// <summary>
            /// The description of the planet
            /// </summary>
            public string Description = "This planet has not yet been discovered.";
            /// <summary>
            /// The planet's type
            /// </summary>
            public PlanetType Type = PlanetType.Planet;

            /// <summary>
            /// How rough the surface of the planet is. 0 is perfectly smooth
            /// Default: 16
            /// </summary>
            public int Roughness = 16;
            /// <summary>
            /// The outermost ground color
            /// </summary>
            public Color GroundColor = Color.DarkGreen;
            /// <summary>
            /// The innermost ground color
            /// </summary>
            public Color InnerGroundColor = Color.Black;
            /// <summary>
            /// The air color
            /// </summary>
            public Color AirColor = Color.Blue;
            /// <summary>
            /// The water color at the surface of the water
            /// </summary>
            public Color WaterColor = Color.Aqua;
            /// <summary>
            /// Sets this planet's render style (must be set BEFORE building geometry)
            /// </summary>
            public RenderStyle RenderStyle = RenderStyle.Advanced;
            /// <summary>
            /// The height map stepping for the planet, default 1024
            /// </summary>
            public int HeightmapStepping = 1024;
            /// <summary>
            /// The amount of triangles generated for the planet's air, default 72
            /// </summary>
            public int AirStepping = 72;
            /// <summary>
            /// The amount of triangles generated for the planet's water, default 72
            /// </summary>
            public int WaterStepping = 72;

            /// <summary>
            /// The depth of the water on this planet
            /// </summary>
            public float WaterDepth = 0;

            /// <summary>
            /// How thick the atmosphere of the planet is from it's surface
            /// </summary>
            public float AtomsphereDepth = 0;
            /// <summary>
            /// The Breathablilty of the atmosphere from 0 to 100
            /// </summary>
            public float AtomosphereDensity = 0;
            /// <summary>
            /// The toxicity of the atmosphere, from 0 to 100
            /// </summary>
            public float AtmosphereToxicity = 0;

            /// <summary>
            /// The tempurateure of the star, only used for stars
            /// </summary>
            public float StarTemp = 5000;
            /// <summary>
            /// The internal tempurate of the planet
            /// <remarks>Default: 0</remarks>
            /// </summary>
            public float InternalTemp = 0;

            /// <summary>
            /// The radius of the planet
            /// </summary>
            public float PlanetRadius = 100;
            /// <summary>
            /// The percentage of the planet radius that the crust desends
            /// </summary>
            public double CrustDepth = 0.1;
            /// <summary>
            /// How far this planet orbits from it's parent
            /// </summary>
            public OrbitalInfo OrbitInfo;
            /// <summary>
            /// The parent planet that this planet orbits around
            /// </summary>
            public Planet OrbitPlanet;
            /// <summary>
            /// An array of all planets that orbit this planet
            /// </summary>
            public Planet[] Moons = new Planet[0];
            
            /// <summary>
            /// The initial angle that the planet is relative to it's
            /// parent in <b>degrees</b>
            /// </summary>
            public double InitialOrbitAngle = 0;
            /// <summary>
            /// The angle in <b>degrees</b> that the planet rotates around
            /// it's parent every game tick
            /// </summary>
            public double OrbitAngleSpeed = 0;
            /// <summary>
            /// The initial angle that the planet is rotated in 
            /// <b>degrees</b>
            /// </summary>
            public double InitialPlanetAngle = 0;
            /// <summary>
            /// The angle in <b>degrees</b> that the planet rotates around
            /// it's axis every game tick
            /// </summary>
            public double PlanetAngleSpeed = -0.1F;

            /// <summary>
            /// Writes the Planet setting to XML
            /// </summary>
            /// <param name="writer">The XMLWriter to use</param>
            public void WriteToFile(XmlWriter writer)
            {
                XmlSerializer s = new XmlSerializer(typeof(PlanetSetting));
                s.Serialize(writer, this);    
            }

            public static PlanetSetting ReadFromXML(SolarSystem system, XmlReader reader)
            {
                XmlSerializer s = new XmlSerializer(typeof(PlanetSetting));
                return (PlanetSetting)s.Deserialize(reader);                
            }
        }
        
        /// <summary>
        /// Represents the state of a planet
        /// </summary>
        public struct PlanetState
        {
            /// <summary>
            /// The angle in <b>degrees</b> to the orgin planet
            /// </summary>
            public double OrbitAngle;
            /// <summary>
            /// The angle in <b>degrees</b> that this planet has rotated
            /// </summary>
            public double PlanetAngle;

            /// <summary>
            /// The heightmap of the planet
            /// </summary>
            public float[] HeightMap;
        }
    }
}
