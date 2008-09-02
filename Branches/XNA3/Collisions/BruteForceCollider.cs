using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Interfaces;

namespace FarseerGames.FarseerPhysics.Collisions
{
    public class BruteForceCollider : IBroadPhaseCollider
    {
        private readonly PhysicsSimulator _physicsSimulator;
        private Arbiter _arbiter;
        private Geom _geometryA;
        private Geom _geometryB;

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
            for (int i = 0; i < _physicsSimulator.geomList.Count - 1; i++)
            {
                for (int j = i + 1; j < _physicsSimulator.geomList.Count; j++)
                {
                    _geometryA = _physicsSimulator.geomList[i];
                    _geometryB = _physicsSimulator.geomList[j];
                    //possible early exits
                    if (!_geometryA.body.enabled || !_geometryB.body.enabled)
                    {
                        continue;
                    }

                    if ((_geometryA.collisionGroup == _geometryB.collisionGroup) && _geometryA.collisionGroup != 0 &&
                        _geometryB.collisionGroup != 0)
                    {
                        continue;
                    }

                    if (!_geometryA.collisionEnabled || !_geometryB.collisionEnabled)
                    {
                        continue;
                    }

                    if (_geometryA.body.isStatic && _geometryB.body.isStatic)
                    {
                        //don't collide two static bodies
                        continue;
                    }

                    if (_geometryA.body == _geometryB.body)
                    {
                        //don't collide two geometries connected to the same body
                        continue;
                    }

                    if (((_geometryA.collisionCategories & _geometryB.collidesWith) == CollisionCategories.None) &
                        ((_geometryB.collisionCategories & _geometryA.collidesWith) == CollisionCategories.None))
                    {
                        continue;
                    }

                    bool intersection = true;

                    #region INLINE: if (AABB.Intersect(_geometryA.aabb, _geometryB.aabb)) ....

                    if (_geometryA.aabb.min.X > _geometryB.aabb.max.X || _geometryB.aabb.min.X > _geometryA.aabb.max.X)
                    {
                        intersection = false;
                    }
                    else if (_geometryA.aabb.min.Y > _geometryB.aabb.Max.Y || _geometryB.aabb.min.Y > _geometryA.aabb.Max.Y)
                    {
                        intersection = false;
                    }

                    #endregion

                    if (intersection)
                    {
                        _arbiter = _physicsSimulator.arbiterPool.Fetch();
                        _arbiter.ConstructArbiter(_geometryA, _geometryB, _physicsSimulator);

                        if (!_physicsSimulator.arbiterList.Contains(_arbiter))
                        {
                            _physicsSimulator.arbiterList.Add(_arbiter);
                        }
                        else
                        {
                            _physicsSimulator.arbiterPool.Release(_arbiter);
                        }
                    }
                }
            }
        }
    }
}