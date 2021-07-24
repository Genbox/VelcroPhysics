/*
* Velcro Physics:
* Copyright (c) 2017 Ian Qvist
* 
* Original source Box2D:
* Copyright (c) 2006-2011 Erin Catto http://www.box2d.org 
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

using Genbox.VelcroPhysics.Collision.ContactSystem;
using Genbox.VelcroPhysics.Collision.Shapes;
using Genbox.VelcroPhysics.Definitions;
using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Dynamics.Solver;
using Genbox.VelcroPhysics.Factories;
using Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Framework;
using Genbox.VelcroPhysics.Utilities;
using Microsoft.Xna.Framework;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Tests
{
    internal class BreakableTest : Test
    {
        private readonly Body _body1;
        private Vector2 _velocity;
        private float _angularVelocity;
        private readonly PolygonShape _shape1;
        private readonly PolygonShape _shape2;
        private readonly Fixture _piece1;
        private Fixture _piece2;

        private bool _broke;
        private bool _break;

        private BreakableTest()
        {
            // Ground body
            {
                BodyDef bd = new BodyDef();
                Body ground = BodyFactory.CreateFromDef(World, bd);

                EdgeShape shape = new EdgeShape(new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));
                ground.AddFixture(shape);
            }

            // Breakable dynamic body
            {
                BodyDef bd = new BodyDef();
                bd.Type = BodyType.Dynamic;
                bd.Position = new Vector2(0.0f, 40.0f);
                bd.Angle = 0.25f * MathConstants.Pi;
                _body1 = BodyFactory.CreateFromDef(World, bd);

                _shape1 = new PolygonShape(1.0f);
                _shape1.SetAsBox(0.5f, 0.5f, new Vector2(-0.5f, 0.0f), 0.0f);
                _piece1 = _body1.AddFixture(_shape1);

                _shape2 = new PolygonShape(1.0f);
                _shape2.SetAsBox(0.5f, 0.5f, new Vector2(0.5f, 0.0f), 0.0f);
                _piece2 = _body1.AddFixture(_shape2);
            }

            _break = false;
            _broke = false;
        }

        protected override void PostSolve(Contact contact, ContactVelocityConstraint contactConstraint)
        {
            if (_broke)

                // The body already broke.
                return;

            // Should the body break?
            int count = contact.Manifold.PointCount;

            float maxImpulse = 0.0f;
            for (int i = 0; i < count; ++i)

                //Velcro: We have to do things slightly different here as the PostSolve delegate returns the whole contact
                maxImpulse = MathUtils.Max(maxImpulse, contactConstraint.Points[i].NormalImpulse);

            if (maxImpulse > 40.0f)

                // Flag the body for breaking.
                _break = true;

            base.PostSolve(contact, contactConstraint);
        }

        private void Break()
        {
            // Create two bodies from one.
            Body body1 = _piece1.Body;
            Vector2 center = body1.WorldCenter;

            body1.RemoveFixture(_piece2);
            _piece2 = null;

            BodyDef bd = new BodyDef();
            bd.Type = BodyType.Dynamic;
            bd.Position = body1.Position;
            bd.Angle = body1.Rotation;

            Body body2 = BodyFactory.CreateFromDef(World, bd);
            _piece2 = body2.AddFixture(_shape2);

            // Compute consistent velocities for new bodies based on
            // cached velocity.
            Vector2 center1 = body1.WorldCenter;
            Vector2 center2 = body2.WorldCenter;

            Vector2 velocity1 = _velocity + MathUtils.Cross(_angularVelocity, center1 - center);
            Vector2 velocity2 = _velocity + MathUtils.Cross(_angularVelocity, center2 - center);

            body1.AngularVelocity = _angularVelocity;
            body1.LinearVelocity = velocity1;

            body2.AngularVelocity = _angularVelocity;
            body2.LinearVelocity = velocity2;
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            if (_break)
            {
                Break();
                _broke = true;
                _break = false;
            }

            // Cache velocities to improve movement on breakage.
            if (_broke == false)
            {
                _velocity = _body1.LinearVelocity;
                _angularVelocity = _body1.AngularVelocity;
            }

            base.Update(settings, gameTime);
        }

        internal static Test Create()
        {
            return new BreakableTest();
        }
    }
}