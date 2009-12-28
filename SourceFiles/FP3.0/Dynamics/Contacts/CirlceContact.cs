using System.Diagnostics;

namespace FarseerPhysics
{
    internal class CircleContact : Contact
    {
	    internal CircleContact(Fixture fixtureA, Fixture fixtureB)
            : base(fixtureA, fixtureB)
        {
	        Debug.Assert(_fixtureA.ShapeType == ShapeType.Circle);
	        Debug.Assert(_fixtureB.ShapeType == ShapeType.Circle);
        }

	    internal override void Evaluate()  
        {
	        Body bodyA = _fixtureA.GetBody();
	        Body bodyB = _fixtureB.GetBody();
            Transform xfA, xfB;
            bodyA.GetTransform(out xfA);
            bodyB.GetTransform(out xfB);

	        Collision.CollideCircles(ref _manifold,
						        (CircleShape)_fixtureA.GetShape(), ref xfA,
                                (CircleShape)_fixtureB.GetShape(), ref xfB);
        }
    }
}
