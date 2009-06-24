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
        public static NarrowPhaseCollider NarrowPhaseCollider = NarrowPhaseCollider.DistanceGrid;

        internal ArbiterList arbiterList;
        internal Pool<Arbiter> arbiterPool;

        internal List<Body> bodyAddList;
        internal GenericList<Body> bodyList;
        internal List<Body> bodyRemoveList;

        internal List<Controller> controllerAddList;
        internal GenericList<Controller> controllerList;
        internal List<Controller> controllerRemoveList;

        internal List<Geom> geomAddList;
        internal GenericList<Geom> geomList;
        internal List<Geom> geomRemoveList;

        internal List<Joint> jointAddList;
        internal GenericList<Joint> jointList;
        internal List<Joint> jointRemoveList;

        internal List<Spring> springAddList;
        internal GenericList<Spring> springList;
        internal List<Spring> springRemoveList;

#if (XNA)
        private Stopwatch _sw = new Stopwatch();
#endif
        internal float applyForcesTime = -1;
        internal float applyImpulsesTime = -1;
        internal float broadPhaseCollisionTime = -1;
        internal float narrowPhaseCollisionTime = -1;
        internal float updatePositionsTime = -1;
        internal float updateTime = -1;
        internal float cleanUpTime = -1;

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
                if (geomList.Count > 0)
                    throw new Exception("The GeomList must be empty when setting the broad phase collider type");

                _broadPhaseCollider = value;
            }
        }

#if(XNA)
        [ContentSerializerIgnore]
#endif
        [XmlIgnore]
        /// <summary>
        /// Fully exposed for convenience. Should be treated as. Do not add or remove directly from this list.
        /// </summary>
        public GenericList<Geom> GeomList
        {
            get { return geomList; }
        }

#if(XNA)
        [ContentSerializerIgnore]
#endif
        [XmlIgnore]
        /// <summary>
        /// Fully exposed for convenience. Should be treated as. Do not add or remove directly from this list.
        /// </summary>
        public GenericList<Body> BodyList
        {
            get { return bodyList; }
        }

#if(XNA)
        [ContentSerializerIgnore]
#endif
        [XmlIgnore]
        /// <summary>
        /// Fully exposed for convenience. Should be treated as. Do not add or remove directly from this list.
        /// </summary>
        public GenericList<Controller> ControllerList
        {
            get { return controllerList; }
        }

#if(XNA)
        [ContentSerializerIgnore]
#endif
        [XmlIgnore]
        /// <summary>
        /// Fully exposed for convenience. Should be treated as. Do not add or remove directly from this list.
        /// </summary>
        public GenericList<Spring> SpringList
        {
            get { return springList; }
        }

#if(XNA)
        [ContentSerializerIgnore]
#endif
        [XmlIgnore]
        /// <summary>
        /// Fully exposed for convenience. Should be treated as. Do not add or remove directly from this list.
        /// </summary>
        public GenericList<Joint> JointList
        {
            get { return jointList; }
        }

#if(XNA)
        [ContentSerializerIgnore]
#endif
        [XmlIgnore]
        /// <summary>
        /// Fully exposed for convenience. Should be treated as. Do not add or remove directly from this list.
        /// </summary>
        public ArbiterList ArbiterList
        {
            get { return arbiterList; }
        }

        public float CleanUpTime
        {
            get { return cleanUpTime; }
        }

        public float BroadPhaseCollisionTime
        {
            get { return broadPhaseCollisionTime; }
        }

        public float NarrowPhaseCollisionTime
        {
            get { return narrowPhaseCollisionTime; }
        }

        public float ApplyForcesTime
        {
            get { return applyForcesTime; }
        }

        public float ApplyImpulsesTime
        {
            get { return applyImpulsesTime; }
        }

        public float UpdatePositionsTime
        {
            get { return updatePositionsTime; }
        }

        public float UpdateTime
        {
            get { return updateTime; }
        }

        private void ConstructPhysicsSimulator(Vector2 gravity)
        {
            geomList = new GenericList<Geom>();
            geomAddList = new List<Geom>();
            geomRemoveList = new List<Geom>();

            bodyList = new GenericList<Body>();
            bodyAddList = new List<Body>();
            bodyRemoveList = new List<Body>();

            controllerList = new GenericList<Controller>();
            controllerAddList = new List<Controller>();
            controllerRemoveList = new List<Controller>();

            jointList = new GenericList<Joint>();
            jointAddList = new List<Joint>();
            jointRemoveList = new List<Joint>();

            springList = new GenericList<Spring>();
            springAddList = new List<Spring>();
            springRemoveList = new List<Spring>();

            _broadPhaseCollider = new SelectiveSweepCollider(this);

            arbiterList = new ArbiterList();
            Gravity = gravity;

            //Poolsize of 10, will grow as needed.
            arbiterPool = new Pool<Arbiter>(100);
        }

        public void Add(Geom geometry)
        {
            if (geometry == null)
                throw new ArgumentNullException("geometry", "Can't add null geometry");

            if (!geomAddList.Contains(geometry))
            {
                geomAddList.Add(geometry);
            }
        }

        public void Remove(Geom geometry)
        {
            if (geometry == null)
                throw new ArgumentNullException("geometry", "Can't remove null geometry");

            geomRemoveList.Add(geometry);
        }

        public void Add(Body body)
        {
            if (body == null)
                throw new ArgumentNullException("body", "Can't add null body");

            if (!bodyAddList.Contains(body))
            {
                bodyAddList.Add(body);
            }
        }

        public void Remove(Body body)
        {
            if (body == null)
                throw new ArgumentNullException("body", "Can't remove null body");

            bodyRemoveList.Add(body);
        }

        public void Add(Controller controller)
        {
            if (controller == null)
                throw new ArgumentNullException("controller", "Can't add null controller");

            if (!controllerAddList.Contains(controller))
            {
                controllerAddList.Add(controller);
            }
        }

        public void Remove(Controller controller)
        {
            if (controller == null)
                throw new ArgumentNullException("controller", "Can't remove null controller");

            controllerRemoveList.Add(controller);
        }

        public void Add(Joint joint)
        {
            if (joint == null)
                throw new ArgumentNullException("joint", "Can't add null joint");

            if (!jointAddList.Contains(joint))
            {
                jointAddList.Add(joint);
            }
        }

        public void Remove(Joint joint)
        {
            if (joint == null)
                throw new ArgumentNullException("joint", "Can't remove null joint");

            jointRemoveList.Add(joint);
        }

        public void Add(Spring spring)
        {
            if (spring == null)
                throw new ArgumentNullException("spring", "Can't add null spring");

            if (!springAddList.Contains(spring))
            {
                springAddList.Add(spring);
            }
        }

        public void Remove(Spring spring)
        {
            if (spring == null)
                throw new ArgumentNullException("spring", "Can't remove null spring");

            springRemoveList.Add(spring);
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

#if (XNA)
            if (EnableDiagnostics)
                cleanUpTime = _sw.ElapsedTicks;
#endif

            //If there is no change in time, no need to calculate anything.
            if (dt == 0 || !Enabled)
                return;

            DoBroadPhaseCollision();
#if (XNA)
            if (EnableDiagnostics)
                broadPhaseCollisionTime = _sw.ElapsedTicks - cleanUpTime;
#endif
            DoNarrowPhaseCollision();
#if (XNA)
            if (EnableDiagnostics)
                narrowPhaseCollisionTime = _sw.ElapsedTicks - broadPhaseCollisionTime - cleanUpTime;
#endif
            ApplyForces(dt , dtReal);
#if (XNA)
            if (EnableDiagnostics)
                applyForcesTime = _sw.ElapsedTicks - narrowPhaseCollisionTime - broadPhaseCollisionTime - cleanUpTime;
#endif
            ApplyImpulses(dt);

#if (XNA)
            if (EnableDiagnostics)
                applyImpulsesTime = _sw.ElapsedTicks - applyForcesTime - narrowPhaseCollisionTime - broadPhaseCollisionTime - cleanUpTime;
#endif
            UpdatePositions(dt);
#if (XNA)
            if (EnableDiagnostics)
                updatePositionsTime = _sw.ElapsedTicks - applyImpulsesTime - applyForcesTime - narrowPhaseCollisionTime - broadPhaseCollisionTime - cleanUpTime;
#endif
#if (XNA)

            if (EnableDiagnostics)
            {
                _sw.Stop();
                updateTime = _sw.ElapsedTicks;

                cleanUpTime = 1000 * cleanUpTime / Stopwatch.Frequency;
                broadPhaseCollisionTime = 1000 * broadPhaseCollisionTime / Stopwatch.Frequency;
                narrowPhaseCollisionTime = 1000 * narrowPhaseCollisionTime / Stopwatch.Frequency;
                applyForcesTime = 1000 * applyForcesTime / Stopwatch.Frequency;
                applyImpulsesTime = 1000 * applyImpulsesTime / Stopwatch.Frequency;
                updatePositionsTime = 1000 * updatePositionsTime / Stopwatch.Frequency;
                updateTime = 1000 * updateTime / Stopwatch.Frequency;
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
            foreach (Geom geometry in geomList)
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
            foreach (Geom geom in geomList)
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
            for (int i = 0; i < arbiterList.Count; i++)
            {
                arbiterList[i].Collide();
            }
            arbiterList.RemoveContactCountEqualsZero(arbiterPool);
        }

        /// <summary>
        /// Applies the forces to all controllers, springs, joints and bodies.
        /// This step also
        /// </summary>
        /// <param name="dt">The delta time.</param>
        /// <param name="dtReal">The real delta time.</param>
        private void ApplyForces(float dt, float dtReal)
        {
            for (int i = 0; i < controllerList.Count; i++)
            {
                if (!controllerList[i].Enabled || controllerList[i].IsDisposed)
                    continue;

                controllerList[i].Update(dt, dtReal);
            }

            for (int i = 0; i < springList.Count; i++)
            {
                if (!springList[i].Enabled || springList[i].IsDisposed)
                    continue;

                springList[i].Update(dt);
            }

            for (int i = 0; i < bodyList.Count; i++)
            {
                if (!bodyList[i].Enabled || bodyList[i].isStatic || bodyList[i].IsDisposed)
                    continue;

                //Apply accumulated external impules
                bodyList[i].ApplyImpulses();

                if (!bodyList[i].IgnoreGravity)
                {
                    bodyList[i].force.X = bodyList[i].force.X + (Gravity.X * bodyList[i].mass);
                    bodyList[i].force.Y = bodyList[i].force.Y + (Gravity.Y * bodyList[i].mass);
                }

                bodyList[i].IntegrateVelocity(dt);
                bodyList[i].ClearForce();
                bodyList[i].ClearTorque();
            }
        }

        /// <summary>
        /// Applies the impulses to all joints and arbiters.
        /// </summary>
        /// <param name="dt">The delta time.</param>
        private void ApplyImpulses(float dt)
        {
            float inverseDt = 1f / dt;

            for (int i = 0; i < jointList.Count; i++)
            {
                if (!jointList[i].Enabled || jointList[i].IsDisposed)
                    continue;

                jointList[i].PreStep(inverseDt);
            }

            for (int i = 0; i < arbiterList.Count; i++)
            {
                if (!arbiterList[i].GeometryA.CollisionResponseEnabled || !arbiterList[i].GeometryB.CollisionResponseEnabled)
                    continue;

                arbiterList[i].PreStepImpulse(inverseDt);
            }

            for (int h = 0; h < Iterations; h++)
            {
                for (int i = 0; i < jointList.Count; i++)
                {
                    if (!jointList[i].Enabled || jointList[i].IsDisposed)
                        continue;

                    jointList[i].Update();
                }

                for (int i = 0; i < arbiterList.Count; i++)
                {
                    if (!arbiterList[i].GeometryA.CollisionResponseEnabled || !arbiterList[i].GeometryB.CollisionResponseEnabled)
                        continue;

                    arbiterList[i].ApplyImpulse();
                }
            }
        }

        /// <summary>
        /// Updates the position on all bodies.
        /// </summary>
        /// <param name="dt">The delta time.</param>
        private void UpdatePositions(float dt)
        {
            for (int i = 0; i < bodyList.Count; i++)
            {
                if (!bodyList[i].Enabled || bodyList[i].isStatic || bodyList[i].IsDisposed)
                    continue;

                bodyList[i].IntegratePosition(dt);
            }
        }

        /// <summary>
        /// Processes the added geometries, springs, joints, bodies and controllers.
        /// </summary>
        private void ProcessAddedItems()
        {
            //Add any new geometries
            _tempCount = geomAddList.Count;
            for (int i = 0; i < _tempCount; i++)
            {
                if (!geomList.Contains(geomAddList[i]))
                {
                    geomAddList[i].InSimulation = true;
                    geomList.Add(geomAddList[i]);

                    //Add the new geometry to the broad phase collider.
                    _broadPhaseCollider.Add(geomAddList[i]);
                }
            }
            geomAddList.Clear();

            //Add any new bodies
            _tempCount = bodyAddList.Count;
            for (int i = 0; i < _tempCount; i++)
            {
                if (!bodyList.Contains(bodyAddList[i]))
                {
                    bodyList.Add(bodyAddList[i]);
                }
            }
            bodyAddList.Clear();

            //Add any new controllers
            _tempCount = controllerAddList.Count;
            for (int i = 0; i < _tempCount; i++)
            {
                if (!controllerList.Contains(controllerAddList[i]))
                {
                    controllerList.Add(controllerAddList[i]);
                }
            }
            controllerAddList.Clear();

            //Add any new joints
            _tempCount = jointAddList.Count;
            for (int i = 0; i < _tempCount; i++)
            {
                if (!jointList.Contains(jointAddList[i]))
                {
                    jointList.Add(jointAddList[i]);
                }
            }
            jointAddList.Clear();

            //Add any new springs
            _tempCount = springAddList.Count;
            for (int i = 0; i < _tempCount; i++)
            {
                if (!springList.Contains(springAddList[i]))
                {
                    springList.Add(springAddList[i]);
                }
            }
            springAddList.Clear();
        }

        /// <summary>
        /// Processes the removed geometries (and their arbiters), bodies, controllers, joints and springs.
        /// </summary>
        private void ProcessRemovedItems()
        {
            //Remove any new geometries
            _tempCount = geomRemoveList.Count;
            for (int i = 0; i < _tempCount; i++)
            {
                geomRemoveList[i].InSimulation = false;
                geomList.Remove(geomRemoveList[i]);

                //Remove any arbiters associated with the geometries being removed
                for (int j = arbiterList.Count; j > 0; j--)
                {
                    if (arbiterList[j - 1].GeometryA == geomRemoveList[i] ||
                        arbiterList[j - 1].GeometryB == geomRemoveList[i])
                    {
                        //TODO: Should we create a RemoveComplete method and remove all Contacts associated
                        //with the arbiter?
                        arbiterList.Remove(arbiterList[j - 1]);
                    }
                }
            }

            if (geomRemoveList.Count > 0)
            {
                _broadPhaseCollider.ProcessRemovedGeoms();
            }

            geomRemoveList.Clear();

            //Remove any new bodies
            _tempCount = bodyRemoveList.Count;
            for (int i = 0; i < _tempCount; i++)
            {
                bodyList.Remove(bodyRemoveList[i]);
            }
            bodyRemoveList.Clear();

            //Remove any new controllers
            _tempCount = controllerRemoveList.Count;
            for (int i = 0; i < _tempCount; i++)
            {
                controllerList.Remove(controllerRemoveList[i]);
            }
            controllerRemoveList.Clear();

            //Remove any new joints
            int jointRemoveCount = jointRemoveList.Count;
            for (int i = 0; i < jointRemoveCount; i++)
            {
                jointList.Remove(jointRemoveList[i]);
            }
            jointRemoveList.Clear();

            //Remove any new springs
            _tempCount = springRemoveList.Count;
            for (int i = 0; i < _tempCount; i++)
            {
                springList.Remove(springRemoveList[i]);
            }
            springRemoveList.Clear();
        }

        /// <summary>
        /// Processes the disposed controllers, joints, springs, bodies and cleans up the arbiter list.
        /// </summary>
        private void ProcessDisposedItems()
        {
            //Allow each controller to validate itself. this is where a controller can Dispose of itself if need be.
            for (int i = 0; i < controllerList.Count; i++)
            {
                controllerList[i].Validate();
            }

            //Allow each joint to validate itself. this is where a joint can Dispose of itself if need be.
            for (int i = 0; i < jointList.Count; i++)
            {
                jointList[i].Validate();
            }

            //Allow each spring to validate itself. this is where a spring can Dispose of itself if need be.
            for (int i = 0; i < springList.Count; i++)
            {
                springList[i].Validate();
            }

            _tempCount = geomList.RemoveDisposed();

            if (_tempCount > 0)
            {
                _broadPhaseCollider.ProcessDisposedGeoms();
            }

            bodyList.RemoveDisposed();
            controllerList.RemoveDisposed();
            springList.RemoveDisposed();
            jointList.RemoveDisposed();

            //Clean up the arbiterlist
            arbiterList.CleanArbiterList(arbiterPool);
        }

        #region Temp variables
        private int _tempCount;
        #endregion
    }
}