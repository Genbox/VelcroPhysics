using System;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics.Joints;
using FarseerGames.FarseerPhysics.Dynamics.Springs;
using FarseerGames.FarseerPhysics.Factories;
#if(XNA)
using Microsoft.Xna.Framework;
#else
using FarseerGames.FarseerPhysics.Mathematics;
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
        private GenericList<Body> _bodies; // holds all bodies for this path
        private Vertices _controlPoints; // holds all control points for this path
        private GenericList<Geom> _geoms; // holds all geoms for this path
        private GenericList<Spring> _springs; // holds all springs for this path
        private float _height; // width and height of bodies to create
        private GenericList<Joint> _joints; // holds all the joints for this path
        private bool _loop; // is this path a loop
        private float _mass; // mass of bodies to create
        private bool _recalculate = true; // will be set to true if path needs to be recalculated
        private float _width; // width and height of bodies to create
        private float _linkWidth; // distance between links. Decoupled from body width.

        /// <summary>
        /// Initializes a new instance of the <see cref="Path"/> class.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="mass">The mass.</param>
        /// <param name="endless">if set to <c>true</c> [endless].</param>
        public Path(float width, float height, float mass, bool endless)
            : this(width, height, width, mass, endless)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Path"/> class.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="linkWidth">The distance between links.</param> 
        /// <param name="mass">The mass.</param>
        /// <param name="endless">if set to <c>true</c> [endless].</param>
        public Path(float width, float height, float linkWidth, float mass, bool endless)
        {
            _width = width;
            _linkWidth = linkWidth;
            _height = height;
            _loop = endless;
            _mass = mass;
            _geoms = new GenericList<Geom>(8);
            _bodies = new GenericList<Body>(8);
            _joints = new GenericList<Joint>(8);
            _springs = new GenericList<Spring>(8);
            _controlPoints = new Vertices();
        }

        /// <summary>
        /// Gets the bodies.
        /// </summary>
        /// <Value>The bodies.</Value>
        public GenericList<Body> Bodies
        {
            get { return _bodies; }
        }

        /// <summary>
        /// Gets the joints.
        /// </summary>
        /// <Value>The joints.</Value>
        public GenericList<Joint> Joints
        {
            get { return _joints; }
        }

        /// <summary>
        /// Gets the geoms.
        /// </summary>
        /// <Value>The geoms.</Value>
        public GenericList<Geom> Geoms
        {
            get { return _geoms; }
        }

        /// <summary>
        /// Gets the springs.
        /// </summary>
        /// <Value>The springs.</Value>
        public GenericList<Spring> Springs
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
        /// <param name="min">The min.</param>
        /// <param name="max">The max.</param>
        /// <param name="springConstant">The spring constant.</param>
        /// <param name="dampingConstant">The damping constant.</param>
        public void LinkBodies(LinkType type, float min, float max, float springConstant, float dampingConstant)
        {
            LinkBodies(type, min, max, springConstant, dampingConstant, 1f);
        }

        /// <summary>
        /// Links the bodies.
        /// </summary>
        /// <param name="type">The type of Joint to link with.</param>
        /// <param name="min">The min.</param>
        /// <param name="max">The max.</param>
        /// <param name="springConstant">The spring constant.</param>
        /// <param name="dampingConstant">The damping constant.</param>
        /// <param name="springRestLengthFactor">The spring rest length factor</param>
        public void LinkBodies(LinkType type, float min, float max, float springConstant, float dampingConstant, float springRestLengthFactor)
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
                    if (_bodies[i].position.X < _bodies[i + 1].position.X)
                    {
                        midDeltaX = Math.Abs(_bodies[i].position.X - _bodies[i + 1].position.X) * 0.5f;
                        // find x axis midpoint
                    }
                    else
                    {
                        midDeltaX = (_bodies[i + 1].position.X - _bodies[i].position.X) * 0.5f; // find x axis midpoint
                    }
                    if (_bodies[i].position.Y < _bodies[i + 1].position.Y)
                    {
                        midDeltaY = Math.Abs(_bodies[i].position.Y - _bodies[i + 1].position.Y) * 0.5f;
                        // find x axis midpoint
                    }
                    else
                    {
                        midDeltaY = (_bodies[i + 1].position.Y - _bodies[i].position.Y) * 0.5f; // find x axis midpoint
                    }

                    midPoint = new Vector2(_bodies[i].position.X + midDeltaX, _bodies[i].position.Y + midDeltaY);
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
                            if (_bodies[i].Position.X < _bodies[i + 1].Position.X)
                            {
                                linearSpring = SpringFactory.Instance.CreateLinearSpring(_bodies[i], new Vector2(_width / 2.0f, 0), _bodies[i + 1],
                                    new Vector2(-_width / 2.0f, 0), springConstant, dampingConstant);
                            }
                            else
                            {
                                linearSpring = SpringFactory.Instance.CreateLinearSpring(_bodies[i], new Vector2(-_width / 2.0f, 0), _bodies[i + 1],
                                    new Vector2(_width / 2.0f, 0), springConstant, dampingConstant);
                            }
                            if (i >= 1)
                            {
                                a = (LinearSpring)_springs[i - 1];
                                linearSpring.RestLength = Vector2.Distance(a.AttachPoint2, linearSpring.AttachPoint1) * springRestLengthFactor;
                            }
                            _springs.Add(linearSpring);
                            break;
                        case LinkType.PinJoint:
                            if (_bodies[i].Position.X < _bodies[i + 1].Position.X)
                            {
                                pinJoint = JointFactory.Instance.CreatePinJoint(_bodies[i], new Vector2(_width / 2.0f, 0), _bodies[i + 1], new Vector2(-_width / 2.0f, 0));
                            }
                            else
                            {
                                pinJoint = JointFactory.Instance.CreatePinJoint(_bodies[i], new Vector2(-_width / 2.0f, 0), _bodies[i + 1], new Vector2(_width / 2.0f, 0));
                            }
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
                            if (_bodies[i].Position.X < _bodies[i + 1].Position.X)
                            {
                                sliderJoint = JointFactory.Instance.CreateSliderJoint(_bodies[i], new Vector2(_width / 2.0f, 0), _bodies[i + 1], new Vector2(-_width / 2.0f, 0), min, max);
                            }
                            else
                            {
                                sliderJoint = JointFactory.Instance.CreateSliderJoint(_bodies[i], new Vector2(-_width / 2.0f, 0), _bodies[i + 1], new Vector2(_width / 2.0f, 0), min, max);
                            }
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
                if (_bodies[0].position.X < _bodies[_bodies.Count - 1].position.X)
                {
                    midDeltaX = Math.Abs(_bodies[0].position.X - _bodies[_bodies.Count - 1].position.X) * 0.5f;
                    // find x axis midpoint
                }
                else
                {
                    midDeltaX = (_bodies[_bodies.Count - 1].position.X - _bodies[0].position.X) * 0.5f;
                    // find x axis midpoint
                }
                if (_bodies[0].position.Y < _bodies[_bodies.Count - 1].position.Y)
                {
                    midDeltaY = Math.Abs(_bodies[0].position.Y - _bodies[_bodies.Count - 1].position.Y) * 0.5f;
                    // find x axis midpoint
                }
                else
                {
                    midDeltaY = (_bodies[_bodies.Count - 1].position.Y - _bodies[0].position.Y) * 0.5f;
                    // find x axis midpoint
                }

                midPoint = new Vector2(_bodies[0].position.X + midDeltaX, _bodies[0].position.Y + midDeltaY);
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
        /// Adds the path to the physics simulator.
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
        /// Remove all the elements of a path from the physics simulator.
        /// </summary>
        /// <param name="physicsSimulator"></param>
        public void RemoveFromPhysicsSimulator(PhysicsSimulator physicsSimulator)
        {
            foreach (Body body in _bodies)
                physicsSimulator.Remove(body);
            foreach (Geom geom in _geoms)
                physicsSimulator.Remove(geom);
            foreach (Joint joint in _joints)
                physicsSimulator.Remove(joint);
            foreach (Spring spring in _springs)
                physicsSimulator.Remove(spring);
        }    

        /// <summary>
        /// Creates rectangular geoms that match the size of the bodies.
        /// Then adds the geometries to the given physics simulator.
        /// </summary>
        /// <param name="physicsSimulator">The physics simulator.</param>
        /// <param name="collisionGroup">The collision group.</param>
        public void CreateGeoms(PhysicsSimulator physicsSimulator, int collisionGroup)
        {
            CreateGeoms(collisionGroup);

            foreach (Geom geom in _geoms)
            {
                physicsSimulator.Add(geom);
            }
        }

        /// <summary>
        /// Creates rectangular geoms that match the size of the bodies.
        /// </summary>
        /// <param name="collisionGroup">The collision group.</param>
        public void CreateGeoms(int collisionGroup)
        {
            foreach (Body body in _bodies)
            {
                Geom geom = GeomFactory.Instance.CreateRectangleGeom(body, _width, _height);
                geom.CollisionGroup = collisionGroup;
                _geoms.Add(geom);
            }
        }

        /// <summary>
        /// Creates rectangular geoms that match the size of the bodies.
        /// Then adds the geometries to the given physics simulator.
        /// </summary>
        /// <param name="collisionCategory">The collision category of the geometries.</param>
        /// <param name="collidesWith">The collisioncategory the geometries should collide with..</param>
        /// <param name="physicsSimulator">The physics simulator.</param>
        public void CreateGeoms(CollisionCategory collisionCategory, CollisionCategory collidesWith, PhysicsSimulator physicsSimulator)
        {
            CreateGeoms(collisionCategory, collidesWith);

            foreach (Geom geom in _geoms)
            {
                physicsSimulator.Add(geom);
            }
        }

        /// <summary>
        /// Creates rectangular geoms that match the size of the bodies.
        /// </summary>
        /// <param name="collisionCategory">The collision category.</param>
        /// <param name="collidesWith">What collision group geometries collides with.</param>
        public void CreateGeoms(CollisionCategory collisionCategory, CollisionCategory collidesWith)
        {
            foreach (Body body in _bodies)
            {
                Geom geom = GeomFactory.Instance.CreateRectangleGeom(body, _width, _height);
                geom.CollisionCategories = collisionCategory;
                geom.CollidesWith = collidesWith;
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
            Vector2 temp1;
            Vector2 temp2;

            foreach (Vector2 controlPoint in _controlPoints)
            {
                temp1 = new Vector2(controlPoint.X - (_controlPointSize / 2), controlPoint.Y - (_controlPointSize / 2));
                temp2 = new Vector2(controlPoint.X + (_controlPointSize / 2), controlPoint.Y + (_controlPointSize / 2));

                controlPointAABB = new AABB(ref temp1, ref temp2);

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
            Body tempBody;
            Vector2 tempVectorA = new Vector2();
            Vector2 tempVectorC = new Vector2();

            if (_recalculate) // only do the update if something has changed
            {
                float k;

                // first we get our curve ready
                Curve xCurve = new Curve();
                Curve yCurve = new Curve();
                float curveIncrement = 1.0f / _controlPoints.Count;

                for (int i = 0; i < _controlPoints.Count; i++) // for all the control points 
                {
                    k = curveIncrement * (i + 1);
                    xCurve.Keys.Add(new CurveKey(k, _controlPoints[i].X)); // set the keys for x and y
                    yCurve.Keys.Add(new CurveKey(k, _controlPoints[i].Y)); // with a time from 0-1
                }

                k = 0.0f;

                xCurve.ComputeTangents(CurveTangent.Smooth); // compute x tangents
                yCurve.ComputeTangents(CurveTangent.Smooth); // compute y tangents

                // next we find the first point at 1/2 the width because we are finding where the body's center will be placed
                while (distance < (_linkWidth / 2.0f)) // while the distance along the curve is <= to width / 2  
                {
                    k += _precision; // we increment along the line at this precision coeffient
                    tempVectorA = new Vector2(xCurve.Evaluate(k), yCurve.Evaluate(k));
                    distance = Vector2.Distance(_controlPoints[0], tempVectorA); // get the distance
                }

                while (distance < _linkWidth) // while the distance along the curve is <= to width / 2  
                {
                    k += _precision; // we increment along the line at this precision coeffient
                    tempVectorC = new Vector2(xCurve.Evaluate(k), yCurve.Evaluate(k));
                    distance = Vector2.Distance(_controlPoints[0], tempVectorC); // get the distance
                }

                tempBody = BodyFactory.Instance.CreateRectangleBody(_width, _height, _mass); // create the first body
                tempBody.Position = tempVectorA;
                tempBody.Rotation = Vertices.FindNormalAngle(Vertices.FindVertexNormal(_controlPoints[0], tempVectorA, tempVectorC));
                // set the angle

                _bodies.Add(tempBody); // add the first body

                Vector2 tempVectorB = tempVectorA;

                // now that our first body is done we can start on all our other body's
                // since the curve was created with a time of 0-1 we can just stop creating bodies when k is 1
                while (k < 1.0f)
                {
                    distance = 0.0f;
                    // next we find the first point at the width because we are finding where the body's center will be placed
                    while ((distance < _linkWidth) && (k < 1.0f)) // while the distance along the curve is <= to width
                    {
                        k += _precision; // we increment along the line at this precision coeffient
                        tempVectorA = new Vector2(xCurve.Evaluate(k), yCurve.Evaluate(k));
                        distance = Vector2.Distance(tempVectorA, tempVectorB); // get the distance
                    }
                    distance = 0.0f;
                    while ((distance < _linkWidth) && (k < 1.0f)) // while the distance along the curve is <= to width
                    {
                        k += _precision; // we increment along the line at this precision coeffient
                        tempVectorC = new Vector2(xCurve.Evaluate(k), yCurve.Evaluate(k));
                        distance = Vector2.Distance(tempVectorA, tempVectorC); // get the distance
                    }
                    tempBody = BodyFactory.Instance.CreateRectangleBody(_width, _height, _mass);
                    // create the first body
                    tempBody.Position = tempVectorA;
                    tempBody.Rotation = Vertices.FindNormalAngle(Vertices.FindVertexNormal(tempVectorB, tempVectorA, tempVectorC));
                    // set the angle

                    _bodies.Add(tempBody); // add all the rest of the bodies

                    tempVectorB = tempVectorA;
                }
                MoveControlPoint(tempVectorC, _controlPoints.Count - 1);
                _recalculate = false;
            }
        }
    }
}