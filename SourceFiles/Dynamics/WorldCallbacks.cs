/*
* Farseer Physics Engine based on Box2D.XNA port:
* Copyright (c) 2010 Ian Qvist
* 
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

using FarseerPhysics.Collision;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Dynamics.Joints;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Dynamics
{
    /// <summary>
    /// Called for each fixture found in the query. You control how the ray cast
    /// proceeds by returning a float:
    /// return -1: ignore this fixture and continue
    /// return 0: terminate the ray cast
    /// return fraction: clip the ray to this point
    /// return 1: don't clip the ray and continue
    /// @param fixture the fixture hit by the ray
    /// @param point the point of initial intersection
    /// @param normal the normal vector at the point of intersection
    /// @return -1 to filter, 0 to terminate, fraction to clip the ray for
    /// closest hit, 1 to continue
    /// </summary>
    public delegate float RayCastCallback(Fixture fixture, Vector2 point, Vector2 normal, float fraction);

    /// <summary>
    /// This delegate is called when a contact is deleted
    /// </summary>
    public delegate void EndContactDelegate(Contact contact);

    /// <summary>
    /// This delegate is called when a contact is created
    /// </summary>
    public delegate void BeginContactDelegate(Contact contact);

    public delegate void PreSolveDelegate(Contact contact, ref Manifold oldManifold);

    public delegate void PostSolveDelegate(Contact contact, ref ContactImpulse impulse);

    public delegate void FixtureDelegate(Fixture fixture);

    public delegate void JointDelegate(Joint joint);

    public delegate void BodyDelegate(Body body);

    public delegate bool CollisionFilterDelegate(Fixture fixtureA, Fixture fixtureB);

    public sealed class DefaultContactFilter
    {
        public DefaultContactFilter(World world)
        {
            world.ContactManager.ContactFilter += ShouldCollide;
        }

        private static bool ShouldCollide(Fixture fixtureA, Fixture fixtureB)
        {
            if (fixtureA.CollisionGroup == fixtureB.CollisionGroup && fixtureA.CollisionGroup != 0)
            {
                return fixtureA.CollisionGroup > 0;
            }

            bool collide = (fixtureA.CollidesWith & fixtureB.CollisionCategories) != 0 && (fixtureA.CollisionCategories & fixtureB.CollidesWith) != 0;

            if (collide)
            {
                if (fixtureA.IsFixtureIgnored(fixtureB) || fixtureB.IsFixtureIgnored(fixtureA))
                {
                    return false;
                }
            }

            return collide;
        }
    }

    public struct ContactImpulse
    {
        public FixedArray2<float> NormalImpulses;
        public FixedArray2<float> TangentImpulses;
    }
}