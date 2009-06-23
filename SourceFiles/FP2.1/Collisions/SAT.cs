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

        /// <summary>
        /// Returns the contact list from two possibly intersecting Geom's. 
        /// This is the stationary version of this function. It doesn't 
        /// account for linear or angular motion.
        /// </summary>
        /// <param name="geomA">The first Geom.</param>
        /// <param name="geomB">The second Geom.</param>
        /// <param name="contactList">Set of Contacts between the two Geoms.
        /// NOTE- this will be empty if no contacts are present.</param>
        public void Collide(Geom geomA, Geom geomB, ContactList contactList)
        {
            PolygonCollisionResult result = PolygonCollision(geomA.WorldVertices, geomB.WorldVertices, geomB.body.LinearVelocity - geomA.body.LinearVelocity);
            float distance = result.MinimumTranslationVector.Length();
            int contactsDetected = 0;
            Vector2 normal = Vector2.Normalize(-result.MinimumTranslationVector);

            if (result.Intersect)
            {
                for (int i = 0; i < geomA.WorldVertices.Count; i++)
                {
                    if (contactsDetected <= PhysicsSimulator.MaxContactsToDetect)
                    {
                        if (InsidePolygon(geomB.WorldVertices, geomA.WorldVertices[i]))
                        {
                            if (!geomA.Body.IsStatic)
                            {
                                if (distance > 0.001f)
                                {
                                    Contact c = new Contact(geomA.WorldVertices[i], normal, -distance, new ContactId(geomA.id, i, geomB.id));
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
                    if (contactsDetected <= PhysicsSimulator.MaxContactsToDetect)
                    {
                        if (InsidePolygon(geomA.WorldVertices, geomB.WorldVertices[i]))
                        {
                            if (!geomB.Body.IsStatic)
                            {
                                if (distance > 0.001f)
                                {
                                    Contact c = new Contact(geomB.WorldVertices[i], normal, -distance, new ContactId(geomB.id, i, geomA.id));
                                    contactList.Add(c);
                                    contactsDetected++;
                                }
                            }
                        }
                    }
                    else break;
                }
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

        private void ProjectPolygon(Vector2 axis, Vertices polygon, ref float min, ref float max)
        {
            // To project a point on an axis use the dot product

            float dotProduct = Vector2.Dot(axis, polygon[0]);
            min = dotProduct;
            max = dotProduct;
            for (int i = 0; i < polygon.Count; i++)
            {
                dotProduct = Vector2.Dot(polygon[i], axis);
                if (dotProduct < min)
                {
                    min = dotProduct;
                }
                else
                {
                    if (dotProduct > max)
                    {
                        max = dotProduct;
                    }
                }
            }
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

        /// <summary>
        /// Check if polygon A is going to collide with polygon B.
        /// The last parameter is the *relative* velocity 
        /// of the polygons (i.e. velocityA - velocityB)
        /// </summary>
        /// <param name="polygonA">The polygon A.</param>
        /// <param name="polygonB">The polygon B.</param>
        /// <param name="velocity">The velocity.</param>
        /// <returns></returns>
        internal PolygonCollisionResult PolygonCollision(Vertices polygonA, Vertices polygonB, Vector2 velocity)
        {
            PolygonCollisionResult result = new PolygonCollisionResult();
            result.Intersect = true;
            result.WillIntersect = true;

            int edgeCountA = polygonA.Count;
            int edgeCountB = polygonB.Count;
            float minIntervalDistance = float.PositiveInfinity;
            Vector2 translationAxis = new Vector2();
            Vector2 edge;

            // Loop through all the edges of both polygons
            for (int edgeIndex = 0; edgeIndex < edgeCountA + edgeCountB; edgeIndex++)
            {
                if (edgeIndex < edgeCountA)
                {
                    edge = polygonA.GetEdge(edgeIndex);
                }
                else
                {
                    edge = polygonB.GetEdge(edgeIndex - edgeCountA);
                }

                // ===== 1. Find if the polygons are currently intersecting =====

                // Find the axis perpendicular to the current edge
                Vector2 axis = new Vector2(-edge.Y, edge.X);
                axis.Normalize();

                // Find the projection of the polygon on the current axis
                float minA = 0; float minB = 0; float maxA = 0; float maxB = 0;
                ProjectPolygon(axis, polygonA, ref minA, ref maxA);
                ProjectPolygon(axis, polygonB, ref minB, ref maxB);

                // Check if the polygon projections are currentlty intersecting
                float intervalDistance = IntervalDistance(minA, maxA, minB, maxB);

                if (intervalDistance > 0)
                    result.Intersect = false;

                // ===== 2. Now find if the polygons *will* intersect =====


                // Project the velocity on the current axis

                //float velocityProjection = Vector2.Dot(axis, velocity);

                // Get the projection of polygon A during the movement

                //if (velocityProjection < 0) {
                //    minA += velocityProjection;
                //} else {
                //    maxA += velocityProjection;
                //}

                // Do the same test as above for the new projection
                //float intervalDistance = IntervalDistance(minA, maxA, minB, maxB);
                //if (intervalDistance > 0) result.WillIntersect = false;

                // If the polygons are not intersecting and won't intersect, exit the loop
                //if (!result.Intersect && !result.WillIntersect) break;
                if (!result.Intersect) break;

                // Check if the current interval distance is the minimum one. If so store
                // the interval distance and the current distance.
                // This will be used to calculate the minimum translation vector
                intervalDistance = Math.Abs(intervalDistance);
                if (intervalDistance < minIntervalDistance)
                {
                    minIntervalDistance = intervalDistance;
                    translationAxis = axis;

                    Vector2 d = polygonA.GetCentroid() - polygonB.GetCentroid();
                    if (Vector2.Dot(d, translationAxis) < 0)
                        translationAxis = -translationAxis;
                }
            }

            // The minimum translation vector
            // can be used to push the polygons appart.
            if (result.WillIntersect)
                result.MinimumTranslationVector =
                       translationAxis * minIntervalDistance;

            return result;
        }


        private int GetMiddleIndex(int i0, int i1, int N)
        {
            if (i0 < i1)
                return (i0 + i1) / 2;
            else
                return ((i0 + i1 + N) / 2) % N;
        }

    }

    /// <summary>
    /// Structure that stores the results of the PolygonCollision function
    /// </summary>
    internal struct PolygonCollisionResult
    {
        // Are the polygons going to intersect forward in time?
        public bool WillIntersect;

        // Are the polygons currently intersecting?
        public bool Intersect;

        // The translation to apply to the first polygon to push the polygons apart.
        public Vector2 MinimumTranslationVector;
    }
}
