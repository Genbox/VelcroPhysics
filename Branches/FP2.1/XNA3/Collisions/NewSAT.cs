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
            int contactsDetected = 0;
            
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

                    Vector2 d = geomA.WorldVertices.GetCentroid() - geomB.WorldVertices.GetCentroid();
                    if (Vector2.Dot(d, translationAxis) < 0)
                        translationAxis = -translationAxis;
                }
            }

            Vector2 normalA = -Vector2.Normalize(translationAxis * minIntervalDistance);
            float distanceA = (translationAxis * minIntervalDistance).Length();
            
            // For each axis check if separation occurs
            for (int i = 0; i < geomB.WorldVertices.Count; i++)
            {
                // Get the normal for each edge.
                Vector2 axis = geomB.WorldVertices.GetEdgeNormal(i);
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

                    Vector2 d = geomA.WorldVertices.GetCentroid() - geomB.WorldVertices.GetCentroid();
                    if (Vector2.Dot(d, translationAxis) < 0)
                        translationAxis = -translationAxis;
                }
            }
            
            // here we separate the polygons
            //minIntervalDistance -= 0.2f;

            if (minIntervalDistance > 15f)
            {
                // if A is static and B is not

                if (geomA.Body.IsStatic && !geomB.Body.IsStatic)
                {
                    geomB.Body.Position += translationAxis * minIntervalDistance;
                }
                // if B is static and A is not
                if (geomB.Body.IsStatic && !geomA.Body.IsStatic)
                {
                    geomA.Body.Position += translationAxis * minIntervalDistance;
                }
                // if both are not static, then move both bodies by half
                if (!geomB.Body.IsStatic && !geomA.Body.IsStatic)
                {
                    geomA.Body.Position += (translationAxis * minIntervalDistance) / 2.0f;
                    geomB.Body.Position -= (translationAxis * minIntervalDistance) / 2.0f;
                }
            }
            // now find vertices still in contact with other poly and create contacts

            //Vector2 normalB = Vector2.Normalize(translationAxis * minIntervalDistance);
            float distanceB = (translationAxis * minIntervalDistance).Length();

            if (distanceA < distanceB)
                distanceB = distanceA;

            for (int i = 0; i < geomA.WorldVertices.Count; i++)
            {
                if (contactsDetected < PhysicsSimulator.MaxContactsToDetect)
                {
                    if (InsidePolygon(geomB.WorldVertices, geomA.WorldVertices[i]))
                    {
                        //if (!geomA.Body.IsStatic)
                        {
                            if (distanceA > 0.001f)
                            {
                                Contact c = new Contact(geomA.WorldVertices[i], normalA, -distanceB, new ContactId(geomA.id, i, geomB.id));
                                contactList.Add(c);
                                contactsDetected++;
                            }
                        }
                    }
                }
                else break;
            }

            contactsDetected = 0;
          
            for (int i = 0; i < geomB.WorldVertices.Count; i++)
            {
                if (contactsDetected < PhysicsSimulator.MaxContactsToDetect)
                {
                    if (InsidePolygon(geomA.WorldVertices, geomB.WorldVertices[i]))
                    {
                        //if (!geomB.Body.IsStatic)
                        {
                            if (distanceB > 0.001f)
                            {
                                Contact c = new Contact(geomB.WorldVertices[i], normalA, -distanceB, new ContactId(geomA.id, i, geomB.id));
                                contactList.Add(c);
                                contactsDetected++;
                            }
                        }
                    }
                }
                else break;
            }
        }

        public bool Intersect(Geom geom, ref Vector2 position)
        {
            return InsidePolygon(geom.LocalVertices, position);
        }

        private bool InsidePolygon(Vertices polygon, Vector2 position)
        {
            int counter = 0;
            int i;

            Vector2 p1 = polygon[0];
            for (i = 1; i <= polygon.Count; i++)
            {
                Vector2 p2 = polygon[i % polygon.Count];
                if (position.Y > Math.Min(p1.Y, p2.Y))
                {
                    if (position.Y <= Math.Max(p1.Y, p2.Y))
                    {
                        if (position.X <= Math.Max(p1.X, p2.X))
                        {
                            if (p1.Y != p2.Y)
                            {
                                double xinters = (position.Y - p1.Y) * (p2.X - p1.X) / (p2.Y - p1.Y) + p1.X;
                                if (p1.X == p2.X || position.X <= xinters)
                                    counter++;
                            }
                        }
                    }
                }
                p1 = p2;
            }

            if (counter % 2 == 0)
                return false;

            return true;
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