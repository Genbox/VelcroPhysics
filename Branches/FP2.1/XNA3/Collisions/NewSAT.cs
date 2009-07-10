using System;
using FarseerGames.FarseerPhysics.Interfaces;

#if (XNA)
using Microsoft.Xna.Framework;
#else
using FarseerGames.FarseerPhysics.Mathematics;
#endif

namespace FarseerGames.FarseerPhysics.Collisions
{
    public class SAT : INarrowPhaseCollider
    {
        private static SAT _instance;

        private SAT()
        {
        }

        public static SAT Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SAT();
                }
                return _instance;
            }
        }

        public void Collide(Geom geomA, Geom geomB, ContactList contactList)
        {
            float minA, maxA;
            float minB, maxB;
            float intervalDistance, minIntervalDistance = float.MaxValue;
            Vector2 translationAxis = new Vector2();
            
            // For each axis check if separation occurs
            for (int i = 0; i < geomA.WorldVertices.Count; i++)
            {
                // Get the normal for each edge.
                Vector2 axis = geomA.WorldVertices.GetEdgeNormal(i);
                // Project both polygons to the axis.
                geomA.WorldVertices.ProjectToAxis(ref axis, out minA, out maxA);
                geomB.WorldVertices.ProjectToAxis(ref axis, out minB, out maxB);
                // If the interval is negative then we have a collision.
                intervalDistance = IntervalDistance(minA, maxA, minB, maxB);
                if (intervalDistance > 0.0f)
                    return;

                // Check if the current interval distance is the minimum one. If so store
                // the interval distance and the current distance.
                // This will be used to calculate the minimum translation vector
                intervalDistance = Math.Abs(intervalDistance);
                if (intervalDistance < minIntervalDistance)
                {
                    minIntervalDistance = intervalDistance;
                    translationAxis = axis;

                    Vector2 d = geomA.WorldVertices.GetCentroid() - geomA.WorldVertices.GetCentroid();
                    if (Vector2.Dot(d, translationAxis) < 0)
                        translationAxis = -translationAxis;
                }
            }

            // We have made it through all the axis and none offer separation.
            geomA.Body.Position += translationAxis * minIntervalDistance;
            geomB.Body.Position -= translationAxis * minIntervalDistance;
        }

        public bool Intersect(Geom geom, ref Vector2 point)
        {
            return false;
        }

        /// <summary>
        /// Calculate the distance between [minA, maxA] and [minB, maxB]
        /// The distance will be negative if the intervals overlap
        /// </summary>
        /// <param name="minA">The min A.</param>
        /// <param name="maxA">The max A.</param>
        /// <param name="minB">The min B.</param>
        /// <param name="maxB">The max B.</param>
        /// <returns></returns>
        public float IntervalDistance(float minA, float maxA, float minB, float maxB)
        {
            if (minA < minB)
            {
                return minB - maxA;
            }

            return minA - maxB;
        }
    }
}