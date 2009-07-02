using System;
using System.Collections.Generic;
using System.Text;

using Box2DX.Collision;
using Box2DX.Common;

namespace Box2DX.Dynamics
{
    class PolyAndEdgeContact : Contact
    {
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

            Collision.Collision.CollidePolyAndEdge(out Manifold, 
                (PolygonShape)_fixtureA.GetShape(), bodyA.GetXForm(),
                (EdgeShape)_fixtureB.GetShape(), bodyB.GetXForm());
        }

        public override float ComputeTOI(Sweep sweepA, Sweep sweepB)
        {
           Collision.Collision.TOIInput input = new Collision.Collision.TOIInput();
	       input.SweepA = sweepA;
	       input.SweepA = sweepB;
	       input.SweepRadiusA = _fixtureA.ComputeSweepRadius(sweepA.LocalCenter);
	       input.SweepRadiusB = _fixtureB.ComputeSweepRadius(sweepB.LocalCenter);
	       input.Tolerance = Settings.LinearSlop;

           return Collision.Collision.TimeOfImpact(input, (PolygonShape)_fixtureA.GetShape(), (EdgeShape)_fixtureB.GetShape());
        }

        new public static Contact Create(Fixture fixtureA, Fixture fixtureB)
        {
            return new PolyAndEdgeContact(fixtureA, fixtureB);
        }

        new public static void Destroy(Contact contact)
        {
            contact = null;
        }
    }
}
