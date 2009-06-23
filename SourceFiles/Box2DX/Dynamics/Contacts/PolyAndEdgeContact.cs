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

        public Manifold[] GetManifolds()
        {
            return new Manifold[] { _manifold };
        }

        public PolyAndEdgeContact(Fixture fixtureA, Fixture fixtureB)
            : base(fixtureA, fixtureB)
        {
            Box2DXDebug.Assert(_fixtureA.GetType() == ShapeType.PolygonShape);
            Box2DXDebug.Assert(_fixtureB.GetType() == ShapeType.EdgeShape);
        }

        public override void Evaluate()
        {
            Body bodyA = _fixtureA.GetBody();
            Body bodyB = _fixtureB.GetBody();

            Collision.Collision.CollidePolyAndEdge(ref _manifold, (PolygonShape)_fixtureA.GetShape(), bodyA.GetXForm(),
                (EdgeShape)_fixtureB.GetShape(), bodyB.GetXForm());
        }

        public override float ComputeTOI(Sweep sweepA, Sweep sweepB)
        {
           Box2DX.Collision.Collision.TOIInput input = new Box2DX.Collision.Collision.TOIInput;
	       input.SweepA = sweepA;
	       input.SweepA = sweepB;
	       input.SweepRadiusA = _fixtureA.ComputeSweepRadius(sweepA.localCenter);
	       input.SweepRadiusB = _fixtureB.ComputeSweepRadius(sweepB.localCenter);
	       input.Tolerance = Settings.LinearSlop;

	       return Box2DX.Collision.Collision.TimeOfImpact(input, _fixtureA.GetShape(), _fixtureB.GetShape());
        }

        new public static PolyAndEdgeContact Create(Fixture fixture1, Fixture fixture2)
        {
            return new PolyAndEdgeContact(fixture1, fixture2);
        }

        new public static void Destroy(ref PolyAndEdgeContact contact)
        {
            contact = null;
        }
    }
}
