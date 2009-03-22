using System;
using FarseerGames.FarseerPhysics.Interfaces;

#if (XNA)
using Microsoft.Xna.Framework;
#else
using FarseerGames.FarseerPhysics.Mathematics;
#endif

namespace FarseerGames.FarseerPhysics.Collisions
{
    public class SAT : INarrowPhaseCollider
    {
        private PhysicsSimulator _physicsSimulator;

        // 
        public SAT(PhysicsSimulator physicsSimulator)
        {
            _physicsSimulator = physicsSimulator;
        }

        /// <summary>
        /// Returns the contact list from two possibly intersecting Geom's. 
        /// This is the stationary version of this function. It doesn't 
        /// account for linear or angular motion.
        /// </summary>
        /// <param name="geomA">The first Geom.</param>
        /// <param name="geomB">The second Geom.</param>
        /// <param name="contactList">Set of Contacts between the two Geoms.
        /// NOTE- this will be empty if no contacts are present.</param>
        public void Collide(Geom geomA, Geom geomB, ContactList contactList)
        {
            ProjInfo infoA, infoB;
            Contact contact;
            Vector2 normal;
            
            if (TestIntersection(geomA.WorldVertices, geomB.WorldVertices, out infoA, out infoB))
            {
                
            }
        }

        private bool TestIntersection(Vertices A, Vertices B, out ProjInfo infoA, out ProjInfo infoB)
        {
            infoA = new ProjInfo();
            infoB = new ProjInfo();

            for (int i = A.Count - 1, j = 0; j < A.Count; i = j++)
            {
                Vector2 P = A[j];
                Vector2 D = A.GetEdgeNormal(i);

                ComputeInterval(A, D, out infoA);
                ComputeInterval(B, D, out infoB);

                if (Vector2.Dot(D, B[infoB.index[0]] - P) > 0.0f)
                {
                    return false;
                }
            }

            for (int i = B.Count - 1, j = 0; j < B.Count; i = j++)
            {
                Vector2 P = B[j];
                Vector2 D = B.GetEdgeNormal(i);

                ComputeInterval(A, D, out infoA);
                ComputeInterval(B, D, out infoB);

                if (Vector2.Dot(D, A[infoA.index[0]] - P) > 0.0f)
                {
                    return false;
                }
            }
            return true;
        }

        private void ComputeInterval(Vertices C, Vector2 D, out ProjInfo info)
        {
            info = new ProjInfo();

            info.index[0] = GetExtremeIndex(C, -D, out info.isUnique[0]);
            info.min = Vector2.Dot(D, C[info.index[0]]);
            info.index[1] = GetExtremeIndex(C, D, out info.isUnique[1]);
            info.max = Vector2.Dot(D, C[info.index[1]]);
        }

        private int GetExtremeIndex(Vertices C, Vector2 D, out bool isUnique)
        {
            int i = 0;
            int j = 0;

            isUnique = true;

            while (true)
            {
                int mid = GetMiddleIndex(i, j, C.Count);
                if (Vector2.Dot(D, C.GetEdge(mid)) > 0.0f)
                {
                    if (mid != i)
                        i = mid;
                    else
                        return j;
                }
                else
                {
                    if (Vector2.Dot(D, C.GetEdge(mid - 1)) < 0.0f)
                        j = mid;
                    else
                    {
                        isUnique = false;
                        return mid;
                    }
                }
            }
        }

        private int GetMiddleIndex(int i, int j, int N)
        {
            if (i < j)
                return (i + j) / 2;
            else 
                return ((i + j + N) / 2 % N);
        }

        private bool InsidePolygon(Vertices polygon, Vector2 p)
        {
          int counter = 0;
          int i;
          float xinters;
          Vector2 p1,p2;

          p1 = polygon[0];
          for (i=1;i<=polygon.Count;i++) {
              p2 = polygon[i % polygon.Count];
            if (p.Y > Math.Min(p1.Y,p2.Y)) {
              if (p.Y <= Math.Max(p1.Y,p2.Y)) {
                if (p.X <= Math.Max(p1.X,p2.X)) {
                  if (p1.Y != p2.Y) {
                    xinters = (p.Y-p1.Y)*(p2.X-p1.X)/(p2.Y-p1.Y)+p1.X;
                    if (p1.X == p2.X || p.X <= xinters)
                      counter++;
                  }
                }
              }
            }
            p1 = p2;
          }

          if (counter % 2 == 0)
            return false;
          else
            return true;
        }
    }

    public class ProjInfo
    {
        public float min, max;
        public int[] index = new int[2];
        public bool[] isUnique = new bool[2];
    }
}