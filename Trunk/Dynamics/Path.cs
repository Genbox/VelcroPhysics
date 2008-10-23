using System;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics.Joints;
using FarseerGames.FarseerPhysics.Factories;
#if(XNA)
using Microsoft.Xna.Framework;
#else
using FarseerGames.FarseerPhysics.Mathematics;
#endif

namespace FarseerGames.FarseerPhysics.Dynamics.PathGenerator
{
    public class Path
    {
        private BodyList _bodies; // holds all bodies for this path
        private Vertices _controlPoints; // holds all control points for this path
        private const float _controlPointSize = 6; // size of control point used in PointInControlPoint
        private GeomList _geoms; // holds all geoms for this path
        private float _height; // width and height of bodies to create
        private JointList _joints; // holds all the joints for this path
        private bool _loop; // is this path a loop
        private float _mass; // width and height of bodies to create
        private const float _precision = 0.0005f; // a coeffient used to decide how precise to place bodies
        private bool _recalculate = true; // will be set to true if path needs to be recalculated
        private float _width; // width and height of bodies to create


        /// <summary>
        /// Initializes a new instance of the <see cref="Path"/> class.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="mass">The mass.</param>
        /// <param name="endless">if set to <c>true</c> [endless].</param>
        public Path(float width, float height, float mass, bool endless)
        {
            _width = width;
            _height = height;
            _loop = endless;
            _mass = mass;
            _geoms = new GeomList();
            _bodies = new BodyList();
            _joints = new JointList();
            _controlPoints = new Vertices();
        }

        public BodyList Bodies
        {
            get { return _bodies; }
        }

        public JointList Joints
        {
            get { return _joints; }
        }

        public GeomList Geoms
        {
            get { return _geoms; }
        }

        public Vertices ControlPoints
        {
            get { return _controlPoints; }
        }

        // NOTE: I may add an enum here for joint type NEED TO ADD LOOP CODE AND NEEDS TO BE REWRITTEN CAUSE IT LOOKS WAY MORE 
        // COMPLICATED THEN IT IS
        // Type of joints I may include - Pin, Rev, Slider, and maybe linear spring

        /// <summary>
        /// Links the bodies.
        /// </summary>
        public void LinkBodies()
        {
            RevoluteJoint r;
            Vector2 midPoint;
            float midDeltaX;
            float midDeltaY;

            for (int i = 0; i < _bodies.Count; i++)
            {
                if (i < _bodies.Count - 1)
                {
                    if (_bodies[i].Position.X < _bodies[i + 1].Position.X)
                    {
                        midDeltaX = Math.Abs(_bodies[i].Position.X - _bodies[i + 1].Position.X)*0.5f;
                            // find x axis midpoint
                    }
                    else
                    {
                        midDeltaX = (_bodies[i + 1].Position.X - _bodies[i].Position.X)*0.5f; // find x axis midpoint
                    }
                    if (_bodies[i].Position.Y < _bodies[i + 1].Position.Y)
                    {
                        midDeltaY = Math.Abs(_bodies[i].Position.Y - _bodies[i + 1].Position.Y)*0.5f;
                            // find x axis midpoint
                    }
                    else
                    {
                        midDeltaY = (_bodies[i + 1].Position.Y - _bodies[i].Position.Y)*0.5f; // find x axis midpoint
                    }

                    midPoint = new Vector2(_bodies[i].Position.X + midDeltaX, _bodies[i].Position.Y + midDeltaY);
                        // set midPoint

                    r = JointFactory.Instance.CreateRevoluteJoint(_bodies[i], _bodies[i + 1], midPoint);
                    r.BiasFactor = 0.2f;
                    r.Softness = 0.01f;
                    _joints.Add(r);
                }
                else if (_loop)
                {
                    if (_bodies[0].Position.X < _bodies[_bodies.Count - 1].Position.X)
                    {
                        midDeltaX = Math.Abs(_bodies[0].Position.X - _bodies[_bodies.Count - 1].Position.X)*0.5f;
                            // find x axis midpoint
                    }
                    else
                    {
                        midDeltaX = (_bodies[_bodies.Count - 1].Position.X - _bodies[0].Position.X)*0.5f;
                            // find x axis midpoint
                    }
                    if (_bodies[0].Position.Y < _bodies[_bodies.Count - 1].Position.Y)
                    {
                        midDeltaY = Math.Abs(_bodies[0].Position.Y - _bodies[_bodies.Count - 1].Position.Y)*0.5f;
                            // find x axis midpoint
                    }
                    else
                    {
                        midDeltaY = (_bodies[_bodies.Count - 1].Position.Y - _bodies[0].Position.Y)*0.5f;
                            // find x axis midpoint
                    }

                    midPoint = new Vector2(_bodies[0].Position.X + midDeltaX, _bodies[0].Position.Y + midDeltaY);
                        // set midPoint

                    r = JointFactory.Instance.CreateRevoluteJoint(_bodies[0], _bodies[_bodies.Count - 1], midPoint);
                    r.BiasFactor = 0.2f;
                    r.Softness = 0.01f;
                    _joints.Add(r);
                }
            }
        }

        /// <summary>
        /// Adds to physics simulator.
        /// </summary>
        /// <param name="ps">The physics simulator.</param>
        public void AddToPhysicsSimulator(PhysicsSimulator ps)
        {
            foreach (Body b in _bodies)
                ps.Add(b);
            foreach (Geom g in _geoms)
                ps.Add(g);
            foreach (Joint j in _joints)
                ps.Add(j);
        }

        /// <summary>
        /// Creates rectangular geoms that match the size of the bodies.
        /// </summary>
        public void CreateGeoms()
        {
            Geom g;
            foreach (Body b in _bodies)
            {
                g = GeomFactory.Instance.CreateRectangleGeom(b, _width, _height);
                g.CollisionCategories = CollisionCategory.Cat2;
                g.CollidesWith = CollisionCategory.Cat1;
                _geoms.Add(g);
            }
        }

        // This is used in my editing application.
        /// <summary>
        /// Gets index of control point if point is inside it.
        /// </summary>
        /// <param name="point">Point to test against.</param>
        /// <returns>Index of control point or -1 if no intersection.</returns>
        public int PointInControlPoint(Vector2 point)
        {
            AABB controlPointAABB;

            foreach (Vector2 v in _controlPoints)
            {
                controlPointAABB = new AABB(new Vector2(v.X - (_controlPointSize/2), v.Y - (_controlPointSize/2)),
                                            new Vector2(v.X + (_controlPointSize/2), v.Y + (_controlPointSize/2)));

                if (controlPointAABB.Contains(ref point))
                    return _controlPoints.IndexOf(v);
            }
            return -1;
        }

        public void MoveControlPoint(Vector2 position, int index)
        {
            _controlPoints[index] = position;
            _recalculate = true;
        }

        /// <summary>
        /// Adds a control point to the paths end.
        /// </summary>
        /// <param name="controlPoint">Vector2 to add.</param>
        public void Add(Vector2 controlPoint)
        {
            _controlPoints.Add(controlPoint); // then add it
            _recalculate = true; // be sure to recalculate the curve
        }

        public void Add(Body b)
        {
            _bodies.Add(b);
        }

        public void Add(Geom g)
        {
            _geoms.Add(g);
        }

        public void Add(Joint j)
        {
            _joints.Add(j);
        }

        /// <summary>
        /// Removes a control point from the path.
        /// </summary>
        /// <param name="controlPoint">Vector2 to remove.</param>
        public void Remove(Vector2 controlPoint)
        {
            _controlPoints.Remove(controlPoint);
            _recalculate = true; // be sure to recalculate the curve
        }

        /// <summary>
        /// Removes a control point from the path by index.
        /// </summary>
        /// <param name="index">Index of Vector2 to remove.</param>
        public void Remove(int index)
        {
            _controlPoints.RemoveAt(index);
            _recalculate = true; // be sure to recalculate the curve
        }

        /// <summary>
        /// Performs a complete update of the path.
        /// NOTE: should not be performed on a path
        /// in simulation.
        /// </summary>
        public void Update()
        {
            float distance = 0.0f;
            float k;
            Body tempBody;
            Vector2 tempVectorA = new Vector2();
            Vector2 tempVectorB;
            Vector2 tempVectorC = new Vector2();

            if (_recalculate) // only do the update if something has changed
            {
                // first we get our curve ready
                Curve xCurve = new Curve();
                Curve yCurve = new Curve();
                float curveIncrement = 1.0f/_controlPoints.Count;

                for (int i = 0; i < _controlPoints.Count; i++) // for all the control points 
                {
                    k = curveIncrement*(i + 1);
                    xCurve.Keys.Add(new CurveKey(k, _controlPoints[i].X)); // set the keys for x and y
                    yCurve.Keys.Add(new CurveKey(k, _controlPoints[i].Y)); // with a time from 0-1
                }

                k = 0.0f;

                xCurve.ComputeTangents(CurveTangent.Smooth); // compute x tangents
                yCurve.ComputeTangents(CurveTangent.Smooth); // compute y tangents

                // next we find the first point at 1/2 the width because we are finding where the body's center will be placed
                while (distance < (_width/2.0f)) // while the distance along the curve is <= to width / 2  
                {
                    k += _precision; // we increment along the line at this precision coeffient
                    tempVectorA = new Vector2(xCurve.Evaluate(k), yCurve.Evaluate(k));
                    distance = Vector2.Distance(_controlPoints[0], tempVectorA); // get the distance
                }

                while (distance < _width) // while the distance along the curve is <= to width / 2  
                {
                    k += _precision; // we increment along the line at this precision coeffient
                    tempVectorC = new Vector2(xCurve.Evaluate(k), yCurve.Evaluate(k));
                    distance = Vector2.Distance(_controlPoints[0], tempVectorC); // get the distance
                }

                tempBody = BodyFactory.Instance.CreateRectangleBody(_width, _height, _mass); // create the first body
                tempBody.Position = tempVectorA;
                tempBody.Rotation = FindNormalAngle(FindVertexNormal(_controlPoints[0], tempVectorA, tempVectorC));
                    // set the angle

                _bodies.Add(tempBody); // add the first body

                tempVectorB = tempVectorA;

                // now that our first body is done we can start on all our other body's
                // since the curve was created with a time of 0-1 we can just stop creating bodies when k is 1
                while (k < 1.0f)
                {
                    distance = 0.0f;
                    // next we find the first point at the width because we are finding where the body's center will be placed
                    while ((distance < _width) && (k < 1.0f)) // while the distance along the curve is <= to width
                    {
                        k += _precision; // we increment along the line at this precision coeffient
                        tempVectorA = new Vector2(xCurve.Evaluate(k), yCurve.Evaluate(k));
                        distance = Vector2.Distance(tempVectorA, tempVectorB); // get the distance
                    }
                    distance = 0.0f;
                    while ((distance < _width) && (k < 1.0f)) // while the distance along the curve is <= to width
                    {
                        k += _precision; // we increment along the line at this precision coeffient
                        tempVectorC = new Vector2(xCurve.Evaluate(k), yCurve.Evaluate(k));
                        distance = Vector2.Distance(tempVectorA, tempVectorC); // get the distance
                    }
                    tempBody = BodyFactory.Instance.CreateRectangleBody(_width, _height, _mass);
                        // create the first body
                    tempBody.Position = tempVectorA;
                    tempBody.Rotation = FindNormalAngle(FindVertexNormal(tempVectorB, tempVectorA, tempVectorC));
                        // set the angle

                    _bodies.Add(tempBody); // add all the rest of the bodies

                    tempVectorB = tempVectorA;
                }
                MoveControlPoint(tempVectorC, _controlPoints.Count - 1);
                _recalculate = false;
            }
        }

        // NOTE: Below are some internal functions to find things like normals and angles.

        /// <summary>
        /// Finds the mid-point of two Vector2.
        /// </summary>
        /// <param name="a">First Vector2.</param>
        /// <param name="b">Other Vector2.</param>
        /// <returns>Mid-point Vector2.</returns>
        public static Vector2 FindMidpoint(Vector2 a, Vector2 b)
        {
            float midDeltaX, midDeltaY;

            if (a.X < b.X)
                midDeltaX = Math.Abs((a.X - b.X)*0.5f); // find x axis midpoint
            else
                midDeltaX = (b.X - a.X)*0.5f; // find x axis midpoint
            if (a.Y < b.Y)
                midDeltaY = Math.Abs((a.Y - b.Y)*0.5f); // find y axis midpoint
            else
                midDeltaY = (b.Y - a.Y)*0.5f; // find y axis midpoint

            return (new Vector2(a.X + midDeltaX, a.Y + midDeltaY)); // return mid point
        }

        /// <summary>
        /// Finds the angle of an edge.
        /// </summary>
        /// <param name="a">First Vector2.</param>
        /// <param name="b">Other Vector2.</param>
        /// <returns>Normal of the edge.</returns>
        private Vector2 FindEdgeNormal(Vector2 a, Vector2 b)
        {
            Vector2 n, t;

            t = new Vector2(a.X - b.X, a.Y - b.Y);

            n.X = -t.Y; // get 2D normal
            n.Y = t.X; // works only on counter clockwise polygons

            return n; // we don't bother normalizing because we do this when we find the vertex normal
        }

        private Vector2 FindVertexNormal(Vector2 a, Vector2 b, Vector2 c)
        {
            Vector2 normal = FindEdgeNormal(a, b) + FindEdgeNormal(b, c);

            normal.Normalize();

            return normal;
        }

        private float FindNormalAngle(Vector2 n)
        {
            if ((n.Y > 0.0f) && (n.X > 0.0f))
                return (float) Math.Atan(n.X/-n.Y);
            
            if ((n.Y < 0.0f) && (n.X > 0.0f))
                return (float) Math.Atan(n.X/-n.Y); // good
            
            if ((n.Y > 0.0f) && (n.X < 0.0f))
                return (float) Math.Atan(-n.X/n.Y);
            
            if ((n.Y < 0.0f) && (n.X < 0.0f))
                return (float) Math.Atan(-n.X/n.Y); // good

            return 0.0f;
        }
    }
}