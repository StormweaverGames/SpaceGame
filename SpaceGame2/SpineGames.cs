﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Xml.Serialization;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using Spine_Library.Graphics;
using Spine_Library.Input;
using Spine_Library.Instances;
using Spine_Library.Inventories;
using Spine_Library.SkeletalAnimation;
using Spine_Library.Tools;
using Spine_Library._3DFuncs;

namespace Spine_Library
{
    namespace Inventories
    {
        /// <summary>
        /// Handles all the items to be used by entities, inventories,
        /// etc...
        /// </summary>
        public class ItemHandler
        {
            ItemBase[] itemList;
            int itemCount = 0;

            public ItemHandler(int listSize)
            {
                itemList = new ItemBase[listSize];
            }

            /// <summary>
            /// Adds an item to the list
            /// </summary>
            /// <param name="item">The item to add (casted to an ItemBase)</param>
            /// <returns>The ID of the item, -1 if unsucessfull</returns>
            public int addItemToList(ItemBase item)
            {
                int id = getFirstOpenID();
                if (id != -1)
                {
                    item.identifier = id;
                    itemList[id] = item;
                    itemCount++;
                    return id;
                }
                return -1;
            }

            /// <summary>
            /// Removes an item from the list with the given ID
            /// </summary>
            /// <param name="id">The ID of the item to remove</param>
            /// <returns>True if the remove was sucessfull</returns>
            public bool removeItemFromList(int id)
            {
                try
                {
                    if (itemList[id] != null)
                    {
                        itemList[id] = null;
                        itemCount--;
                        return true;
                    }
                    return false;
                }
                catch (IndexOutOfRangeException)
                {
                }
                return false;
            }

            /// <summary>
            /// Removes an item from the list with the given name
            /// </summary>
            /// <param name="name">The name of the item to remove</param>
            /// <returns>True if the remove was sucessfull</returns>
            public bool removeItemFromList(string name)
            {
                try
                {
                    for (int i = 0; i < itemList.Length; i++)
                    {
                        if (itemList[i] != null)
                        {
                            if (itemList[i].name == name)
                            {
                                itemList[i] = null;
                                itemCount--;
                                return true;
                            }
                        }
                    }
                    return false;
                }
                catch (IndexOutOfRangeException)
                {
                }
                return false;
            }

            /// <summary>
            /// returns the first available ID
            /// </summary>
            /// <returns>First open ID</returns>
            private int getFirstOpenID()
            {
                for (int i = 0; i < itemList.Length; i++)
                    if (itemList[i] == null)
                        return i;
                return -1;
            }

            /// <summary>
            /// Gets the number of items that the itmeHandler is 
            /// holding
            /// </summary>
            /// <returns>ItemCount</returns>
            public int getItemCount()
            {
                return itemCount;
            }

            /// <summary>
            /// Gets a string of all the item names seperated by newlines
            /// </summary>
            /// <returns>List of names</returns>
            public string getCombinedNames()
            {
                string r = "";
                foreach (ItemBase i in itemList)
                {
                    if (i != null)
                    {
                        r += i.name + "\n";
                    }
                }
                return r;
            }

            /// <summary>
            /// Holds the important values and functions for
            /// items
            /// </summary>
            public class ItemBase
            {
                public Texture2D myTexture;
                public int identifier;
                public double metaData;
                public string name;

                public ItemBase(Texture2D texture, string name)
                {
                    this.myTexture = texture;
                    this.name = name;
                }
            }
        }

        /// <summary>
        /// Handles inventories for players or other things
        /// </summary>
        public class Inventory
        {
            public ItemHandler.ItemBase[] items;
            public Inventory(int slotCount)
            {
            }

            private class Item
            {
                short stackSize;
                ItemHandler.ItemBase BaseItem;

                public Item(ItemHandler.ItemBase BaseItem, short stackSize)
                {
                    this.BaseItem = BaseItem;
                    this.stackSize = stackSize;
                }

                public short getItemCount()
                {
                    return stackSize;
                }
            }
        }
    }

    namespace Tools
    {
        /// <summary>
        /// Handles the calculation of the framerate
        /// </summary>
        public static class FPSHandler
        {
            static int framesInSecond = 0, FPS = 0;
            static double timer = 0;

            /// <summary>
            /// Handles the FPS counter for the draw event
            /// </summary>
            /// <param name="gameTime">The current GameTime</param>
            public static void onDraw(GameTime gameTime)
            {
                framesInSecond++;
                timer += gameTime.ElapsedGameTime.Milliseconds;

                if (timer >= 1000)
                {
                    timer = 0;
                    FPS = framesInSecond;
                    framesInSecond = 0;
                }
            }

            /// <summary>
            /// Returns the framerate as coounted by this object
            /// </summary>
            /// <returns>Framerate</returns>
            public static int getFrameRate()
            {
                return FPS;
            }

            /// <summary>
            /// Gets a multiplier that concides with 60 FPS
            /// </summary>
            /// <returns></returns>
            public static double getCommonDiff()
            {
                //return 60D / FPS;
                return 1D;
            }
        }

        public class Timer
        {
            TimeSpan timeRemaining;
            bool active = true;

            public Timer(TimeSpan time)
            {
                timeRemaining = time;
            }

            public bool tick(GameTime gameTime)
            {
                if (active)
                {
                    timeRemaining -= gameTime.ElapsedGameTime;
                    if (timeRemaining <= TimeSpan.Zero)
                    {
                        active = false;
                        return true;
                    }
                    return false;
                }
                return false;
            }
        }

        /// <summary>
        /// Acess to some custom Math functions
        /// </summary>
        public abstract class extraMath
        {
            /// <summary>
            /// Calculates the vector at the end of the given line
            /// from another vector
            /// </summary>
            /// <param name="vector">The orgin of the line</param>
            /// <param name="angle">The angle in RADIANS from the line</param>
            /// <param name="length">The length of the line</param>
            /// <returns>Offset Vector</returns>
            public static Vector2 calculateVector(Vector2 vector, double angle, double length)
            {
                Vector2 returnVect = new Vector2(vector.X + (float)lengthdir_x(angle, length), vector.Y + (float)lengthdir_y(angle, length));
                return returnVect;
            }

            public static Vector2 calculateVectorOffset(double angle, double length)
            {
                return calculateVector(Vector2.Zero, angle, length);
            }

            /// <summary>
            /// Calculates the angle between two vectors
            /// </summary>
            /// <param name="point1">The orgin vector to calculate from</param>
            /// <param name="point2">The vector to calculate to</param>
            /// <returns>Angle in radians</returns>
            public static double findAngle(Vector2 point1, Vector2 point2)
            {
                return Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);
            }

            /// <summary>
            /// Uses the midpoint displacement algorithm to return
            /// an array of values
            /// </summary>
            /// <param name="h">The smoothing level to use (higher = rougher)</param>
            /// <param name="baseVal">The base value to generate around</param>
            /// <param name="length">The length of the array (must be a multiple of 2)</param>
            /// <returns>A integer heightmap array</returns>
            public static float[] MidpointDisplacement(int h, int baseVal, int length)
            {
                //arguments:
                //
                //argument0 = room width
                //
                //argument1 = height change variable
                //
                //argument2 = land id
                //
                float[] output = new float[length + 1];
                Random RandNum = new Random();

                for (int xx = 0; xx <= length; xx++)
                {
                    output[xx] = baseVal;
                }
                output[0] = baseVal;

                //generate values
                for (int rep = 2; rep < length; rep *= 2)
                {
                    for (int i = 1; i <= rep; i += 1)
                    {

                        int x1 = (length / rep) * (i - 1);
                        int x2 = (length / rep) * i;
                        float avg = (output[x1] + output[x2]) / 2;
                        int Rand = RandNum.Next(-h, h);
                        output[(x1 + x2) / 2] = avg + (Rand);
                    }
                    h /= 2;
                }

                //returns array
                return output;
            }

            /// <summary>
            /// Returns how many radians change there should be given
            /// a speed relative to circumfrence and a height from
            /// the orgin point
            /// </summary>
            /// <param name="radius">The distance from the center point</param>
            /// <param name="speed">The speed in pixels to rotate around the orgin</param>
            /// <returns>The angle change in RADIANS</returns>
            public static double findCircumfenceAngleChange(double radius, double speed)
            {
                //determine value of n
                double n = Math.Acos((Math.Pow(speed, 2) - 2 * Math.Pow(radius, 2)) / (-2 * Math.Pow(radius, 2)));
                //make sure n is non NAN
                if (n == Double.NaN)
                {
                    //set the return to be -1
                    n = -1;
                    //throw an exception
                    throw (new ArithmeticException("Number is NAN!"));
                }
                //return n
                return n;
            }

            /// <summary>
            /// Determines the altitude from a given eliptical orbit
            /// and an angle in RADIANS
            /// </summary>
            /// <param name="length">The length of the elliptical orbit</param>
            /// <param name="width">The width of the elliptical orbit</param>
            /// <param name="theta">The angle in RADIANS from the orgin</param>
            /// <returns>The altitude at the given point</returns>
            public static double getAltitudeFromCenteredOrbit(double length, double width, double theta)
            {
                return length * width / (Math.Sqrt(Math.Pow((length * Math.Cos(theta)), 2) + Math.Pow((width * Math.Sin(theta)), 2)));
            }

            /// <summary>
            /// Determines the altitude from a given eliptical orbit
            /// and an angle in RADIANS
            /// </summary>
            /// <param name="length">The length of the elliptical orbit</param>
            /// <param name="width">The width of the elliptical orbit</param>
            /// <param name="offset">The offset along the length</param>
            /// <param name="theta">The angle in RADIANS from the orgin</param>
            /// <param name="angleOffset">The angle in RADIANS that the orbit is rotated by</param>
            /// <returns>The altitude at the given point</returns>
            public static double getAltitudeFromOffsetOrbit(double length, double width, double offset, double theta, double angleOffset)
            {
                theta -= angleOffset;
                return ((length * width) / Math.Sqrt(Math.Pow(width * Math.Cos(theta), 2) + Math.Pow(length * Math.Sin(theta), 2)));
            }

            /// <summary>
            /// Maps the given value from one number range to another
            /// </summary>
            /// <param name="lowVal">The low value in the orgin number range</param>
            /// <param name="highVal">The high value in the orgin number range<</param>
            /// <param name="newLowVal">The low value in the new number range<</param>
            /// <param name="newHighVal">The high value in the new number range<</param>
            /// <param name="value">The value to map</param>
            /// <returns>The value mapped to the new range</returns>
            public static double map(double lowVal, double highVal, double newLowVal, double newHighVal, double value)
            {
                double range = newHighVal - newLowVal;
                double oldRange = highVal - lowVal;
                double multiplier = range / oldRange;
                return newLowVal + ((value - lowVal) * multiplier);

            }

            /// <summary>
            /// Returns the angle in RADIANS that is 90° to
            /// the angle from the orgin to the point
            /// </summary>
            /// <param name="orgin">The orgin point</param>
            /// <param name="relativePoint">The point to calculate angle for</param>
            /// <returns>Angle in RADIANS that is 90° from the angle to orgin</returns>
            public static double getDrawAngle(Vector2 orgin, Vector2 relativePoint)
            {
                return -findAngle(orgin, relativePoint) + Math.PI / 2;
            }

            /// <summary>
            /// Returns the percentage that value is of maxValue
            /// </summary>
            /// <param name="maxValue">The top number, ex: number of marks on test.
            /// Cannot be 0</param>
            /// <param name="value">The value to be checked. ex: mark on test. 
            /// May be higher than maxValue</param>
            /// <returns>A percentage</returns>
            public static double getPercent(double maxValue, double value)
            {
                return ((value / maxValue) * 100.0);
            }

            /// <summary>
            /// Gets an array of colors for the texture. Thanks to Cyral from Stack Exchange
            /// </summary>
            /// <param name="texture">The texture to get the colors from</param>
            /// <returns>A 2D array of colors that represents the texture</returns>
            public static uint[] getTextureData(Texture2D texture)
            {
                //gets the 1D color[] from the teture
                uint[] colorList = new uint[texture.Width * texture.Height];
                //Get the colors
                texture.GetData(colorList);

                return colorList; //Done!
            }

            /// <summary>
            /// Return the Color with opposite RGB values, but with same 
            /// alpha value
            /// </summary>
            /// <param name="color">The color to get the opposite of</param>
            /// <returns>The opposite Color to color</returns>
            public static Color oppositeColor(Color color)
            {
                return Color.FromNonPremultiplied(255 - color.R, 255 - color.G, 255 - color.B, color.A);
            }

            public static double lengthdir_x(double angle, double length)
            {
                return length * Math.Cos(angle);
            }

            public static double dir_x(double length, double xChange)
            {
                return Math.Acos(xChange / length);
            }

            public static double lengthdir_y(double angle, double length)
            {
                return length * Math.Sin(angle);
            }

            public static double dir_y(double length, double yChange)
            {
                return Math.Asin(yChange / length);
            }

            public static double lengthdir_z(double angle, double length)
            {
                return length * Math.Sin(angle);
            }

            public static double anglePullZ(double angle, double length)
            {
                double x = length * Math.Sin(angle);
                return Math.Sqrt(Math.Pow(length, 2) - Math.Pow(x, 2));
            }

            public static double dir_z(double length, double zChange)
            {
                return Math.Asin(zChange / length);
            }

            public static double getDistance(Vector2 vect1, Vector2 vect2)
            {
                return Math.Sqrt(Math.Pow(vect2.X - vect1.X, 2) + Math.Pow(vect2.Y - vect1.Y, 2));
            }

            public static Vector3 getVector3(Vector2 yawPitch, double length)
            {
                return new Vector3(
                            extraMath.calculateVectorOffset(yawPitch.X, extraMath.anglePullZ(yawPitch.Y, length)),
                            (float)extraMath.lengthdir_z(yawPitch.Y, length));
            }

            public static Texture2D getSubTexture(GraphicsDevice graphics, Rectangle sourceRect, Texture2D orginImage)
            {
                Texture2D cropTexture = new Texture2D(graphics, sourceRect.Width, sourceRect.Height);
                Color[] data = new Color[sourceRect.Width * sourceRect.Height];
                orginImage.GetData(0, sourceRect, data, 0, data.Length);
                cropTexture.SetData(data);
                return cropTexture;
            }

            public static BoundingBox shiftBox(BoundingBox box, Vector3 position)
            {
                return new BoundingBox(box.Min + position, box.Max + position);
            }

            public static Vector3 shiftVector(Vector3 vector)
            {
                return new Vector3(vector.X, vector.Z, vector.Y);
            }

            public static Vector3 clampVector(Vector3 vector, Vector3 min, Vector3 max)
            {
                vector.X = MathHelper.Clamp(vector.X, min.X, max.X);
                vector.Y = MathHelper.Clamp(vector.Y, min.Y, max.Y);
                vector.Z = MathHelper.Clamp(vector.Z, min.Z, max.Z);
                return vector;
            }
        }

        public static class TextFuncs
        {
            static char[] nonNumerical = new char[] {
            'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z',
            ',','<','>','/','?',':',';',"'".ToCharArray()[0],'"', '{','[','}',']','!','@','#','$','%','^','&','*','(',')',
            '_','-','+','=','|','\\','`','~'};

            public static string PrepParse(string input)
            {
                string output = input.ToLower();
                foreach (char c in nonNumerical)
                    output = output.Replace(c.ToString(), "");
                return output;
            }
        }

        /// <summary>
        /// Holds global models
        /// </summary>
        public class ModelManager
        {
            /// <summary>
            /// The dictionary of models
            /// </summary>
            static Dictionary<string, VertexModel> models = new Dictionary<string, VertexModel>();
            /// <summary>
            /// A dictionary if ID's used for fast refrence
            /// </summary>
            static Dictionary<string, int> ids = new Dictionary<string, int>();

            /// <summary>
            /// Adds a new vertexModel to the list
            /// </summary>
            /// <param name="model">The model to add</param>
            /// <param name="name">The name of the model</param>
            /// <returns>The integer reference of the model added</returns>
            public static int addModel(VertexModel model, string name = "")
            {
                models.Add(name, model);
                ids.Add(name, models.Count - 1);
                return models.Count - 1;
            }

            /// <summary>
            /// Gets the index of the specified model
            /// </summary>
            /// <param name="name">The name of the model</param>
            /// <returns>The index of the model</returns>
            public static int getIndex(string name)
            {
                return ids[name];
            }

            /// <summary>
            /// Gets a model using the spcified ID
            /// </summary>
            /// <param name="id">The ID of the model</param>
            /// <returns>The model being searched for</returns>
            public static VertexModel getModel(int id)
            {
                return models.Values.ElementAt(id);
            }

            /// <summary>
            /// Gets a model from the name
            /// </summary>
            /// <param name="name">The name of the model to get</param>
            /// <returns>The model, or null if it does not exist</returns>
            public static VertexModel getModel(string name)
            {
                if (models.ContainsKey(name)) //makes sure the model exists
                    return models[name];
                return null;
            }

            /// <summary>
            /// Renders an instance
            /// </summary>
            /// <param name="instance">The instance to render</param>
            /// <param name="effect">The basicEffect to use</param>
            public static void drawModel(Instance3D instance, BasicEffect effect)
            {
                getModel(instance.model).render(effect, instance.position, instance.getMatrix(), 1F);
            }
        }

        /// <summary>
        /// Holds global textures
        /// </summary>
        public class TextureManager
        {
            /// <summary>
            /// The dictionary to hold the textures
            /// </summary>
            static Dictionary<string, Texture2D> texs = new Dictionary<string, Texture2D>();

            /// <summary>
            /// Adds a new texture to the dictionary
            /// </summary>
            /// <param name="texture">The texture to add to the dictionary</param>
            /// <param name="name">The name of the texture</param>
            /// <returns>The ID in the dictionary</returns>
            public static int addTexture(Texture2D texture, string name = "")
            {
                texs.Add(name, texture);
                return texs.Count - 1;
            }

            /// <summary>
            /// Gets a texture from the given name
            /// </summary>
            /// <param name="name">The name of the texture to get</param>
            /// <returns>The texture, or null if the texture does not exist</returns>
            public static Texture2D getTex(string name)
            {
                if (texs.ContainsKey(name)) //makes sure the texture exists
                    return texs[name];
                return null;
            }

            /// <summary>
            /// Gets a texture from the given name
            /// </summary>
            /// <param ID="ID">The id of the texture to get</param>
            /// <returns>The texture, or null if the texture does not exist</returns>
            public static Texture2D getTex(int ID)
            {
                if (texs.Count > ID) //makes sure the texture exists
                    return texs.Values.ElementAt(ID);
                return null;
            }

            public static int getID(string name)
            {
                if (texs.ContainsKey(name)) //makes sure the texture exists
                    return texs.Keys.ToList().IndexOf(name);
                return -1;
            }

            public static string getName(int ID)
            {
                return texs.Keys.ElementAt(ID);
            }

            public static void loadToGlobalModel()
            {
                foreach (string key in texs.Keys)
                {
                    GlobalStaticModel.addTexture(texs[key], key);
                }
            }
        }

        /// <summary>
        /// Handles global font loading
        /// </summary>
        public class FontManager
        {
            /// <summary>
            /// The dictionary to hold the fonts
            /// </summary>
            static Dictionary<string, SpriteFont> fonts = new Dictionary<string, SpriteFont>();

            /// <summary>
            /// Adds a new font to the dictionairy
            /// </summary>
            /// <param name="font">The font to add</param>
            /// <param name="key">The name of the front to be refrence</param>
            public static void addFont(SpriteFont font, string key = "")
            {
                fonts.Add(key, font);
            }

            /// <summary>
            /// Gets a font from the dictionary
            /// </summary>
            /// <param name="key">The name of the font</param>
            /// <returns>The font, or null if the font does not exist</returns>
            public static SpriteFont getFont(string key)
            {
                if (fonts.Keys.Contains(key)) //makes sure that the font exists
                    return fonts[key];
                return null;
            }
        }
    }

    namespace Input
    {
        /// <summary>
        /// Watches a key for a keypress. Note that to make sure action does not 
        /// repeat, use "keywatcher.wasPressed = false;" after action
        /// </summary>
        public class KeyWatcher
        {
            Keys key;
            List<Keys> keys;
            List<keyState> keyStates = new List<keyState>();
            public EventHandler isPressed;
            public EventHandler isDown;
            bool isKeyDown = false, wasPreviouslyDown = false;
            public bool wasPressed = false, wasReleased = false;
            byte type = 0;

            /// <summary>
            /// Create a new keywatcher
            /// </summary>
            /// <param name="key">The key to watch</param>
            /// <param name="pressedEvent">The event to raise when the button is pressed</param>
            public KeyWatcher(Keys key, EventHandler pressedEvent = null)
            {
                this.key = key;
                if (pressedEvent != null)
                    this.isPressed = pressedEvent;
            }

            /// <summary>
            /// Create a new keywatcher that uses multiple keys for one action
            /// or each individual key for that action. Ex: Ctrl + S, + / Add
            /// </summary>
            /// <param name="key">The keys to watch</param>
            /// <param name="all">True if all keys need to be pressed at once</param>
            /// <param name="pressedEvent">The event to raise when the button is pressed</param>
            public KeyWatcher(List<Keys> keys, bool all = false, EventHandler pressedEvent = null)
            {
                this.keys = keys;
                if (all)
                    this.type = 1;
                else
                    this.type = 2;
                foreach (Keys k in keys)
                {
                    keyStates.Add(new keyState(k));
                }

                this.isPressed = pressedEvent;
            }

            /// <summary>
            /// Updates the key watcher
            /// </summary>
            public void update()
            {
                switch (type)
                {
                    #region single key type
                    case 0:
                        if (Keyboard.GetState().IsKeyDown(key))
                        {
                            if (isDown != null)
                                isDown.Invoke(this, EventArgs.Empty);

                            if (!isKeyDown)
                            {
                                wasPressed = true;
                                isKeyDown = true;
                            }
                            else
                            {
                                wasPressed = false;
                            }
                            wasPreviouslyDown = true;
                        }
                        else
                        {
                            if (wasPreviouslyDown)
                                wasReleased = true;
                            else
                                wasReleased = false;
                            isKeyDown = false;
                            wasPreviouslyDown = false;
                        }
                        break;
                    #endregion

                    #region all down type
                    case 1:
                        int count = 0;
                        foreach (Keys k in keys)
                        {
                            if (Keyboard.GetState().IsKeyDown(k))
                                count++;
                        }

                        if (count == keys.Count)
                        {
                            if (isDown != null)
                                isDown.Invoke(this, EventArgs.Empty);

                            if (!isKeyDown)
                            {
                                wasPressed = true;
                                isKeyDown = true;
                            }
                            else
                            {
                                wasPressed = false;
                            }
                        }
                        else
                        {
                            if (wasPreviouslyDown)
                                wasReleased = true;
                            else
                                wasReleased = false;
                            isKeyDown = false;
                        }
                        break;
                    #endregion

                    #region multiple type
                    case 2:
                        bool temp2 = false;
                        foreach (keyState k in keyStates)
                        {
                            k.update();
                            if (k.isKeyDown)
                                temp2 = true;
                        }

                        if (temp2)
                        {
                            if (isDown != null)
                                isDown.Invoke(this, EventArgs.Empty);

                            if (!isKeyDown)
                            {
                                wasPressed = true;
                                isKeyDown = true;
                            }
                            else
                            {
                                wasPressed = false;
                            }
                        }
                        else
                        {
                            if (wasPreviouslyDown)
                                wasReleased = true;
                            else
                                wasReleased = false;
                            isKeyDown = false;
                        }
                        break;
                    #endregion
                }

                if (wasPressed)
                {
                    if (isPressed != null)
                        isPressed.Invoke(this, new EventArgs());
                }
            }

            /// <summary>
            /// Holds values for multiple key types
            /// </summary>
            private class keyState
            {
                public bool isKeyDown = false;
                public bool wasPressed;
                Keys key;

                public keyState(Keys key)
                {
                    this.key = key;
                }

                public void update()
                {
                    if (Keyboard.GetState().IsKeyDown(key))
                    {
                        if (!isKeyDown)
                        {
                            wasPressed = true;
                            isKeyDown = true;
                        }
                        else
                        {
                            wasPressed = false;
                        }
                    }
                    else
                    {
                        isKeyDown = false;
                    }
                }
            }
        }

        /// <summary>
        /// Handles visual buttons for GUI's and such
        /// </summary>
        public class VisualButton
        {
            Point orgin;
            Rectangle rect;
            Texture2D texture;
            bool isPressed, wasPressed, wasPreviouslyDown, wasReleased;
            int type;
            public const byte T_NORMAL = 0, T_TOGGLE = 1;
            SpriteFont font;
            string text;
            int stringWidthHalf, stringHeightHalf;
            Color upColor, downColor, color;
            BasicEffect basicEffect;

            public VisualButton(Rectangle rect, Texture2D tex, byte type, Color upColor, Color downColor)
            {
                this.rect = rect;
                this.orgin = new Point(rect.X + (rect.Width / 2), rect.Y + (rect.Height / 2));
                this.texture = tex;
                this.type = type;
                this.upColor = upColor;
                this.downColor = downColor;
            }

            public VisualButton(Point centre, Texture2D tex, byte type, Color upColor, Color downColor, SpriteFont font = null, string text = null, BasicEffect basicEffect = null)
            {
                this.orgin = centre;
                this.texture = tex;
                this.type = type;
                this.upColor = upColor;
                this.downColor = downColor;
                this.font = font;
                this.text = text;
                this.basicEffect = basicEffect;

                if (text != null & font != null)
                {
                    stringWidthHalf = (int)font.MeasureString(this.text).X / 2;
                    stringHeightHalf = (int)font.MeasureString(this.text).Y / 2;
                }
            }

            /// <summary>
            /// Updates the button and checks for input
            /// </summary>
            public void tick()
            {
                if (font != null)
                    rect = new Rectangle(orgin.X - stringWidthHalf - 2, orgin.Y - stringHeightHalf, stringWidthHalf * 2 + 4, stringHeightHalf * 2);
                else
                    rect = new Rectangle(orgin.X - rect.Width / 2, orgin.Y - rect.Height / 2, rect.Width, rect.Height);
                MouseState m = Mouse.GetState();

                if (wasPressed)
                    wasPressed = false;

                if (m.LeftButton == ButtonState.Pressed)
                {
                    if (!isPressed)
                    {
                        if (rect.Contains(new Point(m.X, m.Y)))
                        {
                            isPressed = true;
                            wasPressed = true;
                        }
                    }
                }
                else
                {
                    if (wasPreviouslyDown & rect.Contains(new Point(m.X, m.Y)))
                    {
                        wasReleased = true;
                    }
                    else
                        wasReleased = false;
                    isPressed = false;
                }

                if (isPressed & rect.Contains(new Point(m.X, m.Y)))
                    color = downColor;
                else
                    color = upColor;

                wasPreviouslyDown = isPressed;
            }

            /// <summary>
            /// Renders the button using hte specified SpriteBatch
            /// </summary>
            /// <param name="spriteBatch">The SpriteBatch to draw with</param>
            public void render(SpriteBatch spriteBatch)
            {
                if (font != null)
                {
                    if (texture != null)
                        spriteBatch.Draw(texture,
                            new Rectangle(orgin.X - stringWidthHalf - 2, orgin.Y - stringHeightHalf, stringWidthHalf * 2 + 4, stringHeightHalf * 2), color);
                    else
                        if (basicEffect != null)
                        {
                            Graphics.AdvancedDrawFuncs.DrawRect(basicEffect, rect, Color.Red, Color.Red, false);
                        }
                    spriteBatch.DrawString(font, text, new Vector2(orgin.X - stringWidthHalf, orgin.Y - stringHeightHalf), extraMath.oppositeColor(color));

                }
                else
                    if (texture != null)
                        spriteBatch.Draw(texture, rect, color);
            }

            /// <summary>
            /// Sets the texture for the button to use
            /// </summary>
            /// <param name="tex">The texture to use</param>
            public void setTexture(Texture2D tex)
            {
                this.texture = tex;
            }

            /// <summary>
            /// Returns true if the button was just pressed
            /// </summary>
            /// <returns>wasPressed</returns>
            public bool getPressed()
            {
                return wasPressed;
            }

            /// <summary>
            /// Returns true if th button was just released
            /// </summary>
            /// <returns>wasReleased</returns>
            public bool getReleased()
            {
                return wasReleased;
            }

            /// <summary>
            /// Returns true if the button is currently down
            /// </summary>
            /// <returns>isPressed</returns>
            public bool getIsDown()
            {
                return isPressed;
            }

            /// <summary>
            /// Sets the Color of this button
            /// </summary>
            /// <param name="color">The color to draw in</param>
            public void setColor(Color color)
            {
                this.color = color;
                this.upColor = color;
                this.downColor = color;
            }

            /// <summary>
            /// Sets the dimensions of the rectangle. Only used when there
            /// is no text to display
            /// </summary>
            /// <param name="width">The width of the button</param>
            /// <param name="height">The height of the button</param>
            public void setRectDimensions(int width, int height)
            {
                rect.Width = width;
                rect.Height = height;
            }

            /// <summary>
            /// Set the x co-ordinate for the orgin
            /// </summary>
            /// <param name="X">The X co-ordinate to move to</param>
            public void setX(int X)
            {
                orgin.X = X;
            }

            /// <summary>
            /// Set the y co-ordinate for the orgin
            /// </summary>
            /// <param name="Y">The Y co-ordinate to move to</param>
            public void setY(int Y)
            {
                orgin.Y = Y;
            }

            /// <summary>
            /// Set the X and Y co-ordinates of the orgin
            /// </summary>
            /// <param name="X">The X co-ordinate to move to</param>
            /// <param name="Y">The Y co-ordinate to move to</param>
            public void setXY(int X, int Y)
            {
                orgin.X = X;
                orgin.Y = Y;
            }
        }

        /// <summary>
        /// Handles menu sliders
        /// </summary>
        public class Slider
        {
            double value = 0;
            byte type;
            SpriteBatch spriteBatch;
            Rectangle rect;
            Texture2D horizontal, vertical;

            public const byte C_HORIZONTAL = 0, C_VERTICAL = 1;

            /// <summary>
            /// Initializes the slider
            /// </summary>
            /// <param name="value">The initial value of the slider</param>
            /// <param name="spriteBatch">The SpriteBatch to render with</param>
            /// <param name="rect">The rectangle to draw in</param>
            /// <param name="horizontal">The horizontal texture</param>
            /// <param name="vertical">The vertical texture</param>
            public Slider(float value, SpriteBatch spriteBatch, Rectangle rect, Texture2D horizontal, Texture2D vertical, byte type = 0)
            {
                this.value = value;
                this.spriteBatch = spriteBatch;
                this.rect = rect;
                this.horizontal = horizontal;
                this.vertical = vertical;
                this.type = type;
            }

            /// <summary>
            /// Updates the slider
            /// </summary>
            /// <param name="doRender">True is the slider should render</param>
            public void update(bool doRender)
            {
                MouseState m = Mouse.GetState();

                switch (type)
                {
                    case 0:
                        if (m.LeftButton == ButtonState.Pressed)
                        {
                            if (m.X > rect.X & m.X < rect.X + rect.Width)
                            {
                                if (m.Y > rect.Y & m.Y < rect.Y + rect.Height)
                                {
                                    value = ((m.X - (double)rect.X) / rect.Width);
                                }
                            }
                        }
                        break;
                    case 1:
                        if (m.LeftButton == ButtonState.Pressed)
                        {
                            if (m.X > rect.X & m.X < rect.X + rect.Width)
                            {
                                if (m.Y > rect.Y & m.Y < rect.Y + rect.Height)
                                {
                                    value = ((m.Y - (double)rect.Y) / (double)rect.Height);
                                }
                            }
                        }
                        break;
                }

                if (doRender)
                    render();
            }

            /// <summary>
            /// Renders the slider
            /// </summary>
            public void render()
            {
                switch (type)
                {
                    case 0:
                        spriteBatch.Draw(horizontal, new Rectangle(rect.X, rect.Y + rect.Height / 2 - (horizontal.Height / 2), rect.Width, horizontal.Height), Color.White);
                        spriteBatch.Draw(vertical, new Rectangle(rect.X + (int)(rect.Width * value), rect.Y, vertical.Width, rect.Height), Color.White);
                        break;
                    case 1:
                        spriteBatch.Draw(horizontal, new Rectangle(rect.X + rect.Width / 2, rect.Y, 1, rect.Height), Color.White);
                        spriteBatch.Draw(vertical, new Rectangle(rect.X, rect.Y + (int)(rect.Height * value), rect.Width, 1), Color.White);
                        break;
                }
            }

            /// <summary>
            /// Gets the value in the slider, between 0.0F and 1.0F
            /// </summary>
            /// <returns>Value</returns>
            public float getValue() { return (float)value; }

            /// <summary>
            /// Sets the slider to the specified position
            /// </summary>
            /// <param name="value">Position between 0 and 1 to set to</param>
            public void setValue(float value)
            {
                this.value = MathHelper.Clamp(value, 0, 1);
            }

            /// <summary>
            /// Sets the slider's retangle
            /// </summary>
            /// <param name="rect">The new rectangle to use</param>
            public void setRect(Rectangle rect)
            {
                this.rect = rect;
            }
        }
    }

    namespace Instances
    {
        /// <summary>
        /// Holds some basic values that can be inherited to allow easier
        /// parenting stuff
        /// </summary>
        public class Instance2D
        {
            public Vector2 position;
            public double speed, direction, HP, metaData;
        }
    }

    namespace SkeletalAnimation
    {
        public class Skeleton : IComparable
        {
            public Vector2 orgin;
            public Limb centralLimb = null;
            List<Limb> limbs = new List<Limb>();
            public string name;

            public Skeleton(Vector2 orgin, string name = "New Skeleton")
            {
                this.name = name;
                this.orgin = orgin;
                this.addCentralLimb(orgin);
            }

            public Limb addCentralLimb(Vector2 position)
            {
                limbs.Add(new Limb(this, null, null));
                return limbs.Last();
            }

            public Limb addLimb(Limb parent, Texture2D tex, double angle = 0, double length = 0)
            {
                limbs.Add(new Limb(this, parent, tex, angle, length));
                return limbs.Last();
            }

            public Limb addLimb(Limb parent, Texture2D tex, Vector2 spriteOrgin, double angle = 0, double length = 0)
            {
                limbs.Add(new Limb(this, spriteOrgin, parent, tex, angle, length));
                return limbs.Last();
            }

            public void buildWaveForLimb(int id)
            {
                Limb limb = limbs[id];

                limb.addTimerNode(0, 1000, 1.2);
                limb.addTimerNode(1.2, 2000, -1.2);
                limb.addTimerNode(-1.2, 1000, 0);
            }

            public void update(GameTime gameTime, Vector2 position)
            {
                orgin = position;

                foreach (Limb limb in limbs)
                    limb.update(gameTime);
            }

            public void draw(SpriteBatch spriteBatch, SpriteEffects effects = SpriteEffects.None)
            {
                foreach (Limb limb in limbs)
                    limb.draw(spriteBatch, effects);
            }

            public Limb getLastLimb()
            {
                return limbs.Last();
            }

            public Limb getLimb(int ID)
            {
                try
                {
                    return limbs.ElementAt(ID);
                }
                catch (ArgumentOutOfRangeException)
                {
                    return null;
                }
            }

            public List<Limb> getCollection()
            {
                return limbs;
            }

            public int getParentID(Limb parent)
            {
                return limbs.IndexOf(parent);
            }

            public int CompareTo(Object skeleton)
            {
                return name.CompareTo(((Skeleton)skeleton).name);
            }

            public void writeToStream(BinaryWriter binWriter)
            {
                binWriter.Write("Skeleton Save Version 0.0.1");
                binWriter.Write(name);
                binWriter.Write(orgin.X);
                binWriter.Write(orgin.Y);

                binWriter.Write(limbs.Count);
                foreach (Limb limb in limbs)
                {
                    limb.writeToStream(binWriter);
                }
            }

            public static Skeleton readFromStream(GraphicsDevice graphics, BinaryReader binReader)
            {
                binReader.ReadString();
                string name = binReader.ReadString();
                Vector2 orgin = new Vector2(binReader.ReadSingle(), binReader.ReadSingle());

                Skeleton skeleton = new Skeleton(orgin, name);
                skeleton.limbs.RemoveAt(0);

                int rep = binReader.ReadInt32();
                for (int i = 0; i < rep; i++)
                {
                    skeleton.limbs.Add(Limb.readFromStream(skeleton, graphics, binReader));
                }

                return skeleton;
            }

            /// <summary>
            /// A limb as part of the skeleton, can have other limbs jointed to it
            /// </summary>
            public class Limb : Joint
            {
                string name;
                double time = 0;
                int reference = 0;
                Skeleton system;
                Limb parent;
                Texture2D texture;
                List<TimerNodes> timerNodes = new List<TimerNodes>();
                Vector2 spriteOrgin = new Vector2(0, 0);

                /// <summary>
                /// Creates a new limb with the sprite orgin of (0,0)
                /// </summary>
                /// <param name="skeleton">The skeleton that the limb is being added to</param>
                /// <param name="parent">The parent limb for this limb</param>
                /// <param name="texture">The texture for this limb to draw in</param>
                /// <param name="startAngle">The initial angle of the limb in RADIANS</param>
                /// <param name="startLength">The length of the limb</param>
                public Limb(Skeleton skeleton, Limb parent, Texture2D texture, double startAngle = 0, double startLength = 0, string name = "New Limb")
                    : base(Vector2.Zero, startAngle, startLength)
                {
                    this.system = skeleton;
                    this.parent = parent;
                    this.texture = texture;
                    this.angle = startAngle;
                    this.length = startLength;
                    this.spriteOrgin = new Vector2(0, 0);
                    this.name = name;
                }

                /// <summary>
                /// Creates a new limb with an offset sprite orgin
                /// </summary>
                /// <param name="skeleton">The skeleton that this limb is being added to</param>
                /// <param name="spriteOrgin">The orgin of the sprite</param>
                /// <param name="parent">The parent limb for this limb</param>
                /// <param name="texture">The texture of the limb</param>
                /// <param name="startAngle">The initial angle of the limb, in RADIANS</param>
                /// <param name="startLength">The length of the limb</param>
                public Limb(Skeleton skeleton, Vector2 spriteOrgin, Limb parent, Texture2D texture, double startAngle = 0, double startLength = 0, string name = "New Limb")
                    : base(Vector2.Zero, startAngle, startLength)
                {
                    this.system = skeleton;
                    this.parent = parent;
                    this.texture = texture;
                    this.angle = startAngle;
                    this.length = startLength;
                    this.spriteOrgin = spriteOrgin;
                    this.name = name;
                }

                /// <summary>
                /// Adds a new timer node to the end of the animation
                /// </summary>
                /// <param name="startAngle">The angle for the limb to start at</param>
                /// <param name="timer">The number of milliseconds between this 
                /// Keyframe and the next</param>
                public void addTimerNode(double startAngle = 0, double timer = 0, double endAngle = 0)
                {
                    timerNodes.Add(new TimerNodes(startAngle, timer, endAngle));
                }

                /// <summary>
                /// Adds a new timer node to the end of the animation
                /// </summary>
                private void addTimerNode(TimerNodes timerNode)
                {
                    timerNodes.Add(timerNode);
                }

                /// <summary>
                /// Updates this limb and all it's children limbs
                /// </summary>
                /// <param name="gameTime">The GameTime of the game, used for timing<param>
                public void update(GameTime gameTime)
                {
                    if (timerNodes.Count >= 1)
                    {
                        time += gameTime.ElapsedGameTime.Milliseconds;

                        if (time >= timerNodes[reference].timerToNext)
                        {
                            time = 0;
                            reference += 1;
                            if (reference == timerNodes.Count)
                                reference = 0;
                        }

                        angle = (double)MathHelper.Lerp(
                            (float)(timerNodes[reference].startAngle),
                            (float)(timerNodes[reference].endAngle),
                            (float)(extraMath.getPercent(timerNodes[reference].timerToNext, time) / 100D)
                            );
                    }

                    endPoint = extraMath.calculateVector(position, angle, length);

                    if (parent != null)
                    {
                        position = parent.endPoint;
                    }
                    else
                    {
                        position = system.orgin;
                    }
                }

                /// <summary>
                /// Draws this limb and all it's children limbs
                /// </summary>
                /// <param name="spriteBatch">The spritebatch to draw with</param>
                /// <param name="effects">The SpriteEffects to use, defaults to none</param>
                public void draw(SpriteBatch spriteBatch, SpriteEffects effects = SpriteEffects.None)
                {
                    if (texture != null)
                        spriteBatch.Draw(texture, position, null, Color.White, -(float)(angle), spriteOrgin, new Vector2((float)(length / texture.Width), 1F), SpriteEffects.None, 1F);
                }

                /// <summary>
                /// Writes this Limb to the stream
                /// </summary>
                /// <param name="binWriter">The BinaryWriter to write to</param>
                public void writeToStream(BinaryWriter binWriter)
                {
                    //writes the name
                    binWriter.Write(name);

                    binWriter.Write(spriteOrgin.X);
                    binWriter.Write(spriteOrgin.Y);

                    if (texture != null)
                    {
                        binWriter.Write(true);
                        //save the texture into the stream
                        uint[] cols = extraMath.getTextureData(texture);

                        binWriter.Write((Int32)texture.Width);
                        binWriter.Write((Int32)texture.Height);

                        for (int i = 0; i < texture.Width * texture.Height; i++)
                        {
                            binWriter.Write((uint)cols[i]);
                        }
                    }
                    else
                        binWriter.Write(false);

                    //writes the ID of the parent limb
                    binWriter.Write(system.getParentID((Limb)parent));

                    //write the values
                    binWriter.Write(angle);
                    binWriter.Write(length);


                    //write the number of timerNodes
                    binWriter.Write(timerNodes.Count);

                    //write the timerNodes
                    foreach (TimerNodes t in timerNodes)
                    {
                        t.WriteToStream(binWriter);
                    }
                }

                /// <summary>
                /// Read the limb from the stream
                /// </summary>
                /// <param name="parent">The skeleton to add this limb to</param>
                /// <param name="limb">The limb that this is attached to</param>
                /// <param name="graphics">The GraphicsDevice to create the texture to</param>
                /// <param name="binReader">The BinaryReader to read from</param>
                /// <returns>A Limb loaded from the file</returns>
                public static Limb readFromStream(Skeleton parent, GraphicsDevice graphics, BinaryReader binReader)
                {
                    string name = binReader.ReadString();

                    Texture2D tex = null;

                    Vector2 spriteOrgin = new Vector2(binReader.ReadSingle(), binReader.ReadSingle());

                    //gets the texture if it exists
                    bool texture = binReader.ReadBoolean();
                    if (texture)
                    {
                        int width = binReader.ReadInt32();
                        int height = binReader.ReadInt32();

                        tex = new Texture2D(graphics, width, height);
                        uint[] dat = new uint[height * width];

                        for (int b = 0; b < height * width; b++)
                        {
                            dat[b] = binReader.ReadUInt32();
                        }

                        tex.SetData(dat);
                    }

                    //finds the index of the parent limb
                    int parentID = binReader.ReadInt32();

                    //creates the limb
                    Limb loadLimb = new Limb(parent, spriteOrgin, parent.getLimb(parentID), tex, binReader.ReadDouble(), binReader.ReadDouble(), name);

                    //gets it's timerNodes
                    int i = binReader.ReadInt32();
                    for (int b = 0; b < i; b++)
                    {
                        loadLimb.addTimerNode(TimerNodes.readFromStream(binReader));
                    }

                    return loadLimb;
                }

                /// <summary>
                /// Handles a timer node, which contains a start and end 
                /// angle and a time in milliseconds between them
                /// </summary>
                private class TimerNodes
                {
                    public double timerToNext;
                    public double startAngle, endAngle;

                    /// <summary>
                    /// Create a new timer node
                    /// </summary>
                    /// <param name="timer">The time in milliseconds between the start and end angle</param>
                    /// <param name="startAngle">The angle to start from</param>
                    /// <param name="endAngle">The angle to end at</param>
                    public TimerNodes(double startAngle, double timer, double endAngle)
                    {
                        this.timerToNext = timer;
                        this.startAngle = startAngle;
                        this.endAngle = endAngle;
                    }

                    /// <summary>
                    /// Writes this TimerNode to the binWriter's stream
                    /// </summary>
                    /// <param name="binWriter">The BinaryWriter to write to</param>
                    public void WriteToStream(BinaryWriter binWriter)
                    {
                        binWriter.Write(startAngle);
                        binWriter.Write(timerToNext);
                        binWriter.Write(endAngle);
                    }

                    /// <summary>
                    /// Gets the TimerNode that is written to the stream
                    /// </summary>
                    /// <param name="binReader">The BinaryReader to read from</param>
                    /// <returns>A TimerNode that is loaded from the stream</returns>
                    public static TimerNodes readFromStream(BinaryReader binReader)
                    {
                        return new TimerNodes(binReader.ReadDouble(), binReader.ReadDouble(), binReader.ReadDouble());
                    }
                }
            }

            public class Joint
            {
                public double angle, length;
                public Vector2 position;
                public Vector2 endPoint = Vector2.Zero;

                /// <summary>
                /// Creates a new joint with the specified parameters
                /// </summary>
                /// <param name="position">The joint's position</param>
                /// <param name="angle">The angle of the joint</param>
                /// <param name="length">The length of the joint</param>
                public Joint(Vector2 position, double angle = 0, double length = 0)
                {
                    this.angle = angle;
                    this.length = length;
                    this.position = position;
                    this.endPoint = position;
                }
            }
        }

        public class StickSkeleton
        {
            List<Limb> limbs = new List<Limb>();
            Vector2 position;

            public StickSkeleton()
            {
                limbs.Add(new Limb(null, 0, 0, Color.Transparent));
            }

            public int addLimb(int parentId, float angle, float length, Color color, byte type = 0)
            {
                limbs.Add(new Limb(limbs.ElementAt(parentId), angle, length, color, type));
                return limbs.Count - 1;
            }

            private int addLimb(Limb limb)
            {
                limbs.Add(limb);
                return limbs.Count - 1;
            }

            public void render(BasicEffect effect, GameTime gameTime)
            {
                limbs[0].setPos(position);

                foreach (Limb limb in limbs)
                    limb.render(effect, position, gameTime);
            }

            public void buildWaveForLimb(int index)
            {
                limbs[index].addTimerNode(MathHelper.ToRadians(270), 100, MathHelper.ToRadians(310));
                limbs[index].addTimerNode(MathHelper.ToRadians(310), 100, MathHelper.ToRadians(230));
                limbs[index].addTimerNode(MathHelper.ToRadians(230), 50, MathHelper.ToRadians(270));
            }

            public StickSkeleton getDuplicate()
            {
                StickSkeleton skele = new StickSkeleton();

                skele.limbs = this.limbs;

                for (int i = 1; i < limbs.Count; i++)
                {
                    skele.limbs[i].changeParent(skele.limbs[limbs.IndexOf(limbs[i].parent)]);
                }

                return skele;
            }

            public void setPos(Vector2 pos)
            {
                position = pos;
            }

            private class Limb
            {
                double angle, length;
                double time;
                int reference;
                byte type;
                Vector3 pos, endpos;
                Color color;
                public Limb parent;
                List<VertexPositionColor> vertLines = new List<VertexPositionColor>();
                List<TimerNodes> timerNodes = new List<TimerNodes>();

                public Limb(Limb parent, double angle, double length, Color color, byte type = 0)
                {
                    this.parent = parent;
                    this.angle = angle;
                    this.length = length;
                    this.color = color;
                    this.type = type;

                    switch (type)
                    {
                        case 0:
                            vertLines.Add(new VertexPositionColor(new Vector3(0, 0, 0), color));
                            vertLines.Add(new VertexPositionColor(new Vector3((float)length, 0, 0), color));
                            break;
                        case 1:
                            double increment = (Math.PI * 2) / 20;
                            double a = 0;
                            for (int i = 0; i <= 20; i++, a += increment)
                            {
                                vertLines.Add(new VertexPositionColor(new Vector3(
                                    extraMath.calculateVector(extraMath.calculateVectorOffset(0, length / 2), a, length / 2), 0), color));
                            }
                            break;
                    }
                }

                public void render(BasicEffect effect, Vector2 orgin, GameTime gameTime)
                {
                    if (parent != null)
                        pos = parent.endpos;

                    #region timerNodes
                    if (timerNodes.Count >= 1)
                    {
                        time += gameTime.ElapsedGameTime.Milliseconds;

                        if (time >= timerNodes[reference].timerToNext)
                        {
                            time = 0;
                            reference += 1;
                            if (reference == timerNodes.Count)
                                reference = 0;
                        }

                        angle = (double)MathHelper.Lerp(
                            (float)(timerNodes[reference].startAngle),
                            (float)(timerNodes[reference].endAngle),
                            (float)(extraMath.getPercent(timerNodes[reference].timerToNext, time) / 100D)
                            );
                    }
                    #endregion

                    endpos = new Vector3(extraMath.calculateVector(new Vector2(pos.X, pos.Y), angle, length), 0);

                    Matrix world = effect.World;
                    Matrix transform = Matrix.CreateRotationZ((float)angle) * Matrix.CreateTranslation(pos);
                    effect.World = transform;
                    effect.CurrentTechnique.Passes[0].Apply();
                    effect.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, vertLines.ToArray(), 0, vertLines.Count / 2);
                    effect.World = world;
                }

                public void changeParent(Limb newParent)
                {
                    this.parent = newParent;
                }

                public void setPos(Vector2 position)
                {
                    this.pos = new Vector3(position, 0);
                }

                public void addTimerNode(double startAngle, double timer, double endAngle)
                {
                    timerNodes.Add(new TimerNodes(startAngle, timer, endAngle));
                }

                private void addTimerNode(TimerNodes timerNode)
                {
                    timerNodes.Add(timerNode);
                }

                public void saveToStream(BinaryWriter binWriter)
                {
                    binWriter.Write(angle);
                    binWriter.Write(length);
                    binWriter.Write(color.ToVector4().X);
                    binWriter.Write(color.ToVector4().Y);
                    binWriter.Write(color.ToVector4().Z);
                    binWriter.Write(color.ToVector4().W);
                    binWriter.Write(type);

                    binWriter.Write(timerNodes.Count);
                    foreach (TimerNodes timerNode in timerNodes)
                        timerNode.WriteToStream(binWriter);
                }

                public static Limb readFromStream(Limb parent, BinaryReader binReader)
                {
                    Limb ret = new Limb(parent, binReader.ReadSingle(), binReader.ReadSingle(),
                        new Color(binReader.ReadSingle(), binReader.ReadSingle(), binReader.ReadSingle(), binReader.ReadSingle()),
                        binReader.ReadByte());

                    int tnc = binReader.ReadInt32();
                    for (int i = 0; i < tnc; i++)
                    {
                        ret.addTimerNode(TimerNodes.readFromStream(binReader));
                    }

                    return ret;
                }

                /// <summary>
                /// Handles a timer node, which contains a start and end 
                /// angle and a time in milliseconds between them
                /// </summary>
                private class TimerNodes
                {
                    public double timerToNext;
                    public double startAngle, endAngle;

                    /// <summary>
                    /// Create a new timer node
                    /// </summary>
                    /// <param name="timer">The time in milliseconds between the start and end angle</param>
                    /// <param name="startAngle">The angle to start from</param>
                    /// <param name="endAngle">The angle to end at</param>
                    public TimerNodes(double startAngle, double timer, double endAngle)
                    {
                        this.timerToNext = timer;
                        this.startAngle = startAngle;
                        this.endAngle = endAngle;
                    }

                    /// <summary>
                    /// Writes this TimerNode to the binWriter's stream
                    /// </summary>
                    /// <param name="binWriter">The BinaryWriter to write to</param>
                    public void WriteToStream(BinaryWriter binWriter)
                    {
                        binWriter.Write(startAngle);
                        binWriter.Write(timerToNext);
                        binWriter.Write(endAngle);
                    }

                    /// <summary>
                    /// Gets the TimerNode that is written to the stream
                    /// </summary>
                    /// <param name="binReader">The BinaryReader to read from</param>
                    /// <returns>A TimerNode that is loaded from the stream</returns>
                    public static TimerNodes readFromStream(BinaryReader binReader)
                    {
                        return new TimerNodes(binReader.ReadDouble(), binReader.ReadDouble(), binReader.ReadDouble());
                    }
                }
            }
        }
    }

    namespace Graphics
    {
        /// <summary>
        /// Handles a sprite animated from a tile sheet
        /// </summary>
        public class AnimatedSprite
        {
            Rectangle sourceRect;
            SpriteBatch batch;
            int frameWidth, frameHeight, hFrames, vFrames;
            Texture2D texture;
            public int frameRate;
            public int myFrame, drawFrame;

            /// <summary>
            /// Creates a new animated sprite
            /// </summary>
            /// <param name="batch">The spritebatch used to draw</param>
            /// <param name="texture">The texture sheet for the sprite</param>
            /// <param name="frameWidth">How wide one frame is</param>
            /// <param name="frameHeight">How tall one frame is</param>
            /// <param name="hFrames">The number of horizontal frames</param>
            /// <param name="vFrames">the number of vertical frames</param>
            /// <param name="ticksBtwnFrames"></param>
            public AnimatedSprite(SpriteBatch batch, Texture2D texture, int frameWidth = -1, int frameHeight = -1, int hFrames = 1, int vFrames = 1, int ticksBtwnFrames = 1)
            {
                if (frameWidth == -1)
                    frameWidth = texture.Width;
                if (frameHeight == -1)
                    frameHeight = texture.Height;
                this.batch = batch;
                this.texture = texture;
                this.frameWidth = frameWidth;
                this.frameHeight = frameHeight;
                this.hFrames = hFrames - 1;
                this.vFrames = vFrames - 1;
                this.frameRate = ticksBtwnFrames;

                sourceRect = new Rectangle(0, 0, frameWidth, frameHeight);
            }

            /// <summary>
            /// Ticks the animated sprite and draws it to the rectangle
            /// </summary>
            /// <param name="stateID">The state of the animation</param>
            /// <param name="destinationRect">The rectangle to draw to</param>
            public void tickFrames(Rectangle destinationRect, byte stateID = 0)
            {
                if (frameRate > 0)
                {
                    myFrame++;
                    if (myFrame >= frameRate)
                    {
                        myFrame = 0;
                        drawFrame++;
                        if (drawFrame > hFrames)
                        {
                            drawFrame = 0;
                        }
                    }

                    sourceRect = new Rectangle(drawFrame * frameWidth, stateID * frameHeight, frameWidth, frameHeight);
                }
                else
                    drawFrame = 0;
                batch.Draw(texture, destinationRect, sourceRect, Color.White);
            }

            /// <summary>
            /// Ticks the animated sprite and draws it to the rectangle
            /// </summary>
            /// <param name="stateID">The state of the animation</param>
            /// <param name="destinationRect">The position to draw from</param>
            public void tickFrames(Vector2 position, byte stateID = 0, float scale = 1F)
            {
                if (frameRate > 0)
                {
                    myFrame++;
                    if (myFrame >= frameRate)
                    {
                        myFrame = 0;
                        drawFrame++;
                        if (drawFrame > hFrames)
                        {
                            drawFrame = 0;
                        }
                    }

                    sourceRect = new Rectangle(drawFrame * frameWidth, stateID * frameHeight, frameWidth, frameHeight);
                }
                else
                    drawFrame = 0;
                batch.Draw(texture, position, sourceRect, Color.White, 0F, new Vector2(frameWidth / 2, frameHeight), scale, SpriteEffects.None, 0);
            }

            public AnimatedSprite getCopy()
            {
                return new AnimatedSprite(batch, texture, frameWidth, frameHeight, hFrames, vFrames, frameRate);
            }
        }

        /// <summary>
        /// A visual prop
        /// </summary>
        public class VisualProp
        {
            public bool canBeDisposed = false;
            Texture2D texture;
            string text = null;
            float stringWidth, stringHeight;
            Color color = Color.White;
            double alphaFade, redFade, blueFade, greenFade, r, g, b, a;
            Keys key;
            SpriteFont font;
            Rectangle rect;

            /// <summary>
            /// Creates a new VisualProp
            /// </summary>
            /// <param name="color">The color to draw with</param>
            /// <param name="text">The text to draw</param>
            /// <param name="texture">The texture to draw</param>
            /// <param name="font">The SpriteFont to draw the text with</param>
            public VisualProp(Color color, Rectangle rect, string text = null, Texture2D texture = null, SpriteFont font = null)
            {
                this.color = color;
                r = color.R;
                g = color.G;
                b = color.B;
                a = color.A;

                this.text = text;
                this.texture = texture;
                this.font = font;
                this.rect = rect;

                if (font != null)
                {
                    this.stringWidth = font.MeasureString(text).X;
                    this.stringHeight = font.MeasureString(text).Y;
                }
            }

            /// <summary>
            /// Makes this visualprop fade out over the requested time
            /// </summary>
            /// <param name="startAlpha">The initial alpha</param>
            /// <param name="time">The time to fully fade out</param>
            public void setFadeOut(int startAlpha, TimeSpan time)
            {
                a = startAlpha;
                alphaFade = -(startAlpha / time.TotalMilliseconds);
            }

            /// <summary>
            /// Makes this visualprop fade out over the requested time
            /// </summary>
            /// <param name="startAlpha">The initial alpha</param>
            /// <param name="time">The time to fully fade out</param>
            public void setColorFade(Color startColor, Color endColor, TimeSpan time)
            {
                color = startColor;
                a = color.A;
                r = color.R;
                g = color.G;
                b = color.B;

                alphaFade = -((a - endColor.A) / time.TotalMilliseconds);
                redFade = -((r - endColor.R) / time.TotalMilliseconds);
                greenFade = -((g - endColor.G) / time.TotalMilliseconds);
                blueFade = -((b - endColor.B) / time.TotalMilliseconds);
            }

            /// <summary>
            /// Makes it so pressing the specified key set's it's disposable state to true
            /// </summary>
            /// <param name="key">The key to dispose on</param>
            public void setRemoveOnKey(Keys key)
            {
                this.key = key;
            }

            /// <summary>
            /// Ticks this VisualProp, and draws it using the spriteBatch
            /// </summary>
            /// <param name="batch">The spritebatch to draw with</param>
            /// <param name="rect">The rectangle to draw in</param>
            public void tick(SpriteBatch batch, GameTime time)
            {
                if (key != Keys.None)
                    if (Keyboard.GetState().IsKeyDown(key))
                        canBeDisposed = true;

                r += redFade * time.ElapsedGameTime.TotalMilliseconds;
                g += greenFade * time.ElapsedGameTime.TotalMilliseconds;
                b += blueFade * time.ElapsedGameTime.TotalMilliseconds;
                a += alphaFade * time.ElapsedGameTime.TotalMilliseconds;

                if (a <= 0)
                {
                    canBeDisposed = true;
                    return;
                }
                color = Color.FromNonPremultiplied((int)r, (int)g, (int)b, (int)a);

                if (texture != null)
                    batch.Draw(texture, rect, color);
                if (text != null & font != null)
                    batch.DrawString(font, text,
                        new Vector2(rect.X + (int)((rect.Width / 2) - stringWidth / 2), rect.Y + (int)((rect.Height / 2) - stringHeight / 2)), color);
            }

            /// <summary>
            /// Ticks this VisualProp, and draws it using the spriteBatch
            /// </summary>
            /// <param name="batch">The spritebatch to draw with</param>
            /// <param name="rect">The rectangle to draw in</param>
            public void tick(SpriteBatch batch, Vector2 winPos, GameTime time)
            {
                if (key != Keys.None)
                    if (Keyboard.GetState().IsKeyDown(key))
                        canBeDisposed = true;

                r += redFade * time.ElapsedGameTime.Seconds;
                g += greenFade * time.ElapsedGameTime.Seconds;
                b += blueFade * time.ElapsedGameTime.Seconds;
                a += alphaFade * time.ElapsedGameTime.Seconds;

                if (a <= 0)
                {
                    canBeDisposed = true;
                    return;
                }
                color = Color.FromNonPremultiplied((int)r, (int)g, (int)b, (int)a);

                if (texture != null)
                    batch.Draw(texture, new Rectangle(rect.X - (int)winPos.X, rect.Y - (int)winPos.Y, rect.Width, rect.Height), color);
                if (text != null & font != null)
                    batch.DrawString(font, text,
                        new Vector2(rect.X + (int)((rect.Width / 2) - stringWidth / 2), rect.Y + (int)((rect.Height / 2) - stringHeight / 2)), color);
            }
        }

        /// <summary>
        /// Acess to some basic drawing functions
        /// </summary>
        public abstract class DrawFunctions
        {
            /// <summary>
            /// draws a line between two vectors
            /// </summary>
            /// <param name="drawTex">A blank texture to draw from</param>
            /// <param name="batch">The SpriteBatch to draw to</param>
            /// <param name="width">The width of the line</param>
            /// <param name="color">The color to draw in</param>
            /// <param name="point1">The point to draw from</param>
            /// <param name="point2">The point to draw to</param>
            public static void drawLine(Texture2D drawTex, SpriteBatch batch, float width, Color color, Vector2 point1, Vector2 point2)
            {
                float angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);
                float length = Vector2.Distance(point1, point2);

                batch.Draw(drawTex, point1, null, color, angle, Vector2.Zero, new Vector2(length, width), SpriteEffects.None, 0);
            }

            /// <summary>
            /// draws a line from a vector with a length and direction
            /// </summary>
            /// <param name="drawTex">A blank texture to draw from</param>
            /// <param name="batch">The SpriteBatch to draw to</param>
            /// <param name="width">The width of the line</param>
            /// <param name="color">The color to draw in</param>
            /// <param name="point1">The orgin point of the line</param>
            /// <param name="length">The length of the line from the orgin</param>
            /// <param name="angle">The angle in RADIANS from to orgin to draw line</param>
            public static void drawLine(Texture2D drawTex, SpriteBatch batch, float width, Color color, Vector2 point1, int length, float angle)
            {
                batch.Draw(drawTex, point1, null, color, angle, Vector2.Zero, new Vector2(length, width), SpriteEffects.None, 0);
            }

            /// <summary>
            /// draws an arrow with the base at vector1 and the tip at vector2
            /// </summary>
            /// <param name="drawTex">A blank texture to draw from</param>
            /// <param name="batch">The SpriteBatch to draw to</param>
            /// <param name="lineWidth">The width of the line to draw</param>
            /// <param name="color">The color of the arrow</param>
            /// <param name="point1">The orgin point of the arrow</param>
            /// <param name="point2">The point of the arrow</param>
            public static void drawArrow(Texture2D drawTex, SpriteBatch batch, float lineWidth, Color color, Vector2 point1, Vector2 point2)
            {
                float angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);
                float length = Vector2.Distance(point1, point2);
                drawLine(drawTex, batch, lineWidth, color, point1, point2);
                batch.Draw(drawTex, point2, null, color, angle - (float)(Math.PI / 1.15), Vector2.Zero, new Vector2(length / 10, lineWidth), SpriteEffects.None, 0);
                batch.Draw(drawTex, point2, null, color, angle + (float)(Math.PI / 1.15), Vector2.Zero, new Vector2(length / 10, lineWidth), SpriteEffects.None, 0);
            }

            /// <summary>
            /// draws the rectangle from an XNA rectangle
            /// </summary>
            /// <param name="drawTex">A blank texture to draw from</param>
            /// <param name="batch">the SpriteBatch to draw to</param>
            /// <param name="lineWidth">The width of the lines to draw</param>
            /// <param name="color">The color to draw in</param>
            /// <param name="rect">The rectangle to draw</param>
            public static void drawRectangle(Texture2D drawTex, SpriteBatch batch, float lineWidth, Color color, Rectangle rect)
            {
                Vector2 point1 = new Vector2(rect.X, rect.Y);
                Vector2 point2 = new Vector2(rect.X + rect.Width, rect.Y);
                Vector2 point3 = new Vector2(rect.X, rect.Y + rect.Height);
                Vector2 point4 = new Vector2(rect.X + rect.Width, rect.Y + rect.Height);

                drawLine(drawTex, batch, lineWidth, color, point1, point2);
                drawLine(drawTex, batch, lineWidth, color, point1, point3);
                drawLine(drawTex, batch, lineWidth, color, point2, point4);
                drawLine(drawTex, batch, lineWidth, color, new Vector2(point3.X - lineWidth, point3.Y), point4);
            }

            /// <summary>
            /// Draws a rectangle between two vectors
            /// </summary>
            /// <param name="drawTex">A blank texture to draw width</param>
            /// <param name="batch">The SpriteBatch to draw from</param>
            /// <param name="lineWidth">The width of the lines to draw</param>
            /// <param name="color">The color or the rectangle</param>
            /// <param name="vector1">The upper-left corner of the rectangle</param>
            /// <param name="vector2">The lower-right corner of the rectangle</param>
            public static void drawRectangle(Texture2D drawTex, SpriteBatch batch, float lineWidth, Color color, Vector2 vector1, Vector2 vector2)
            {
                Vector2 point1 = vector1;
                Vector2 point2 = vector2;
                Vector2 point3 = new Vector2(vector1.X, vector2.Y);
                Vector2 point4 = new Vector2(vector2.X, vector1.Y);

                drawLine(drawTex, batch, lineWidth, color, point1, point4);
                drawLine(drawTex, batch, lineWidth, color, point1, point3);
                drawLine(drawTex, batch, lineWidth, color, point2, point4);
                drawLine(drawTex, batch, lineWidth, color, new Vector2(vector1.X - lineWidth, vector2.Y),
                    new Vector2(vector2.X + lineWidth, vector2.Y));
            }

            /// <summary>
            /// Draws a rectangle to the spritebatch from the vector,
            /// length, and width
            /// </summary>
            /// <param name="drawTex">A blank texture to draw width</param>
            /// <param name="batch">The SpiteBatch to draw to</param>
            /// <param name="lineWidth"> The width of the lines to draw</param>
            /// <param name="color">The color of the rectangle</param>
            /// <param name="vector1">The point of orgin for the rectangle</param>
            /// <param name="width">The rectangle's width</param>
            /// <param name="height">The rectangle's height</param>
            public static void drawRectangle(Texture2D drawTex, SpriteBatch batch, float lineWidth, Color color, Vector2 vector1, int width, int height)
            {
                Vector2 point1 = vector1; // top left
                Vector2 point2 = new Vector2(vector1.X + width, vector1.Y + height); // bottom right
                Vector2 point3 = new Vector2(vector1.X, vector1.Y + height); // bottom left
                Vector2 point4 = new Vector2(vector1.X + width, vector1.Y); // top right

                drawLine(drawTex, batch, lineWidth, color, point1, point4);
                drawLine(drawTex, batch, lineWidth, color, point1, point3);
                drawLine(drawTex, batch, lineWidth, color, point2, point4);
                drawLine(drawTex, batch, lineWidth, color, new Vector2(vector1.X - lineWidth, vector1.Y + height),
                    new Vector2(vector1.X + width + lineWidth, vector1.Y + height));
            }

            /// <summary>
            /// Draw a circle from a center and a radius
            /// </summary>
            /// <param name="drawTex">A blank texture to draw from</param>
            /// <param name="batch">The SpriteBatch to draw to</param>
            /// <param name="center">The center of the circle</param>
            /// <param name="stepping">The number of line segments to draw</param>
            /// <param name="radius">The circle's radius</param>
            /// <param name="color">The color to draw in</param>
            public static void drawCircle(Texture2D drawTex, SpriteBatch batch, Color color, Vector2 center, int stepping, int radius)
            {
                //figure out the difference
                double increment = (Math.PI * 2) / stepping;

                //render
                double angle = 0;
                for (int i = 0; i < stepping; i++)
                {
                    //draw outline
                    drawLine(drawTex, batch, 2F, color, extraMath.calculateVector(center, angle, radius),
                        extraMath.calculateVector(center, angle + increment, radius));
                    angle += increment;
                }
            }

            /// <summary>
            /// Draws a slightly buggy triangle
            /// </summary>
            /// <param name="drawTex">A blank texture to use</param>
            /// <param name="batch">The SpriteBatch to draw to</param>
            /// <param name="drawColor">The color to draw in</param>
            /// <param name="lineWidth">The width of the lines to draw</param>
            /// <param name="point1">The top point of the triangle</param>
            /// <param name="point2">The lower left point of the triangle</param>
            /// <param name="point3">The lower right point of the triangle</param>
            public static void drawTriangle(Texture2D drawTex, SpriteBatch batch, Color drawColor, float lineWidth, Vector2 point1, Vector2 point2, Vector2 point3)
            {
                //P1\\
                //    \\
                //      \\ 
                //        \\
                //P2========P3
                drawLine(drawTex, batch, lineWidth, drawColor, point1, point2);
                drawLine(drawTex, batch, lineWidth, drawColor, point2, point3);
                drawLine(drawTex, batch, lineWidth, drawColor, point3, point1);
            }

            /// <summary>
            /// Draws the texture tiled over the rectangle, with the tiling starting at the 
            /// top - left of the rectangle
            /// </summary>
            /// <param name="batch">The SpriteBatch to draw with</param>
            /// <param name="tex">The textur to tile</param>
            /// <param name="rect">The rectangle to tile in</param>
            /// <param name="color">The color to draw the textures with</param>
            /// <param name="size">The width/height to draw each texture tile</param>
            /// <param name="offset">The offset to draw everything at</param>
            public static void drawOffsetTiledTexture(SpriteBatch batch, Texture2D tex, Rectangle rect, Color color, float size, Vector2 offset)
            {
                rect.Width = (int)(rect.Width / size);
                rect.Height = (int)(rect.Height / size);

                //end standard drawing and begin advanced drawing
                batch.End();
                batch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.LinearWrap,
                    DepthStencilState.Default, RasterizerState.CullNone);

                //hold temporary sampler state
                SamplerState temp = batch.GraphicsDevice.SamplerStates[0];
                //set the sampler state
                batch.GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
                //draw the object
                batch.Draw(tex, new Vector2(rect.X - offset.X, rect.Y - offset.Y), new Rectangle(0, 0, rect.Width, rect.Height)
                    , color, 0F, Vector2.Zero, size, SpriteEffects.None, 0F);
                //reset sampler state
                batch.GraphicsDevice.SamplerStates[0] = temp;

                //end fancy drawing and return to normal drawing
                batch.End();
                batch.Begin();
            }

            /// <summary>
            /// Draws the texture tiled over the rectangle
            /// </summary>
            /// <param name="batch">The SpriteBatch to draw with</param>
            /// <param name="tex">The textur to tile</param>
            /// <param name="rect">The rectangle to tile in</param>
            /// <param name="color">The color to draw the textures with</param>
            /// <param name="size">The width/height to draw each texture tile</param>
            /// <param name="offset">The offset to draw everything at</param>
            public static void drawTiledTexture(SpriteBatch batch, Texture2D tex, Rectangle rect, Color color, float size, Vector2 offset)
            {

                rect.Width = (int)(rect.Width / size);
                rect.Height = (int)(rect.Height / size);

                //end standard drawing and begin advanced drawing
                batch.End();
                batch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.LinearWrap,
                    DepthStencilState.Default, RasterizerState.CullNone);

                //hold temporary sampler state
                SamplerState temp = batch.GraphicsDevice.SamplerStates[0];
                //set the sampler state
                batch.GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
                //draw the object
                batch.Draw(tex, new Vector2(rect.X - offset.X, rect.Y - offset.Y), rect, color, 0F, Vector2.Zero, size, SpriteEffects.None, 0F);
                //reset sampler state
                batch.GraphicsDevice.SamplerStates[0] = temp;

                //end fancy drawing and return to normal drawing
                batch.End();
                batch.Begin();
            }

            /// <summary>
            /// Draws the texture tiled over the rectangle
            /// </summary>
            /// <param name="batch">The SpriteBatch to draw with</param>
            /// <param name="tex">The textur to tile</param>
            /// <param name="rect">The rectangle to tile in</param>
            /// <param name="color">The color to draw the textures with</param>
            /// <param name="size">The width/height to draw each texture tile</param>
            public static void drawTiledTexture(SpriteBatch batch, Texture2D tex, Rectangle rect, Color color, float size)
            {
                drawTiledTexture(batch, tex, rect, color, size, new Vector2(0, 0));
            }

            /// <summary>
            /// Draws the texture tiled over the rectangle
            /// </summary>
            /// <param name="batch">The SpriteBatch to draw with</param>
            /// <param name="tex">The textur to tile</param>
            /// <param name="rect">The rectangle to tile in</param>
            /// <param name="color">The color to draw the textures with</param>
            /// <param name="size">The width/height to draw each texture tile</param>
            public static void drawTiledTexture(SpriteBatch batch, Texture2D tex, Rectangle rect, Color color, Vector2 offset)
            {
                drawTiledTexture(batch, tex, rect, color, 1F, offset);
            }

            /// <summary>
            /// Draws the texture tiled over the rectangle
            /// </summary>
            /// <param name="batch">The SpriteBatch to draw with</param>
            /// <param name="tex">The textur to tile</param>
            /// <param name="rect">The rectangle to tile in</param>
            /// <param name="color">The color to draw the textures with</param>
            public static void drawTiledTexture(SpriteBatch batch, Texture2D tex, Rectangle rect, Color color)
            {
                drawTiledTexture(batch, tex, rect, color, 1F, new Vector2(0, 0));
            }

            public static void drawCenteredText(SpriteBatch batch, SpriteFont font, string text, Vector2 pos, Color color)
            {
                batch.DrawString(font, text, pos, color, 0F, new Vector2(
                    font.MeasureString(text).X / 2,
                    font.MeasureString(text).Y / 2),
                    1F, SpriteEffects.None, 1F);
            }

            public static void drawHorizontalCenteredText(SpriteBatch batch, SpriteFont font, string text, Vector2 pos, Color color)
            {
                batch.DrawString(font, text, pos, color, 0F, new Vector2(
                    font.MeasureString(text).X / 2, 0),
                    1F, SpriteEffects.None, 1F);
            }
        }

        /// <summary>
        /// Draws advanced shapes and such using the GraphicsDevice
        /// </summary>
        public abstract class AdvancedDrawFuncs
        {
            /// <summary>
            /// Draws a line
            /// </summary>
            /// <param name="basicEffect">The BasicEffect to draw with</param>
            /// <param name="vect">The 1st point</param>
            /// <param name="vect2">The 2nd point</param>
            /// <param name="col1">The color of point 1</param>
            /// <param name="col2">The color of point 2</param>
            public static void DrawLine(BasicEffect basicEffect, Vector2 vect, Vector2 vect2, Color col1, Color col2)
            {
                float x1 = vect.X;
                float y1 = vect.Y;
                float x2 = vect2.X;
                float y2 = vect2.Y;
                VertexPositionColor[] vertices;
                vertices = new VertexPositionColor[2];
                vertices[0].Position = new Vector3(x1, y1, 0);
                vertices[0].Color = col1;
                vertices[1].Position = new Vector3(x2, y2, 0);
                vertices[1].Color = col2;
                basicEffect.CurrentTechnique.Passes[0].Apply();
                basicEffect.GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineList, vertices, 0, 1);
            }

            /// <summary>
            /// Draws a line
            /// </summary>
            /// <param name="basicEffect">The BasicEffect to draw in</param>
            /// <param name="x1">The x co-ordinate of point 1</param>
            /// <param name="y1">The y co-ordinate of point 1</param>
            /// <param name="x2">The x co-ordinate of point 2</param>
            /// <param name="y2">The y co-ordinate of point 2</param>
            /// <param name="col1">The color of point 1</param>
            /// <param name="col2">The color of point 2</param>
            public static void DrawLine(BasicEffect basicEffect, float x1, float y1, float x2, float y2, Color col1, Color col2)
            {
                VertexPositionColor[] vertices;
                vertices = new VertexPositionColor[2];
                vertices[0].Position = new Vector3(x1, y1, 0);
                vertices[0].Color = col1;
                vertices[1].Position = new Vector3(x2, y2, 0);
                vertices[1].Color = col2;
                basicEffect.CurrentTechnique.Passes[0].Apply();
                basicEffect.GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineList, vertices, 0, 1);
            }

            /// <summary>
            /// Draws a rectangle between the 4 corners
            /// </summary>
            /// <param name="basicEffect">The BasicEffect to draw in</param>
            /// <param name="x1">The top-left x co-ordinate</param>
            /// <param name="y1">The top-left y co-ordinate</param>
            /// <param name="x2">The bottom-right x co-ordinate</param>
            /// <param name="y2">The bottom-right y co-ordinate</param>
            /// <param name="col1">The color of the outline</param>
            /// <param name="col2">The inner color</param>
            /// <param name="filled">true if the rectangle should be filled</param>
            public static void DrawRect(BasicEffect basicEffect, float x1, float y1, float x2, float y2, Color col1, Color col2, bool filled)
            {
                if (filled == true)
                {
                    drawRectFill(basicEffect, x1, y1, x2, y2, col2);
                }
                DrawLine(basicEffect, x1, y1, x2, y1, col1, col1);
                DrawLine(basicEffect, x1, y2, x2, y2, col1, col1);
                DrawLine(basicEffect, x1, y1, x1, y2, col1, col1);
                DrawLine(basicEffect, x2, y1, x2, y2, col1, col1);
            }

            /// <summary>
            /// Draws the interior of the rectangle
            /// </summary>
            /// <param name="basicEffect">The BasicEffect to draw in</param>
            /// <param name="x1">The top-left x of the rectangle</param>
            /// <param name="y1">The top-left y of the rectangle</param>
            /// <param name="x2">The bottom-right x of the rectangle</param>
            /// <param name="y2">The bottom-right y of the rectangle</param>
            /// <param name="col">The color to draw with</param>
            public static void drawRectFill(BasicEffect basicEffect, float x1, float y1, float x2, float y2, Color col)
            {
                VertexPositionColor[] vertices;
                vertices = new VertexPositionColor[6];
                vertices[0].Position = new Vector3(x1, y1, 0.5F);
                vertices[0].Color = col;
                vertices[1].Position = new Vector3(x1, y2, 0);
                vertices[1].Color = col;
                vertices[2].Position = new Vector3(x2, y2, 0);
                vertices[2].Color = col;
                vertices[3].Position = new Vector3(x2, y1, 0);
                vertices[3].Color = col;
                vertices[4].Position = new Vector3(x1, y1, 0);
                vertices[4].Color = col;
                vertices[5].Position = new Vector3(x1, y2, 0);
                vertices[5].Color = col;
                basicEffect.CurrentTechnique.Passes[0].Apply();

                short[] triangleStripIndices = new short[6] { 0, 1, 2, 3, 4, 5 };
                basicEffect.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionColor>(PrimitiveType.TriangleStrip, vertices, 0, 6, triangleStripIndices, 0, 4);
            }

            /// <summary>
            /// Draws a texture on a quad, currently broken
            /// </summary>
            /// <param name="basicEffect">The BasicEffect to draw in</param>
            /// <param name="x1">The top-left x co-ordinate</param>
            /// <param name="y1">The top-left y co-ordinate</param>
            /// <param name="x2">The bottom-right x co-ordinate</param>
            /// <param name="y2">The bottom-right y co-ordinate</param>
            /// <param name="tex">The texture to draw</param>
            /// <param name="col">The color to draw in</param>
            public static void drawTexture(BasicEffect basicEffect, float x1, float y1, float x2, float y2, Texture2D tex, Color col, Vector2 winPos)
            {
                Texture2D temp = basicEffect.Texture;
                int[] indexData = new int[6];
                indexData[0] = 0;
                indexData[1] = 2;
                indexData[2] = 3;

                indexData[3] = 0;
                indexData[4] = 1;
                indexData[5] = 2;

                VertexPositionColorTexture[] vertexData = new VertexPositionColorTexture[4];

                vertexData[0] = new VertexPositionColorTexture(new Vector3(x1, y1, 0), col, new Vector2(0, 0));
                vertexData[1] = new VertexPositionColorTexture(new Vector3(x2, y1, 0), col, new Vector2(1, 0));
                vertexData[2] = new VertexPositionColorTexture(new Vector3(x2, y2, 0), col, new Vector2(1, 1));
                vertexData[3] = new VertexPositionColorTexture(new Vector3(x1, y2, 0), col, new Vector2(0, 1));

                bool texEnabled = basicEffect.TextureEnabled;


                basicEffect.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
                basicEffect.Texture = tex;
                basicEffect.TextureEnabled = true;
                basicEffect.DiffuseColor = Color.White.ToVector3();
                basicEffect.CurrentTechnique.Passes[0].Apply();

                basicEffect.GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
                basicEffect.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertexData, 0, 4, indexData, 0, 2);
                basicEffect.GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;

                basicEffect.Texture = temp;
                basicEffect.TextureEnabled = texEnabled;
            }

            /// <summary>
            /// Draws a texture on a quad, currently broken
            /// </summary>
            /// <param name="basicEffect">The BasicEffect to draw in</param>
            /// <param name="x1">The top-left x co-ordinate</param>
            /// <param name="y1">The top-left y co-ordinate</param>
            /// <param name="x2">The bottom-right x co-ordinate</param>
            /// <param name="y2">The bottom-right y co-ordinate</param>
            /// <param name="tex">The texture to draw</param>
            /// <param name="col">The color to draw in</param>
            public static void drawTiledTexture(BasicEffect basicEffect, float x1, float y1, float x2, float y2, Texture2D tex, Color col, Vector2 winPos)
            {

                VertexPositionColorTexture[] vertexData = new VertexPositionColorTexture[6];

                vertexData[0] = new VertexPositionColorTexture(new Vector3(x1, y1, 0), col, new Vector2(x1 / tex.Width, y1 / tex.Height));
                vertexData[1] = new VertexPositionColorTexture(new Vector3(x2, y1, 0), col, new Vector2(x2 / tex.Width, y1 / tex.Height));
                vertexData[2] = new VertexPositionColorTexture(new Vector3(x2, y2, 0), col, new Vector2(x2 / tex.Width, y2 / tex.Height));
                vertexData[3] = new VertexPositionColorTexture(new Vector3(x1, y1, 0), col, new Vector2(x1 / tex.Width, y1 / tex.Height));
                vertexData[4] = new VertexPositionColorTexture(new Vector3(x1, y2, 0), col, new Vector2(x1 / tex.Width, y2 / tex.Height));
                vertexData[5] = new VertexPositionColorTexture(new Vector3(x2, y2, 0), col, new Vector2(x2 / tex.Width, y2 / tex.Height));

                bool texEnabled = basicEffect.TextureEnabled;
                Texture2D temp = basicEffect.Texture;

                basicEffect.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
                basicEffect.Texture = tex;
                basicEffect.TextureEnabled = true;
                basicEffect.DiffuseColor = Color.White.ToVector3();
                basicEffect.GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
                basicEffect.CurrentTechnique.Passes[0].Apply();

                basicEffect.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, vertexData, 0, 2);
                basicEffect.GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;

                basicEffect.Texture = temp;
                basicEffect.TextureEnabled = texEnabled;
            }

            //draw a rectangle using 2 vectors
            public static void DrawRect(BasicEffect basicEffect, Vector2 vect, Vector2 vect2, Color col1, Color col2, bool filled)
            {
                float x1 = vect.X;
                float y1 = vect.Y;
                float x2 = vect2.X;
                float y2 = vect2.Y;

                if (filled == true)
                {
                    drawRectFill(basicEffect, vect.X, vect.Y, vect2.X, vect2.Y, col2);
                }
                DrawLine(basicEffect, x1, y1, x2, y1, col1, col1);
                DrawLine(basicEffect, x1, y2, x2, y2, col1, col1);
                DrawLine(basicEffect, x1, y1, x1, y2, col1, col1);
                DrawLine(basicEffect, x2, y1, x2, y2, col1, col1);
            }

            //draw a rectangle from a rectangle
            public static void DrawRect(BasicEffect basicEffect, Rectangle rect, Color col1, Color col2, bool filled)
            {
                float x1 = rect.X;
                float y1 = rect.Y;
                float x2 = rect.X + rect.Width;
                float y2 = rect.Y + rect.Height;

                if (filled == true)
                {
                    drawRectFill(basicEffect, rect.X, rect.Y, rect.X + rect.Width, rect.Y + rect.Height, col2);
                }
                DrawLine(basicEffect, x1, y1, x2, y1, col1, col1);
                DrawLine(basicEffect, x1, y2, x2, y2, col1, col1);
                DrawLine(basicEffect, x1, y1, x1, y2, col1, col1);
                DrawLine(basicEffect, x2, y1, x2, y2, col1, col1);
            }

            //draws a triangle
            public static void drawTriangle(BasicEffect basicEffect, Vector2 pos1, Vector2 pos2, Vector2 pos3, Color col)
            {
                VertexPositionColor[] vertices;
                vertices = new VertexPositionColor[3];
                vertices[0].Position = new Vector3(pos1.X, pos1.Y, 0);
                vertices[0].Color = col;
                vertices[1].Position = new Vector3(pos2.X, pos2.Y, 0);
                vertices[1].Color = col;
                vertices[2].Position = new Vector3(pos3.X, pos3.Y, 0);
                vertices[2].Color = col;
                basicEffect.CurrentTechnique.Passes[0].Apply();
                basicEffect.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, 0, 1, VertexPositionColor.VertexDeclaration);
            }

            //WTF do you think it does?
            public static void drawCircle(BasicEffect basicEffect, Vector2 center, double radius, int stepping, Color innerColor, Color outerColor)
            {
                List<VertexPositionColor> vertices = new List<VertexPositionColor>();

                //figure out the difference
                double increment = (Math.PI * 2) / stepping;

                //render
                double angle = 0;
                for (int i = 0; i < stepping; i++, angle += increment)
                {
                    vertices.Add(new VertexPositionColor(new Vector3(extraMath.calculateVector(center, angle, radius), 0), outerColor));
                    vertices.Add(new VertexPositionColor(new Vector3(center, 0), innerColor));
                    vertices.Add(new VertexPositionColor(new Vector3(extraMath.calculateVector(center, angle + increment, radius), 0), outerColor));
                }
                basicEffect.CurrentTechnique.Passes[0].Apply();
                basicEffect.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, vertices.ToArray(), 0, vertices.Count / 3, VertexPositionColor.VertexDeclaration);
            }

            public static void drawUnfilledCircle(BasicEffect basicEffect, Vector2 center, double radius, int stepping, Color col)
            {
                List<VertexPositionColor> vertices = new List<VertexPositionColor>();

                //figure out the difference
                double increment = (Math.PI * 2) / stepping;

                //render
                double angle = 0;
                for (int i = 0; i <= stepping; i++, angle += increment)
                {
                    vertices.Add(new VertexPositionColor(new Vector3(extraMath.calculateVector(center, angle, radius), 0), col));
                }
                basicEffect.CurrentTechnique.Passes[0].Apply();
                basicEffect.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineStrip, vertices.ToArray(), 0, vertices.Count - 1, VertexPositionColor.VertexDeclaration);
            }

            //draw a simple flag :P
            public static void drawFlag(BasicEffect basicEffect, Vector2 vect, double size, Color col, int win_x, int win_y)
            {
                //bottom of shaft
                float x1 = vect.X - win_x;
                float y1 = vect.Y - win_y;

                //top of shaft
                float x2 = x1;
                float y2 = (float)(y1 - size);

                //tip of flag
                float x3 = (float)(x1 + (size / 4));
                float y3 = (float)(y2 + (size / 8));

                //where bottom of flag meets the mast
                float x4 = x1;
                float y4 = (float)(y2 + (size / 4));
                //draw flag
                DrawLine(basicEffect, x1, y1, x2, y2, Color.Red, Color.Red);
                drawTriangle(basicEffect, new Vector2(x2, y2), new Vector2(x3, y3), new Vector2(x4, y4), col);
            }

            //get a color from a gradient
            public static Color getColorFromGradient(Color color1, Color color2, int stretch, float getPos, int alpha)
            {
                float R;
                float G;
                float B;

                int R1 = color1.R;
                int G1 = color1.G;
                int B1 = color1.B;

                int R2 = color2.R;
                int G2 = color2.G;
                int B2 = color2.B;

                float Rchange;
                float Gchange;
                float Bchange;

                Rchange = (R2 - R1) / stretch;
                Bchange = (B2 - B1) / stretch;
                Gchange = (G2 - G1) / stretch;

                R = R2 - (Rchange * getPos);
                G = G2 - (Gchange * getPos);
                B = B2 - (Bchange * getPos);

                return Color.FromNonPremultiplied((int)R, (int)G, (int)B, 255);
            }

            public class Plane
            {
                VertexPositionColorTexture[] verts;
                Texture2D tex;

                public Plane(float x1, float y1, float x2, float y2, Texture2D tex, Color col)
                {
                    this.tex = tex;

                    verts = new VertexPositionColorTexture[6];
                    verts[0] = new VertexPositionColorTexture(new Vector3(x1, y1, -0.1F), col, new Vector2(x1 / tex.Width, y1 / tex.Height));
                    verts[1] = new VertexPositionColorTexture(new Vector3(x2, y1, -0.1F), col, new Vector2(x2 / tex.Width, y1 / tex.Height));
                    verts[2] = new VertexPositionColorTexture(new Vector3(x2, y2, -0.1F), col, new Vector2(x2 / tex.Width, y2 / tex.Height));
                    verts[3] = new VertexPositionColorTexture(new Vector3(x1, y1, -0.1F), col, new Vector2(x1 / tex.Width, y1 / tex.Height));
                    verts[4] = new VertexPositionColorTexture(new Vector3(x1, y2, -0.1F), col, new Vector2(x1 / tex.Width, y2 / tex.Height));
                    verts[5] = new VertexPositionColorTexture(new Vector3(x2, y2, -0.1F), col, new Vector2(x2 / tex.Width, y2 / tex.Height));
                }

                public void render(BasicEffect basicEffect, Vector2 winPos)
                {
                    basicEffect.Texture = tex;
                    basicEffect.TextureEnabled = true;
                    basicEffect.GraphicsDevice.BlendState = BlendState.AlphaBlend;
                    basicEffect.GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
                    basicEffect.CurrentTechnique.Passes[0].Apply();

                    basicEffect.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, verts, 0, 2);

                    basicEffect.GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;

                    basicEffect.TextureEnabled = false;
                    basicEffect.CurrentTechnique.Passes[0].Apply();
                }
            }
        }
    }

    namespace _3DFuncs
    {
        /// <summary>
        /// An instance of a vertexModel
        /// </summary>
        public class Instance3D
        {
            /// <summary>
            /// The position of this instance
            /// </summary>
            public Vector3 position;
            /// <summary>
            /// The orgin of this model
            /// </summary>
            public Vector3 orgin;
            /// <summary>
            /// The relative rotation angles, when the direction angles
            /// are 0
            /// </summary>
            public float relativeYaw, relativePitch, relativeRoll, yaw, pitch, roll;
            /// <summary>
            /// The world matrix
            /// </summary>
            Matrix world;
            /// <summary>
            /// The refrence to the model
            /// </summary>
            public int model, texID = 0;

            /// <summary>
            /// Sets this instance's world matrix to the specified matrix
            /// </summary>
            /// <param name="world">The matrix to set as the world</param>
            public void setMatrix(Matrix world) { this.world = world; }

            /// <summary>
            /// Get's this instance's matrix
            /// </summary>
            /// <returns>The world matrix to set the instance's matrix to</returns>
            public Matrix getMatrix() { return world; }

            /// <summary>
            /// Rebuilds the world matrix for this instance
            /// </summary>
            /// <param name="orgin">The new orgin to use</param>
            /// <param name="scale">The scale to use</param>
            public void rebuildMatrix(Vector3 orgin, float scale = 1F)
            {
                world = Matrix.CreateTranslation(orgin) * Matrix.CreateScale(scale) *
                    Matrix.CreateRotationX(relativeRoll + roll) * Matrix.CreateRotationY(relativePitch + pitch) *
                    Matrix.CreateRotationZ(relativeYaw + yaw) * Matrix.CreateTranslation(position);
                this.orgin = orgin;
            }
        }

        public class VertexModel
        {
            public Vector3 orgin, position;
            public List<VertexPositionColorTexture> verts = new List<VertexPositionColorTexture>();
            public List<VertexPositionColor> colorVerts = new List<VertexPositionColor>();
            public List<VertexPositionColor> lineList = new List<VertexPositionColor>();
            public BoundingBox boundingBox;
            public BoundingSphere boundingSphere;
            public float yaw, pitch, roll, relativeYaw, relativePitch, relativeRoll;
            public int texID = 0;

            public void render(BasicEffect basicEffect, Vector3 position, Vector3 rotation, float scale = 1F)
            {
                Matrix world = basicEffect.World;
                basicEffect.Texture = TextureManager.getTex(texID);

                if (verts.Count >= 3)
                {
                    basicEffect.World = Matrix.CreateTranslation(orgin) * Matrix.CreateScale(scale) *
                        Matrix.CreateRotationX(rotation.X + relativeRoll) * Matrix.CreateRotationY(rotation.Y + relativePitch) *
                        Matrix.CreateRotationZ(rotation.Z + relativeYaw) * Matrix.CreateTranslation(position);

                    basicEffect.CurrentTechnique.Passes[0].Apply();
                    basicEffect.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, verts.ToArray(), 0, verts.Count / 3);
                }
                if (colorVerts.Count >= 3)
                {
                    basicEffect.World = Matrix.CreateTranslation(orgin) * Matrix.CreateScale(scale) *
                        Matrix.CreateRotationX(rotation.X + relativeRoll) * Matrix.CreateRotationY(rotation.Y + relativePitch) *
                        Matrix.CreateRotationZ(rotation.Z + relativeYaw) * Matrix.CreateTranslation(position);

                    basicEffect.TextureEnabled = false;
                    basicEffect.CurrentTechnique.Passes[0].Apply();
                    basicEffect.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, colorVerts.ToArray(), 0, colorVerts.Count / 3);
                    basicEffect.TextureEnabled = true;
                }
                if (lineList.Count >= 2)
                {
                    basicEffect.World = Matrix.CreateTranslation(orgin) * Matrix.CreateScale(scale);
                    basicEffect.TextureEnabled = false;
                    basicEffect.CurrentTechnique.Passes[0].Apply();
                    basicEffect.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, lineList.ToArray(), 0, lineList.Count / 2);
                    basicEffect.TextureEnabled = true;
                }

                basicEffect.World = world;
            }

            public void render(BasicEffect basicEffect, Vector3 position, Vector2 rotation, float scale = 1F)
            {
                Matrix world = basicEffect.World;
                basicEffect.Texture = TextureManager.getTex(texID);

                basicEffect.World = Matrix.CreateTranslation(orgin) * Matrix.CreateScale(scale) *
                    Matrix.CreateRotationX(rotation.X + relativeRoll + roll) * Matrix.CreateRotationY(relativePitch + pitch) *
                    Matrix.CreateRotationZ(rotation.Y + relativeYaw + yaw) * Matrix.CreateTranslation(position);

                if (verts.Count >= 3)
                {

                    basicEffect.CurrentTechnique.Passes[0].Apply();
                    basicEffect.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, verts.ToArray(), 0, verts.Count / 3);
                }
                if (colorVerts.Count >= 3)
                {
                    basicEffect.TextureEnabled = false;
                    basicEffect.CurrentTechnique.Passes[0].Apply();
                    basicEffect.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, colorVerts.ToArray(), 0, colorVerts.Count / 3);
                    basicEffect.TextureEnabled = true;
                }
                if (lineList.Count >= 2)
                {
                    basicEffect.World = Matrix.CreateTranslation(orgin) * Matrix.CreateScale(scale);
                    basicEffect.TextureEnabled = false;
                    basicEffect.CurrentTechnique.Passes[0].Apply();
                    basicEffect.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, lineList.ToArray(), 0, lineList.Count / 2);
                    basicEffect.TextureEnabled = true;
                }

                basicEffect.World = world;
            }

            public void render(BasicEffect basicEffect, Vector3 position, Matrix world, float scale = 1F)
            {
                Matrix tempWorld = basicEffect.World;
                basicEffect.Texture = TextureManager.getTex(texID);

                basicEffect.World = world;

                if (verts.Count >= 3)
                {
                    basicEffect.CurrentTechnique.Passes[0].Apply();

                    if (verts.Count() <= Int16.MaxValue)
                    {
                        basicEffect.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, verts.ToArray(), 0, (int)(verts.Count() / 3));
                    }
                    else
                    {
                        for (int i = 0; i < verts.Count(); i += Int16.MaxValue - 1)
                        {
                            basicEffect.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, verts.ToArray(), i,
                                Math.Min((Int16.MaxValue - 1) / 3, (verts.Count() - i) / 3));
                        }
                    }
                }
                if (colorVerts.Count >= 3)
                {
                    basicEffect.TextureEnabled = false;
                    basicEffect.CurrentTechnique.Passes[0].Apply();
                    basicEffect.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, colorVerts.ToArray(), 0, colorVerts.Count / 3);
                    basicEffect.TextureEnabled = true;
                }
                if (lineList.Count >= 2)
                {
                    basicEffect.World = Matrix.CreateTranslation(orgin) * Matrix.CreateScale(scale);
                    basicEffect.TextureEnabled = false;
                    basicEffect.CurrentTechnique.Passes[0].Apply();
                    basicEffect.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, lineList.ToArray(), 0, lineList.Count / 2);
                    basicEffect.TextureEnabled = true;
                }

                basicEffect.World = tempWorld;
            }

            public void setYaw(float yaw) { this.yaw = yaw; }
            public void setPitch(float pitch) { this.pitch = pitch; }
            public void setRoll(float roll) { this.roll = roll; }
        }

        public class GlobalStaticModel
        {
            static Dictionary<string, VertexPositionColorTexture[]> verts = new Dictionary<string, VertexPositionColorTexture[]>();
            static Dictionary<string, List<VertexPositionColorTexture>> temp = new Dictionary<string, List<VertexPositionColorTexture>>();
            static Dictionary<string, Texture2D> texs = new Dictionary<string, Texture2D>();

            public static void addTexture(Texture2D texture, string name = null)
            {
                if (name != null)
                {
                    texs.Add(name, texture);
                    verts.Add(name, new VertexPositionColorTexture[] { });
                    temp.Add(name, new List<VertexPositionColorTexture>());
                }
                else
                {
                    verts.Add(getFirstOpenName(), new VertexPositionColorTexture[] { });
                    temp.Add(getFirstOpenName(), new List<VertexPositionColorTexture>());
                    texs.Add(getFirstOpenName(), texture);
                }
            }

            public static void addModel(string texName, Instance3D instance)
            {
                if (texs.Keys.Contains(texName))
                {
                    VertexModel m = ModelManager.getModel(instance.model);
                    for (int i = 0; i < m.verts.Count; i++)
                    {
                        VertexPositionColorTexture v = m.verts[i];
                        v.Position = Vector3.Transform(v.Position, instance.getMatrix());
                        temp[texName].Add(v);
                    }
                    verts[texName] = temp[texName].ToArray();
                }
            }

            public static void addModel(Instance3D instance)
            {
                string texName = TextureManager.getName(ModelManager.getModel(instance.model).texID);

                if (texs.ContainsKey(texName))
                {
                    VertexModel m = ModelManager.getModel(instance.model);
                    for (int i = 0; i < m.verts.Count; i++)
                    {
                        VertexPositionColorTexture v = m.verts[i];
                        v.Position = Vector3.Transform(v.Position, instance.getMatrix());
                        temp[texName].Add(v);
                    }
                    verts[texName] = temp[texName].ToArray();
                }
            }

            public static void finalize()
            {
                temp.Clear();
            }

            public static void render(BasicEffect effect, Matrix transform)
            {
                effect.TextureEnabled = true;
                effect.World = transform;

                foreach (string key in verts.Keys)
                {
                    if (verts[key].Count() >= 3)
                    {
                        effect.Texture = texs[key];

                        effect.CurrentTechnique.Passes[0].Apply();

                        if (verts[key].Count() <= Int16.MaxValue)
                        {
                            effect.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, verts[key], 0, verts[key].Count() / 3);
                        }
                        else
                        {
                            for (int i = 0; i < verts[key].Count(); i += Int16.MaxValue - 1)
                            {
                                effect.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, verts[key], i,
                                    Math.Min((Int16.MaxValue - 1) / 3, (verts[key].Count() - i) / 3));
                            }
                        }
                    }
                }
            }

            static string getFirstOpenName(int pointer = 0)
            {
                if (!texs.Keys.Contains("tex_" + pointer))
                    return "tex_" + pointer;
                else
                    getFirstOpenName(pointer + 1);
                return "";
            }
        }

        public class MultiModel : Instance3D
        {
            public BoundingSphere boundingSphere;
            public float yaw, pitch, roll;
            public Vector3 position;
            public List<VertexModel> models = new List<VertexModel>();

            public void render(BasicEffect basicEffect, float scale = 1F)
            {
                foreach (VertexModel model in models)
                {
                    model.relativeYaw = yaw;
                    model.relativePitch = pitch;
                    model.relativeRoll = roll;
                    model.render(basicEffect, position, new Vector3(pitch, roll, yaw), scale);
                }
            }

            public void addModel(VertexModel model)
            {
                models.Add(model);
            }
        }

        public class Line : VertexModel
        {
            public Line(Vector3 pos1, Vector3 pos2, Color color)
            {
                lineList.Add(new VertexPositionColor(pos1, color));
                lineList.Add(new VertexPositionColor(pos2, color));
            }
        }

        public class Cylinder : VertexModel
        {
            public Cylinder(Vector3 position, string skin, int radius = 1, float height = 1, int stepping = 20)
            {
                Vector2 center = new Vector2(0, 0);
                texID = TextureManager.getID(skin);
                this.orgin = position;
                //figure out the difference
                double increment = (Math.PI * 2) / stepping;

                float texOff = 10F / stepping;
                //render
                double angle = 0;
                for (int i = 0; i < stepping; i++, angle += increment)
                {
                    verts.Add(new VertexPositionColorTexture(new Vector3(extraMath.calculateVector(center, angle, radius), 0), Color.Brown, new Vector2(texOff * (float)(angle), 0)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(extraMath.calculateVector(center, angle, radius), height), Color.Brown, new Vector2(texOff * (float)(angle), -height)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(extraMath.calculateVector(center, angle + increment, radius), height), Color.Brown, new Vector2(texOff * (float)(angle + increment), -height)));

                    verts.Add(new VertexPositionColorTexture(new Vector3(extraMath.calculateVector(center, angle, radius), 0), Color.Brown, new Vector2(texOff * (float)(angle), 0)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(extraMath.calculateVector(center, angle + increment, radius), height), Color.Brown, new Vector2(texOff * (float)(angle + increment), -height)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(extraMath.calculateVector(center, angle + increment, radius), 0), Color.Brown, new Vector2(texOff * (float)(angle + increment), 0)));
                }
            }
        }

        public class ClosedCylinder : VertexModel
        {
            public ClosedCylinder(Vector3 position, string skin, float radius = 1, float height = 1, int stepping = 20)
            {
                Vector2 center = new Vector2(0, 0);
                texID = TextureManager.getID(skin);
                this.orgin = position;
                //figure out the difference
                double increment = (Math.PI * 2) / stepping;

                float texOff = 10F / stepping;
                //render
                double angle = 0;
                for (int i = 0; i < stepping; i++, angle += increment)
                {
                    verts.Add(new VertexPositionColorTexture(new Vector3(extraMath.calculateVector(center, angle, radius), 0), Color.Brown, new Vector2(texOff * (float)(angle), 0)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(extraMath.calculateVector(center, angle, radius), height), Color.Brown, new Vector2(texOff * (float)(angle), -height)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(extraMath.calculateVector(center, angle + increment, radius), height), Color.Brown, new Vector2(texOff * (float)(angle + increment), -height)));

                    verts.Add(new VertexPositionColorTexture(new Vector3(extraMath.calculateVector(center, angle, radius), 0), Color.Brown, new Vector2(texOff * (float)(angle), 0)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(extraMath.calculateVector(center, angle + increment, radius), height), Color.Brown, new Vector2(texOff * (float)(angle + increment), -height)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(extraMath.calculateVector(center, angle + increment, radius), 0), Color.Brown, new Vector2(texOff * (float)(angle + increment), 0)));

                    verts.Add(new VertexPositionColorTexture(new Vector3(extraMath.calculateVector(center, angle, radius), height), Color.Brown, extraMath.calculateVector(new Vector2(0.5F, 0.5F), angle, radius)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(center, height), Color.Brown, extraMath.calculateVector(new Vector2(0.5F, 0.5F), angle, 0)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(extraMath.calculateVector(center, angle + increment, radius), height), Color.Brown, extraMath.calculateVector(new Vector2(0.5F, 0.5F), angle + increment, radius)));

                    verts.Add(new VertexPositionColorTexture(new Vector3(extraMath.calculateVector(center, angle, radius), 0), Color.Brown, extraMath.calculateVector(new Vector2(0.5F, 0.5F), angle, radius)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(extraMath.calculateVector(center, angle + increment, radius), 0), Color.Brown, extraMath.calculateVector(new Vector2(0.5F, 0.5F), angle + increment, radius)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(center, 0), Color.Brown, extraMath.calculateVector(new Vector2(0.5F, 0.5F), angle, 0)));
                }
            }
        }

        public class BackFaceCylinder : VertexModel
        {
            public BackFaceCylinder(Vector3 position, string skin, Color color, float radius = 1, float height = 20, int stepping = 20)
            {
                Vector2 center = new Vector2(0, 0);
                texID = TextureManager.getID(skin);
                this.orgin = position;
                //figure out the difference
                double increment = (Math.PI * 2) / stepping;

                float texOff = 10F / stepping;
                //render
                double angle = 0;
                for (int i = 0; i < stepping; i++, angle += increment)
                {
                    verts.Add(new VertexPositionColorTexture(new Vector3(extraMath.calculateVector(center, angle, radius), 0), color, new Vector2(texOff * (float)(angle), 0)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(extraMath.calculateVector(center, angle, radius), height), color, new Vector2(texOff * (float)(angle), -height)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(extraMath.calculateVector(center, angle + increment, radius), height), color, new Vector2(texOff * (float)(angle + increment), -height)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(extraMath.calculateVector(center, angle, radius), 0), color, new Vector2(texOff * (float)(angle), 0)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(extraMath.calculateVector(center, angle + increment, radius), height), color, new Vector2(texOff * (float)(angle + increment), -height)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(extraMath.calculateVector(center, angle + increment, radius), 0), color, new Vector2(texOff * (float)(angle + increment), 0)));

                    verts.Add(new VertexPositionColorTexture(new Vector3(extraMath.calculateVector(center, angle, radius), 0), color, new Vector2(texOff * (float)(angle), 0)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(extraMath.calculateVector(center, angle + increment, radius), height), color, new Vector2(texOff * (float)(angle + increment), -height)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(extraMath.calculateVector(center, angle, radius), height), color, new Vector2(texOff * (float)(angle), -height)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(extraMath.calculateVector(center, angle, radius), 0), color, new Vector2(texOff * (float)(angle), 0)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(extraMath.calculateVector(center, angle + increment, radius), 0), color, new Vector2(texOff * (float)(angle + increment), 0)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(extraMath.calculateVector(center, angle + increment, radius), height), color, new Vector2(texOff * (float)(angle + increment), -height)));
                }
            }
        }

        public class CylinderFace : VertexModel
        {
            public CylinderFace(Vector3 position, string skin, Color color, float radius = 1, int stepping = 20)
            {
                Vector2 center = new Vector2(0, 0);
                texID = TextureManager.getID(skin);
                this.orgin = position;
                //figure out the difference
                double increment = (Math.PI * 2) / stepping;

                float texOff = 10F / stepping;
                //render
                double angle = 0;
                for (int i = 0; i < stepping; i++, angle += increment)
                {
                    verts.Add(new VertexPositionColorTexture(new Vector3(extraMath.calculateVector(center, angle, radius), 0), color, extraMath.calculateVector(new Vector2(0.5F, 0.5F), angle, radius)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(center, 0), color, extraMath.calculateVector(new Vector2(0.5F, 0.5F), angle, 0)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(extraMath.calculateVector(center, angle + increment, radius), 0), color, extraMath.calculateVector(new Vector2(0.5F, 0.5F), angle + increment, radius)));

                    verts.Add(new VertexPositionColorTexture(new Vector3(extraMath.calculateVector(center, angle, radius), 0), color, extraMath.calculateVector(new Vector2(0.5F, 0.5F), angle, radius)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(extraMath.calculateVector(center, angle + increment, radius), 0), color, extraMath.calculateVector(new Vector2(0.5F, 0.5F), angle + increment, radius)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(center, 0), color, extraMath.calculateVector(new Vector2(0.5F, 0.5F), angle, 0)));
                }
            }
        }

        public class Pyramid4 : VertexModel
        {
            public Pyramid4(Vector3 position, string tex, Color color, float height = 10, float width = 10, float length = 10)
            {
                this.orgin = position;
                texID = TextureManager.getID(tex);

                Vector3 PEAK = new Vector3(width / 2, length / 2, height),
                    TLF = new Vector3(0, length, 0),
                    TRF = new Vector3(width, length, 0),
                    TRB = new Vector3(width, 0, 0),
                    TLB = new Vector3(0, 0, 0);

                this.verts.Add(new VertexPositionColorTexture(PEAK, color, new Vector2(length / 2, 0)));
                this.verts.Add(new VertexPositionColorTexture(TLF, color, new Vector2(0, height / 2)));
                this.verts.Add(new VertexPositionColorTexture(TRF, color, new Vector2(length, height / 2)));
                this.verts.Add(new VertexPositionColorTexture(PEAK, color, new Vector2(length / 2, 0)));
                this.verts.Add(new VertexPositionColorTexture(TRB, color, new Vector2(length, height / 2)));
                this.verts.Add(new VertexPositionColorTexture(TLB, color, new Vector2(0, height / 2)));
                this.verts.Add(new VertexPositionColorTexture(PEAK, color, new Vector2(length / 2, 0)));
                this.verts.Add(new VertexPositionColorTexture(TLB, color, new Vector2(0, height / 2)));
                this.verts.Add(new VertexPositionColorTexture(TLF, color, new Vector2(length, height / 2)));
                this.verts.Add(new VertexPositionColorTexture(PEAK, color, new Vector2(length / 2, 0)));
                this.verts.Add(new VertexPositionColorTexture(TRF, color, new Vector2(length, height / 2)));
                this.verts.Add(new VertexPositionColorTexture(TRB, color, new Vector2(0, height / 2)));

                //bottom of roof
                this.verts.Add(new VertexPositionColorTexture(TLB, color, new Vector2(0, 0)));
                this.verts.Add(new VertexPositionColorTexture(TRF, color, new Vector2(width, length)));
                this.verts.Add(new VertexPositionColorTexture(TLF, color, new Vector2(0, length)));
                this.verts.Add(new VertexPositionColorTexture(TLB, color, new Vector2(0, 0)));
                this.verts.Add(new VertexPositionColorTexture(TRB, color, new Vector2(width, 0)));
                this.verts.Add(new VertexPositionColorTexture(TRF, color, new Vector2(width, length)));
            }
        }

        public class Fence : VertexModel
        {
            public readonly Vector3 start, end;
            public readonly float height;

            public Fence(string skin, Vector3 start, Vector3 end, float height)
            {
                position = start;
                texID = TextureManager.getID(skin);

                this.height = height;
                this.start = new Vector3(Math.Min(start.X, end.X), Math.Min(start.Y, end.Y), Math.Min(start.Z, end.Z));
                this.end = new Vector3(Math.Max(start.X, end.X), Math.Max(start.Y, end.Y), Math.Max(start.Z, end.Z));

                boundingBox = new BoundingBox(start + new Vector3(-0.5F, -0.5F, 0), end + new Vector3(0.5F, 0.5F, height));

                yaw = (float)extraMath.findAngle(new Vector2(start.X, start.Y), new Vector2(end.X, end.Y));

                int length = (int)extraMath.getDistance(new Vector2(start.X, start.Y), new Vector2(end.X, end.Y));
                int x = 0;
                for (x = 0; x < length; x += 3)
                {
                    #region posts
                    verts.Add(new VertexPositionColorTexture(new Vector3(x - 0.25F, 0.25F, 0), Color.White, new Vector2(0, 1)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(x + 0.25F, 0.25F, height - 0.25F), Color.White, new Vector2(0.25F, 0.25F)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(x - 0.25F, 0.25F, height - 0.25F), Color.White, new Vector2(0, 0.25F)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(x - 0.25F, 0.25F, 0), Color.White, new Vector2(0, 1)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(x + 0.25F, 0.25F, 0), Color.White, new Vector2(0.25F, 1)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(x + 0.25F, 0.25F, height - 0.25F), Color.White, new Vector2(0.25F, 0.25F)));

                    verts.Add(new VertexPositionColorTexture(new Vector3(x - 0.25F, -0.25F, 0), Color.White, new Vector2(0, 1)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(x - 0.25F, -0.25F, height - 0.25F), Color.White, new Vector2(0, 0.25F)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(x + 0.25F, -0.25F, height - 0.25F), Color.White, new Vector2(0.25F, 0.25F)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(x - 0.25F, -0.25F, 0), Color.White, new Vector2(0, 1)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(x + 0.25F, -0.25F, height - 0.25F), Color.White, new Vector2(0.25F, 0.25F)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(x + 0.25F, -0.25F, 0), Color.White, new Vector2(0.25F, 1)));

                    verts.Add(new VertexPositionColorTexture(new Vector3(x - 0.25F, -0.25F, 0), Color.White, new Vector2(0, 1)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(x - 0.25F, 0.25F, 0), Color.White, new Vector2(0.25F, 1)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(x - 0.25F, -0.25F, height - 0.25F), Color.White, new Vector2(0, 0.25F)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(x - 0.25F, 0.25F, 0), Color.White, new Vector2(0.25F, 1)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(x - 0.25F, 0.25F, height - 0.25F), Color.White, new Vector2(0.25F, 0.35F)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(x - 0.25F, -0.25F, height - 0.25F), Color.White, new Vector2(0, 0.25F)));

                    verts.Add(new VertexPositionColorTexture(new Vector3(x + 0.25F, -0.25F, 0), Color.White, new Vector2(0, 1)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(x + 0.25F, -0.25F, height - 0.25F), Color.White, new Vector2(0, 0.25F)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(x + 0.25F, 0.25F, 0), Color.White, new Vector2(0.25F, 1)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(x + 0.25F, 0.25F, 0), Color.White, new Vector2(0.25F, 1)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(x + 0.25F, -0.25F, height - 0.25F), Color.White, new Vector2(0, 0.25F)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(x + 0.25F, 0.25F, height - 0.25F), Color.White, new Vector2(0.25F, 0.25F)));

                    verts.Add(new VertexPositionColorTexture(new Vector3(x - 0.25F, -0.25F, height - 0.25F), Color.White, new Vector2(0F, 0.5F)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(x - 0.25F, 0.25F, height - 0.25F), Color.White, new Vector2(0.25F, 0.5F)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(x, -0.25F, height), Color.White, new Vector2(0F, 0.25F)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(x - 0.25F, 0.25F, height - 0.25F), Color.White, new Vector2(0.25F, 0.5F)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(x, 0.25F, height), Color.White, new Vector2(0.25F, 0.25F)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(x, -0.25F, height), Color.White, new Vector2(0F, 0.25F)));

                    verts.Add(new VertexPositionColorTexture(new Vector3(x + 0.25F, -0.25F, height - 0.25F), Color.White, new Vector2(0F, 0.5F)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(x, -0.25F, height), Color.White, new Vector2(0F, 0.25F)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(x + 0.25F, 0.25F, height - 0.25F), Color.White, new Vector2(0.25F, 0.5F)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(x + 0.25F, 0.25F, height - 0.25F), Color.White, new Vector2(0.25F, 0.5F)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(x, -0.25F, height), Color.White, new Vector2(0F, 0.25F)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(x, 0.25F, height), Color.White, new Vector2(0.25F, 0.25F)));

                    verts.Add(new VertexPositionColorTexture(new Vector3(x - 0.25F, 0.25F, height - 0.25F), Color.White, new Vector2(0F, 0.25F)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(x + 0.25F, 0.25F, height - 0.25F), Color.White, new Vector2(0.25F, 0.25F)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(x, 0.25F, height), Color.White, new Vector2(0.125F, 0)));

                    verts.Add(new VertexPositionColorTexture(new Vector3(x - 0.25F, -0.25F, height - 0.25F), Color.White, new Vector2(0F, 0.25F)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(x, -0.25F, height), Color.White, new Vector2(0.125F, 0)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(x + 0.25F, -0.25F, height - 0.25F), Color.White, new Vector2(0.25F, 0.25F)));
                    #endregion

                    float barLength = 2.75F;
                    if (x + barLength > length)
                    {
                        barLength = length - x;

                        #region End Post
                        verts.Add(new VertexPositionColorTexture(new Vector3(x + barLength - 0.25F, 0.25F, 0), Color.White, new Vector2(0, 1)));
                        verts.Add(new VertexPositionColorTexture(new Vector3(x + barLength + 0.25F, 0.25F, height - 0.25F), Color.White, new Vector2(0.25F, 0.25F)));
                        verts.Add(new VertexPositionColorTexture(new Vector3(x + barLength - 0.25F, 0.25F, height - 0.25F), Color.White, new Vector2(0, 0.25F)));
                        verts.Add(new VertexPositionColorTexture(new Vector3(x + barLength - 0.25F, 0.25F, 0), Color.White, new Vector2(0, 1)));
                        verts.Add(new VertexPositionColorTexture(new Vector3(x + barLength + 0.25F, 0.25F, 0), Color.White, new Vector2(0.25F, 1)));
                        verts.Add(new VertexPositionColorTexture(new Vector3(x + barLength + 0.25F, 0.25F, height - 0.25F), Color.White, new Vector2(0.25F, 0.25F)));

                        verts.Add(new VertexPositionColorTexture(new Vector3(x + barLength - 0.25F, -0.25F, 0), Color.White, new Vector2(0, 1)));
                        verts.Add(new VertexPositionColorTexture(new Vector3(x + barLength - 0.25F, -0.25F, height - 0.25F), Color.White, new Vector2(0, 0.25F)));
                        verts.Add(new VertexPositionColorTexture(new Vector3(x + barLength + 0.25F, -0.25F, height - 0.25F), Color.White, new Vector2(0.25F, 0.25F)));
                        verts.Add(new VertexPositionColorTexture(new Vector3(x + barLength - 0.25F, -0.25F, 0), Color.White, new Vector2(0, 1)));
                        verts.Add(new VertexPositionColorTexture(new Vector3(x + barLength + 0.25F, -0.25F, height - 0.25F), Color.White, new Vector2(0.25F, 0.25F)));
                        verts.Add(new VertexPositionColorTexture(new Vector3(x + barLength + 0.25F, -0.25F, 0), Color.White, new Vector2(0.25F, 1)));

                        verts.Add(new VertexPositionColorTexture(new Vector3(x + barLength - 0.25F, -0.25F, 0), Color.White, new Vector2(0, 1)));
                        verts.Add(new VertexPositionColorTexture(new Vector3(x + barLength - 0.25F, 0.25F, 0), Color.White, new Vector2(0.25F, 1)));
                        verts.Add(new VertexPositionColorTexture(new Vector3(x + barLength - 0.25F, -0.25F, height - 0.25F), Color.White, new Vector2(0, 0.25F)));
                        verts.Add(new VertexPositionColorTexture(new Vector3(x + barLength - 0.25F, 0.25F, 0), Color.White, new Vector2(0.25F, 1)));
                        verts.Add(new VertexPositionColorTexture(new Vector3(x + barLength - 0.25F, 0.25F, height - 0.25F), Color.White, new Vector2(0.25F, 0.35F)));
                        verts.Add(new VertexPositionColorTexture(new Vector3(x + barLength - 0.25F, -0.25F, height - 0.25F), Color.White, new Vector2(0, 0.25F)));

                        verts.Add(new VertexPositionColorTexture(new Vector3(x + barLength + 0.25F, -0.25F, 0), Color.White, new Vector2(0, 1)));
                        verts.Add(new VertexPositionColorTexture(new Vector3(x + barLength + 0.25F, -0.25F, height - 0.25F), Color.White, new Vector2(0, 0.25F)));
                        verts.Add(new VertexPositionColorTexture(new Vector3(x + barLength + 0.25F, 0.25F, 0), Color.White, new Vector2(0.25F, 1)));
                        verts.Add(new VertexPositionColorTexture(new Vector3(x + barLength + 0.25F, 0.25F, 0), Color.White, new Vector2(0.25F, 1)));
                        verts.Add(new VertexPositionColorTexture(new Vector3(x + barLength + 0.25F, -0.25F, height - 0.25F), Color.White, new Vector2(0, 0.25F)));
                        verts.Add(new VertexPositionColorTexture(new Vector3(x + barLength + 0.25F, 0.25F, height - 0.25F), Color.White, new Vector2(0.25F, 0.25F)));

                        verts.Add(new VertexPositionColorTexture(new Vector3(x + barLength - 0.25F, -0.25F, height - 0.25F), Color.White, new Vector2(0F, 0.5F)));
                        verts.Add(new VertexPositionColorTexture(new Vector3(x + barLength - 0.25F, 0.25F, height - 0.25F), Color.White, new Vector2(0.25F, 0.5F)));
                        verts.Add(new VertexPositionColorTexture(new Vector3(x + barLength, -0.25F, height), Color.White, new Vector2(0F, 0.25F)));
                        verts.Add(new VertexPositionColorTexture(new Vector3(x + barLength - 0.25F, 0.25F, height - 0.25F), Color.White, new Vector2(0.25F, 0.5F)));
                        verts.Add(new VertexPositionColorTexture(new Vector3(x + barLength, 0.25F, height), Color.White, new Vector2(0.25F, 0.25F)));
                        verts.Add(new VertexPositionColorTexture(new Vector3(x + barLength, -0.25F, height), Color.White, new Vector2(0F, 0.25F)));

                        verts.Add(new VertexPositionColorTexture(new Vector3(x + barLength + 0.25F, -0.25F, height - 0.25F), Color.White, new Vector2(0F, 0.5F)));
                        verts.Add(new VertexPositionColorTexture(new Vector3(x + barLength, -0.25F, height), Color.White, new Vector2(0F, 0.25F)));
                        verts.Add(new VertexPositionColorTexture(new Vector3(x + barLength + 0.25F, 0.25F, height - 0.25F), Color.White, new Vector2(0.25F, 0.5F)));
                        verts.Add(new VertexPositionColorTexture(new Vector3(x + barLength + 0.25F, 0.25F, height - 0.25F), Color.White, new Vector2(0.25F, 0.5F)));
                        verts.Add(new VertexPositionColorTexture(new Vector3(x + barLength, -0.25F, height), Color.White, new Vector2(0F, 0.25F)));
                        verts.Add(new VertexPositionColorTexture(new Vector3(x + barLength, 0.25F, height), Color.White, new Vector2(0.25F, 0.25F)));

                        verts.Add(new VertexPositionColorTexture(new Vector3(x + barLength - 0.25F, 0.25F, height - 0.25F), Color.White, new Vector2(0F, 0.25F)));
                        verts.Add(new VertexPositionColorTexture(new Vector3(x + barLength + 0.25F, 0.25F, height - 0.25F), Color.White, new Vector2(0.25F, 0.25F)));
                        verts.Add(new VertexPositionColorTexture(new Vector3(x + barLength, 0.25F, height), Color.White, new Vector2(0.125F, 0)));

                        verts.Add(new VertexPositionColorTexture(new Vector3(x + barLength - 0.25F, -0.25F, height - 0.25F), Color.White, new Vector2(0F, 0.25F)));
                        verts.Add(new VertexPositionColorTexture(new Vector3(x + barLength, -0.25F, height), Color.White, new Vector2(0.125F, 0)));
                        verts.Add(new VertexPositionColorTexture(new Vector3(x + barLength + 0.25F, -0.25F, height - 0.25F), Color.White, new Vector2(0.25F, 0.25F)));
                        #endregion
                    }

                    #region upper bars
                    verts.Add(new VertexPositionColorTexture(new Vector3(x + 0.25F, 0.2F, height - 0.3F), Color.White, new Vector2(0.25F, 0)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(x + 0.25F, 0.2F, height - 0.6F), Color.White, new Vector2(0.25F, 0.25F)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(x + barLength, 0.2F, height - 0.6F), Color.White, new Vector2(1, 0.25F)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(x + 0.25F, 0.2F, height - 0.3F), Color.White, new Vector2(0.25F, 0)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(x + barLength, 0.2F, height - 0.6F), Color.White, new Vector2(1, 0.25F)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(x + barLength, 0.2F, height - 0.3F), Color.White, new Vector2(1, 0)));

                    verts.Add(new VertexPositionColorTexture(new Vector3(x + 0.25F, -0.2F, height - 0.3F), Color.White, new Vector2(0.25F, 0)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(x + barLength, -0.2F, height - 0.6F), Color.White, new Vector2(1, 0.25F)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(x + 0.25F, -0.2F, height - 0.6F), Color.White, new Vector2(0.25F, 0.25F)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(x + 0.25F, -0.2F, height - 0.3F), Color.White, new Vector2(0.25F, 0)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(x + barLength, -0.2F, height - 0.3F), Color.White, new Vector2(1, 0)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(x + barLength, -0.2F, height - 0.6F), Color.White, new Vector2(1, 0.25F)));

                    verts.Add(new VertexPositionColorTexture(new Vector3(x + 0.25F, -0.2F, height - 0.3F), Color.White, new Vector2(0.25F, 0)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(x + barLength, 0.2F, height - 0.3F), Color.White, new Vector2(1, 0.25F)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(x + barLength, -0.2F, height - 0.3F), Color.White, new Vector2(1, 0)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(x + 0.25F, -0.2F, height - 0.3F), Color.White, new Vector2(0.25F, 0)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(x + 0.25F, 0.2F, height - 0.3F), Color.White, new Vector2(0.25F, 0.25F)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(x + barLength, 0.2F, height - 0.3F), Color.White, new Vector2(1, 0.25F)));

                    verts.Add(new VertexPositionColorTexture(new Vector3(x + 0.25F, -0.2F, height - 0.6F), Color.White, new Vector2(0.25F, 0)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(x + barLength, -0.2F, height - 0.6F), Color.White, new Vector2(1, 0)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(x + barLength, 0.2F, height - 0.6F), Color.White, new Vector2(1, 0.25F)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(x + 0.25F, -0.2F, height - 0.6F), Color.White, new Vector2(0.25F, 0)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(x + barLength, 0.2F, height - 0.6F), Color.White, new Vector2(1, 0.25F)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(x + 0.25F, 0.2F, height - 0.6F), Color.White, new Vector2(0.25F, 0.25F)));
                    #endregion

                    #region lower bars
                    verts.Add(new VertexPositionColorTexture(new Vector3(x + 0.25F, 0.2F, height - 1F), Color.White, new Vector2(0.25F, 0)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(x + 0.25F, 0.2F, height - 1.3F), Color.White, new Vector2(0.25F, 0.25F)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(x + barLength, 0.2F, height - 1.3F), Color.White, new Vector2(1, 0.25F)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(x + 0.25F, 0.2F, height - 1F), Color.White, new Vector2(0.25F, 0)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(x + barLength, 0.2F, height - 1.3F), Color.White, new Vector2(1, 0.25F)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(x + barLength, 0.2F, height - 1F), Color.White, new Vector2(1, 0)));

                    verts.Add(new VertexPositionColorTexture(new Vector3(x + 0.25F, -0.2F, height - 1F), Color.White, new Vector2(0.25F, 0)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(x + barLength, -0.2F, height - 1.3F), Color.White, new Vector2(1, 0.25F)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(x + 0.25F, -0.2F, height - 1.3F), Color.White, new Vector2(0.25F, 0.25F)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(x + 0.25F, -0.2F, height - 1F), Color.White, new Vector2(0.25F, 0)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(x + barLength, -0.2F, height - 1F), Color.White, new Vector2(1, 0)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(x + barLength, -0.2F, height - 1.3F), Color.White, new Vector2(1, 0.25F)));

                    verts.Add(new VertexPositionColorTexture(new Vector3(x + 0.25F, -0.2F, height - 1F), Color.White, new Vector2(0.25F, 0)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(x + barLength, 0.2F, height - 1F), Color.White, new Vector2(1, 0.25F)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(x + barLength, -0.2F, height - 1F), Color.White, new Vector2(1, 0)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(x + 0.25F, -0.2F, height - 1F), Color.White, new Vector2(0.25F, 0)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(x + 0.25F, 0.2F, height - 1F), Color.White, new Vector2(0.25F, 0.25F)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(x + barLength, 0.2F, height - 1F), Color.White, new Vector2(1, 0.25F)));

                    verts.Add(new VertexPositionColorTexture(new Vector3(x + 0.25F, -0.2F, height - 1.3F), Color.White, new Vector2(0.25F, 0)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(x + barLength, -0.2F, height - 1.3F), Color.White, new Vector2(1, 0)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(x + barLength, 0.2F, height - 1.3F), Color.White, new Vector2(1, 0.25F)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(x + 0.25F, -0.2F, height - 1.3F), Color.White, new Vector2(0.25F, 0)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(x + barLength, 0.2F, height - 1.3F), Color.White, new Vector2(1, 0.25F)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(x + 0.25F, 0.2F, height - 1.3F), Color.White, new Vector2(0.25F, 0.25F)));
                    #endregion
                }
            }
        }

        public class Quad : VertexModel
        {
            public Quad(string tex, Vector3 vert1, Vector3 vert2, Color color)
            {
                texID = TextureManager.getID(tex);
                this.position = vert1;
                this.orgin = new Vector3((vert2.X - vert1.X) / 2, (vert2.Y - vert1.Y) / 2, 0);

                verts.Add(new VertexPositionColorTexture(vert1, color, new Vector2(vert1.X, vert1.Y)));
                verts.Add(new VertexPositionColorTexture(new Vector3(vert1.X, vert2.Y, (vert1.Z + vert2.Z) / 2), color, new Vector2(vert1.X, vert2.Y)));
                verts.Add(new VertexPositionColorTexture(vert2, color, new Vector2(vert2.X, vert2.Y)));

                verts.Add(new VertexPositionColorTexture(vert1, color, new Vector2(vert1.X, vert1.Y)));
                verts.Add(new VertexPositionColorTexture(vert2, color, new Vector2(vert2.X, vert2.Y)));
                verts.Add(new VertexPositionColorTexture(new Vector3(vert2.X, vert1.Y, (vert1.Z + vert2.Z) / 2), color, new Vector2(vert2.X, vert1.Y)));
            }
        }

        public class Block : VertexModel
        {
            public Block(string tex, Vector3 position, Vector3 endPos, Color color, float stretch = 1F)
            {
                this.orgin = new Vector3(
                    Math.Min(position.X, endPos.X),
                    Math.Min(position.Y, endPos.Y),
                    Math.Min(position.Z, endPos.Z));
                texID = TextureManager.getID(tex);

                float minX = 0, maxX = Math.Max(position.X, endPos.X) - Math.Min(position.X, endPos.X),
                    minY = 0, maxY = Math.Max(position.Y, endPos.Y) - Math.Min(position.Y, endPos.Y),
                    minZ = 0, maxZ = Math.Max(position.Z, endPos.Z) - Math.Min(position.Z, endPos.Z);

                //boundingBox = new BoundingBox(new Vector3(minX, minY, minZ), new Vector3(maxX, maxY, maxZ));

                verts.Add(new VertexPositionColorTexture(new Vector3(minX, minY, minZ), color, new Vector2(minX / stretch, minY / stretch)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, maxY, minZ), color, new Vector2(maxX / stretch, maxY / stretch)));
                verts.Add(new VertexPositionColorTexture(new Vector3(minX, maxY, minZ), color, new Vector2(minX / stretch, maxY / stretch)));
                verts.Add(new VertexPositionColorTexture(new Vector3(minX, minY, minZ), color, new Vector2(minX / stretch, minY / stretch)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, minY, minZ), color, new Vector2(maxX / stretch, minY / stretch)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, maxY, minZ), color, new Vector2(maxX / stretch, maxY / stretch)));

                verts.Add(new VertexPositionColorTexture(new Vector3(minX, minY, maxZ), color, new Vector2(minX / stretch, minY / stretch)));
                verts.Add(new VertexPositionColorTexture(new Vector3(minX, maxY, maxZ), color, new Vector2(minX / stretch, maxY / stretch)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, maxY, maxZ), color, new Vector2(maxX / stretch, maxY / stretch)));
                verts.Add(new VertexPositionColorTexture(new Vector3(minX, minY, maxZ), color, new Vector2(minX / stretch, minY / stretch)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, maxY, maxZ), color, new Vector2(maxX / stretch, maxY / stretch)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, minY, maxZ), color, new Vector2(maxX / stretch, minY / stretch)));

                verts.Add(new VertexPositionColorTexture(new Vector3(minX, minY, minZ), color, new Vector2(0, maxZ)));
                verts.Add(new VertexPositionColorTexture(new Vector3(minX, maxY, minZ), color, new Vector2(maxY, maxZ)));
                verts.Add(new VertexPositionColorTexture(new Vector3(minX, minY, maxZ), color, new Vector2(0, 0)));
                verts.Add(new VertexPositionColorTexture(new Vector3(minX, maxY, minZ), color, new Vector2(maxY, maxZ)));
                verts.Add(new VertexPositionColorTexture(new Vector3(minX, maxY, maxZ), color, new Vector2(maxY, 0)));
                verts.Add(new VertexPositionColorTexture(new Vector3(minX, minY, maxZ), color, new Vector2(0, 0)));

                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, minY, minZ), color, new Vector2(0, maxZ)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, minY, maxZ), color, new Vector2(0, 0)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, maxY, minZ), color, new Vector2(maxY, maxZ)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, maxY, minZ), color, new Vector2(maxY, maxZ)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, minY, maxZ), color, new Vector2(0, 0)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, maxY, maxZ), color, new Vector2(maxY, 0)));

                verts.Add(new VertexPositionColorTexture(new Vector3(minX, maxY, maxZ), color, new Vector2(0, 0)));
                verts.Add(new VertexPositionColorTexture(new Vector3(minX, maxY, minZ), color, new Vector2(0, maxZ)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, maxY, minZ), color, new Vector2(maxX, maxZ)));
                verts.Add(new VertexPositionColorTexture(new Vector3(minX, maxY, maxZ), color, new Vector2(0, 0)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, maxY, minZ), color, new Vector2(maxX, maxZ)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, maxY, maxZ), color, new Vector2(maxX, 0)));

                verts.Add(new VertexPositionColorTexture(new Vector3(minX, minY, maxZ), color, new Vector2(0, 0)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, minY, minZ), color, new Vector2(maxX, maxZ)));
                verts.Add(new VertexPositionColorTexture(new Vector3(minX, minY, minZ), color, new Vector2(0, maxZ)));
                verts.Add(new VertexPositionColorTexture(new Vector3(minX, minY, maxZ), color, new Vector2(0, 0)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, minY, maxZ), color, new Vector2(maxX, 0)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, minY, minZ), color, new Vector2(maxX, maxZ)));
            }

            public Block(string tex, Vector3 position, Vector3 endPos, Color color)
            {
                this.orgin = new Vector3(
                    Math.Min(position.X, endPos.X),
                    Math.Min(position.Y, endPos.Y),
                    Math.Min(position.Z, endPos.Z));
                texID = TextureManager.getID(tex);

                float minX = 0, maxX = Math.Max(position.X, endPos.X) - Math.Min(position.X, endPos.X),
                    minY = 0, maxY = Math.Max(position.Y, endPos.Y) - Math.Min(position.Y, endPos.Y),
                    minZ = 0, maxZ = Math.Max(position.Z, endPos.Z) - Math.Min(position.Z, endPos.Z);

                //boundingBox = new BoundingBox(new Vector3(minX, minY, minZ), new Vector3(maxX, maxY, maxZ));

                verts.Add(new VertexPositionColorTexture(new Vector3(minX, minY, minZ), color, new Vector2(0, 0)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, maxY, minZ), color, new Vector2(1, 1)));
                verts.Add(new VertexPositionColorTexture(new Vector3(minX, maxY, minZ), color, new Vector2(0, 1)));
                verts.Add(new VertexPositionColorTexture(new Vector3(minX, minY, minZ), color, new Vector2(0, 0)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, minY, minZ), color, new Vector2(1, 0)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, maxY, minZ), color, new Vector2(1, 1)));

                verts.Add(new VertexPositionColorTexture(new Vector3(minX, minY, maxZ), color, new Vector2(0, 0)));
                verts.Add(new VertexPositionColorTexture(new Vector3(minX, maxY, maxZ), color, new Vector2(0, 1)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, maxY, maxZ), color, new Vector2(1, 1)));
                verts.Add(new VertexPositionColorTexture(new Vector3(minX, minY, maxZ), color, new Vector2(0, 0)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, maxY, maxZ), color, new Vector2(1, 1)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, minY, maxZ), color, new Vector2(1, 0)));

                verts.Add(new VertexPositionColorTexture(new Vector3(minX, minY, minZ), color, new Vector2(0, maxZ)));
                verts.Add(new VertexPositionColorTexture(new Vector3(minX, maxY, minZ), color, new Vector2(maxY, maxZ)));
                verts.Add(new VertexPositionColorTexture(new Vector3(minX, minY, maxZ), color, new Vector2(0, 0)));
                verts.Add(new VertexPositionColorTexture(new Vector3(minX, maxY, minZ), color, new Vector2(maxY, maxZ)));
                verts.Add(new VertexPositionColorTexture(new Vector3(minX, maxY, maxZ), color, new Vector2(maxY, 0)));
                verts.Add(new VertexPositionColorTexture(new Vector3(minX, minY, maxZ), color, new Vector2(0, 0)));

                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, minY, minZ), color, new Vector2(0, maxZ)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, minY, maxZ), color, new Vector2(0, 0)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, maxY, minZ), color, new Vector2(maxY, maxZ)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, maxY, minZ), color, new Vector2(maxY, maxZ)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, minY, maxZ), color, new Vector2(0, 0)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, maxY, maxZ), color, new Vector2(maxY, 0)));

                verts.Add(new VertexPositionColorTexture(new Vector3(minX, maxY, maxZ), color, new Vector2(0, 0)));
                verts.Add(new VertexPositionColorTexture(new Vector3(minX, maxY, minZ), color, new Vector2(0, maxZ)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, maxY, minZ), color, new Vector2(maxX, maxZ)));
                verts.Add(new VertexPositionColorTexture(new Vector3(minX, maxY, maxZ), color, new Vector2(0, 0)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, maxY, minZ), color, new Vector2(maxX, maxZ)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, maxY, maxZ), color, new Vector2(maxX, 0)));

                verts.Add(new VertexPositionColorTexture(new Vector3(minX, minY, maxZ), color, new Vector2(0, 0)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, minY, minZ), color, new Vector2(maxX, maxZ)));
                verts.Add(new VertexPositionColorTexture(new Vector3(minX, minY, minZ), color, new Vector2(0, maxZ)));
                verts.Add(new VertexPositionColorTexture(new Vector3(minX, minY, maxZ), color, new Vector2(0, 0)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, minY, maxZ), color, new Vector2(maxX, 0)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, minY, minZ), color, new Vector2(maxX, maxZ)));
            }

            public Block(string tex, BoundingBox box, Color color)
            {
                this.orgin = box.Min;
                texID = TextureManager.getID(tex);

                float minX = 0, maxX = box.Max.X - box.Min.X,
                    minY = 0, maxY = box.Max.Y - box.Min.Y,
                    minZ = 0, maxZ = box.Max.Z - box.Min.Z;

                //boundingBox = new BoundingBox(new Vector3(minX, minY, minZ), new Vector3(maxX, maxY, maxZ));

                verts.Add(new VertexPositionColorTexture(new Vector3(minX, minY, minZ), color, new Vector2(0, 0)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, maxY, minZ), color, new Vector2(1, 1)));
                verts.Add(new VertexPositionColorTexture(new Vector3(minX, maxY, minZ), color, new Vector2(0, 1)));
                verts.Add(new VertexPositionColorTexture(new Vector3(minX, minY, minZ), color, new Vector2(0, 0)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, minY, minZ), color, new Vector2(1, 0)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, maxY, minZ), color, new Vector2(1, 1)));

                verts.Add(new VertexPositionColorTexture(new Vector3(minX, minY, maxZ), color, new Vector2(0, 0)));
                verts.Add(new VertexPositionColorTexture(new Vector3(minX, maxY, maxZ), color, new Vector2(0, 1)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, maxY, maxZ), color, new Vector2(1, 1)));
                verts.Add(new VertexPositionColorTexture(new Vector3(minX, minY, maxZ), color, new Vector2(0, 0)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, maxY, maxZ), color, new Vector2(1, 1)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, minY, maxZ), color, new Vector2(1, 0)));

                verts.Add(new VertexPositionColorTexture(new Vector3(minX, minY, minZ), color, new Vector2(0, maxZ)));
                verts.Add(new VertexPositionColorTexture(new Vector3(minX, maxY, minZ), color, new Vector2(maxY, maxZ)));
                verts.Add(new VertexPositionColorTexture(new Vector3(minX, minY, maxZ), color, new Vector2(0, 0)));
                verts.Add(new VertexPositionColorTexture(new Vector3(minX, maxY, minZ), color, new Vector2(maxY, maxZ)));
                verts.Add(new VertexPositionColorTexture(new Vector3(minX, maxY, maxZ), color, new Vector2(maxY, 0)));
                verts.Add(new VertexPositionColorTexture(new Vector3(minX, minY, maxZ), color, new Vector2(0, 0)));

                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, minY, minZ), color, new Vector2(0, maxZ)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, minY, maxZ), color, new Vector2(0, 0)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, maxY, minZ), color, new Vector2(maxY, maxZ)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, maxY, minZ), color, new Vector2(maxY, maxZ)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, minY, maxZ), color, new Vector2(0, 0)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, maxY, maxZ), color, new Vector2(maxY, 0)));

                verts.Add(new VertexPositionColorTexture(new Vector3(minX, maxY, maxZ), color, new Vector2(0, 0)));
                verts.Add(new VertexPositionColorTexture(new Vector3(minX, maxY, minZ), color, new Vector2(0, maxZ)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, maxY, minZ), color, new Vector2(maxX, maxZ)));
                verts.Add(new VertexPositionColorTexture(new Vector3(minX, maxY, maxZ), color, new Vector2(0, 0)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, maxY, minZ), color, new Vector2(maxX, maxZ)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, maxY, maxZ), color, new Vector2(maxX, 0)));

                verts.Add(new VertexPositionColorTexture(new Vector3(minX, minY, maxZ), color, new Vector2(0, 0)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, minY, minZ), color, new Vector2(maxX, maxZ)));
                verts.Add(new VertexPositionColorTexture(new Vector3(minX, minY, minZ), color, new Vector2(0, maxZ)));
                verts.Add(new VertexPositionColorTexture(new Vector3(minX, minY, maxZ), color, new Vector2(0, 0)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, minY, maxZ), color, new Vector2(maxX, 0)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, minY, minZ), color, new Vector2(maxX, maxZ)));
            }
        }

        public class ChopperBlade : VertexModel
        {
            public ChopperBlade(string tex, Vector3 position, Color color, float stretch = 1F)
            {
                this.orgin = position;
                texID = TextureManager.getID(tex);

                ModelAdditions.AddOffsetCubeToModel(verts, new Vector3(-10 * stretch, -0.5F * stretch, 0),
                    new Vector3(10 * stretch, 0.5F * stretch, 0.25F * stretch), Color.White);

                ModelAdditions.AddOffsetCubeToModel(verts, new Vector3(-0.5F * stretch, -10 * stretch, 0),
                    new Vector3(0.5F * stretch, 10 * stretch, 0.25F * stretch), Color.White);

                ModelAdditions.AddOffsetCubeToModel(verts, new Vector3(-0.4F * stretch, -0.4F * stretch, -1),
                    new Vector3(0.4F * stretch, 0.4F * stretch, 0.25F * stretch), Color.White);
            }
        }

        public class Cone : VertexModel
        {
            public Cone(Vector3 position, string skin, Color color, int radius = 1, int height = 20, int stepping = 20)
            {
                Vector2 center = new Vector2(0, 0);
                texID = TextureManager.getID(skin);
                this.orgin = position;
                //figure out the difference
                double increment = (Math.PI * 2) / stepping;

                float texOff = 10F / stepping;
                //render
                double angle = 0;
                for (int i = 0; i < stepping; i++, angle += increment)
                {
                    verts.Add(new VertexPositionColorTexture(new Vector3(extraMath.calculateVector(center, angle, radius), 0), color, extraMath.calculateVector(new Vector2(0.5F, 0.5F), angle, radius)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(center, height), color, extraMath.calculateVector(new Vector2(0.5F, 0.5F), angle, 0)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(extraMath.calculateVector(center, angle + increment, radius), 0), color, extraMath.calculateVector(new Vector2(0.5F, 0.5F), angle + increment, radius)));

                    verts.Add(new VertexPositionColorTexture(new Vector3(extraMath.calculateVector(center, angle, radius), 0), color, extraMath.calculateVector(new Vector2(0.5F, 0.5F), angle, radius)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(extraMath.calculateVector(center, angle + increment, radius), 0), color, extraMath.calculateVector(new Vector2(0.5F, 0.5F), angle + increment, radius)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(center, 0), color, extraMath.calculateVector(new Vector2(0.5F, 0.5F), angle, 0)));
                }
            }
        }

        public class Sphere : VertexModel
        {
            public Sphere(Vector3 pos, Color color, float radius = 1, float stepping = 90)
            {
                boundingSphere = new BoundingSphere(pos, radius);

                Vector3 rad = new Vector3(radius, 0, 0);

                for (int x = 0; x < stepping; x++) //90 circles, difference between each is 4 degrees
                {
                    float difx = 360.0f / (float)stepping;
                    for (int y = 0; y < stepping; y++) //90 veritces, difference between each is 4 degrees 
                    {
                        float dify = 360.0f / (float)stepping;
                        Matrix zrot = Matrix.CreateRotationZ(MathHelper.ToRadians(y * dify)); //rotate vertex around z
                        Matrix yrot = Matrix.CreateRotationY(MathHelper.ToRadians(x * difx)); //rotate circle around y
                        Vector3 point = Vector3.Transform(Vector3.Transform(rad, zrot), yrot);//transformation
                        Matrix zrot2 = Matrix.CreateRotationZ(MathHelper.ToRadians((y + 1) * dify)); //rotate vertex around z
                        Matrix yrot2 = Matrix.CreateRotationY(MathHelper.ToRadians((x + 1) * difx)); //rotate circle around y
                        Vector3 point2 = Vector3.Transform(Vector3.Transform(rad, zrot2), yrot2);//transformation
                        Matrix zrot3 = Matrix.CreateRotationZ(MathHelper.ToRadians((y + 1) * dify)); //rotate vertex around z
                        Matrix yrot3 = Matrix.CreateRotationY(MathHelper.ToRadians(x * difx)); //rotate circle around y
                        Vector3 point3 = Vector3.Transform(Vector3.Transform(rad, zrot3), yrot3);//transformation
                        Matrix zrot4 = Matrix.CreateRotationZ(MathHelper.ToRadians(y * dify)); //rotate vertex around z
                        Matrix yrot4 = Matrix.CreateRotationY(MathHelper.ToRadians((x + 1) * difx)); //rotate circle around y
                        Vector3 point4 = Vector3.Transform(Vector3.Transform(rad, zrot4), yrot4);//transformation

                        colorVerts.Add(new VertexPositionColor(point + pos, color));
                        colorVerts.Add(new VertexPositionColor(point2 + pos, color));
                        colorVerts.Add(new VertexPositionColor(point3 + pos, color));

                        colorVerts.Add(new VertexPositionColor(point + pos, color));
                        colorVerts.Add(new VertexPositionColor(point4 + pos, color));
                        colorVerts.Add(new VertexPositionColor(point2 + pos, color));
                    }
                }
            }
        }

        public class BoundingPlane
        {
            BoundingBox box;
            Plane plane;

            public BoundingPlane(BoundingBox box, Plane plane)
            {
                this.box = box;
                this.plane = plane;
            }

            public float? Collides(Ray ray)
            {
                if (ray.Intersects(box) != null)
                {
                    ray.Position -= new Vector3(box.Min.X, box.Min.Y, 0);
                    float? val = ray.Intersects(plane);
                    return val;
                }
                return null;
            }
        }

        public static class ModelAdditions
        {
            public static void AddCubeToModel(List<VertexPositionColorTexture> verts, Vector3 position, Vector3 endPos, Color color, float stretch = 1F)
            {
                float minX = 0, maxX = Math.Max(position.X, endPos.X) - Math.Min(position.X, endPos.X),
                    minY = 0, maxY = Math.Max(position.Y, endPos.Y) - Math.Min(position.Y, endPos.Y),
                    minZ = 0, maxZ = Math.Max(position.Z, endPos.Z) - Math.Min(position.Z, endPos.Z);

                verts.Add(new VertexPositionColorTexture(new Vector3(minX, minY, minZ), color, new Vector2(minX / stretch, minY / stretch)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, maxY, minZ), color, new Vector2(maxX / stretch, maxY / stretch)));
                verts.Add(new VertexPositionColorTexture(new Vector3(minX, maxY, minZ), color, new Vector2(minX / stretch, maxY / stretch)));
                verts.Add(new VertexPositionColorTexture(new Vector3(minX, minY, minZ), color, new Vector2(minX / stretch, minY / stretch)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, minY, minZ), color, new Vector2(maxX / stretch, minY / stretch)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, maxY, minZ), color, new Vector2(maxX / stretch, maxY / stretch)));

                verts.Add(new VertexPositionColorTexture(new Vector3(minX, minY, maxZ), color, new Vector2(minX / stretch, minY / stretch)));
                verts.Add(new VertexPositionColorTexture(new Vector3(minX, maxY, maxZ), color, new Vector2(minX / stretch, maxY / stretch)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, maxY, maxZ), color, new Vector2(maxX / stretch, maxY / stretch)));
                verts.Add(new VertexPositionColorTexture(new Vector3(minX, minY, maxZ), color, new Vector2(minX / stretch, minY / stretch)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, maxY, maxZ), color, new Vector2(maxX / stretch, maxY / stretch)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, minY, maxZ), color, new Vector2(maxX / stretch, minY / stretch)));

                verts.Add(new VertexPositionColorTexture(new Vector3(minX, minY, minZ), color, new Vector2(0, maxZ)));
                verts.Add(new VertexPositionColorTexture(new Vector3(minX, maxY, minZ), color, new Vector2(maxY, maxZ)));
                verts.Add(new VertexPositionColorTexture(new Vector3(minX, minY, maxZ), color, new Vector2(0, 0)));
                verts.Add(new VertexPositionColorTexture(new Vector3(minX, maxY, minZ), color, new Vector2(maxY, maxZ)));
                verts.Add(new VertexPositionColorTexture(new Vector3(minX, maxY, maxZ), color, new Vector2(maxY, 0)));
                verts.Add(new VertexPositionColorTexture(new Vector3(minX, minY, maxZ), color, new Vector2(0, 0)));

                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, minY, minZ), color, new Vector2(0, maxZ)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, minY, maxZ), color, new Vector2(0, 0)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, maxY, minZ), color, new Vector2(maxY, maxZ)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, maxY, minZ), color, new Vector2(maxY, maxZ)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, minY, maxZ), color, new Vector2(0, 0)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, maxY, maxZ), color, new Vector2(maxY, 0)));

                verts.Add(new VertexPositionColorTexture(new Vector3(minX, maxY, maxZ), color, new Vector2(0, 0)));
                verts.Add(new VertexPositionColorTexture(new Vector3(minX, maxY, minZ), color, new Vector2(0, maxZ)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, maxY, minZ), color, new Vector2(maxX, maxZ)));
                verts.Add(new VertexPositionColorTexture(new Vector3(minX, maxY, maxZ), color, new Vector2(0, 0)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, maxY, minZ), color, new Vector2(maxX, maxZ)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, maxY, maxZ), color, new Vector2(maxX, 0)));

                verts.Add(new VertexPositionColorTexture(new Vector3(minX, minY, maxZ), color, new Vector2(0, 0)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, minY, minZ), color, new Vector2(maxX, maxZ)));
                verts.Add(new VertexPositionColorTexture(new Vector3(minX, minY, minZ), color, new Vector2(0, maxZ)));
                verts.Add(new VertexPositionColorTexture(new Vector3(minX, minY, maxZ), color, new Vector2(0, 0)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, minY, maxZ), color, new Vector2(maxX, 0)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, minY, minZ), color, new Vector2(maxX, maxZ)));

            }

            public static void AddOffsetCubeToModel(List<VertexPositionColorTexture> verts, Vector3 position, Vector3 endPos, Color color, float stretch = 1F)
            {
                float minX = Math.Min(position.X, endPos.X), maxX = Math.Max(position.X, endPos.X),
                    minY = Math.Min(position.Y, endPos.Y), maxY = Math.Max(position.Y, endPos.Y),
                    minZ = Math.Min(position.Z, endPos.Z), maxZ = Math.Max(position.Z, endPos.Z);

                verts.Add(new VertexPositionColorTexture(new Vector3(minX, minY, minZ), color, new Vector2(minX / stretch, minY / stretch)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, maxY, minZ), color, new Vector2(maxX / stretch, maxY / stretch)));
                verts.Add(new VertexPositionColorTexture(new Vector3(minX, maxY, minZ), color, new Vector2(minX / stretch, maxY / stretch)));
                verts.Add(new VertexPositionColorTexture(new Vector3(minX, minY, minZ), color, new Vector2(minX / stretch, minY / stretch)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, minY, minZ), color, new Vector2(maxX / stretch, minY / stretch)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, maxY, minZ), color, new Vector2(maxX / stretch, maxY / stretch)));

                verts.Add(new VertexPositionColorTexture(new Vector3(minX, minY, maxZ), color, new Vector2(minX / stretch, minY / stretch)));
                verts.Add(new VertexPositionColorTexture(new Vector3(minX, maxY, maxZ), color, new Vector2(minX / stretch, maxY / stretch)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, maxY, maxZ), color, new Vector2(maxX / stretch, maxY / stretch)));
                verts.Add(new VertexPositionColorTexture(new Vector3(minX, minY, maxZ), color, new Vector2(minX / stretch, minY / stretch)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, maxY, maxZ), color, new Vector2(maxX / stretch, maxY / stretch)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, minY, maxZ), color, new Vector2(maxX / stretch, minY / stretch)));

                verts.Add(new VertexPositionColorTexture(new Vector3(minX, minY, minZ), color, new Vector2(0, maxZ)));
                verts.Add(new VertexPositionColorTexture(new Vector3(minX, maxY, minZ), color, new Vector2(maxY, maxZ)));
                verts.Add(new VertexPositionColorTexture(new Vector3(minX, minY, maxZ), color, new Vector2(0, 0)));
                verts.Add(new VertexPositionColorTexture(new Vector3(minX, maxY, minZ), color, new Vector2(maxY, maxZ)));
                verts.Add(new VertexPositionColorTexture(new Vector3(minX, maxY, maxZ), color, new Vector2(maxY, 0)));
                verts.Add(new VertexPositionColorTexture(new Vector3(minX, minY, maxZ), color, new Vector2(0, 0)));

                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, minY, minZ), color, new Vector2(0, maxZ)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, minY, maxZ), color, new Vector2(0, 0)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, maxY, minZ), color, new Vector2(maxY, maxZ)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, maxY, minZ), color, new Vector2(maxY, maxZ)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, minY, maxZ), color, new Vector2(0, 0)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, maxY, maxZ), color, new Vector2(maxY, 0)));

                verts.Add(new VertexPositionColorTexture(new Vector3(minX, maxY, maxZ), color, new Vector2(0, 0)));
                verts.Add(new VertexPositionColorTexture(new Vector3(minX, maxY, minZ), color, new Vector2(0, maxZ)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, maxY, minZ), color, new Vector2(maxX, maxZ)));
                verts.Add(new VertexPositionColorTexture(new Vector3(minX, maxY, maxZ), color, new Vector2(0, 0)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, maxY, minZ), color, new Vector2(maxX, maxZ)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, maxY, maxZ), color, new Vector2(maxX, 0)));

                verts.Add(new VertexPositionColorTexture(new Vector3(minX, minY, maxZ), color, new Vector2(0, 0)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, minY, minZ), color, new Vector2(maxX, maxZ)));
                verts.Add(new VertexPositionColorTexture(new Vector3(minX, minY, minZ), color, new Vector2(0, maxZ)));
                verts.Add(new VertexPositionColorTexture(new Vector3(minX, minY, maxZ), color, new Vector2(0, 0)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, minY, maxZ), color, new Vector2(maxX, 0)));
                verts.Add(new VertexPositionColorTexture(new Vector3(maxX, minY, minZ), color, new Vector2(maxX, maxZ)));

            }

            public static void AddOffsetCubeToModel(List<VertexPositionColor> verts, Vector3 position, Vector3 endPos, Color color, float stretch = 1F)
            {
                float minX = Math.Min(position.X, endPos.X), maxX = Math.Max(position.X, endPos.X),
                    minY = Math.Min(position.Y, endPos.Y), maxY = Math.Max(position.Y, endPos.Y),
                    minZ = Math.Min(position.Z, endPos.Z), maxZ = Math.Max(position.Z, endPos.Z);

                verts.Add(new VertexPositionColor(new Vector3(minX, minY, minZ), color));
                verts.Add(new VertexPositionColor(new Vector3(maxX, maxY, minZ), color));
                verts.Add(new VertexPositionColor(new Vector3(minX, maxY, minZ), color));
                verts.Add(new VertexPositionColor(new Vector3(minX, minY, minZ), color));
                verts.Add(new VertexPositionColor(new Vector3(maxX, minY, minZ), color));
                verts.Add(new VertexPositionColor(new Vector3(maxX, maxY, minZ), color));

                verts.Add(new VertexPositionColor(new Vector3(minX, minY, maxZ), color));
                verts.Add(new VertexPositionColor(new Vector3(minX, maxY, maxZ), color));
                verts.Add(new VertexPositionColor(new Vector3(maxX, maxY, maxZ), color));
                verts.Add(new VertexPositionColor(new Vector3(minX, minY, maxZ), color));
                verts.Add(new VertexPositionColor(new Vector3(maxX, maxY, maxZ), color));
                verts.Add(new VertexPositionColor(new Vector3(maxX, minY, maxZ), color));

                verts.Add(new VertexPositionColor(new Vector3(minX, minY, minZ), color));
                verts.Add(new VertexPositionColor(new Vector3(minX, maxY, minZ), color));
                verts.Add(new VertexPositionColor(new Vector3(minX, minY, maxZ), color));
                verts.Add(new VertexPositionColor(new Vector3(minX, maxY, minZ), color));
                verts.Add(new VertexPositionColor(new Vector3(minX, maxY, maxZ), color));
                verts.Add(new VertexPositionColor(new Vector3(minX, minY, maxZ), color));

                verts.Add(new VertexPositionColor(new Vector3(maxX, minY, minZ), color));
                verts.Add(new VertexPositionColor(new Vector3(maxX, minY, maxZ), color));
                verts.Add(new VertexPositionColor(new Vector3(maxX, maxY, minZ), color));
                verts.Add(new VertexPositionColor(new Vector3(maxX, maxY, minZ), color));
                verts.Add(new VertexPositionColor(new Vector3(maxX, minY, maxZ), color));
                verts.Add(new VertexPositionColor(new Vector3(maxX, maxY, maxZ), color));

                verts.Add(new VertexPositionColor(new Vector3(minX, maxY, maxZ), color));
                verts.Add(new VertexPositionColor(new Vector3(minX, maxY, minZ), color));
                verts.Add(new VertexPositionColor(new Vector3(maxX, maxY, minZ), color));
                verts.Add(new VertexPositionColor(new Vector3(minX, maxY, maxZ), color));
                verts.Add(new VertexPositionColor(new Vector3(maxX, maxY, minZ), color));
                verts.Add(new VertexPositionColor(new Vector3(maxX, maxY, maxZ), color));

                verts.Add(new VertexPositionColor(new Vector3(minX, minY, maxZ), color));
                verts.Add(new VertexPositionColor(new Vector3(maxX, minY, minZ), color));
                verts.Add(new VertexPositionColor(new Vector3(minX, minY, minZ), color));
                verts.Add(new VertexPositionColor(new Vector3(minX, minY, maxZ), color));
                verts.Add(new VertexPositionColor(new Vector3(maxX, minY, maxZ), color));
                verts.Add(new VertexPositionColor(new Vector3(maxX, minY, minZ), color));

            }

            public static void AddCylinderToModel(List<VertexPositionColorTexture> verts, Vector3 position, Texture2D tex, Color color, float radius = 1F, float height = 1F, int stepping = 20)
            {
                Vector2 center = new Vector2(0, 0);

                //figure out the difference
                double increment = (Math.PI * 2) / stepping;

                float texOff = 10F / stepping;
                //render
                double angle = 0;
                for (int i = 0; i < stepping; i++, angle += increment)
                {
                    verts.Add(new VertexPositionColorTexture(new Vector3(extraMath.calculateVector(center, angle, radius), 0) + position, color, new Vector2(texOff * (float)(angle), 0)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(extraMath.calculateVector(center, angle, radius), height) + position, color, new Vector2(texOff * (float)(angle), -height)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(extraMath.calculateVector(center, angle + increment, radius), height) + position, color, new Vector2(texOff * (float)(angle + increment), -height)));

                    verts.Add(new VertexPositionColorTexture(new Vector3(extraMath.calculateVector(center, angle, radius), 0) + position, color, new Vector2(texOff * (float)(angle), 0)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(extraMath.calculateVector(center, angle + increment, radius), height) + position, color, new Vector2(texOff * (float)(angle + increment), -height)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(extraMath.calculateVector(center, angle + increment, radius), 0) + position, color, new Vector2(texOff * (float)(angle + increment), 0)));

                    verts.Add(new VertexPositionColorTexture(new Vector3(extraMath.calculateVector(center, angle, radius), height) + position, color, extraMath.calculateVector(new Vector2(0.5F, 0.5F), angle, radius)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(center, height) + position, color, extraMath.calculateVector(new Vector2(0.5F, 0.5F), angle, 0)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(extraMath.calculateVector(center, angle + increment, radius), height) + position, color, extraMath.calculateVector(new Vector2(0.5F, 0.5F), angle + increment, radius)));

                    verts.Add(new VertexPositionColorTexture(new Vector3(extraMath.calculateVector(center, angle, radius), 0) + position, color, extraMath.calculateVector(new Vector2(0.5F, 0.5F), angle, radius)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(extraMath.calculateVector(center, angle + increment, radius), 0) + position, color, extraMath.calculateVector(new Vector2(0.5F, 0.5F), angle + increment, radius)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(center, 0) + position, color, extraMath.calculateVector(new Vector2(0.5F, 0.5F), angle, 0)));
                }
            }

            public static void AddCylinderToModel(List<VertexPositionColor> verts, Vector3 position, Color color, float radius = 1F, float height = 1F, int stepping = 20)
            {
                Vector2 center = new Vector2(0, 0);

                //figure out the difference
                double increment = (Math.PI * 2) / stepping;

                float texOff = 10F / stepping;
                //render
                double angle = 0;
                for (int i = 0; i < stepping; i++, angle += increment)
                {
                    verts.Add(new VertexPositionColor(new Vector3(extraMath.calculateVector(center, angle, radius), 0) + position, color));
                    verts.Add(new VertexPositionColor(new Vector3(extraMath.calculateVector(center, angle, radius), height) + position, color));
                    verts.Add(new VertexPositionColor(new Vector3(extraMath.calculateVector(center, angle + increment, radius), height) + position, color));

                    verts.Add(new VertexPositionColor(new Vector3(extraMath.calculateVector(center, angle, radius), 0) + position, color));
                    verts.Add(new VertexPositionColor(new Vector3(extraMath.calculateVector(center, angle + increment, radius), height) + position, color));
                    verts.Add(new VertexPositionColor(new Vector3(extraMath.calculateVector(center, angle + increment, radius), 0) + position, color));

                    verts.Add(new VertexPositionColor(new Vector3(extraMath.calculateVector(center, angle, radius), height) + position, color));
                    verts.Add(new VertexPositionColor(new Vector3(center, height) + position, color));
                    verts.Add(new VertexPositionColor(new Vector3(extraMath.calculateVector(center, angle + increment, radius), height) + position, color));

                    verts.Add(new VertexPositionColor(new Vector3(extraMath.calculateVector(center, angle, radius), 0) + position, color));
                    verts.Add(new VertexPositionColor(new Vector3(extraMath.calculateVector(center, angle + increment, radius), 0) + position, color));
                    verts.Add(new VertexPositionColor(new Vector3(center, 0) + position, color));
                }
            }

            public static void addSphereToModel(List<VertexPositionColor> verts, Vector3 pos, Color color, float radius = 1, int stepping = 90)
            {
                Vector3 rad = new Vector3(radius, 0, 0);

                for (int x = 0; x < stepping; x++) //90 circles, difference between each is 4 degrees
                {
                    float difx = 360.0f / (float)stepping;
                    for (int y = 0; y < stepping; y++) //90 veritces, difference between each is 4 degrees 
                    {
                        float dify = 360.0f / (float)stepping;
                        Matrix zrot = Matrix.CreateRotationZ(MathHelper.ToRadians(y * dify)); //rotate vertex around z
                        Matrix yrot = Matrix.CreateRotationY(MathHelper.ToRadians(x * difx)); //rotate circle around y
                        Vector3 point = Vector3.Transform(Vector3.Transform(rad, zrot), yrot);//transformation
                        Matrix zrot2 = Matrix.CreateRotationZ(MathHelper.ToRadians((y + 1) * dify)); //rotate vertex around z
                        Matrix yrot2 = Matrix.CreateRotationY(MathHelper.ToRadians((x + 1) * difx)); //rotate circle around y
                        Vector3 point2 = Vector3.Transform(Vector3.Transform(rad, zrot2), yrot2);//transformation
                        Matrix zrot3 = Matrix.CreateRotationZ(MathHelper.ToRadians((y + 1) * dify)); //rotate vertex around z
                        Matrix yrot3 = Matrix.CreateRotationY(MathHelper.ToRadians(x * difx)); //rotate circle around y
                        Vector3 point3 = Vector3.Transform(Vector3.Transform(rad, zrot3), yrot3);//transformation
                        Matrix zrot4 = Matrix.CreateRotationZ(MathHelper.ToRadians(y * dify)); //rotate vertex around z
                        Matrix yrot4 = Matrix.CreateRotationY(MathHelper.ToRadians((x + 1) * difx)); //rotate circle around y
                        Vector3 point4 = Vector3.Transform(Vector3.Transform(rad, zrot4), yrot4);//transformation

                        verts.Add(new VertexPositionColor(point + pos, color));
                        verts.Add(new VertexPositionColor(point2 + pos, color));
                        verts.Add(new VertexPositionColor(point3 + pos, color));

                        verts.Add(new VertexPositionColor(point + pos, color));
                        verts.Add(new VertexPositionColor(point4 + pos, color));
                        verts.Add(new VertexPositionColor(point2 + pos, color));
                    }
                }
            }

            public static void addLaserToModel(List<VertexPositionColor> verts, Color color, float size = 1, float speed = 1.5F)
            {
                verts.Add(new VertexPositionColor(new Vector3(-speed * 2F, 0, 0), color));
                verts.Add(new VertexPositionColor(new Vector3(0, -size / 20F, -size / 20F), color));
                verts.Add(new VertexPositionColor(new Vector3(0, size / 20F, -size / 20F), color));

                verts.Add(new VertexPositionColor(new Vector3(-speed * 2F, 0, 0), color));
                verts.Add(new VertexPositionColor(new Vector3(0, size / 20F, -size / 20F), color));
                verts.Add(new VertexPositionColor(new Vector3(0, 0, size / 20F), color));

                verts.Add(new VertexPositionColor(new Vector3(-speed * 2F, 0, 0), color));
                verts.Add(new VertexPositionColor(new Vector3(0, 0, size / 20F), color));
                verts.Add(new VertexPositionColor(new Vector3(0, -size / 20F, -size / 20F), color));

                verts.Add(new VertexPositionColor(new Vector3(0.5F, 0, 0), color));
                verts.Add(new VertexPositionColor(new Vector3(0F, size / 20F, -size / 20F), color));
                verts.Add(new VertexPositionColor(new Vector3(0F, -size / 20F, -size / 20F), color));

                verts.Add(new VertexPositionColor(new Vector3(0.5F, 0, 0), color));
                verts.Add(new VertexPositionColor(new Vector3(0, 0, size / 20F), color));
                verts.Add(new VertexPositionColor(new Vector3(0, size / 20F, -size / 20F), color));

                verts.Add(new VertexPositionColor(new Vector3(0.5F, 0, 0), color));
                verts.Add(new VertexPositionColor(new Vector3(0, -size / 20F, -size / 20F), color));
                verts.Add(new VertexPositionColor(new Vector3(0, 0, size / 20F), color));
            }

            public static void addPlaneToModel(List<VertexPositionColorTexture> verts, Color color, float size = 1, float rotation = 0F)
            {
                verts.Add(new VertexPositionColorTexture(new Vector3(extraMath.calculateVectorOffset(rotation, -size), -size), color, new Vector2(0, 1)));
                verts.Add(new VertexPositionColorTexture(new Vector3(extraMath.calculateVectorOffset(rotation, size), -size), color, new Vector2(1, 1)));
                verts.Add(new VertexPositionColorTexture(new Vector3(extraMath.calculateVectorOffset(rotation, -size), size), color, new Vector2(0, 0)));

                verts.Add(new VertexPositionColorTexture(new Vector3(extraMath.calculateVectorOffset(rotation, -size), size), color, new Vector2(0, 0)));
                verts.Add(new VertexPositionColorTexture(new Vector3(extraMath.calculateVectorOffset(rotation, size), -size), color, new Vector2(1, 1)));
                verts.Add(new VertexPositionColorTexture(new Vector3(extraMath.calculateVectorOffset(rotation, size), size), color, new Vector2(1, 0)));
            }
        }
    }
}
