using System;
using System.Collections.Generic;
using System.Diagnostics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Controllers;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Dynamics.Joints;
using FarseerGames.FarseerPhysics.Interfaces;
using Microsoft.Xna.Framework;

namespace FarseerGames.FarseerPhysics
{
    public class PhysicsSimulator : IDisposable
    {
        private const int _arbiterPoolSize = 10; //initial arbiter size.  will grow as needed
        private Stopwatch _sw = new Stopwatch();
        private Body _body;
        private IBroadPhaseCollider _broadPhaseCollider;
        private bool _enabled = true;
        private Vector2 _gravity = Vector2.Zero;
        private Vector2 _gravityForce;

        //default settings
        private int _iterations = 5;
        internal float allowedPenetration = .05f;
        internal float applyForcesTime = -1;
        internal float applyImpulsesTime = -1;
        internal ArbiterList arbiterList;
        internal Pool<Arbiter> arbiterPool;
        internal float biasFactor = .8f;

        internal List<Body> bodyAddList;
        internal BodyList bodyList;
        internal List<Body> bodyRemoveList;
        internal float broadPhaseCollisionTime = -1;
        internal float cleanUpTime = -1;

        internal List<Controller> controllerAddList;
        internal ControllerList controllerList;
        internal List<Controller> controllerRemoveList;

        public bool EnableDiagnostics;
        internal FrictionType frictionType = FrictionType.Average;
        internal List<Geom> geomAddList;
        internal GeomList geomList;
        internal List<Geom> geomRemoveList;

        internal List<Joint> jointAddList;
        internal JointList jointList;
        internal List<Joint> jointRemoveList;
        internal int maxContactsToDetect = 3;
        internal int maxContactsToResolve = 2;
        internal float narrowPhaseCollisionTime = -1;
        internal float updatePositionsTime = -1;
        internal float updateTime = -1;

        #region Added by Daniel Pramel 08/17/08

        private InactivityController _inactivityController;

        /// <summary>
        /// Returns the InactivityController to automatically disable not used bodies.
        /// It is disabled by default!
        /// </summary>
        public InactivityController InactivityController
        {
            get { return _inactivityController; }
        }

        #endregion

        public PhysicsSimulator()
        {
            ConstructPhysicsSimulator(Vector2.Zero);
        }

        public PhysicsSimulator(Vector2 gravity)
        {
            ConstructPhysicsSimulator(gravity);
        }

        /// <summary>
        /// Fully exposed for convenience. Should be treated as. Do not add or remove directly from this list.
        /// </summary>
        public GeomList GeomList
        {
            get { return geomList; }
            //set { geometryList = value; }
        }

        /// <summary>
        /// Fully exposed for convenience. Should be treated as. Do not add or remove directly from this list.
        /// </summary>
        public BodyList BodyList
        {
            get { return bodyList; }
            //set { bodyList = value; }
        }

        /// <summary>
        /// Fully exposed for convenience. Should be treated as. Do not add or remove directly from this list.
        /// </summary>
        public ControllerList ControllerList
        {
            get { return controllerList; }
            //set { controllerList = value; }
        }

        /// <summary>
        /// Fully exposed for convenience. Should be treated as. Do not add or remove directly from this list.
        /// </summary>
        public JointList JointList
        {
            get { return jointList; }
            //set { controllerList = value; }
        }

        /// <summary>
        /// Fully exposed for convenience. Should be treated as. Do not add or remove directly from this list.
        /// </summary>
        public ArbiterList ArbiterList
        {
            get { return arbiterList; }
        }

        public Vector2 Gravity
        {
            get { return _gravity; }
            set { _gravity = value; }
        }

        public int Iterations
        {
            get { return _iterations; }
            set { _iterations = value; }
        }

        public float AllowedPenetration
        {
            get { return allowedPenetration; }
            set { allowedPenetration = value; }
        }

        public float BiasFactor
        {
            get { return biasFactor; }
            set { biasFactor = value; }
        }

        public int MaxContactsToDetect
        {
            get { return maxContactsToDetect; }
            set { maxContactsToDetect = value; }
        }

        public int MaxContactsToResolve
        {
            get { return maxContactsToResolve; }
            set { maxContactsToResolve = value; }
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

        public FrictionType FrictionType
        {
            get { return frictionType; }
            set { frictionType = value; }
        }

        /// <summary>
        /// If false, calling Update() will have no affect.  Essentially "pauses" the physics engine.
        /// </summary>
        public bool Enabled
        {
            get { return _enabled; }
            set { _enabled = value; }
        }

        private void ConstructPhysicsSimulator(Vector2 gravity)
        {
            geomList = new GeomList();
            geomAddList = new List<Geom>();
            geomRemoveList = new List<Geom>();

            bodyList = new BodyList();
            bodyAddList = new List<Body>();
            bodyRemoveList = new List<Body>();

            controllerList = new ControllerList();
            controllerAddList = new List<Controller>();
            controllerRemoveList = new List<Controller>();

            jointList = new JointList();
            jointAddList = new List<Joint>();
            jointRemoveList = new List<Joint>();

            _broadPhaseCollider = new SelectiveSweepCollider(this);

            arbiterList = new ArbiterList();
            _gravity = gravity;

            arbiterPool = new Pool<Arbiter>(_arbiterPoolSize);

            #region Added by Daniel Pramel 08/17/08

            _inactivityController = new InactivityController(this);

            _scaling = new Scaling(0.001f, 0.01f);

            #endregion
        }


        public void SetBroadPhaseCollider(IBroadPhaseCollider broadPhaseCollider)
        {
            if (geomList.Count > 0)
            {
                throw new Exception("The GeomList must be empty when setting the broad phase collider type");
            }
            _broadPhaseCollider = broadPhaseCollider;
        }

        public void Add(Geom geometry)
        {
            if (!geomAddList.Contains(geometry))
            {
                geomAddList.Add(geometry);
            }
        }

        public void Remove(Geom geometry)
        {
            geomRemoveList.Add(geometry);
        }

        public void Add(Body body)
        {
            if (!bodyAddList.Contains(body))
            {
                bodyAddList.Add(body);
            }
        }

        public void Remove(Body body)
        {
            bodyRemoveList.Add(body);
        }

        public void Add(Controller controller)
        {
            if (!controllerAddList.Contains(controller))
            {
                controllerAddList.Add(controller);
            }
        }

        public void Remove(Controller controller)
        {
            controllerRemoveList.Add(controller);
        }

        public void Add(Joint joint)
        {
            if (!jointAddList.Contains(joint))
            {
                jointAddList.Add(joint);
            }
        }

        public void Remove(Joint joint)
        {
            jointRemoveList.Add(joint);
        }

        public void Clear()
        {
            //arbiterList.Clear();
            //geomList.Clear();
            //bodyList.Clear();
            //jointList.Clear();
            //controllerList.Clear();
            //arbiterPool = new Pool<Arbiter>(_arbiterPoolSize); 
            //_broadPhaseCollider = new SelectiveSweepCollider(this);
            ConstructPhysicsSimulator(_gravity);
        }

        public void Update(float dt)
        {
            Update(dt, 0);
        }

        public void Update(float dt, float dtReal)
        {
            if (dt == 0)
            {
                return;
            }

            if (EnableDiagnostics) _sw.Start();

            #region Added by Daniel Pramel 08/24/08

            dt = _scaling.GetUpdateInterval(dt);
            if (dt == 0)
            {
                return;
            }


            if (_scaling.UpdateInterval < dtReal)
            {
                _scaling.IncreaseUpdateInterval();
            }
            else
            {
                _scaling.DecreaseUpdateInterval();
            }

            #endregion

            ProcessAddedItems();
            //moved to before 'removeitems' to avoid confusion when calling add/remove without calling update.
            ProcessRemovedItems();
            ProcessDisposedItems();

            if (!_enabled) return;

            if (EnableDiagnostics) cleanUpTime = _sw.ElapsedTicks;
            DoBroadPhaseCollision();
            if (EnableDiagnostics) broadPhaseCollisionTime = _sw.ElapsedTicks - cleanUpTime;
            DoNarrowPhaseCollision();
            if (EnableDiagnostics) narrowPhaseCollisionTime = _sw.ElapsedTicks - broadPhaseCollisionTime - cleanUpTime;
            ApplyForces(dt);
            if (EnableDiagnostics)
                applyForcesTime = _sw.ElapsedTicks - narrowPhaseCollisionTime - broadPhaseCollisionTime - cleanUpTime;
            ApplyImpulses(dt);
            if (EnableDiagnostics)
                applyImpulsesTime = _sw.ElapsedTicks - applyForcesTime - narrowPhaseCollisionTime -
                                    broadPhaseCollisionTime - cleanUpTime;
            UpdatePositions(dt);
            if (EnableDiagnostics)
                updatePositionsTime = _sw.ElapsedTicks - applyImpulsesTime - applyForcesTime - narrowPhaseCollisionTime -
                                      broadPhaseCollisionTime - cleanUpTime;

            if (EnableDiagnostics)
            {
                _sw.Stop();
                updateTime = _sw.ElapsedTicks;

                cleanUpTime = 1000*cleanUpTime/Stopwatch.Frequency;
                broadPhaseCollisionTime = 1000*broadPhaseCollisionTime/Stopwatch.Frequency;
                narrowPhaseCollisionTime = 1000*narrowPhaseCollisionTime/Stopwatch.Frequency;
                applyForcesTime = 1000*applyForcesTime/Stopwatch.Frequency;
                applyImpulsesTime = 1000*applyImpulsesTime/Stopwatch.Frequency;
                updatePositionsTime = 1000*updatePositionsTime/Stopwatch.Frequency;
                updateTime = 1000*updateTime/Stopwatch.Frequency;
                _sw.Reset();
            }
        }

        public Geom Collide(float x, float y)
        {
            return Collide(new Vector2(x, y));
        }

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

        public List<Geom> CollideAll(float x, float y)
        {
            return CollideAll(new Vector2(x, y));
        }

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

        private void DoBroadPhaseCollision()
        {
            _broadPhaseCollider.Update();
        }

        private void DoNarrowPhaseCollision()
        {
            for (int i = 0; i < arbiterList.Count; i++)
            {
                arbiterList[i].Collide();
            }
            arbiterList.RemoveContactCountEqualsZero(arbiterPool);
        }

        private void ApplyForces(float dt)
        {
            for (int i = 0; i < controllerList.Count; i++)
            {
                if (controllerList[i].Enabled)
                {
                    controllerList[i].Update(dt);
                }
            }

            for (int i = 0; i < bodyList.Count; i++)
            {
                _body = bodyList[i];
                if (!_body.Enabled)
                {
                    continue;
                }
                //apply accumulated external impules
                _body.ApplyImpulses();

                if (!_body.ignoreGravity)
                {
                    _gravityForce.X = _gravity.X*_body.mass;
                    _gravityForce.Y = _gravity.Y*_body.mass;

                    #region INLINE: _body.ApplyForce(ref _gravityForce);

                    _body.force.X = _body.force.X + _gravityForce.X;
                    _body.force.Y = _body.force.Y + _gravityForce.Y;

                    #endregion
                }

                _body.IntegrateVelocity(dt);
                _body.ClearForce();
                _body.ClearTorque();
            }
        }

        private void ApplyImpulses(float dt)
        {
            float inverseDt = 1f/dt;

            for (int i = 0; i < jointList.Count; i++)
            {
                if (jointList[i].Enabled)
                {
                    jointList[i].PreStep(inverseDt);
                }
            }

            for (int i = 0; i < arbiterList.Count; i++)
            {
                arbiterList[i].PreStepImpulse(inverseDt);
            }

            for (int h = 0; h < _iterations; h++)
            {
                for (int i = 0; i < jointList.Count; i++)
                {
                    if (jointList[i].Enabled)
                    {
                        jointList[i].Update();
                    }
                }

                for (int i = 0; i < arbiterList.Count; i++)
                {
                    arbiterList[i].ApplyImpulse();
                }
            }
        }

        private void UpdatePositions(float dt)
        {
            for (int i = 0; i < bodyList.Count; i++)
            {
                if (!bodyList[i].enabled)
                {
                    continue;
                }
                bodyList[i].IntegratePosition(dt);
            }
        }

        private void ProcessAddedItems()
        {
            //add any new geometries
            for (int i = 0; i < geomAddList.Count; i++)
            {
                if (!geomList.Contains(geomAddList[i]))
                {
                    geomAddList[i].isRemoved = false;
                    geomList.Add(geomAddList[i]);

                    _broadPhaseCollider.Add(geomAddList[i]);
                }
            }
            geomAddList.Clear();

            //add any new bodies
            for (int i = 0; i < bodyAddList.Count; i++)
            {
                if (!bodyList.Contains(bodyAddList[i]))
                {
                    bodyList.Add(bodyAddList[i]);
                }
            }
            bodyAddList.Clear();

            //add any new controllers
            for (int i = 0; i < controllerAddList.Count; i++)
            {
                if (!controllerList.Contains(controllerAddList[i]))
                {
                    controllerList.Add(controllerAddList[i]);
                }
            }
            controllerAddList.Clear();

            //add any new joints
            for (int i = 0; i < jointAddList.Count; i++)
            {
                if (!jointList.Contains(jointAddList[i]))
                {
                    jointList.Add(jointAddList[i]);
                }
            }
            jointAddList.Clear();
        }

        private void ProcessRemovedItems()
        {
            //remove any new geometries
            for (int i = 0; i < geomRemoveList.Count; i++)
            {
                geomRemoveList[i].isRemoved = true;
                geomList.Remove(geomRemoveList[i]);

                //remove any arbiters associated with the geometries being removed
                for (int j = arbiterList.Count; j > 0; j--)
                {
                    if (arbiterList[j - 1].GeometryA == geomRemoveList[i] ||
                        arbiterList[j - 1].GeometryB == geomRemoveList[i])
                    {
                        arbiterList.Remove(arbiterList[j - 1]);
                    }
                }
            }

            if (geomRemoveList.Count > 0)
            {
                _broadPhaseCollider.ProcessRemovedGeoms();
            }

            geomRemoveList.Clear();

            //remove any new bodies
            for (int i = 0; i < bodyRemoveList.Count; i++)
            {
                bodyList.Remove(bodyRemoveList[i]);
            }
            bodyRemoveList.Clear();

            //remove any new controllers
            for (int i = 0; i < controllerRemoveList.Count; i++)
            {
                controllerList.Remove(controllerRemoveList[i]);
            }
            controllerRemoveList.Clear();

            //remove any new joints
            for (int i = 0; i < jointRemoveList.Count; i++)
            {
                jointList.Remove(jointRemoveList[i]);
            }
            jointRemoveList.Clear();
        }

        private void ProcessDisposedItems()
        {
            //allow each controller to validate itself. this is where a controller can Dispose of itself if need be.
            for (int i = 0; i < controllerList.Count; i++)
            {
                controllerList[i].Validate();
            }
            int disposedGeomCount = geomList.RemoveDisposed();

            if (disposedGeomCount > 0)
            {
                _broadPhaseCollider.ProcessDisposedGeoms();
            }

            bodyList.RemoveDisposed();
            controllerList.RemoveDisposed();

            //allow each joint to validate itself. this is where a joint can Dispose of itself if need be.
            for (int i = 0; i < jointList.Count; i++)
            {
                jointList[i].Validate();
            }
            jointList.RemoveDisposed();

            //remove all arbiters that contain 1 or more disposed rigid bodies.
            arbiterList.RemoveContainsDisposedBody(arbiterPool);
        }

        #region Added by Daniel Pramel 08/24/08

        private Scaling _scaling;

        public Scaling Scaling
        {
            get { return _scaling; }
            set { _scaling = value; }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            _inactivityController.Dispose();
        }

        #endregion
    }
}