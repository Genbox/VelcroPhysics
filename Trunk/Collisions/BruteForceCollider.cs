using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Interfaces;

namespace FarseerGames.FarseerPhysics.Collisions
{
    /// <summary>
    /// A broad phase collider that uses a brute force algorithm.
    /// </summary>
    public class BruteForceCollider : IBroadPhaseCollider
    {
        private Geom _geometryA;
        private Geom _geometryB;
        private PhysicsSimulator _physicsSimulator;

        public BruteForceCollider(PhysicsSimulator physicsSimulator)
        {
            _physicsSimulator = physicsSimulator;
        }

        #region IBroadPhaseCollider Members

        /// <summary>
        /// Fires when a broad phase collision occurs
        /// </summary>
        public event BroadPhaseCollisionHandler OnBroadPhaseCollision;

        ///<summary>
        /// Not required by brute force collider
        ///</summary>
        public void ProcessRemovedGeoms()
        {
        }

        ///<summary>
        /// Not required by brute force collider
        ///</summary>
        public void ProcessDisposedGeoms()
        {
        }

        ///<summary>
        /// Not required by brute force collider
        ///</summary>
        public void Add(Geom geom)
        {
        }

        /// <summary>
        /// Updates this instance.
        /// </summary>
        public void Update()
        {
            DoCollision();
        }

        #endregion

        private void DoCollision()
        {
            //Iterate all the geoms and check against the next
            for (int i = 0; i < _physicsSimulator.geomList.Count - 1; i++)
            {
                for (int j = i + 1; j < _physicsSimulator.geomList.Count; j++)
                {
                    _geometryA = _physicsSimulator.geomList[i];
                    _geometryB = _physicsSimulator.geomList[j];

                    if (!_geometryA.body.Enabled || !_geometryB.body.Enabled)
                        continue;

                    if ((_geometryA.collisionGroup == _geometryB.collisionGroup) &&
                        _geometryA.collisionGroup != 0 && _geometryB.collisionGroup != 0)
                        continue;

                    if (!_geometryA.collisionEnabled || !_geometryB.collisionEnabled)
                        continue;

                    if (_geometryA.body.isStatic && _geometryB.body.isStatic)
                        continue;

                    if (_geometryA.body == _geometryB.body)
                        continue;

                    if (((_geometryA.CollisionCategories & _geometryB.CollidesWith) == CollisionCategory.None) &
                        ((_geometryB.CollisionCategories & _geometryA.CollidesWith) == CollisionCategory.None))
                    {
                        continue;
                    }

                    //Assume intersection
                    bool intersection = true;

                    #region INLINE: if (AABB.Intersect(_geometryA.aabb, _geometryB.aabb)) ....

                    //Check if there is no intersection
                    if (_geometryA.AABB.min.X > _geometryB.AABB.max.X || _geometryB.AABB.min.X > _geometryA.AABB.max.X)
                    {
                        intersection = false;
                    }
                    else if (_geometryA.AABB.min.Y > _geometryB.AABB.Max.Y ||
                             _geometryB.AABB.min.Y > _geometryA.AABB.Max.Y)
                    {
                        intersection = false;
                    }

                    #endregion

                    //Call the OnBroadPhaseCollision event first. If the user aborts the collision
                    //it will not create an arbiter

                    if (OnBroadPhaseCollision != null)
                        intersection = OnBroadPhaseCollision(_geometryA, _geometryB);

                    //If the user aborted the intersection, continue to the next geometry.
                    if (!intersection)
                        continue;

                    //Note: Commented this out and copy-paste into from other colliders
                    //_arbiter = _physicsSimulator.arbiterPool.Fetch();
                    //_arbiter.ConstructArbiter(_geometryA, _geometryB, _physicsSimulator);

                    //if (_physicsSimulator.arbiterList.Contains(_arbiter))
                    //{
                    //    _physicsSimulator.arbiterPool.Insert(_arbiter);
                    //}
                    //else
                    //{
                    //    _physicsSimulator.arbiterList.Add(_arbiter);
                    //}

                    Arbiter arbiter = _physicsSimulator.arbiterPool.Fetch();
                    arbiter.ConstructArbiter(_geometryA, _geometryB, _physicsSimulator);

                    if (!_physicsSimulator.arbiterList.Contains(arbiter))
                        _physicsSimulator.arbiterList.Add(arbiter);
                    else
                        _physicsSimulator.arbiterPool.Insert(arbiter);
                }
            }
        }
    }
}