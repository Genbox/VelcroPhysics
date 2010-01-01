/*
* Box2D.XNA port of Box2D:
* Copyright (c) 2009 Brandon Furtwangler, Nathan Furtwangler
*
* Original source Box2D:
* Copyright (c) 2006-2009 Erin Catto http://www.gphysics.com 
* 
* This software is provided 'as-is', without any express or implied 
* warranty.  In no event will the authors be held liable for any damages 
* arising from the use of this software. 
* Permission is granted to anyone to use this software for any purpose, 
* including commercial applications, and to alter it and redistribute it 
* freely, subject to the following restrictions: 
* 1. The origin of this software must not be misrepresented; you must not 
* claim that you wrote the original software. If you use this software 
* in a product, an acknowledgment in the product documentation would be 
* appreciated but is not required. 
* 2. Altered source versions must be plainly marked as such, and must not be 
* misrepresented as being the original software. 
* 3. This notice may not be removed or altered from any source distribution. 
*/

using System.Diagnostics;

namespace FarseerPhysics
{
    internal class CircleContact : Contact
    {
	    internal CircleContact(Fixture fixtureA, Fixture fixtureB)
            : base(fixtureA, fixtureB)
        {
	        Debug.Assert(FixtureA.ShapeType == ShapeType.Circle);
	        Debug.Assert(FixtureB.ShapeType == ShapeType.Circle);
        }

        protected override void Evaluate()  
        {
	        Body bodyA = FixtureA.GetBody();
	        Body bodyB = FixtureB.GetBody();
            Transform xfA, xfB;
            bodyA.GetTransform(out xfA);
            bodyB.GetTransform(out xfB);

	        Collision.CollideCircles(ref Manifold,
						        (CircleShape)FixtureA.Shape, ref xfA,
                                (CircleShape)FixtureB.Shape, ref xfB);
        }
    }
}
