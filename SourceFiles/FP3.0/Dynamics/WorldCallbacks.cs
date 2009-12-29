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
    public interface IDestructionListener
    {
        void SayGoodbye(Joint joint);
        void SayGoodbye(Fixture fixture);
    }

    public interface IContactFilter
    {
        bool ShouldCollide(Fixture fixtureA, Fixture fixtureB);
        bool RayCollide(object userData, Fixture fixture);
    }

    public struct ContactImpulse
    {
        public FixedArray2<float> normalImpulses;
        public FixedArray2<float> tangentImpulses;
    }

    public interface IContactListener
    {
        void BeginContact(Contact contact);
        void EndContact(Contact contact);
        void PreSolve(Contact contact, ref Manifold oldManifold);
        void PostSolve(Contact contact, ref ContactImpulse impulse);
    }

    public class DefaultContactFilter : IContactFilter
    {
        public bool ShouldCollide(Fixture fixtureA, Fixture fixtureB)
        {
            Filter filterA;
            fixtureA.GetFilterData(out filterA);

            Filter filterB;
            fixtureB.GetFilterData(out filterB);

            if (filterA.groupIndex == filterB.groupIndex && filterA.groupIndex != 0)
            {
                return filterA.groupIndex > 0;
            }

            bool collide = (filterA.maskBits & filterB.categoryBits) != 0 && (filterA.categoryBits & filterB.maskBits) != 0;

            return collide;
        }

        public bool RayCollide(object userData, Fixture fixture)
        {
            // By default, cast userData as a fixture, and then collide if the shapes would collide
            if (userData == null)
            {
                return true;
            }

            return ShouldCollide((Fixture)userData, fixture);
        }
    }

    public class DefaultContactListener : IContactListener
    {
        public void BeginContact(Contact contact) { }
        public void EndContact(Contact contact) { }
        public void PreSolve(Contact contact, ref Manifold oldManifold) { }
        public void PostSolve(Contact contact, ref ContactImpulse impulse) { }
    }
}
