using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace SpaceGame2
{
    /// <summary>
    /// Represents an object's orbital settings
    /// </summary>
    public class OrbitalInfo
    {
        #region Orbital Vars
        Vector2 centre;
        /// <summary>
        /// Gets or sets the orbital centre, ie. the body that this body is orbiting around
        /// </summary>
        public Vector2 Centre
        {
            get { return centre; }
            set
            {
                centre = value;
                RebuildVars();
            }
        }

        float apoapsis;
        /// <summary>
        /// Gets or sets this planets apoapsis
        /// </summary>
        public float Apopsis
        {
            get { return apoapsis; }
            set
            {
                apoapsis = value;
                RebuildVars();
            }
        }

        float perapsis;
        /// <summary>
        /// Gets or sets this planets perapsis
        /// </summary>
        public float Perapsis
        {
            get { return perapsis; }
            set
            {
                perapsis = value;
                RebuildVars();
            }
        }

        float axisMajor;
        /// <summary>
        /// Gets or sets the major axis in <b>Degrees</b>. This is the angle relative to
        /// +X that the orbit's perapsis/apoapsis lie on
        /// </summary>
        public float AxisMajor
        {
            get { return axisMajor * toDeg; }
            set
            {
                axisMajor = value * toRad;
                RebuildVars();
            }
        }

        /// <summary>
        /// Gets the Eccentricity of this orbit. 0 being circular and 1 being a straight line
        /// </summary>
        public float Eccentricity
        {
            get
            {
                focalLength = ((apoapsis + perapsis) / 2) - perapsis;
                float radX = ((apoapsis + perapsis) / 2);
                float radY = apoapsis - (apoapsis - perapsis);

                return (float)Math.Sqrt((Math.Pow(radX, 2) - Math.Pow(radY, 2)) / Math.Pow(radX, 2));
            }
        }

        /// <summary>
        /// Gets the centre of this orbit's ellipse
        /// </summary>
        public Vector2 EllipseCentre
        {
            get
            {
                return new Vector2((float)centreX, (float)(centreY));
            }
        }

        public float OrbitalSpeed = 0.0005F;
        #endregion

        #region Math Vars
        /// <summary>
        /// The radius parallel to the axis major
        /// </summary>
        float radX;
        /// <summary>
        /// The radius perpindicular to the axis major
        /// </summary>
        float radY;

        /// <summary>
        /// The centre X of this orbit's ellipse
        /// </summary>
        float centreX;
        /// <summary>
        /// The centre Y of this orbit's ellipse
        /// </summary>
        float centreY;

        /// <summary>
        /// The distance from the orbit ellipse's orgin and it's host position
        /// </summary>
        float focalLength;
        #endregion

        /// <summary>
        /// Creates a new perfectly circular orbital info object
        /// </summary>
        /// <param name="Apoapsis">The distance to orbit from the centre</param>
        /// <param name="AxisMajor">The major axis to orbit on</param>
        /// <param name="Centre">The focal point of this orbit</param>
        public OrbitalInfo(float Apoapsis, float AxisMajor, Vector2 Centre)
        {
            this.apoapsis = Apoapsis;
            this.perapsis = Apoapsis;
            this.axisMajor = AxisMajor;
            this.centre = Centre;

            RebuildVars();
        }

        /// <summary>
        /// Creates a new perfectly circular orbital info object
        /// </summary>
        /// <param name="Apoapsis">The distance to orbit from the centre</param>
        public OrbitalInfo(float Apoapsis)
        {
            this.apoapsis = Apoapsis;
            this.perapsis = Apoapsis;
            this.centre = Vector2.Zero;

            RebuildVars();
        }

        /// <summary>
        /// Creates a new perfectly circular orbital info object
        /// </summary>
        /// <param name="Apoapsis">The apoapsis of this orbit</param>
        /// <param name="Perapsis">The perapsis of this orbit</param>
        /// <param name="AxisMajor">The major axis to orbit on</param>
        /// <param name="Centre">The focal point of this orbit</param>
        public OrbitalInfo(float Apoapsis, float Perapsis, float AxisMajor, Vector2 Centre)
        {
            this.apoapsis = Math.Max(Apoapsis, Perapsis);
            this.perapsis = Math.Min(Apoapsis, Perapsis);
            this.axisMajor = AxisMajor;
            this.centre = Centre;

            RebuildVars();
        }

        /// <summary>
        /// Rebuilds all the math variables
        /// </summary>
        private void RebuildVars()
        {
            focalLength = ((apoapsis + perapsis) / 2) - perapsis;
            radX = ((apoapsis + perapsis) / 2);
            radY = apoapsis - (apoapsis - perapsis);

            centreX = Centre.X - LengthdirX(axisMajor + 180, focalLength);
            centreY = Centre.Y - LengthdirY(axisMajor + 180, focalLength);
        }

        /// <summary>
        /// Gets the point at the given angle
        /// </summary>
        /// <param name="theta">The angle in <B>Degrees</B></param>
        /// <returns></returns>
        public Vector2 GetPoint(double theta)
        {
            float range = Apopsis - Perapsis;


            if (Apopsis != Perapsis)
            {
                double x = centreX + radX * Math.Cos(theta) * Math.Cos(axisMajor) -
                    radY * Math.Sin(theta) * Math.Sin(axisMajor);

                double y = centreY + radX * Math.Cos(theta + Math.PI) * Math.Sin(axisMajor) +
                    radY * Math.Sin(theta) * Math.Cos(axisMajor);

                return new Vector2((float)x, (float)y);
            }
            else
                return new Vector2(
                    (float)LengthdirX(theta, Apopsis),
                    (float)LengthdirY(theta, Apopsis));
        }

        /// <summary>
        /// Gets the radius at the specified theta
        /// </summary>
        /// <param name="theta"></param>
        /// <returns></returns>
        public float GetRadius(double theta)
        {
            if (Apopsis != Perapsis)
                return (GetPoint(theta) - Centre).Length();
            else
                return (float)Apopsis;
        }

        public static implicit operator OrbitalInfo(int distance)
        {
            return new OrbitalInfo(distance);
        }

        #region Maths
        const float toRad = (float)(Math.PI / 180.00);
        const float toDeg = (float)(180.00 * Math.PI);

        /// <summary>
        /// Gets this degree as radians
        /// </summary>
        /// <returns>this float in radians</returns>
        public static float ToRad(float degrees)
        {
            return degrees * toRad;
        }

        /// <summary>
        /// Gets this radian as a degree
        /// </summary>
        /// <returns>degrees in this radian</returns>
        public static float ToDeg(float radians)
        {
            return radians * toDeg;
        }

        /// <summary>
        /// Gets the change in x over the given length from an angle
        /// </summary>
        /// <param name="angle">The angle in <b>degrees</b></param>
        /// <param name="length">The length of the arm</param>
        /// <returns>The change in x over <i>length</i></returns>
        public static float LengthdirX(double angle, float length)
        {
            return (float)(length * Math.Cos(angle * toRad));
        }

        /// <summary>
        /// Gets the change in y over the given length from an angle
        /// </summary>
        /// <param name="angle">The angle in <b>degrees</b></param>
        /// <param name="length">The length of the arm</param>
        /// <returns>The change in y over <i>length</i></returns>
        public static float LengthdirY(double angle, float length)
        {
            return (float)(length * Math.Sin(angle * toRad));
        }

        /// <summary>
        /// Wraps the given value between a min and a max
        /// </summary>
        /// <param name="min">The minimum value to wrap to</param>
        /// <param name="max">The maximum value to wrap to</param>
        /// <param name="val">The value to wrap</param>
        /// <returns><i>val</i> wrapped between <i>max</i> and <i>min</i></returns>
        public static float Wrap(float min, float max, float val)
        {
            float range = max - min;

            while (val < min)
                val += range;
            while (val > max)
                val -= range;

            return val;
        }

        /// <summary>
        /// Lerps between 2 values
        /// </summary>
        /// <param name="val1">The bottom value</param>
        /// <param name="val2">The the top value</param>
        /// <param name="amount">The amount to lerp by</param>
        /// <returns>a lerp between val1 and val2</returns>
        public static float Lerp(float val1, float val2, float amount)
        {
            float range = val2 - val1;

            return val1 + (range * amount);
        }
        #endregion
    }
}
