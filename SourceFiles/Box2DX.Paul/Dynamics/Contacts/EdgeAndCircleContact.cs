using System;
using System.Collections.Generic;
using System.Text;

using Box2DX.Collision;
using Box2DX.Common;

namespace Box2DX.Dynamics
{
    class EdgeAndCircleContact : Contact
    {
        public Manifold _manifold = new Manifold();

        public override Manifold[] GetManifolds()
        {
            return new Manifold[] { _manifold };
        }

        public EdgeAndCircleContact(Shape s1, Shape s2)
			: base(s1, s2)
		{
			Box2DXDebug.Assert(_shape1.GetType() == ShapeType.EdgeShape);
			Box2DXDebug.Assert(_shape2.GetType() == ShapeType.CircleShape);
			_manifold.PointCount = 0;
            _manifold.Points[0].NormalImpulse = 0.0f;
            _manifold.Points[0].TangentImpulse = 0.0f;
		}

        public override void Evaluate(ContactListener listener)
        {
            Body b1 = _shape1.GetBody();
            Body b2 = _shape2.GetBody();
#warning "needfix"
            //memcpy(&m0, &_manifold, sizeof(Manifold));
            Manifold m0 = _manifold.Clone();

            Collision.Collision.CollideEdgeAndCircle(ref _manifold, (EdgeShape)_shape1, b1.GetXForm(),
                (CircleShape)_shape2, b2.GetXForm());

            //bool[] persisted = new bool[] { false, false };

            ContactPoint cp = new ContactPoint();
            cp.Shape1 = _shape1;
            cp.Shape2 = _shape2;
            cp.Friction = Settings.MixFriction(_shape1.Friction, _shape2.Friction);
            cp.Restitution = Settings.MixRestitution(_shape1.Restitution, _shape2.Restitution);

            if (_manifold.PointCount > 0)
            {
                _manifoldCount = 1;
                ManifoldPoint mp = _manifold.Points[0];

                if (m0.PointCount == 0)
                {
                    mp.NormalImpulse = 0.0f;
                    mp.TangentImpulse = 0.0f;

                    if (listener != null)
                    {
                        cp.Position = b1.GetWorldPoint(mp.LocalPoint1);
                        Vec2 v1 = b1.GetLinearVelocityFromLocalPoint(mp.LocalPoint1);
                        Vec2 v2 = b2.GetLinearVelocityFromLocalPoint(mp.LocalPoint2);
                        cp.Velocity = v2 - v1;
                        cp.Normal = _manifold.Normal;
                        cp.Separation = mp.Separation;
                        cp.ID = mp.ID;
                        listener.Add(cp);
                    }
                }
                else
                {
                    ManifoldPoint mp0 = m0.Points[0];
                    mp.NormalImpulse = mp0.NormalImpulse;
                    mp.TangentImpulse = mp0.TangentImpulse;

                    if (listener != null)
                    {
                        cp.Position = b1.GetWorldPoint(mp.LocalPoint1);
                        Vec2 v1 = b1.GetLinearVelocityFromLocalPoint(mp.LocalPoint1);
                        Vec2 v2 = b2.GetLinearVelocityFromLocalPoint(mp.LocalPoint2);
                        cp.Velocity = v2 - v1;
                        cp.Normal = _manifold.Normal;
                        cp.Separation = mp.Separation;
                        cp.ID = mp.ID;
                        listener.Persist(cp);
                    }
                }
            }
            else
            {
                _manifoldCount = 0;
                if (m0.PointCount > 0 && listener != null)
                {
                    ManifoldPoint mp0 = m0.Points[0];
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
        }

        new public static Contact Create(Shape shape1, Shape shape2)
        {
            return new EdgeAndCircleContact(shape1, shape2);
        }

        new public static void Destroy(Contact contact)
        {
            contact = null;
        }
    }
}
