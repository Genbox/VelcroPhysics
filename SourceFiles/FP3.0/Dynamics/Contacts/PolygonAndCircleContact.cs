using System.Diagnostics;

namespace FarseerPhysics
{
    internal class PolygonAndCircleContact : Contact
    {
	    internal PolygonAndCircleContact(Fixture fixtureA, Fixture fixtureB)
            : base(fixtureA, fixtureB)
        {
            Debug.Assert(_fixtureA.ShapeType == ShapeType.Polygon);
	        Debug.Assert(_fixtureB.ShapeType == ShapeType.Circle);
        }

        internal override void Evaluate()  
        {
            Body b1 = _fixtureA.GetBody();
            Body b2 = _fixtureB.GetBody();

            Transform xf1, xf2;
            b1.GetTransform(out xf1);
            b2.GetTransform(out xf2);

	        Collision.CollidePolygonAndCircle(ref _manifold,
                                        (PolygonShape)_fixtureA.GetShape(), ref xf1,
                                        (CircleShape)_fixtureB.GetShape(), ref xf2);
        }
    }
}
