using System;
using Microsoft.Xna.Framework;

namespace VelcroPhysics.Shared
{
    /// <summary>
    /// Rotation
    /// </summary>
    public struct Rot
    {
        /// Sine and cosine
        public float s,
                     c;

        /// <summary>
        /// Initialize from an angle in radians
        /// </summary>
        /// <param name="angle">Angle in radians</param>
        public Rot(float angle)
        {
            // TODO_ERIN optimize
            s = (float)Math.Sin(angle);
            c = (float)Math.Cos(angle);
        }

        /// <summary>
        /// Set using an angle in radians.
        /// </summary>
        /// <param name="angle"></param>
        public void Set(float angle)
        {
            //Velcro: Optimization
            if (angle == 0)
            {
                s = 0;
                c = 1;
            }
            else
            {
                // TODO_ERIN optimize
                s = (float)Math.Sin(angle);
                c = (float)Math.Cos(angle);
            }
        }

        /// <summary>
        /// Set to the identity rotation
        /// </summary>
        public void SetIdentity()
        {
            s = 0.0f;
            c = 1.0f;
        }

        /// <summary>
        /// Get the angle in radians
        /// </summary>
        public float GetAngle()
        {
            return (float)Math.Atan2(s, c);
        }

        /// <summary>
        /// Get the x-axis
        /// </summary>
        public Vector2 GetXAxis()
        {
            return new Vector2(c, s);
        }

        /// <summary>
        /// Get the y-axis
        /// </summary>
        public Vector2 GetYAxis()
        {
            return new Vector2(-s, c);
        }
    }
}