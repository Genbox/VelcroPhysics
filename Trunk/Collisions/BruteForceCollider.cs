using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Interfaces;

namespace FarseerGames.FarseerPhysics.Collisions
{
    /// <summary>
    /// A broad phase collider that uses a brute force algorithm
    /// </summary>
    public class BruteForceCollider : IBroadPhaseCollider
    {
        private Arbiter _arbiter;
        private Geom _geometryA;
        private Geom _geometryB;
        private PhysicsSimulator _physicsSimulator;

        public BruteForceCollider(PhysicsSimulator physicsSimulator)
        {
            _physicsSimulator = physicsSimulator;
        }

        #region IBroadPhaseCollider Members

        public void ProcessRemovedGeoms()
        {
            //not required by brute force collider
        }

        public void ProcessDisposedGeoms()
        {
            //not required by brute force collider
        }

        public void Add(Geom geom)
        {
            //not 
        }

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

                    //Possible early exits
                    if (!_geometryA.Body.enabled || !_geometryB.Body.enabled)
                    {
                        continue;
                    }

                    if ((_geometryA.CollisionGroup == _geometryB.CollisionGroup) && _geometryA.CollisionGroup != 0 &&
                        _geometryB.CollisionGroup != 0)
                    {
                        continue;
                    }

                    if (!_geometryA.CollisionEnabled || !_geometryB.CollisionEnabled)
                    {
                        continue;
                    }

                    //Don't collide two static bodies
                    if (_geometryA.Body.isStatic && _geometryB.Body.isStatic)
                    {
                        continue;
                    }

                    //Don't collide two geometries connected to the same body
                    if (_geometryA.Body == _geometryB.Body)
                    {
                        continue;
                    }

                    if (((_geometryA.CollisionCategories & _geometryB.CollidesWith) == CollisionCategories.None) &
                        ((_geometryB.CollisionCategories & _geometryA.CollidesWith) == CollisionCategories.None))
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

                    if (intersection)
                    {
                        _arbiter = _physicsSimulator.arbiterPool.Fetch();
                        _arbiter.ConstructArbiter(_geometryA, _geometryB, _physicsSimulator);

                        if (_physicsSimulator.arbiterList.Contains(_arbiter))
                        {
                            _physicsSimulator.arbiterPool.Release(_arbiter);
                        }
                        else
                        {
                            _physicsSimulator.arbiterList.Add(_arbiter);
                        }
                    }
                }
            }
        }
    }
}