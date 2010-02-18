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

namespace FarseerPhysics
{
    /// <summary>
    /// This delegate is called when a contact is deleted
    /// </summary>
    public delegate void EndContactDelegate(Contact contact);
    public delegate void BeginContactDelegate(Contact contact);
    public delegate void PreSolveDelegate(Contact contact, ref Manifold oldManifold);
    public delegate void PostSolveDelegate(Contact contact, ref ContactImpulse impulse);

    public delegate void FixtureRemovedDelegate(Fixture fixture);
    public delegate void JointRemovedDelegate(Joint joint);

    public delegate bool CollisionFilterDelegate(Fixture fixtureA, Fixture fixtureB);

    public struct ContactImpulse
    {
        public FixedArray2<float> normalImpulses;
        public FixedArray2<float> tangentImpulses;
    }

    public class DefaultContactFilter
    {
        public DefaultContactFilter(World world)
        {
            world.ContactManager.CollisionFilter += ShouldCollide;
        }

        private static bool ShouldCollide(Fixture fixtureA, Fixture fixtureB)
        {
            if ((fixtureA.CollisionGroup == fixtureB.CollisionGroup) && fixtureA.CollisionGroup != 0 && fixtureB.CollisionGroup != 0)
                return false;

            if (((fixtureA.CollisionCategories & fixtureB.CollidesWith) == CollisionCategory.None) &
                ((fixtureB.CollisionCategories & fixtureA.CollidesWith) == CollisionCategory.None))
                return false;

            if (fixtureA.IsGeometryIgnored(fixtureB) || fixtureB.IsGeometryIgnored(fixtureA))
                return false;

            return true;
        }
    }
}
