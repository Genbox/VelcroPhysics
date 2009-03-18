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
        public void Collide(Geom geomA, Geom geomB, ContactList contactList)
        {
            Vector2 m_ncoll = new Vector2(0, 0);
            Vector2 m_mtd = new Vector2(0, 0);
            float m_tcoll = 0.0f;

            // find collision
            Vector2 delta = geomA.body.LinearVelocity - geomB.body.LinearVelocity;
            CollisionInfo m_collisionInfo = Collide(geomA, geomB, delta);
            bool m_collisionReported = (m_collisionInfo.m_overlapped || m_collisionInfo.m_collided);

            // convert collision info into collison plane info
            if (m_collisionInfo.m_overlapped)
            {
                if (m_collisionInfo.m_mtdLengthSquared <= 0.00001f)
                {
                    //assert(false);
                    m_collisionReported = false;
                    return;
                }

                m_ncoll = m_collisionInfo.m_mtd / (float)Math.Sqrt(m_collisionInfo.m_mtdLengthSquared);
                m_tcoll = 0.0f;
                m_mtd = m_collisionInfo.m_mtd;

            }
            else if (m_collisionInfo.m_collided)
            {
                m_ncoll = m_collisionInfo.m_Nenter;
                m_tcoll = m_collisionInfo.m_tenter;
                m_mtd = new Vector2(0, 0);
            }

            // find contact points at time of collision
            geomA.worldVertices.Translate(geomA.body.LinearVelocity * m_tcoll);
            geomB.worldVertices.Translate(geomB.body.LinearVelocity * m_tcoll);

            SupportPoints asup = GetSupports(geomA, m_ncoll);
            SupportPoints bsup = GetSupports(geomB, -m_ncoll);

            ContactManifold m_manifold = new ContactManifold(asup, bsup);

            // approximate the contact patch to a single contact pair.
            //m_contact = m_manifold.reduction();

            if (!m_collisionReported)
                return;

            //geometry1.TransformNormalToWorld(ref m_ncoll, out m_ncoll);

            Contact contact = new Contact(m_manifold.m_contact[0].m_position[0], m_ncoll, m_manifold.m_contact[0].m_distanceSquared, new ContactId(1, 1, 3));

            contactList.Add(contact);

            contact = new Contact(m_manifold.m_contact[1].m_position[0], m_ncoll, m_manifold.m_contact[1].m_distanceSquared, new ContactId(1, 2, 3));

            contactList.Add(contact);

            // overlapped. then separate the bodies.
            geomA.body.Position += m_mtd * (geomA.body.inverseMass / (geomA.body.inverseMass + geomB.body.inverseMass));
            geomB.body.Position -= m_mtd * (geomB.body.inverseMass / (geomA.body.inverseMass + geomB.body.inverseMass));
        }

        private CollisionInfo Collide(Geom geomA, Geom geomB, Vector2 delta)
        {
            CollisionInfo info = new CollisionInfo();
            // reset info to some weird values
            info.m_overlapped = true;		 // we'll be regressing tests from there
            info.m_collided = true;
            info.m_mtdLengthSquared = -1.0f; // flags mtd as not being calculated yet
            info.m_tenter = 1.0f;			 // flags swept test as not being calculated yet
            info.m_tleave = 0.0f;			 // <--- ....


            // test separation axes of current polygon
            for (int j = geomA.worldVertices.Count - 1, i = 0; i < geomA.worldVertices.Count; j = i, i++)
            {
                Vector2 v0 = geomA.worldVertices[j];
                Vector2 v1 = geomA.worldVertices[i];

                Vector2 edge = v1 - v0; // edge
                Vector2 axis = new Vector2(-edge.Y, edge.X); // sep axis is perpendicular ot the edge

                if (separatedByAxis(axis, geomA, geomB, delta, info))
                    return new CollisionInfo();
            }

            // test separation axes of other polygon
            for (int j = geomB.worldVertices.Count - 1, i = 0; i < geomB.worldVertices.Count; j = i, i++)
            {
                Vector2 v0 = geomB.worldVertices[j];
                Vector2 v1 = geomB.worldVertices[i];

                Vector2 edge = v1 - v0; // edge
                Vector2 axis = new Vector2(-edge.Y, edge.X); // sep axis is perpendicular ot the edge
                if (separatedByAxis(axis, geomB, geomA, delta, info))
                    return new CollisionInfo();
            }

            //assert(!(info.m_overlapped) || (info.m_mtdLengthSquared >= 0.0f));
            //assert(!(info.m_collided)   || (info.m_tenter <= info.m_tleave)); // can happen if delta is very very small

            // sanity checks
            info.m_overlapped &= (info.m_mtdLengthSquared >= 0.0f);
            info.m_collided &= (info.m_tenter <= info.m_tleave);

            // normalise normals
            info.m_Nenter.Normalize();
            info.m_Nleave.Normalize();

            return info;
        }

        private void calculateInterval(Geom geom, Vector2 axis, out float min, out float max)
        {
            min = max = Vector2.Dot(geom.worldVertices[0], axis);

            for (int i = 1; i < geom.worldVertices.Count; i++)
            {
                float d = Vector2.Dot(geom.worldVertices[i], axis);
                if (d < min)
                    min = d;
                else if (d > max)
                    max = d;
            }
        }

        private bool separatedByAxis(Vector2 axis, Geom geomA, Geom geomB, Vector2 delta, CollisionInfo info)
        {
            float mina, maxa;
            float minb, maxb;

            // calculate both polygon intervals along the axis we are testing
            calculateInterval(geomA, axis, out mina, out maxa);
            calculateInterval(geomB, axis, out minb, out maxb);

            // calculate the two possible overlap ranges.
            // either we overlap on the left or right of the polygon.
            float d0 = (maxb - mina); // 'left' side
            float d1 = (minb - maxa); // 'right' side
            float v = Vector2.Dot(axis, delta); // project delta on axis for swept tests

            bool sep_overlap = separatedByAxis_overlap(axis, d0, d1, info);
            bool sep_swept = separatedByAxis_swept(axis, d0, d1, v, info);

            // both tests didnt find any collision
            // return separated
            return (sep_overlap && sep_swept);
        }

        private bool separatedByAxis_overlap(Vector2 axis, float d0, float d1, CollisionInfo info)
        {
            if (!info.m_overlapped)
                return true;

            // intervals do not overlap. 
            // so no overlpa possible.
            if (d0 < 0.0f || d1 > 0.0f)
            {
                info.m_overlapped = false;
                return true;
            }

            // find out if we overlap on the 'right' or 'left' of the polygon.
            float overlap = (d0 < -d1) ? d0 : d1;

            // the axis length squared
            float axis_length_squared = Vector2.Dot(axis, axis);
            //assert(axis_length_squared > 0.00001f);

            // the mtd vector for that axis
            Vector2 sep = axis * (overlap / axis_length_squared);

            // the mtd vector length squared.
            float sep_length_squared = Vector2.Dot(sep, sep);

            // if that vector is smaller than our computed MTD (or the mtd hasn't been computed yet)
            // use that vector as our current mtd.
            if (sep_length_squared < info.m_mtdLengthSquared || (info.m_mtdLengthSquared < 0.0f))
            {
                info.m_mtdLengthSquared = sep_length_squared;
                info.m_mtd = sep;
            }
            return false;
        }

        private bool separatedByAxis_swept(Vector2 axis, float d0, float d1, float v, CollisionInfo info)
        {
            if (!info.m_collided)
                return true;

            // projection too small. ignore test
            if (Math.Abs(v) < 0.0000001f) return true;

            Vector2 N0 = axis;
            Vector2 N1 = -axis;
            float t0 = d0 / v;   // estimated time of collision to the 'left' side
            float t1 = d1 / v;  // estimated time of collision to the 'right' side

            // sort values on axis
            // so we have a valid swept interval
            if (t0 > t1)
            {
                float temp = t0;
                t0 = t1;
                t1 = temp;
                Vector2 tempVector = N0;
                N0 = N1;
                N1 = tempVector;
            }

            // swept interval outside [0, 1] boundaries. 
            // polygons are too far apart
            if (t0 > 1.0f || t1 < 0.0f)
            {
                info.m_collided = false;
                return true;
            }

            // the swept interval of the collison result hasn't been
            // performed yet.
            if (info.m_tenter > info.m_tleave)
            {
                info.m_tenter = t0;
                info.m_tleave = t1;
                info.m_Nenter = N0;
                info.m_Nleave = N1;
                // not separated
                return false;
            }
                // else, make sure our current interval is in 
                // range [info.m_tenter, info.m_tleave];
            else
            {
                // separated.
                if (t0 > info.m_tleave || t1 < info.m_tenter)
                {
                    info.m_collided = false;
                    return true;
                }

                // reduce the collison interval
                // to minima
                if (t0 > info.m_tenter)
                {
                    info.m_tenter = t0;
                    info.m_Nenter = N0;
                }
                if (t1 < info.m_tleave)
                {
                    info.m_tleave = t1;
                    info.m_Nleave = N1;
                }
                // not separated
                return false;
            }
        }

        private SupportPoints GetSupports(Geom geom, Vector2 axis)
        {
            SupportPoints supports = new SupportPoints();

            float min = -1.0f;
            const float threshold = 1.0E-1f;

            int num = geom.worldVertices.Count;
            for (int i = 0; i < num; i++)
            {
                float t = Vector2.Dot(axis, geom.worldVertices[i]);
                if (t < min || i == 0)
                    min = t;
            }

            for (int i = 0; i < num; i++)
            {
                float t = Vector2.Dot(axis, geom.worldVertices[i]);

                if (t < min + threshold)
                {
                    supports.m_support[supports.m_count++] = geom.worldVertices[i];
                    if (supports.m_count == 2) break;
                }
            }
            return supports;
        }
    }

    public class CollisionInfo
    {
        // overlaps
        public bool m_overlapped;
        public float m_mtdLengthSquared;
        public Vector2 m_mtd;

        // swept
        public bool m_collided;
        public Vector2 m_Nenter;
        public Vector2 m_Nleave;
        public float m_tenter;
        public float m_tleave;

        public CollisionInfo()
        {
            // swept test results
            m_tenter = 0.0f;            // <-- impossible value
            m_tleave = 0.0f;            // <-- impossible value
            m_Nenter = new Vector2(0, 0);	 // clear value
            m_Nleave = new Vector2(0, 0);	 // clear value
            m_collided = false;

            // overlap test results
            m_overlapped = false;
            m_mtd = new Vector2(0, 0);
            m_mtdLengthSquared = 0.0f;
        }
    }

    public class SupportPoints
    {
        public static int MAX_SUPPORTS = 4;
        public Vector2[] m_support;
        public int m_count;

        public SupportPoints()
        {
            m_count = 0;
            m_support = new Vector2[4];
        }
    }

    public class ContactPair
    {

        public Vector2[] m_position;
        public float m_distanceSquared;

        public ContactPair()
        {
            m_position = new Vector2[2];

            m_position[0] = m_position[1] = new Vector2(0, 0);
            m_distanceSquared = 0.0f;
        }

        public ContactPair(Vector2 a, Vector2 b)
        {
            m_position = new Vector2[2];

            Vector2 d = (b - a);
            m_position[0] = a;
            m_position[1] = b;
            m_distanceSquared = Vector2.Dot(d, d);
        }

        /*public int CompareContacts(const void* v0, const void* v1)
    { 
        ContactPair* V0 = (ContactPair*) v0;
        ContactPair* V1 = (ContactPair*) v1;
        return (V0->m_distanceSquared > V1->m_distanceSquared)? 1 : -1;
    }*/
    }

    public class ContactManifold
    {
        public ContactPair[] m_contact;
        public int m_count;

        public ContactManifold()
        {
            m_count = 0;
            m_contact = new ContactPair[3];
        }

        public ContactManifold(SupportPoints supports1, SupportPoints supports2)
        {
            m_contact = new ContactPair[4];

            if (supports1.m_count == 1)
            {
                if (supports2.m_count == 1)
                {
                    vertexVertex(supports1.m_support[0], supports2.m_support[0]);
                }
                else if (supports2.m_count == 2)
                {
                    vertexEdge(supports1.m_support[0], supports2.m_support);
                }
                else
                {
                    //assertf(false, "invalid support point count");
                }
            }
            else if (supports1.m_count == 2)
            {
                if (supports2.m_count == 1)
                {
                    edgeVertex(supports1.m_support, supports2.m_support[0]);
                }
                else if (supports2.m_count == 2)
                {
                    edgeEdge(supports1.m_support, supports2.m_support);
                }
                else
                {
                    //assertf(false, "invalid support point count");
                }
            }
            else
            {
                //assertf(false, "invalid support point count");
            }
        }

        private void edgeEdge(Vector2[] edge1, Vector2[] edge2)
        {
            // setup all the potential 4 contact pairs
            m_contact[0] = new ContactPair(edge1[0], closestPointOnEdge(edge2, edge1[0]));
            m_contact[1] = new ContactPair(edge1[1], closestPointOnEdge(edge2, edge1[1]));
            m_contact[2] = new ContactPair(closestPointOnEdge(edge1, edge2[0]), edge2[0]);
            m_contact[3] = new ContactPair(closestPointOnEdge(edge1, edge2[1]), edge2[1]);

            // sort the contact pairs by distance value
            //qsort(m_contact, 4, sizeof(m_contact[0]), CompareContacts);

            // take the closest two
            m_count = 2;
        }

        private void vertexVertex(Vector2 vertex1, Vector2 vertex2)
        {
            m_contact[0] = new ContactPair(vertex1, vertex2);
            m_count = 1;
        }

        private void edgeVertex(Vector2[] edge, Vector2 vertex)
        {
            m_contact[0] = new ContactPair(closestPointOnEdge(edge, vertex), vertex);
            m_count = 1;
        }

        private void vertexEdge(Vector2 vertex, Vector2[] edge)
        {
            m_contact[0] = new ContactPair(vertex, closestPointOnEdge(edge, vertex));
            m_count = 1;
        }

        Vector2 closestPointOnEdge(Vector2[] edge, Vector2 v)
        {
            Vector2 e = edge[1] - edge[0];
            Vector2 d = v - edge[0];
            float t = Vector2.Dot(e, d) / Vector2.Dot(e, e);
            t = (t < 0.0f) ? 0.0f : (t > 1.0f) ? 1.0f : t;
            return edge[0] + e * t;
        }
    }
}