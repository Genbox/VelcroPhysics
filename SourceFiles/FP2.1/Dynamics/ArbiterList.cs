using System.Collections.Generic;
using FarseerGames.FarseerPhysics.Collisions;

namespace FarseerGames.FarseerPhysics.Dynamics
{
    /// <summary>
    /// Provides an implementation of a strongly typed List with Arbiter
    /// </summary>
    public class ArbiterList : List<Arbiter>
    {
        private GeomPairBitmap _geomPairBitmap;

        public ArbiterList(int capacity)
        {
            Capacity = capacity;
        }

        public void PrepareForBroadphaseCollision(List<Geom> geomList)
        {
            if (_geomPairBitmap == null)
            {
                _geomPairBitmap = new GeomPairBitmap(geomList.Count, this);
            }
            else
            {
                _geomPairBitmap.Clear(geomList.Count, this);
            }

            // at this point the Geomlist should be complete, and we can generate new collision ids
            // for the next run of the broadphasecollider
            CollisionIdGenerator collisionIdGenerator = new CollisionIdGenerator();
            foreach (Geom geom in geomList)
            {
                geom.CollisionId = collisionIdGenerator.NextCollisionId();
            }
        }

        public void AddArbiterForGeomPair(PhysicsSimulator physicsSimulator, Geom geom1, Geom geom2)
        {
            if (!_geomPairBitmap.TestAndSet(geom1, geom2))
            {
                Arbiter arbiter = physicsSimulator.arbiterPool.Fetch();
                arbiter.ConstructArbiter(geom1, geom2, physicsSimulator);
                Add(arbiter);
            }
        }

        public void RemoveContactCountEqualsZero(Pool<Arbiter> arbiterPool)
        {
            for (int i = Count - 1; i >= 0; i--)
            {
                //If they don't have any contacts associated with them. Remove them.
                if (this[i].ContactList.Count == 0)
                {
                    Arbiter current = this[i];
                    RemoveAt(i);
                    arbiterPool.Insert(current);

                    //No contacts exist between the two geometries, fire the OnSeperation event.
                    if (current.GeometryA.OnSeparation != null)
                    {
                        current.GeometryA.OnSeparation(current.GeometryA, current.GeometryB);
                    }

                    if (current.GeometryB.OnSeparation != null)
                    {
                        current.GeometryB.OnSeparation(current.GeometryB, current.GeometryA);
                    }
                }
            }
        }

        public void CleanArbiterList(Pool<Arbiter> arbiterPool)
        {
            for (int i = Count - 1; i >= 0; i--)
            {
                if (this[i].ContainsInvalidGeom())
                {
                    Arbiter current = this[i];
                    RemoveAt(i);
                    arbiterPool.Insert(current);
                }
            }
        }
    }
}