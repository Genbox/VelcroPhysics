/*
  Box2DX Copyright (c) 2008 Ihar Kalasouski http://code.google.com/p/box2dx
  Box2D original C++ version Copyright (c) 2006-2007 Erin Catto http://www.gphysics.com

  This software is provided 'as-is', without any express or implied
  warranty.  In no event will the authors be held liable for any damages
  arising from the use of this software.

  Permission is granted to anyone to use this software for any purpose,
  including commercial applications, and to alter it and redistribute it
  freely, subject to the following restrictions:

  1. The origin of this software must not be misrepresented; you must not
     claim that you wrote the original software. If you use this software
     in a product, an acknowledgment in the product documentation would be
     appreciated but is not required.
  2. Altered source versions must be plainly marked as such, and must not be
     misrepresented as being the original software.
  3. This notice may not be removed or altered from any source distribution.
*/

using Box2DX.Collision;

namespace Box2DX.Dynamics
{
    public class PolyAndCircleContact : Contact
    {
        public PolyAndCircleContact(Fixture fixtureA, Fixture fixtureB)
            : base(fixtureA, fixtureB)
        {
            Box2DXDebug.Assert(_fixtureA.GetType() == ShapeType.PolygonShape);
            Box2DXDebug.Assert(_fixtureB.GetType() == ShapeType.CircleShape);
        }

        new public static Contact Create(Fixture fixtureA, Fixture fixtureB)
        {
            return new PolyAndCircleContact(fixtureA, fixtureB);
        }

        new public static void Destroy(Contact contact)
        {
            contact = null;
        }

        public override void Evaluate()
        {
            Body bodyA = _fixtureA.GetBody();
            Body bodyB = _fixtureB.GetBody();

            Collision.Collision.CollidePolygonAndCircle(out Manifold,
                                        (PolygonShape)_fixtureA.GetShape(), bodyA.GetTransform(),
                                        (CircleShape)_fixtureB.GetShape(), bodyB.GetTransform());
        }
    }
}
