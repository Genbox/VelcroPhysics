using System;
using System.Collections.Generic;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Controllers;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Dynamics.Joints;
using FarseerGames.FarseerPhysics.Mathematics;

namespace FarseerGames.FarseerPhysics
{
    public class PhysicsSimulator
    {
        private const int arbiterPoolSize = 10;
        private Body _body;
        private IBroadPhaseCollider _broadPhaseCollider;
        private Vector2 _gravityForce;
        internal Pool<Arbiter> arbiterPool;

        internal List<Body> bodyAddList;
        internal List<Body> bodyRemoveList;

        internal List<Controller> controllerAddList;
        internal List<Controller> controllerRemoveList;

        public bool EnableDiagnostics;
        internal List<Geom> geomAddList;
        internal List<Geom> geomRemoveList;

        //default settings
        internal List<Joint> jointAddList;
        internal List<Joint> jointRemoveList;

        #region Added by Daniel Pramel 08/17/08

        private InactivityController inactivityController;

        /// <summary>
        /// Returns the InactivityController to automatically disable not used bodies.
        /// It is disabled by default!
        /// </summary>
        public InactivityController InactivityController
        {
            get { return inactivityController; }
        }

        #endregion

        public PhysicsSimulator()
        {
            Enabled = true;
            FrictionType = FrictionType.Average;
            UpdateTime = -1;
            UpdatePositionsTime = -1;
            ApplyImpulsesTime = -1;
            ApplyForcesTime = -1;
            NarrowPhaseCollisionTime = -1;
            BroadPhaseCollisionTime = -1;
            CleanUpTime = -1;
            MaxContactsToResolve = 2;
            MaxContactsToDetect = 3;
            BiasFactor = .8f;
            AllowedPenetration = .05f;
            Iterations = 5;
            Gravity = Vector2.Zero;
            ConstructPhysicsSimulator(Vector2.Zero);
        }

        public PhysicsSimulator(Vector2 gravity)
        {
            Enabled = true;
            FrictionType = FrictionType.Average;
            UpdateTime = -1;
            UpdatePositionsTime = -1;
            ApplyImpulsesTime = -1;
            ApplyForcesTime = -1;
            NarrowPhaseCollisionTime = -1;
            BroadPhaseCollisionTime = -1;
            CleanUpTime = -1;
            MaxContactsToResolve = 2;
            MaxContactsToDetect = 3;
            BiasFactor = .8f;
            AllowedPenetration = .05f;
            Iterations = 5;
            Gravity = Vector2.Zero;
            ConstructPhysicsSimulator(gravity);
        }

        /// <summary>
        /// Fully exposed for convenience. Should be treated as readonly. Do not add or remove directly from this list.
        /// </summary>
        public ItemList<Geom> GeomList { get; internal set; }

        /// <summary>
        /// Fully exposed for convenience. Should be treated as readonly. Do not add or remove directly from this list.
        /// </summary>
        public ItemList<Body> BodyList { get; internal set; }

        /// <summary>
        /// Fully exposed for convenience. Should be treated as readonly. Do not add or remove directly from this list.
        /// </summary>
        public ItemList<Controller> ControllerList { get; internal set; }

        /// <summary>
        /// Fully exposed for convenience. Should be treated as readonly. Do not add or remove directly from this list.
        /// </summary>
        public ItemList<Joint> JointList { get; internal set; }

        /// <summary>
        /// Fully exposed for convenience. Should be treated as readonly. Do not add or remove directly from this list.
        /// </summary>
        public ArbiterList ArbiterList { get; internal set; }

        public Vector2 Gravity { get; set; }

        public int Iterations { get; set; }

        public float AllowedPenetration { get; set; }

        public float BiasFactor { get; set; }

        public int MaxContactsToDetect { get; set; }

        public int MaxContactsToResolve { get; set; }

        public float CleanUpTime { get; internal set; }

        public float BroadPhaseCollisionTime { get; internal set; }

        public float NarrowPhaseCollisionTime { get; internal set; }

        public float ApplyForcesTime { get; internal set; }

        public float ApplyImpulsesTime { get; internal set; }

        public float UpdatePositionsTime { get; internal set; }

        public float UpdateTime { get; internal set; }

        public FrictionType FrictionType { get; set; }

        /// <summary>
        /// If false, calling Update() will have no affect.  Essentially "pauses" the physics engine.
        /// </summary>
        public bool Enabled { get; set; }

        private void ConstructPhysicsSimulator(Vector2 gravity)
        {
            GeomList = new ItemList<Geom>();
            geomAddList = new List<Geom>();
            geomRemoveList = new List<Geom>();

            BodyList = new ItemList<Body>();
            bodyAddList = new List<Body>();
            bodyRemoveList = new List<Body>();

            ControllerList = new ItemList<Controller>();
            controllerAddList = new List<Controller>();
            controllerRemoveList = new List<Controller>();

            JointList = new ItemList<Joint>();
            jointAddList = new List<Joint>();
            jointRemoveList = new List<Joint>();

            _broadPhaseCollider = new SelectiveSweepCollider(this);

            ArbiterList = new ArbiterList();
            Gravity = gravity;

            arbiterPool = new Pool<Arbiter>(arbiterPoolSize);

            #region Added by Daniel Pramel 08/17/08

            inactivityController = new InactivityController(this);

            scaling = new Scaling(0.001f, 0.01f);

            #endregion
        }


        public void SetBroadPhaseCollider(IBroadPhaseCollider broadPhaseCollider)
        {
            if (GeomList.Count > 0)
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
            //arbiterPool = new Pool<Arbiter>(arbiterPoolSize); 
            //broadPhaseCollider = new SelectiveSweepCollider(this);
            ConstructPhysicsSimulator(Gravity);
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

            #region Added by Daniel Pramel 08/24/08

            dt = scaling.GetUpdateInterval(dt);
            if (dt == 0)
            {
                return;
            }


            if (scaling.UpdateInterval < dtReal)
            {
                scaling.IncreaseUpdateInterval();
            }
            else
            {
                scaling.DecreaseUpdateInterval();
            }

            #endregion

            ProcessAddedItems();
            //moved to before 'removeitems' to avoid confusion when calling add/remove without calling update.
            ProcessRemovedItems();
            ProcessDisposedItems();

            if (!Enabled) return;

            DoBroadPhaseCollision();
            DoNarrowPhaseCollision();
            ApplyForces(dt);
            ApplyImpulses(dt);
            UpdatePositions(dt);
        }

        public Geom Collide(float x, float y)
        {
            return Collide(new Vector2(x, y));
        }

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

        public List<Geom> CollideAll(float x, float y)
        {
            return CollideAll(new Vector2(x, y));
        }

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

        private void DoBroadPhaseCollision()
        {
            _broadPhaseCollider.Update();
        }

        private void DoNarrowPhaseCollision()
        {
            for (int i = 0; i < ArbiterList.Count; i++)
            {
                ArbiterList[i].Collide();
            }
            ArbiterList.RemoveContactCountEqualsZero(arbiterPool);
        }

        private void ApplyForces(float dt)
        {
            for (int i = 0; i < ControllerList.Count; i++)
            {
                if (ControllerList[i].Enabled)
                {
                    ControllerList[i].Update(dt);
                }
            }

            for (int i = 0; i < BodyList.Count; i++)
            {
                _body = BodyList[i];
                if (!_body.Enabled)
                {
                    continue;
                }
                //apply accumulated external impules
                _body.ApplyImpulses();

                if (!_body.ignoreGravity)
                {
                    _gravityForce.X = Gravity.X*_body._mass;
                    _gravityForce.Y = Gravity.Y*_body._mass;

                    #region INLINE: body.ApplyForce(ref gravityForce);

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

            for (int i = 0; i < JointList.Count; i++)
            {
                if (JointList[i].Enabled)
                {
                    JointList[i].PreStep(inverseDt);
                }
            }

            for (int i = 0; i < ArbiterList.Count; i++)
            {
                ArbiterList[i].PreStepImpulse(inverseDt);
            }

            for (int h = 0; h < Iterations; h++)
            {
                for (int i = 0; i < JointList.Count; i++)
                {
                    if (JointList[i].Enabled)
                    {
                        JointList[i].Update();
                    }
                }

                for (int i = 0; i < ArbiterList.Count; i++)
                {
                    ArbiterList[i].ApplyImpulse();
                }
            }
        }

        private void UpdatePositions(float dt)
        {
            for (int i = 0; i < BodyList.Count; i++)
            {
                if (!BodyList[i].enabled)
                {
                    continue;
                }
                BodyList[i].IntegratePosition(dt);
            }
        }

        //Note: Cleanup, Method never used
        //private void ReleaseArbitersWithDisposedGeom(Arbiter arbiter)
        //{
        //    if (arbiter.ContainsDisposedGeom())
        //    {
        //        arbiterPool.Release(arbiter);
        //    }
        //}

        //Note: Cleanup, Method never used
        //private void ReleaseArbitersWithContactCountZero(Arbiter arbiter)
        //{
        //    if (arbiter.ContactCount == 0)
        //    {
        //        arbiterPool.Release(arbiter);
        //    }
        //}

        private void ProcessAddedItems()
        {
            //add any new geometries
            for (int i = 0; i < geomAddList.Count; i++)
            {
                if (!GeomList.Contains(geomAddList[i]))
                {
                    geomAddList[i].IsRemoved = false;
                    GeomList.Add(geomAddList[i]);

                    _broadPhaseCollider.Add(geomAddList[i]);
                }
            }
            geomAddList.Clear();

            //add any new bodies
            for (int i = 0; i < bodyAddList.Count; i++)
            {
                if (!BodyList.Contains(bodyAddList[i]))
                {
                    BodyList.Add(bodyAddList[i]);
                }
            }
            bodyAddList.Clear();

            //add any new controllers
            for (int i = 0; i < controllerAddList.Count; i++)
            {
                if (!ControllerList.Contains(controllerAddList[i]))
                {
                    ControllerList.Add(controllerAddList[i]);
                }
            }
            controllerAddList.Clear();

            //add any new joints
            for (int i = 0; i < jointAddList.Count; i++)
            {
                if (!JointList.Contains(jointAddList[i]))
                {
                    JointList.Add(jointAddList[i]);
                }
            }
            jointAddList.Clear();
        }

        private void ProcessRemovedItems()
        {
            //remove any new geometries
            for (int i = 0; i < geomRemoveList.Count; i++)
            {
                geomRemoveList[i].IsRemoved = true;
                GeomList.Remove(geomRemoveList[i]);

                //remove any arbiters associated with the geometries being removed
                for (int j = ArbiterList.Count; j > 0; j--)
                {
                    if (ArbiterList[j - 1].GeomA == geomRemoveList[i] ||
                        ArbiterList[j - 1].GeomB == geomRemoveList[i])
                    {
                        ArbiterList.Remove(ArbiterList[j - 1]);
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
                BodyList.Remove(bodyRemoveList[i]);
            }
            bodyRemoveList.Clear();

            //remove any new controllers
            for (int i = 0; i < controllerRemoveList.Count; i++)
            {
                ControllerList.Remove(controllerRemoveList[i]);
            }
            controllerRemoveList.Clear();

            //remove any new joints
            for (int i = 0; i < jointRemoveList.Count; i++)
            {
                JointList.Remove(jointRemoveList[i]);
            }
            jointRemoveList.Clear();
        }

        private void ProcessDisposedItems()
        {
            //allow each controller to validate itself. this is where a controller can Dispose of itself if need be.
            for (int i = 0; i < ControllerList.Count; i++)
            {
                ControllerList[i].Validate();
            }
            int disposedGeomCount = GeomList.RemoveDisposed();

            if (disposedGeomCount > 0)
            {
                _broadPhaseCollider.ProcessDisposedGeoms();
            }

            BodyList.RemoveDisposed();
            ControllerList.RemoveDisposed();

            //allow each joint to validate itself. this is where a joint can Dispose of itself if need be.
            for (int i = 0; i < JointList.Count; i++)
            {
                JointList[i].Validate();
            }
            JointList.RemoveDisposed();

            //remove all arbiters that contain 1 or more disposed rigid bodies.
            ArbiterList.RemoveContainsDisposedBody(arbiterPool);
        }

        #region Added by Daniel Pramel 08/24/08

        private Scaling scaling;

        public Scaling Scaling
        {
            get { return scaling; }
            set { scaling = value; }
        }

        #endregion
    }

    public enum FrictionType
    {
        Average = 0,
        Minimum = 1
    }
}