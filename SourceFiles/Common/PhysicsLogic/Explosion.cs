using System;
using System.Collections.Generic;
using System.Linq;
using FarseerPhysics.Collision;
using FarseerPhysics.Collision.Shapes;
using Microsoft.Xna.Framework;
using FarseerPhysics.Dynamics;

namespace FarseerPhysics.Common.PhysicsLogic
{
    struct ShapeData
    {
        public Body Body;
        public float Min; // absolute angles
        public float Max;
    }

    struct RayData
    {
        public float Angle;
    }

    /// <summary>
    /// This is a comprarer used for 
    /// detecting angle difference between rays
    /// </summary>
    class RayDataComparer : IComparer<RayData>
    {
        int IComparer<RayData>.Compare(RayData a, RayData b)
        {
            float diff = (a.Angle - b.Angle);
            if (diff > 0)
                return 1;
            if (diff < 0)
                return -1;
            return 0;
        }
    }

    /// <summary>
    /// This is an explosive... it explodes.
    /// </summary>
    /// <remarks>
    /// Original Code by Steven Lu
    /// Ported to Farseer 3.0 by Nicolás Hormazábal
    /// </remarks>
    //  (see http://www.box2d.org/forum/viewtopic.php?f=3&t=1688)
    public sealed class Explosive
    {
        const int MaxShapes = 100;
        Dictionary<Fixture, List<Vector2>> _exploded;
        RayDataComparer _rdc;
        List<ShapeData> _data = new List<ShapeData>();
        World _world;

        public Explosive(World world)
        {
            _exploded = new Dictionary<Fixture, List<Vector2>>();
            _rdc = new RayDataComparer();
            _data = new List<ShapeData>();
            _world = world;
        }

        /// <summary>
        /// This makes the explosive explode
        /// </summary>
        /// <param name="pos">
        /// The position where the explosion happens
        /// </param>
        /// <param name="radius">
        /// The explosion radius
        /// </param>
        /// <param name="maxForce">
        /// The explosion force at the explosion point
        /// (then is inversely proportional to the square of the distance)
        /// </param>
        /// <returns>
        /// A dictionnary containing all the "exploded" fixtures
        /// with a list of the applied impulses
        /// </returns>
        public Dictionary<Fixture, List<Vector2>> Explode(Vector2 pos, float radius, float maxForce)
        {
            _exploded.Clear();

            AABB aabb;
            aabb.LowerBound = pos + new Vector2(-radius, -radius);
            aabb.UpperBound = pos + new Vector2(radius, radius);
            Fixture[] shapes = new Fixture[MaxShapes];

            int shapeCount = 0;
            // Query the world for overlapping shapes.
            _world.QueryAABB(
                fixture =>
                {
                    shapes[shapeCount++] = fixture;

                    // Continue the query.
                    return true;
                }, ref aabb);

            // check if the explosion point is contained inside of a shape
            bool isInsideSomething = false;
            for (int i = 0; i < shapeCount; ++i)
            {
                if (shapes[i].TestPoint(ref pos))
                {
                    isInsideSomething = true;
                    break;
                }
            }

            if (!isInsideSomething)
            {
                // per shape max/min angles for now
                RayData[] vals = new RayData[shapeCount * 2];
                int valIndex = 0;
                for (int i = 0; i < shapeCount; ++i)
                {
                    PolygonShape ps;
                    CircleShape cs = shapes[i].Shape as CircleShape;
                    if (cs != null)
                    {
                        Vertices v = new Vertices();
                        Vector2 vec = Vector2.Zero + new Vector2(cs.Radius, 0);
                        v.Add(vec);
                        vec = Vector2.Zero + new Vector2(0, cs.Radius);
                        v.Add(vec);
                        vec = Vector2.Zero + new Vector2(-cs.Radius, cs.Radius);
                        v.Add(vec);
                        vec = Vector2.Zero + new Vector2(0, -cs.Radius);
                        v.Add(vec);
                        ps = new PolygonShape(v, 1);
                    }
                    else
                        ps = shapes[i].Shape as PolygonShape;

                    if ((shapes[i].Body.BodyType == BodyType.Dynamic) && ps != null)
                    {
                        Vector2 toCentroid = shapes[i].Body.GetWorldPoint(ps.MassData.Centroid) - pos;
                        float angleToCentroid = (float)Math.Atan2(toCentroid.Y, toCentroid.X);
                        float min = float.MaxValue;
                        float max = float.MinValue;
                        float minAbsolute = 0.0f;
                        float maxAbsolute = 0.0f;
                        //Vector2 minPt = new Vector2(-1, -1);
                        //Vector2 maxPt = new Vector2(1, 1);

                        for (int j = 0; j < (ps.Vertices.Count()); ++j)
                        {
                            Vector2 toVertex = (shapes[i].Body.GetWorldPoint(ps.Vertices[j]) - pos);
                            float newAngle = (float)Math.Atan2(toVertex.Y, toVertex.X);
                            float diff = (newAngle - angleToCentroid);

                            diff = (diff - MathHelper.Pi) % (2 * MathHelper.Pi); // the minus pi is important. It means cutoff for going other direction is at 180 deg where it needs to be

                            if (diff < 0.0f)
                                diff += 2 * MathHelper.Pi; // correction for not handling negs

                            diff -= MathHelper.Pi;

                            if (Math.Abs(diff) > MathHelper.Pi)
                                throw new ArgumentException("OMG!"); // Something's wrong, point not in shape but exists angle diff > 180

                            if (diff > max)
                            {
                                max = diff;
                                maxAbsolute = newAngle;
                                //maxPt = shapes[i].Body.GetWorldPoint(ps.Vertices[j]);
                            }
                            if (diff < min)
                            {
                                min = diff;
                                minAbsolute = newAngle;
                                //minPt = shapes[i].Body.GetWorldPoint(ps.Vertices[j]);
                            }
                        }

                        vals[valIndex].Angle = minAbsolute;
                        //vals[valIndex].pos = minPt;
                        ++valIndex;
                        vals[valIndex].Angle = maxAbsolute;
                        //vals[valIndex].pos = maxPt;
                        ++valIndex;
                    }
                }

                Array.Sort(vals, 0, valIndex, _rdc);
                _data.Clear();
                bool rayMissed = true;

                for (int i = 0; i < valIndex; ++i)
                {
                    Fixture shape = null;
                    float midpt;

                    int iplus = (i == valIndex - 1 ? 0 : i + 1);
                    if (vals[i].Angle == vals[iplus].Angle)
                        continue;

                    if (i == valIndex - 1)
                    {
                        // the single edgecase
                        midpt = (vals[0].Angle + MathHelper.Pi * 2 + vals[i].Angle);
                    }
                    else
                    {
                        midpt = (vals[i + 1].Angle + vals[i].Angle);
                    }

                    midpt = midpt / 2;

                    Vector2 p1 = pos;
                    Vector2 p2 = radius * new Vector2((float)Math.Cos(midpt),
                        (float)Math.Sin(midpt)) + pos;

                    //float fraction = 0;

                    // RaycastOne
                    bool hitClosest = false;
                    _world.RayCast((f, p, n, fr) =>
                    {
                        Body body = f.Body;
                        if (body.UserData != null)
                        {
                            int index = (int)body.UserData;
                            if (index == 0)
                            {
                                // filter
                                return -1.0f;
                            }
                        }

                        hitClosest = true;
                        shape = f;
                        //fraction = fr;
                        return fr;
                    }, p1, p2);

                    //draws radius points
                    if ((hitClosest) && (shape.Body.BodyType == BodyType.Dynamic))
                    {
                        if ((_data.Count() > 0) && (_data.Last().Body == shape.Body) && (!rayMissed))
                        {
                            int laPos = _data.Count - 1;
                            ShapeData la = _data[laPos];
                            la.Max = vals[iplus].Angle;
                            _data[laPos] = la;
                        }
                        else
                        {
                            // make new
                            ShapeData d;
                            d.Body = shape.Body;
                            d.Min = vals[i].Angle;
                            d.Max = vals[iplus].Angle;
                            _data.Add(d);
                        }

                        if ((_data.Count() > 1)
                            && (i == valIndex - 1)
                            && (_data.Last().Body == _data.First().Body)
                            && (_data.Last().Max == _data.First().Min))
                        {
                            ShapeData fi = _data[0];
                            fi.Min = _data.Last().Min;
                            _data.RemoveAt(_data.Count() - 1);
                            _data[0] = fi;
                            while (_data.First().Min >= _data.First().Max)
                            {
                                fi.Min -= MathHelper.Pi * 2;
                                _data[0] = fi;
                            }
                        }

                        int lastPos = _data.Count - 1;
                        ShapeData last = _data[lastPos];
                        while ((_data.Count() > 0)
                            && (_data.Last().Min >= _data.Last().Max)) // just making sure min<max
                        {
                            last.Min = _data.Last().Min - 2 * MathHelper.Pi;
                            _data[lastPos] = last;
                        }
                        rayMissed = false;
                    }
                    else
                    {
                        // Add entry to data with body = NULL to indicate a lack of objects in this range.
                        // Useful for drawing/graphical/other purposes.
                        // Commented for optimization

                        /*if (_data.Count() > 0 && rayMissed && _data.Last().Body == null)
                        {
                            int laPos = _data.Count - 1;
                            ShapeData la = _data[laPos];
                            la.Max = vals[iplus].Angle;
                            _data[laPos] = la;
                        }
                        else
                        {
                            ShapeData d;
                            d.Body = null;
                            d.Min = vals[i].Angle;
                            d.Max = vals[iplus].Angle;
                            _data.Add(d);
                        }

                        if ((_data.Count() > 1) && (i == valIndex - 1)
                            && (_data.First().Body == null)
                            && (_data.Last().Max == _data.First().Min))
                        {
                            ShapeData fi = _data[0];
                            fi.Min = _data.Last().Min;
                            _data.RemoveAt(_data.Count() - 1);
                            while (_data.First().Min >= _data.First().Max)
                            {
                                fi.Min -= MathHelper.Pi * 2;
                                _data[0] = fi;
                            }
                        }

                        int lastPos = _data.Count - 1;
                        ShapeData last = _data[lastPos];
                        while ((_data.Count() > 0)
                            && (_data.Last().Min >= _data.Last().Max)) // just making sure min<max
                        {
                            last.Min = _data.Last().Min - 2 * MathHelper.Pi;
                            _data[lastPos] = last;
                        }*/

                        rayMissed = true; // raycast did not find a shape
                    }
                }

                for (int i = 0; i < _data.Count(); ++i)
                {
                    const int minRays = 5; // for small arcs -- how many rays per shape/body/segment
                    const float maxAngle = MathHelper.Pi / 15; // max angle between rays (used when segment is large)
                    const float edgeRatio = 1.0f / 40.0f; // ratio of arc length to angle from edges to first ray tested
                    const float maxEdgeOffset = MathHelper.Pi / 90; // two degrees: maximum angle from edges to first ray tested

                    float arclen = _data[i].Max - _data[i].Min;

                    if (_data[i].Body == null)
                    {
                        // It should never enter here if we commented
                        // the part where we add null references
                        for (float j = _data[i].Min; j <= _data[i].Max; j += maxAngle)
                        {
                            // Draw Debug stuff... if you want to.
                            // Nothing found
                        }
                        continue;
                    }

                    float first = MathHelper.Min(maxEdgeOffset, edgeRatio * arclen);
                    int insertedRays = (int)Math.Ceiling(((arclen - 2.0f * first) - (minRays - 1) * maxAngle) / maxAngle);

                    if (insertedRays < 0)
                        insertedRays = 0;

                    float offset = (arclen - first * 2.0f) / ((float)minRays + insertedRays - 1);

                    //int jj = 0;
                    for (float j = _data[i].Min + first; j <= _data[i].Max; j += offset)
                    {
                        Vector2 p1 = pos;
                        Vector2 p2 = pos + radius * new Vector2((float)Math.Cos(j), (float)Math.Sin(j));
                        Vector2 hitpoint = Vector2.Zero;
                        float minlambda = float.MaxValue;

                        List<Fixture> fl = _data[i].Body.FixtureList;
                        for (int x = 0; x < fl.Count; x++)
                        {
                            Fixture f = fl[x];
                            RayCastInput ri;
                            ri.Point1 = p1;
                            ri.Point2 = p2;
                            ri.MaxFraction = 50f;

                            RayCastOutput ro;
                            if (f.RayCast(out ro, ref ri, 0))
                            {
                                if (minlambda > ro.Fraction)
                                {
                                    minlambda = ro.Fraction;
                                    hitpoint = ro.Fraction * p2 + (1 - ro.Fraction) * p1;
                                }
                            }

                            // the force that is to be applied for this particular ray.
                            // offset is angular coverage. lambda*length of segment is distance.
                            float impulse = (arclen / (minRays + insertedRays)) * maxForce * 180.0f / MathHelper.Pi * (1.0f - Math.Min(1.0f, minlambda));

                            // We Apply the impulse!!!
                            Vector2 vectImp = Vector2.Dot(impulse * new Vector2((float)Math.Cos(j),
                                (float)Math.Sin(j)), -ro.Normal) * new Vector2((float)Math.Cos(j),
                                    (float)Math.Sin(j));

                            _data[i].Body.ApplyLinearImpulse(ref vectImp, ref hitpoint);

                            // We gather the fixtures for returning them
                            Vector2 val = Vector2.Zero;
                            List<Vector2> vectorList;
                            if (_exploded.TryGetValue(f, out vectorList))
                            {
                                val.X += Math.Abs(vectImp.X);
                                val.Y += Math.Abs(vectImp.Y);

                                vectorList.Add(val);
                            }
                            else
                            {
                                vectorList = new List<Vector2>();
                                val.X = Math.Abs(vectImp.X);
                                val.Y = Math.Abs(vectImp.Y);

                                vectorList.Add(val);
                                _exploded.Add(f, vectorList);
                            }

                            if (minlambda > 1.0f)
                            {
                                hitpoint = p2;
                            }

                            //++jj;
                        }
                    }
                }
            }

            return _exploded;
        }
    }
}