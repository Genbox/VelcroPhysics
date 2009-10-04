using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Controllers;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Dynamics.Joints;
using FarseerGames.FarseerPhysics.Dynamics.Springs;
using FarseerGames.FarseerPhysics.Interfaces;

#if (XNA)
using Microsoft.Xna.Framework;
using System.Diagnostics;
using Microsoft.Xna.Framework.Content;
#else
using FarseerGames.FarseerPhysics.Mathematics;
#endif

namespace FarseerGames.FarseerPhysics
{
    /// <summary>
    /// Keeps track of bodies, geometries, joints, springs, controllers and other dynamics.
    /// </summary>
    public class PhysicsSimulator
    {
        private IBroadPhaseCollider _broadPhaseCollider;
        internal Pool<Arbiter> arbiterPool;

        private List<Body> _bodyAddList;
        private List<Body> _bodyRemoveList;

        private List<Controller> _controllerAddList;
        private List<Controller> _controllerRemoveList;

        private List<Geom> _geomAddList;
        private List<Geom> _geomRemoveList;

        private List<Joint> _jointAddList;
        private List<Joint> _jointRemoveList;

        private List<Spring> _springAddList;
        private List<Spring> _springRemoveList;

#if (XNA)
        private Stopwatch _sw = new Stopwatch();
#endif

        /// <summary>
        /// Get or set the current narrow phase collider.
        /// You can set it to:
        /// NarrowPhaseCollider.DistanceGrid
        /// or
        /// NarrowPhaseCollider.SAT
        /// </summary>
        public static NarrowPhaseCollider NarrowPhaseCollider = NarrowPhaseCollider.DistanceGrid;

        /// <summary>
        /// If false, the whole simulation stops. It still processes added and removed geometries.
        /// </summary>
        public bool Enabled = true;

        public float BiasFactor = .8f;

        /// <summary>
        /// The maximum number of contacts to detect in the narrow phase.
        /// Default is 10.
        /// </summary>
        public static int MaxContactsToDetect = 10;

        /// <summary>
        /// The maximum number of contacts to resolve in the narrow phase.
        /// Default is 4.
        /// </summary>
        public int MaxContactsToResolve = 4;

        /// <summary>
        /// The type of friction.
        /// Default is FrictionType.Average.
        /// </summary>
        public FrictionType FrictionType = FrictionType.Average;

        /// <summary>
        /// The amount of allowed penetration
        /// Default is .05
        /// </summary>
        public float AllowedPenetration = .05f;

        /// <summary>
        /// Gravity applied to all bodies.
        /// </summary>
        public Vector2 Gravity;

        /// <summary>
        /// The number of iterations the engine should do when applying forces.
        /// Incease this number to have a more stable simulation.
        /// Increasing this will affect performance.
        /// </summary>
        public int Iterations = 5;

        /// <summary>
        /// If true, the physics engine will gather info about how
        /// long different parts of the engine takes to run
        /// </summary>
        public bool EnableDiagnostics;

        /// <summary>
        /// Initializes a new instance of the <see cref="PhysicsSimulator"/> class.
        /// Gravity is set to zero.
        /// </summary>
        public PhysicsSimulator()
        {
            ConstructPhysicsSimulator(Vector2.Zero);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PhysicsSimulator"/> class.
        /// </summary>
        /// <param name="gravity">The gravity.</param>
        public PhysicsSimulator(Vector2 gravity)
        {
            UpdateTime = -1;
            UpdatePositionsTime = -1;
            ApplyImpulsesTime = -1;
            ApplyForcesTime = -1;
            NarrowPhaseCollisionTime = -1;
            BroadPhaseCollisionTime = -1;
            CleanUpTime = -1;
            ConstructPhysicsSimulator(gravity);
        }

        /// <summary>
        /// Gets or sets the broad phase collider.
        /// Make sure that the engine does not contain any geoms when setting the broad phase collider.
        /// </summary>
        /// <value>The current broad phase collider.</value>
        /// <exception cref="Exception">The <see cref="GeomList"/> must be empty when setting the broad phase collider type</exception>
        public IBroadPhaseCollider BroadPhaseCollider
        {
            get { return _broadPhaseCollider; }
            set
            {
                if (GeomList.Count > 0)
                    throw new Exception("The GeomList must be empty when setting the broad phase collider type");

                _broadPhaseCollider = value;
            }
        }

#if (XNA)
        [ContentSerializerIgnore, XmlIgnore]
#endif
        public GenericList<Geom> GeomList { get; internal set; }

#if (XNA)
        [ContentSerializerIgnore, XmlIgnore]
#endif
        public GenericList<Body> BodyList { get; internal set; }

#if (XNA)
        [ContentSerializerIgnore, XmlIgnore]
#endif
        public GenericList<Controller> ControllerList { get; internal set; }

#if (XNA)
        [ContentSerializerIgnore, XmlIgnore]
#endif
        public GenericList<Spring> SpringList { get; internal set; }

#if (XNA)
        [ContentSerializerIgnore, XmlIgnore]
#endif
        public GenericList<Joint> JointList { get; internal set; }

#if (XNA)
        [ContentSerializerIgnore, XmlIgnore]
#endif
        public ArbiterList ArbiterList { get; internal set; }

#if (XNA)
        [ContentSerializerIgnore, XmlIgnore]
#endif
        public float CleanUpTime { get; internal set; }

#if (XNA)
        [ContentSerializerIgnore, XmlIgnore]
#endif
        public float BroadPhaseCollisionTime { get; internal set; }

#if (XNA)
        [ContentSerializerIgnore, XmlIgnore]
#endif
        public float NarrowPhaseCollisionTime { get; internal set; }

#if (XNA)
        [ContentSerializerIgnore, XmlIgnore]
#endif
        public float ApplyForcesTime { get; internal set; }

#if (XNA)
        [ContentSerializerIgnore, XmlIgnore]
#endif
        public float ApplyImpulsesTime { get; internal set; }

#if (XNA)
        [ContentSerializerIgnore, XmlIgnore]
#endif
        public float UpdatePositionsTime { get; internal set; }

#if (XNA)
        [ContentSerializerIgnore, XmlIgnore]
#endif
        public float UpdateTime { get; internal set; }

        private void ConstructPhysicsSimulator(Vector2 gravity)
        {
            GeomList = new GenericList<Geom>(32);
            _geomAddList = new List<Geom>(32);
            _geomRemoveList = new List<Geom>(32);

            BodyList = new GenericList<Body>(32);
            _bodyAddList = new List<Body>(32);
            _bodyRemoveList = new List<Body>(32);

            ControllerList = new GenericList<Controller>(8);
            _controllerAddList = new List<Controller>(8);
            _controllerRemoveList = new List<Controller>(8);

            JointList = new GenericList<Joint>(32);
            _jointAddList = new List<Joint>(32);
            _jointRemoveList = new List<Joint>(32);

            SpringList = new GenericList<Spring>(32);
            _springAddList = new List<Spring>(32);
            _springRemoveList = new List<Spring>(32);

            _broadPhaseCollider = new SelectiveSweepCollider(this);

            Gravity = gravity;

            //Create arbiter list with default capacity of 128
            ArbiterList = new ArbiterList(128);

            //Poolsize of 128, will grow as needed.
            arbiterPool = new Pool<Arbiter>(128);
        }

        public void Add(Geom geometry)
        {
            if (geometry == null)
                throw new ArgumentNullException("geometry", "Can't add null geometry");

            if (!_geomAddList.Contains(geometry))
            {
                _geomAddList.Add(geometry);
            }
        }

        public void Remove(Geom geometry)
        {
            if (geometry == null)
                throw new ArgumentNullException("geometry", "Can't remove null geometry");

            _geomRemoveList.Add(geometry);
        }

        public void Add(Body body)
        {
            if (body == null)
                throw new ArgumentNullException("body", "Can't add null body");

            if (!_bodyAddList.Contains(body))
            {
                _bodyAddList.Add(body);
            }
        }

        public void Remove(Body body)
        {
            if (body == null)
                throw new ArgumentNullException("body", "Can't remove null body");

            _bodyRemoveList.Add(body);
        }

        public void Add(Controller controller)
        {
            if (controller == null)
                throw new ArgumentNullException("controller", "Can't add null controller");

            if (!_controllerAddList.Contains(controller))
            {
                _controllerAddList.Add(controller);
            }
        }

        public void Remove(Controller controller)
        {
            if (controller == null)
                throw new ArgumentNullException("controller", "Can't remove null controller");

            _controllerRemoveList.Add(controller);
        }

        public void Add(Joint joint)
        {
            if (joint == null)
                throw new ArgumentNullException("joint", "Can't add null joint");

            if (!_jointAddList.Contains(joint))
            {
                _jointAddList.Add(joint);
            }
        }

        public void Remove(Joint joint)
        {
            if (joint == null)
                throw new ArgumentNullException("joint", "Can't remove null joint");

            _jointRemoveList.Add(joint);
        }

        public void Add(Spring spring)
        {
            if (spring == null)
                throw new ArgumentNullException("spring", "Can't add null spring");

            if (!_springAddList.Contains(spring))
            {
                _springAddList.Add(spring);
            }
        }

        public void Remove(Spring spring)
        {
            if (spring == null)
                throw new ArgumentNullException("spring", "Can't remove null spring");

            _springRemoveList.Add(spring);
        }

        /// <summary>
        /// Resets the physics simulator back to it's original state. Only gravity is persisted.
        /// </summary>
        public void Clear()
        {
            ConstructPhysicsSimulator(Gravity);
        }

        public void Update(float dt)
        {
            Update(dt, 0);
        }

        /// <summary>
        /// Add new bodies and geometries to the engine without doing an update.
        /// Also removes bodies and geometries. If there is any disposed items, those will get
        /// removed too.
        /// </summary>
        public void ProcessAddedAndRemoved()
        {
            ProcessAddedItems();
            ProcessRemovedItems();
            ProcessDisposedItems();
        }

        /// <summary>
        /// Updates the physics simulator with the specified time change.
        /// </summary>
        /// <param name="dt">The delta time.</param>
        /// <param name="dtReal">The real delta time.</param>
        public void Update(float dt, float dtReal)
        {
#if (XNA)
            if (EnableDiagnostics) _sw.Start();
#endif

            ProcessAddedItems();
            ProcessRemovedItems();
            ProcessDisposedItems();

            ArbiterList.PrepareForBroadphaseCollision(GeomList);

#if (XNA)
            if (EnableDiagnostics)
                CleanUpTime = _sw.ElapsedTicks;
#endif

            //If there is no change in time, no need to calculate anything.
            if (dt == 0 || !Enabled)
                return;

            DoBroadPhaseCollision();
#if (XNA)
            if (EnableDiagnostics)
                BroadPhaseCollisionTime = _sw.ElapsedTicks - CleanUpTime;
#endif
            DoNarrowPhaseCollision();
#if (XNA)
            if (EnableDiagnostics)
                NarrowPhaseCollisionTime = _sw.ElapsedTicks - BroadPhaseCollisionTime - CleanUpTime;
#endif
            ApplyForces(dt, dtReal);
#if (XNA)
            if (EnableDiagnostics)
                ApplyForcesTime = _sw.ElapsedTicks - NarrowPhaseCollisionTime - BroadPhaseCollisionTime - CleanUpTime;
#endif
            ApplyImpulses(dt);

#if (XNA)
            if (EnableDiagnostics)
                ApplyImpulsesTime = _sw.ElapsedTicks - ApplyForcesTime - NarrowPhaseCollisionTime - BroadPhaseCollisionTime - CleanUpTime;
#endif
            UpdatePositions(dt);
#if (XNA)
            if (EnableDiagnostics)
                UpdatePositionsTime = _sw.ElapsedTicks - ApplyImpulsesTime - ApplyForcesTime - NarrowPhaseCollisionTime - BroadPhaseCollisionTime - CleanUpTime;
#endif
#if (XNA)

            if (EnableDiagnostics)
            {
                _sw.Stop();
                UpdateTime = _sw.ElapsedTicks;

                CleanUpTime = 1000 * CleanUpTime / Stopwatch.Frequency;
                BroadPhaseCollisionTime = 1000 * BroadPhaseCollisionTime / Stopwatch.Frequency;
                NarrowPhaseCollisionTime = 1000 * NarrowPhaseCollisionTime / Stopwatch.Frequency;
                ApplyForcesTime = 1000 * ApplyForcesTime / Stopwatch.Frequency;
                ApplyImpulsesTime = 1000 * ApplyImpulsesTime / Stopwatch.Frequency;
                UpdatePositionsTime = 1000 * UpdatePositionsTime / Stopwatch.Frequency;
                UpdateTime = 1000 * UpdateTime / Stopwatch.Frequency;
                _sw.Reset();
            }
#endif
        }

        /// <summary>
        /// Checks if the <see cref="PhysicsSimulator"/> geoms collide with the specified X and Y coordinates.
        /// </summary>
        /// <param name="x">The x value</param>
        /// <param name="y">The y value</param>
        /// <returns>The first geom that collides with the specified x and y coordinates</returns>
        public Geom Collide(float x, float y)
        {
            return Collide(new Vector2(x, y));
        }

        /// <summary>
        /// Checks if the <see cref="PhysicsSimulator"/> geoms collide with the specified point.
        /// </summary>
        /// <param name="point">The point to check against.</param>
        /// <returns>The first geom that collides with the specified point</returns>
        public Geom Collide(Vector2 point)
        {
            foreach (Geom geometry in GeomList)
            {
                if (geometry.Collide(point))
                {
                    return geometry;
                }
            }
            return null;
        }

        /// <summary>
        /// Finds all geoms that collides with the specified X and Y coordinates
        /// </summary>
        /// <param name="x">The x value.</param>
        /// <param name="y">The y value.</param>
        /// <returns>A list of geoms that collided with the X and Y values</returns>
        public List<Geom> CollideAll(float x, float y)
        {
            return CollideAll(new Vector2(x, y));
        }

        /// <summary>
        /// Finds all geoms that collides with the specified point
        /// </summary>
        /// <param name="point">The point to check against.</param>
        /// <returns>A list of geoms that collided with the point</returns>
        public List<Geom> CollideAll(Vector2 point)
        {
            List<Geom> returnGeomList = new List<Geom>();
            foreach (Geom geom in GeomList)
            {
                if (geom.Collide(point))
                {
                    returnGeomList.Add(geom);
                }
            }
            return returnGeomList;
        }

        /// <summary>
        /// Does the broad phase collision detection.
        /// The broad phase is responsible for finding geometries that are in close
        /// vicinity (collideable) to each other.
        /// </summary>
        private void DoBroadPhaseCollision()
        {
            _broadPhaseCollider.Update();
        }

        /// <summary>
        /// Does the narrow phase collision detection.
        /// The narrow phase checks collisionpairs found in the broad phase in detail.
        /// This phase creates contacts between geometries and then applies impulse to them.
        /// </summary>
        private void DoNarrowPhaseCollision()
        {
            for (int i = 0; i < ArbiterList.Count; i++)
            {
                ArbiterList[i].Collide();
            }
            ArbiterList.RemoveContactCountEqualsZero(arbiterPool);
        }

        /// <summary>
        /// Applies the forces to all controllers, springs, joints and bodies.
        /// This step also
        /// </summary>
        /// <param name="dt">The delta time.</param>
        /// <param name="dtReal">The real delta time.</param>
        private void ApplyForces(float dt, float dtReal)
        {
            for (int i = 0; i < ControllerList.Count; i++)
            {
                if (!ControllerList[i].Enabled || ControllerList[i].IsDisposed)
                    continue;

                ControllerList[i].Update(dt, dtReal);
            }

            for (int i = 0; i < SpringList.Count; i++)
            {
                if (!SpringList[i].Enabled || SpringList[i].IsDisposed)
                    continue;

                SpringList[i].Update(dt);
            }

            for (int i = 0; i < BodyList.Count; i++)
            {
                Body body = BodyList[i];
                if (!body.Enabled || body.isStatic || body.IsDisposed)
                    continue;

                //Apply accumulated external impules
                body.ApplyImpulses();

                if (!body.IgnoreGravity)
                {
                    body.force.X = body.force.X + (Gravity.X * body.mass);
                    body.force.Y = body.force.Y + (Gravity.Y * body.mass);
                }

                body.IntegrateVelocity(dt);
                body.ClearForce();
                body.ClearTorque();
            }
        }

        /// <summary>
        /// Applies the impulses to all joints and arbiters.
        /// </summary>
        /// <param name="dt">The delta time.</param>
        private void ApplyImpulses(float dt)
        {
            float inverseDt = 1f / dt;

            for (int i = 0; i < JointList.Count; i++)
            {
                if (!JointList[i].Enabled || JointList[i].IsDisposed)
                    continue;

                JointList[i].PreStep(inverseDt);
            }

            for (int i = 0; i < ArbiterList.Count; i++)
            {
                if (!ArbiterList[i].GeometryA.CollisionResponseEnabled || !ArbiterList[i].GeometryB.CollisionResponseEnabled)
                    continue;

                ArbiterList[i].PreStepImpulse(ref inverseDt);
            }

            for (int h = 0; h < Iterations; h++)
            {
                for (int i = 0; i < JointList.Count; i++)
                {
                    if (!JointList[i].Enabled || JointList[i].IsDisposed)
                        continue;

                    JointList[i].Update();
                }

                for (int i = 0; i < ArbiterList.Count; i++)
                {
                    if (!ArbiterList[i].GeometryA.CollisionResponseEnabled || !ArbiterList[i].GeometryB.CollisionResponseEnabled)
                        continue;

                    ArbiterList[i].ApplyImpulse();
                }
            }
        }

        /// <summary>
        /// Updates the position on all bodies.
        /// </summary>
        /// <param name="dt">The delta time.</param>
        private void UpdatePositions(float dt)
        {
            for (int i = 0; i < BodyList.Count; i++)
            {
                if (!BodyList[i].Enabled || BodyList[i].isStatic || BodyList[i].IsDisposed)
                    continue;

                BodyList[i].IntegratePosition(dt);
            }
        }

        /// <summary>
        /// Processes the added geometries, springs, joints, bodies and controllers.
        /// </summary>
        private void ProcessAddedItems()
        {
            //Add any new geometries
            _tempCount = _geomAddList.Count;
            for (int i = 0; i < _tempCount; i++)
            {
                if (!GeomList.Contains(_geomAddList[i]))
                {
                    _geomAddList[i].InSimulation = true;
                    GeomList.Add(_geomAddList[i]);

                    //Add the new geometry to the broad phase collider.
                    _broadPhaseCollider.Add(_geomAddList[i]);
                }
            }
            _geomAddList.Clear();

            //Add any new bodies
            _tempCount = _bodyAddList.Count;
            for (int i = 0; i < _tempCount; i++)
            {
                if (!BodyList.Contains(_bodyAddList[i]))
                {
                    BodyList.Add(_bodyAddList[i]);
                }
            }
            _bodyAddList.Clear();

            //Add any new controllers
            _tempCount = _controllerAddList.Count;
            for (int i = 0; i < _tempCount; i++)
            {
                if (!ControllerList.Contains(_controllerAddList[i]))
                {
                    ControllerList.Add(_controllerAddList[i]);
                }
            }
            _controllerAddList.Clear();

            //Add any new joints
            _tempCount = _jointAddList.Count;
            for (int i = 0; i < _tempCount; i++)
            {
                if (!JointList.Contains(_jointAddList[i]))
                {
                    JointList.Add(_jointAddList[i]);
                }
            }
            _jointAddList.Clear();

            //Add any new springs
            _tempCount = _springAddList.Count;
            for (int i = 0; i < _tempCount; i++)
            {
                if (!SpringList.Contains(_springAddList[i]))
                {
                    SpringList.Add(_springAddList[i]);
                }
            }
            _springAddList.Clear();
        }

        /// <summary>
        /// Processes the removed geometries (and their arbiters), bodies, controllers, joints and springs.
        /// </summary>
        private void ProcessRemovedItems()
        {
            //Remove any new geometries
            _tempCount = _geomRemoveList.Count;
            for (int i = 0; i < _tempCount; i++)
            {
                _geomRemoveList[i].InSimulation = false;
                GeomList.Remove(_geomRemoveList[i]);

                //Remove any arbiters associated with the geometries being removed
                for (int j = ArbiterList.Count; j > 0; j--)
                {
                    if (ArbiterList[j - 1].GeometryA == _geomRemoveList[i] ||
                        ArbiterList[j - 1].GeometryB == _geomRemoveList[i])
                    {
                        //TODO: Should we create a RemoveComplete method and remove all Contacts associated
                        //with the arbiter?
                        arbiterPool.Insert(ArbiterList[j - 1]);
                        ArbiterList.Remove(ArbiterList[j - 1]);
                    }
                }
            }

            if (_geomRemoveList.Count > 0)
            {
                _broadPhaseCollider.ProcessRemovedGeoms();
            }

            _geomRemoveList.Clear();

            //Remove any new bodies
            _tempCount = _bodyRemoveList.Count;
            for (int i = 0; i < _tempCount; i++)
            {
                BodyList.Remove(_bodyRemoveList[i]);
            }
            _bodyRemoveList.Clear();

            //Remove any new controllers
            _tempCount = _controllerRemoveList.Count;
            for (int i = 0; i < _tempCount; i++)
            {
                ControllerList.Remove(_controllerRemoveList[i]);
            }
            _controllerRemoveList.Clear();

            //Remove any new joints
            int jointRemoveCount = _jointRemoveList.Count;
            for (int i = 0; i < jointRemoveCount; i++)
            {
                JointList.Remove(_jointRemoveList[i]);
            }
            _jointRemoveList.Clear();

            //Remove any new springs
            _tempCount = _springRemoveList.Count;
            for (int i = 0; i < _tempCount; i++)
            {
                SpringList.Remove(_springRemoveList[i]);
            }
            _springRemoveList.Clear();
        }

        /// <summary>
        /// Processes the disposed controllers, joints, springs, bodies and cleans up the arbiter list.
        /// </summary>
        private void ProcessDisposedItems()
        {
            //Allow each controller to validate itself. this is where a controller can Dispose of itself if need be.
            for (int i = 0; i < ControllerList.Count; i++)
            {
                ControllerList[i].Validate();
            }

            //Allow each joint to validate itself. this is where a joint can Dispose of itself if need be.
            for (int i = 0; i < JointList.Count; i++)
            {
                JointList[i].Validate();
            }

            //Allow each spring to validate itself. this is where a spring can Dispose of itself if need be.
            for (int i = 0; i < SpringList.Count; i++)
            {
                SpringList[i].Validate();
            }

            _tempCount = GeomList.RemoveDisposed();

            if (_tempCount > 0)
            {
                _broadPhaseCollider.ProcessDisposedGeoms();
            }

            BodyList.RemoveDisposed();
            ControllerList.RemoveDisposed();
            SpringList.RemoveDisposed();
            JointList.RemoveDisposed();

            //Clean up the arbiterlist
            ArbiterList.CleanArbiterList(arbiterPool);
        }

        #region Temp variables
        private int _tempCount;
        #endregion
    }
}