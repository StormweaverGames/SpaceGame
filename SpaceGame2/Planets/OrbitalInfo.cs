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

        double apoapsis;
        /// <summary>
        /// Gets or sets this planets apoapsis
        /// </summary>
        public double Apopsis
        {
            get { return apoapsis; }
            set
            {
                apoapsis = value;
                RebuildVars();
            }
        }

        double perapsis;
        /// <summary>
        /// Gets or sets this planets perapsis
        /// </summary>
        public double Perapsis
        {
            get { return perapsis; }
            set
            {
                perapsis = value;
                RebuildVars();
            }
        }

        double axisMajor;
        /// <summary>
        /// Gets or sets the major axis in <b>RADIANS</b>. This is the angle relative to
        /// +X that the orbit's perapsis/apoapsis lie on
        /// </summary>
        public double AxisMajor
        {
            get { return axisMajor; }
            set
            {
                axisMajor = value;
                RebuildVars();
            }
        }

        /// <summary>
        /// Gets the Eccentricity of this orbit. 0 being circular and 1 being a straight line
        /// </summary>
        public double Eccentricity
        {
            get
            {
                focalLength = ((apoapsis + perapsis) / 2) - perapsis;
                double radX = ((apoapsis + perapsis) / 2);
                double radY = apoapsis - (apoapsis - perapsis);

                return Math.Sqrt((Math.Pow(radX, 2) - Math.Pow(radY, 2)) / Math.Pow(radX, 2));
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

        public double OrbitalSpeed = 0.0005;
        #endregion

        #region Math Vars
        /// <summary>
        /// The radius parallel to the axis major
        /// </summary>
        double radX;
        /// <summary>
        /// The radius perpindicular to the axis major
        /// </summary>
        double radY;

        /// <summary>
        /// The centre X of this orbit's ellipse
        /// </summary>
        double centreX;
        /// <summary>
        /// The centre Y of this orbit's ellipse
        /// </summary>
        double centreY;

        /// <summary>
        /// The distance from the orbit ellipse's orgin and it's host position
        /// </summary>
        double focalLength;
        #endregion

        /// <summary>
        /// Creates a new perfectly circular orbital info object
        /// </summary>
        /// <param name="Apoapsis">The distance to orbit from the entre</param>
        /// <param name="AxisMajor">The major axis to orbit on</param>
        /// <param name="Centre">The focal point of this orbit</param>
        public OrbitalInfo(double Apoapsis, double AxisMajor, Vector2 Centre)
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
        /// <param name="Apoapsis">The apoapsis of this orbit</param>
        /// <param name="Perapsis">The perapsis of this orbit</param>
        /// <param name="AxisMajor">The major axis to orbit on</param>
        /// <param name="Centre">The focal point of this orbit</param>
        public OrbitalInfo(double Apoapsis, double Perapsis, double AxisMajor, Vector2 Centre)
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

            centreX = Centre.X - LengthdirX(axisMajor + Math.PI, focalLength);
            centreY = Centre.Y - LengthdirY(axisMajor + Math.PI, focalLength);
        }

        /// <summary>
        /// Gets the point at the given angle
        /// </summary>
        /// <param name="theta">The angle in <B>Radians</B></param>
        /// <returns></returns>
        public Vector2 GetPoint(double theta)
        {
            double x = centreX + radX * Math.Cos(theta + Math.PI) * Math.Cos(axisMajor + Math.PI) -
                radY * Math.Sin(theta + Math.PI) * Math.Sin(axisMajor + Math.PI);

            double y = centreY + radX * Math.Cos(theta + Math.PI) * Math.Sin(axisMajor + Math.PI) +
                radY * Math.Sin(theta + Math.PI) * Math.Cos(axisMajor + Math.PI);

            return new Vector2((float)x, (float)y);
        }

        #region Maths
        const double toRad = Math.PI / 180.00;
        const double toDeg = 180.00 * Math.PI;

        /// <summary>
        /// Gets this degree as radians
        /// </summary>
        /// <returns>this double in radians</returns>
        public static double ToRad(double degrees)
        {
            return degrees * toRad;
        }

        /// <summary>
        /// Gets this radian as a degree
        /// </summary>
        /// <returns>degrees in this radian</returns>
        public static double ToDeg(double radians)
        {
            return radians * toDeg;
        }

        /// <summary>
        /// Gets the change in x over the given length from an angle
        /// </summary>
        /// <param name="angle">The angle in <b>degrees</b></param>
        /// <param name="length">The length of the arm</param>
        /// <returns>The change in x over <i>length</i></returns>
        public static double LengthdirX(double angle, double length)
        {
            return length * Math.Cos(angle);
        }

        /// <summary>
        /// Gets the change in y over the given length from an angle
        /// </summary>
        /// <param name="angle">The angle in <b>degrees</b></param>
        /// <param name="length">The length of the arm</param>
        /// <returns>The change in y over <i>length</i></returns>
        public static double LengthdirY(double angle, double length)
        {
            return length * Math.Sin(angle);
        }

        /// <summary>
        /// Wraps the given value between a min and a max
        /// </summary>
        /// <param name="min">The minimum value to wrap to</param>
        /// <param name="max">The maximum value to wrap to</param>
        /// <param name="val">The value to wrap</param>
        /// <returns><i>val</i> wrapped between <i>max</i> and <i>min</i></returns>
        public static double Wrap(double min, double max, double val)
        {
            double range = max - min;

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
        public static double Lerp(double val1, double val2, double amount)
        {
            double range = val2 - val1;

            return val1 + (range * amount);
        }
        #endregion
    }
}
