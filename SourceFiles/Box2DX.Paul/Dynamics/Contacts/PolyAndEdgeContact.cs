using System;
using System.Collections.Generic;
using System.Text;

using Box2DX.Collision;
using Box2DX.Common;

namespace Box2DX.Dynamics
{
    class PolyAndEdgeContact : Contact
    {
        public Manifold _manifold = new Manifold();

        public override Manifold[] GetManifolds()
        {
            return new Manifold[] { _manifold };
        }

        public PolyAndEdgeContact(Shape s1, Shape s2)
            : base(s1, s2)
        {
            Box2DXDebug.Assert(_shape1.GetType() == ShapeType.PolygonShape);
            Box2DXDebug.Assert(_shape2.GetType() == ShapeType.EdgeShape);
            _manifold.PointCount = 0;
        }

        public override void Evaluate(ContactListener listener)
        {
            Body b1 = _shape1.GetBody();
            Body b2 = _shape2.GetBody();
#warning "needfix"
            //memcpy(&m0, &_manifold, sizeof(Manifold));
            Manifold m0 = _manifold.Clone();

            Collision.Collision.CollidePolyAndEdge(ref _manifold, (PolygonShape)_shape1, b1.GetXForm(),
                (EdgeShape)_shape2, b2.GetXForm());

            bool[] persisted = new bool[] { false, false };

            ContactPoint cp = new ContactPoint();
            cp.Shape1 = _shape1;
            cp.Shape2 = _shape2;
            cp.Friction = Settings.MixFriction(_shape1.Friction, _shape2.Friction);
            cp.Restitution = Settings.MixRestitution(_shape1.Restitution, _shape2.Restitution);

            if (_manifold.PointCount > 0)
            {
                // Match old contact ids to new contact ids and copy the
                // stored impulses to warm start the solver.
                for (int i = 0; i < _manifold.PointCount; ++i)
                {
                    ManifoldPoint mp = _manifold.Points[i];
                    mp.NormalImpulse = 0.0f;
                    mp.TangentImpulse = 0.0f;
                    bool found = false;
                    ContactID id = mp.ID;

                    for (int j = 0; j < m0.PointCount; ++j)
                    {
                        if (persisted[j] == true)
                        {
                            continue;
                        }

                        ManifoldPoint mp0 = m0.Points[j];

                        if (mp0.ID.Key == id.Key)
                        {
                            persisted[j] = true;
                            mp.NormalImpulse = mp0.NormalImpulse;
                            mp.TangentImpulse = mp0.TangentImpulse;

                            // A persistent point.
                            found = true;

                            // Report persistent point.
                            if (listener != null)
                            {
                                cp.Position = b1.GetWorldPoint(mp.LocalPoint1);
                                Vec2 v1 = b1.GetLinearVelocityFromLocalPoint(mp.LocalPoint1);
                                Vec2 v2 = b2.GetLinearVelocityFromLocalPoint(mp.LocalPoint2);
                                cp.Velocity = v2 - v1;
                                cp.Normal = _manifold.Normal;
                                cp.Separation = mp.Separation;
                                cp.ID = id;
                                listener.Persist(cp);
                            }
                            break;
                        }
                    }

                    // Report added point.
                    if (found == false && listener != null)
                    {
                        cp.Position = b1.GetWorldPoint(mp.LocalPoint1);
                        Vec2 v1 = b1.GetLinearVelocityFromLocalPoint(mp.LocalPoint1);
                        Vec2 v2 = b2.GetLinearVelocityFromLocalPoint(mp.LocalPoint2);
                        cp.Velocity = v2 - v1;
                        cp.Normal = _manifold.Normal;
                        cp.Separation = mp.Separation;
                        cp.ID = id;
                        listener.Add(cp);
                    }
                }

                _manifoldCount = 1;
            }
            else
            {
                _manifoldCount = 0;
            }

            if (listener == null)
            {
                return;
            }

            // Report removed Points.
            for (int i = 0; i < m0.PointCount; ++i)
            {
                if (persisted[i])
                {
                    continue;
                }

                ManifoldPoint mp0 = m0.Points[i];
                cp.Position = b1.GetWorldPoint(mp0.LocalPoint1);
                Vec2 v1 = b1.GetLinearVelocityFromLocalPoint(mp0.LocalPoint1);
                Vec2 v2 = b2.GetLinearVelocityFromLocalPoint(mp0.LocalPoint2);
                cp.Velocity = v2 - v1;
                cp.Normal = m0.Normal;
                cp.Separation = mp0.Separation;
                cp.ID = mp0.ID;
                listener.Remove(cp);
            }
        }

        new public static Contact Create(Shape shape1, Shape shape2)
        {
            return new PolyAndEdgeContact(shape1, shape2);
        }

        new public static void Destroy(Contact contact)
        {
            contact = null;
        }
    }
}
