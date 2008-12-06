using System;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics.Joints;
using FarseerGames.FarseerPhysics.Dynamics.Springs;
using FarseerGames.FarseerPhysics.Factories;
using FarseerGames.FarseerPhysics.Mathematics;
#if(XNA)
using Microsoft.Xna.Framework;
#else

#endif

namespace FarseerGames.FarseerPhysics.Dynamics
{
    /// <summary>
    /// Path Generator. Used to create dynamic paths along control points.
    /// </summary>
    public class Path
    {
        private const float _controlPointSize = 6; // size of control point used in PointInControlPoint
        private const float _precision = 0.0005f; // a coeffient used to decide how precise to place bodies
        private BodyList _bodies; // holds all bodies for this path
        private Vertices _controlPoints; // holds all control points for this path
        private GeomList _geoms; // holds all geoms for this path
        private SpringList _springs; // holds all springs for this path
        private float _height; // width and height of bodies to create
        private JointList _joints; // holds all the joints for this path
        private bool _loop; // is this path a loop
        private float _mass; // width and height of bodies to create
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
            _springs = new SpringList();
            _controlPoints = new Vertices();
        }

        /// <summary>
        /// Gets the bodies.
        /// </summary>
        /// <Value>The bodies.</Value>
        public BodyList Bodies
        {
            get { return _bodies; }
        }

        /// <summary>
        /// Gets the joints.
        /// </summary>
        /// <Value>The joints.</Value>
        public JointList Joints
        {
            get { return _joints; }
        }

        /// <summary>
        /// Gets the geoms.
        /// </summary>
        /// <Value>The geoms.</Value>
        public GeomList Geoms
        {
            get { return _geoms; }
        }

        /// <summary>
        /// Gets the springs.
        /// </summary>
        /// <Value>The springs.</Value>
        public SpringList Springs
        {
            get { return _springs; }
        }

        /// <summary>
        /// Gets the control points.
        /// </summary>
        /// <Value>The control points.</Value>
        public Vertices ControlPoints
        {
            get { return _controlPoints; }
        }

        /// <summary>
        /// Links the bodies.
        /// </summary>
        /// <param name="type">The type of Joint to link with.</param>
        public void LinkBodies(LinkType type, float min, float max, float springConstant, float dampingConstant)
        {
            RevoluteJoint revoluteJoint;
            PinJoint pinJoint, d;
            SliderJoint sliderJoint;
            LinearSpring linearSpring, a;
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
                    switch (type)
                    {
                        case LinkType.RevoluteJoint:
                            revoluteJoint = JointFactory.Instance.CreateRevoluteJoint(_bodies[i], _bodies[i + 1], midPoint);
                            revoluteJoint.BiasFactor = 0.2f;
                            revoluteJoint.Softness = 0.01f;
                            _joints.Add(revoluteJoint);
                            break;
                        case LinkType.LinearSpring:
                            linearSpring = SpringFactory.Instance.CreateLinearSpring(_bodies[i], new Vector2(-_width/2.0f,0), _bodies[i + 1], 
                                new Vector2(_width/2.0f,0), springConstant, dampingConstant);
                            if (i >= 1)
                            {
                                a = (LinearSpring)_springs[i - 1];
                                linearSpring.RestLength = Vector2.Distance(a.AttachPoint2, linearSpring.AttachPoint1);
                            }
                            _springs.Add(linearSpring);
                            break;
                        case LinkType.PinJoint:
                            pinJoint = JointFactory.Instance.CreatePinJoint(_bodies[i], new Vector2(-_width / 2.0f, 0), _bodies[i + 1], new Vector2(_width/2.0f,0));
                            pinJoint.BiasFactor = 0.2f;
                            pinJoint.Softness = 0.01f;
                            if (i >= 1)
                            {
                                d = (PinJoint)_joints[i - 1];
                                pinJoint.TargetDistance = Vector2.Distance(d.Anchor2, pinJoint.Anchor1);
                            }
                            _joints.Add(pinJoint);
                            break;
                        case LinkType.SliderJoint:
                            sliderJoint = JointFactory.Instance.CreateSliderJoint(_bodies[i], new Vector2(-_width / 2.0f, 0), _bodies[i + 1], new Vector2(_width / 2.0f, 0), min, max);
                            sliderJoint.BiasFactor = 0.2f;
                            sliderJoint.Softness = 0.01f;
                            _joints.Add(sliderJoint);
                            break;
                        default:
                            //should never get here
                            break;

                    }
                }
            }
            if (_loop)
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

                switch (type)
                {
                    case LinkType.RevoluteJoint:
                        revoluteJoint = JointFactory.Instance.CreateRevoluteJoint(_bodies[0], _bodies[_bodies.Count - 1], midPoint);
                        revoluteJoint.BiasFactor = 0.2f;
                        revoluteJoint.Softness = 0.01f;
                        _joints.Add(revoluteJoint);
                        break;
                    case LinkType.LinearSpring:
                        linearSpring = SpringFactory.Instance.CreateLinearSpring(_bodies[0], new Vector2(-_width / 2.0f, 0), _bodies[_bodies.Count - 1], new Vector2(_width / 2.0f, 0),
                            springConstant, dampingConstant);
                        linearSpring.RestLength = _width;
                        _springs.Add(linearSpring);
                        break;
                    case LinkType.PinJoint:
                        pinJoint = JointFactory.Instance.CreatePinJoint(_bodies[0], new Vector2(-_width / 2.0f, 0), _bodies[_bodies.Count - 1], new Vector2(_width / 2.0f, 0));
                        pinJoint.BiasFactor = 0.2f;
                        pinJoint.Softness = 0.01f;
                        pinJoint.TargetDistance = _width;
                        _joints.Add(pinJoint);
                        break;
                    case LinkType.SliderJoint:
                        sliderJoint = JointFactory.Instance.CreateSliderJoint(_bodies[0], new Vector2(-_width / 2.0f, 0), _bodies[_bodies.Count - 1], new Vector2(_width / 2.0f, 0), min, max);
                        sliderJoint.BiasFactor = 0.2f;
                        sliderJoint.Softness = 0.01f;
                        _joints.Add(sliderJoint);
                        break;
                    default:
                        //should never get here
                        break;

                }
            }
        }

        /// <summary>
        /// Adds to physics simulator.
        /// </summary>
        /// <param name="physicsSimulator">The physics simulator.</param>
        public void AddToPhysicsSimulator(PhysicsSimulator physicsSimulator)
        {
            foreach (Body body in _bodies)
                physicsSimulator.Add(body);
            foreach (Geom geom in _geoms)
                physicsSimulator.Add(geom);
            foreach (Joint joint in _joints)
                physicsSimulator.Add(joint);
            foreach (Spring spring in _springs)
                physicsSimulator.Add(spring);
        }

        /// <summary>
        /// Creates rectangular geoms that match the size of the bodies.
        /// </summary>
        public void CreateGeoms()
        {
            Geom geom;
            foreach (Body body in _bodies)
            {
                geom = GeomFactory.Instance.CreateRectangleGeom(body, _width, _height);
                geom.CollisionCategories = CollisionCategory.Cat2;
                geom.CollidesWith = CollisionCategory.Cat1;
                _geoms.Add(geom);
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

            foreach (Vector2 controlPoint in _controlPoints)
            {
                controlPointAABB =
                    new AABB(
                        new Vector2(controlPoint.X - (_controlPointSize/2), controlPoint.Y - (_controlPointSize/2)),
                        new Vector2(controlPoint.X + (_controlPointSize/2), controlPoint.Y + (_controlPointSize/2)));

                if (controlPointAABB.Contains(ref point))
                    return _controlPoints.IndexOf(controlPoint);
            }
            return -1;
        }

        /// <summary>
        /// Moves a control point.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="index">The index.</param>
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

        /// <summary>
        /// Adds the specified body.
        /// </summary>
        /// <param name="body">The body.</param>
        public void Add(Body body)
        {
            _bodies.Add(body);
        }

        /// <summary>
        /// Adds the specified geom.
        /// </summary>
        /// <param name="geom">The geom.</param>
        public void Add(Geom geom)
        {
            _geoms.Add(geom);
        }

        /// <summary>
        /// Adds the specified joint.
        /// </summary>
        /// <param name="joint">The joint.</param>
        public void Add(Joint joint)
        {
            _joints.Add(joint);
        }

        /// <summary>
        /// Adds the specified spring.
        /// </summary>
        /// <param name="spring">The spring.</param>
        public void Add(Spring spring)
        {
            _springs.Add(spring);
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
        /// <param name="firstVector">First Vector2.</param>
        /// <param name="secondVector">Other Vector2.</param>
        /// <returns>Mid-point Vector2.</returns>
        public static Vector2 FindMidpoint(Vector2 firstVector, Vector2 secondVector)
        {
            float midDeltaX, midDeltaY;

            if (firstVector.X < secondVector.X)
                midDeltaX = Math.Abs((firstVector.X - secondVector.X)*0.5f); // find x axis midpoint
            else
                midDeltaX = (secondVector.X - firstVector.X)*0.5f; // find x axis midpoint
            if (firstVector.Y < secondVector.Y)
                midDeltaY = Math.Abs((firstVector.Y - secondVector.Y)*0.5f); // find y axis midpoint
            else
                midDeltaY = (secondVector.Y - firstVector.Y)*0.5f; // find y axis midpoint

            return (new Vector2(firstVector.X + midDeltaX, firstVector.Y + midDeltaY)); // return mid point
        }

        /// <summary>
        /// Finds the angle of an edge.
        /// </summary>
        /// <param name="firstVector">First Vector2.</param>
        /// <param name="secondVector">Other Vector2.</param>
        /// <returns>Normal of the edge.</returns>
        private Vector2 FindEdgeNormal(Vector2 firstVector, Vector2 secondVector)
        {
            //Xbox360 need this variable to be initialized to Vector2.Zero
            Vector2 n = Vector2.Zero;

            Vector2 t = new Vector2(firstVector.X - secondVector.X, firstVector.Y - secondVector.Y);

            n.X = -t.Y; // get 2D normal
            n.Y = t.X; // works only on counter clockwise polygons

            return n; // we don't bother normalizing because we do this when we find the vertex normal
        }

        private Vector2 FindVertexNormal(Vector2 firstVector, Vector2 secondVector, Vector2 c)
        {
            Vector2 normal = FindEdgeNormal(firstVector, secondVector) + FindEdgeNormal(secondVector, c);

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