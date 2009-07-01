using Box2DX.Collision;
using Box2DX.Common;

namespace Box2DX.Dynamics
{
    class EdgeAndCircleContact : Contact
    {
        public EdgeAndCircleContact(Fixture fixtureA, Fixture fixtureB)
            : base(fixtureA, fixtureB)
        {
            Box2DXDebug.Assert(fixtureA.GetType() == ShapeType.EdgeShape);
            Box2DXDebug.Assert(fixtureB.GetType() == ShapeType.CircleShape);
            Manifold.PointCount = 0;
            Manifold.Points[0].NormalImpulse = 0.0f;
            Manifold.Points[0].TangentImpulse = 0.0f;
        }

        public override void Evaluate()
        {
            Body bodyA = _fixtureA.GetBody();
            Body bodyB = _fixtureB.GetBody();

            Collision.Collision.CollideEdgeAndCircle(out Manifold,
                                    (EdgeShape)_fixtureA.GetShape(), bodyA.GetXForm(),
                                    (CircleShape)_fixtureB.GetShape(), bodyB.GetXForm());

        }

        public override float ComputeTOI(Sweep sweepA, Sweep sweepB)
        {
            Collision.Collision.TOIInput input;
            input.SweepA = sweepA;
            input.SweepB = sweepB;
            input.SweepRadiusA = _fixtureA.ComputeSweepRadius(sweepA.LocalCenter);
            input.SweepRadiusB = _fixtureB.ComputeSweepRadius(sweepB.LocalCenter);
            input.Tolerance = Settings.LinearSlop;

            return Collision.Collision.TimeOfImpact(input, (EdgeShape)_fixtureA.GetShape(), (CircleShape)_fixtureB.GetShape());
        }

        new public static Contact Create(Fixture fixtureA, Fixture fixtureB)
        {
            return new EdgeAndCircleContact(fixtureA, fixtureB);
        }

        new public static void Destroy(Contact contact)
        {
            contact = null;
        }
    }
}
