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
            //Iterate all the geoms and check against the next
            for (int i = 0; i < _physicsSimulator.GeomList.Count - 1; i++)
            {
                for (int j = i + 1; j < _physicsSimulator.GeomList.Count; j++)
                {
                    _geometryA = _physicsSimulator.GeomList[i];
                    _geometryB = _physicsSimulator.GeomList[j];

                    if (!_geometryA.body.Enabled || !_geometryB.body.Enabled)
                        continue;

                    if ((_geometryA.CollisionGroup == _geometryB.CollisionGroup) &&
                        _geometryA.CollisionGroup != 0 && _geometryB.CollisionGroup != 0)
                        continue;

                    if (!_geometryA.CollisionEnabled || !_geometryB.CollisionEnabled)
                        continue;

                    if (_geometryA.body.isStatic && _geometryB.body.isStatic)
                        continue;

                    if (_geometryA.body == _geometryB.body)
                        continue;

                    if (((_geometryA.CollisionCategories & _geometryB.CollidesWith) == CollisionCategory.None) &
                        ((_geometryB.CollisionCategories & _geometryA.CollidesWith) == CollisionCategory.None))
                        continue;

                    if (_geometryA.IsGeometryIgnored(_geometryB) || _geometryB.IsGeometryIgnored(_geometryA))
                    {
                        continue;
                    }

                    //Assume intersection
                    bool intersection = true;

                    //Check if there is no intersection
                    if (_geometryA.AABB.min.X > _geometryB.AABB.max.X || _geometryB.AABB.min.X > _geometryA.AABB.max.X)
                        intersection = false;
                    else if (_geometryA.AABB.min.Y > _geometryB.AABB.Max.Y || _geometryB.AABB.min.Y > _geometryA.AABB.Max.Y)
                        intersection = false;

                    //Call the OnBroadPhaseCollision event first. If the user aborts the collision
                    //it will not create an arbiter
                    if (OnBroadPhaseCollision != null)
                        intersection = OnBroadPhaseCollision(_geometryA, _geometryB);

                    //If the user aborted the intersection, continue to the next geometry.
                    if (!intersection)
                        continue;

                    Arbiter arbiter = _physicsSimulator.arbiterPool.Fetch();
                    arbiter.ConstructArbiter(_geometryA, _geometryB, _physicsSimulator);

                    if (!_physicsSimulator.ArbiterList.Contains(arbiter))
                        _physicsSimulator.ArbiterList.Add(arbiter);
                    else
                        _physicsSimulator.arbiterPool.Insert(arbiter);
                }
            }
        }

        #endregion
    }
}