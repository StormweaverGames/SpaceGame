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
using EmberEngine.Tools.AdvancedMath;
using System.Text;


namespace SpaceGame2
{
    /// <summary>
    /// Represents a Vertex with Position, Color and Normal data
    /// </summary>
    public struct VertexPositionColorNormal : IVertexType
    {
        /// <summary>
        /// The position of this vertex
        /// </summary>
        public Vector3 Position;
        /// <summary>
        /// The color of this vertex
        /// </summary>
        public Color Color;
        /// <summary>
        /// The normal of this vertex
        /// </summary>
        public Vector3 Normal;

        /// <summary>
        /// Initializes a new VertexPositionColorNormal
        /// </summary>
        /// <param name="pos">The Position of the vertex</param>
        /// <param name="color">The Color of the vertex</param>
        /// <param name="normal">The Normal of the vertex</param>
        public VertexPositionColorNormal(Vector3 pos, Color color, Vector3 normal)
        {
            this.Position = pos;
            this.Color = color;
            this.Normal = normal;
        }

        VertexDeclaration IVertexType.VertexDeclaration { get { return VertexDeclaration; } }

        /// <summary>
        /// Gets the VertexDeclaration for this vertex
        /// </summary>
        public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration
            (
                new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                new VertexElement(sizeof(float) * 3, VertexElementFormat.Color, VertexElementUsage.Color, 0),
                new VertexElement(sizeof(float) * 3 + 4, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0)
            );

    }

    /// <summary>
    /// Represents a camera's view paramaters
    /// </summary>
    public struct ViewParamaters
    {
        private double rotation;
        /// <summary>
        /// This view's rotation around the z axis in degrees
        /// </summary>
        public double Rotation
        {
            get { return rotation; }
            set
            {
                rotation = Math2.Wrap(0, 360, value);
            }
        }
        /// <summary>
        /// The view matrix
        /// </summary>
        public Matrix View;
        /// <summary>
        /// The projection matrix
        /// </summary>
        public Matrix Projection;
        /// <summary>
        /// The world matrix
        /// </summary>
        public Matrix World;
    }

    /// <summary>
    /// An acess to some basic utility functions
    /// </summary>
    public static class Utils
    {
        static Random Random = new Random();

        /// <summary>
        /// Returns what <i>text</i> should be in order to fit to a certain width
        /// </summary>
        /// <param name="spriteFont">The spritefont to use</param>
        /// <param name="text">The text to fit</param>
        /// <param name="maxLineWidth">The maximum line width</param>
        /// <returns><i>text</i> as it should be written to stay within <i>maxLineWidth</i></returns>
        public static string WrapText(SpriteFont spriteFont, string text, float maxLineWidth)
        {
            string[] words = text.Split(' ');

            StringBuilder sb = new StringBuilder();

            float lineWidth = 0f;

            float spaceWidth = spriteFont.MeasureString(" ").X;

            foreach (string word in words)
            {
                Vector2 size = spriteFont.MeasureString(word);

                if (lineWidth + size.X < maxLineWidth)
                {
                    sb.Append(word + " ");
                    lineWidth += size.X + spaceWidth;
                }
                else
                {
                    sb.Append("\n" + word + " ");
                    lineWidth = size.X + spaceWidth;
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Roughens up an array of floats by a given scale
        /// </summary>
        /// <param name="input">The array to roughen</param>
        /// <param name="scale">The scale to generate random values on</param>
        /// <returns><i>input</i> roughened by a factor of <i>scale</i></returns>
        public static float[] Roughen(float[] input, float scale = 2.0F)
        {
            for (int i = 0; i < input.Length; i++)
                input[i] += (float)(Random.NextDouble() * scale) - (scale / 2);

            return input;
        }

        /// <summary>
        /// Wraps a value between a max and a min
        /// </summary>
        /// <param name="val">The value to wrap</param>
        /// <param name="min">The minimum value</param>
        /// <param name="max">The maximum value</param>
        /// <returns>Val, wrapped to min/max</returns>
        public static double Wrap(double val, double min, double max)
        {
            return val < min ? 
            val + max : (val > max ? 
            val - max : val);
        }

        /// <summary>
        /// Wraps a value between a max and a min
        /// </summary>
        /// <param name="val">The value to wrap</param>
        /// <param name="min">The minimum value</param>
        /// <param name="max">The maximum value</param>
        /// <returns>Val, wrapped to min/max</returns>
        public static float Wrap(this float val, float min, float max)
        {
            return val < min ?
            val + max : (val > max ?
            val - max : val);
        }

        /// <summary>
        /// Wraps a value between a max and a min
        /// </summary>
        /// <param name="val">The value to wrap</param>
        /// <param name="min">The minimum value</param>
        /// <param name="max">The maximum value</param>
        /// <returns>Val, wrapped to min/max</returns>
        public static int Wrap(this int val, int min, int max)
        {
            return val < min ?
            val + max : (val > max ?
            val - max : val);
        }
    }

    /// <summary>
    /// Represents a textured quad
    /// </summary>
    public class Quad
    {
        VertexPositionColorTexture[] verts = new VertexPositionColorTexture[6]{
            new VertexPositionColorTexture(new Vector3(0,0,0), Color.White, new Vector2(0,0)),
            new VertexPositionColorTexture(new Vector3(1,0,1), Color.White, new Vector2(1,1)),
            new VertexPositionColorTexture(new Vector3(0,0,1), Color.White, new Vector2(0,1)),
            
            new VertexPositionColorTexture(new Vector3(0,0,0), Color.White, new Vector2(0,0)),
            new VertexPositionColorTexture(new Vector3(1,0,0), Color.White, new Vector2(1,0)),
            new VertexPositionColorTexture(new Vector3(1,0,1), Color.White, new Vector2(1,1))
        };

        int[] index = new int[6] { 0, 1, 2, 3, 4, 5 };

        /// <summary>
        /// The BasicEffect to draw with
        /// </summary>
        BasicEffect effect;

        /// <summary>
        /// Creates a new quad
        /// </summary>
        /// <param name="scale">The scale of the quad</param>
        /// <param name="tex">The texture of the quad</param>
        /// <param name="color">The color of the quad</param>
        public Quad(GraphicsDevice Graphics, Vector2 scale, Texture2D tex, Color color)
        {
            effect = new BasicEffect(Graphics);
            effect.TextureEnabled = true;
            effect.Texture = tex;

            effect.World = Matrix.CreateScale(scale.X, 0, scale.Y);

            RebuildMesh(color);
        }

        /// <summary>
        /// Rebuilds the mesh
        /// </summary>
        /// <param name="color">The color to use</param>
        public void RebuildMesh(Color color)
        {
            verts = new VertexPositionColorTexture[]{
            new VertexPositionColorTexture(new Vector3(0,0,0), color, new Vector2(0,0)),
            new VertexPositionColorTexture(new Vector3(1,0,1), color, new Vector2(1,1)),
            new VertexPositionColorTexture(new Vector3(0,0,1), color, new Vector2(0,1)),
            
            new VertexPositionColorTexture(new Vector3(0,0,0), color, new Vector2(0,0)),
            new VertexPositionColorTexture(new Vector3(1,0,0), color, new Vector2(1,0)),
            new VertexPositionColorTexture(new Vector3(1,0,1), color, new Vector2(1,1))
        };
        }

        /// <summary>
        /// Renders this Quad
        /// </summary>
        /// <param name="effect">The effect to use</param>
        public void Render(ViewParamaters view)
        {
            effect.View = view.View;
            effect.Projection = view.Projection;

            foreach (EffectPass p in effect.CurrentTechnique.Passes)
            {
                p.Apply();

                //effect.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionColorTexture>(PrimitiveType.TriangleList,
                //    verts, 0, 2, index, 0, 2);

                effect.GraphicsDevice.DrawUserPrimitives<VertexPositionColorTexture>(PrimitiveType.TriangleList,
                    verts, 0, 2);
            }
        }
    }

    public class Extensions
    {
        public static float[] FlareMap(int size, float h, float baseVal = 0)
        {
            float[] temp = new float[size];

            for (int i = 0; i < size; i++)
            {
                temp[i] = (float)(Math.Sin(MathHelper.ToDegrees(i / 3)) * h) + baseVal;
            }

            return temp;
        }
    }
}
