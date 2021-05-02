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

using Genbox.VelcroPhysics.Collision.Shapes;
using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Factories;
using Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Tests
{
    public class EdgeTest : Test
    {
        private readonly Vector2 _offset1;
        private readonly Vector2 _offset2;
        private Body _body1;
        private Body _body2;

        private EdgeTest()
        {
            Vector2[] vertices =
            {
               new Vector2(10.0f, -4.0f),
               new Vector2(10.0f, 0.0f),
               new Vector2(6.0f, 0.0f),
               new Vector2(4.0f, 2.0f),
               new Vector2(2.0f, 0.0f),
               new Vector2(-2.0f, 0.0f),
               new Vector2(-6.0f, 0.0f),
               new Vector2(-8.0f, -3.0f),
               new Vector2(-10.0f, 0.0f),
               new Vector2(-10.0f, -4.0f)
            };

            _offset1 = new Vector2(0.0f, 8.0f);
            _offset2 = new Vector2(0.0f, 16.0f);

            {
                Vector2 v1 = vertices[0] + _offset1;
                Vector2 v2 = vertices[1] + _offset1;
                Vector2 v3 = vertices[2] + _offset1;
                Vector2 v4 = vertices[3] + _offset1;
                Vector2 v5 = vertices[4] + _offset1;
                Vector2 v6 = vertices[5] + _offset1;
                Vector2 v7 = vertices[6] + _offset1;
                Vector2 v8 = vertices[7] + _offset1;
                Vector2 v9 = vertices[8] + _offset1;
                Vector2 v10 = vertices[9] + _offset1;

                Body ground = BodyFactory.CreateBody(World);

                EdgeShape shape;

                shape = new EdgeShape(v10, v1, v2, v3);
                ground.CreateFixture(shape, 0.0f);

                shape = new EdgeShape(v1, v2, v3, v4);
                ground.CreateFixture(shape, 0.0f);

                shape = new EdgeShape(v2, v3, v4, v5);
                ground.CreateFixture(shape, 0.0f);

                shape = new EdgeShape(v3, v4, v5, v6);
                ground.CreateFixture(shape, 0.0f);

                shape = new EdgeShape(v4, v5, v6, v7);
                ground.CreateFixture(shape, 0.0f);

                shape = new EdgeShape(v5, v6, v7, v8);
                ground.CreateFixture(shape, 0.0f);

                shape = new EdgeShape(v6, v7, v8, v9);
                ground.CreateFixture(shape, 0.0f);

                shape = new EdgeShape(v7, v8, v9, v10);
                ground.CreateFixture(shape, 0.0f);

                shape = new EdgeShape(v8, v9, v10, v1);
                ground.CreateFixture(shape, 0.0f);

                shape = new EdgeShape(v9, v10, v1, v2);
                ground.CreateFixture(shape, 0.0f);
            }

            {
                Vector2 v1 = vertices[0] + _offset2;
                Vector2 v2 = vertices[1] + _offset2;
                Vector2 v3 = vertices[2] + _offset2;
                Vector2 v4 = vertices[3] + _offset2;
                Vector2 v5 = vertices[4] + _offset2;
                Vector2 v6 = vertices[5] + _offset2;
                Vector2 v7 = vertices[6] + _offset2;
                Vector2 v8 = vertices[7] + _offset2;
                Vector2 v9 = vertices[8] + _offset2;
                Vector2 v10 = vertices[9] + _offset2;

                Body ground = BodyFactory.CreateBody(World);

                EdgeShape shape;

                shape = new EdgeShape(v1, v2);
                ground.CreateFixture(shape, 0.0f);

                shape = new EdgeShape(v2, v3);
                ground.CreateFixture(shape, 0.0f);

                shape = new EdgeShape(v3, v4);
                ground.CreateFixture(shape, 0.0f);

                shape = new EdgeShape(v4, v5);
                ground.CreateFixture(shape, 0.0f);

                shape = new EdgeShape(v5, v6);
                ground.CreateFixture(shape, 0.0f);

                shape = new EdgeShape(v6, v7);
                ground.CreateFixture(shape, 0.0f);

                shape = new EdgeShape(v7, v8);
                ground.CreateFixture(shape, 0.0f);

                shape = new EdgeShape(v8, v9);
                ground.CreateFixture(shape, 0.0f);

                shape = new EdgeShape(v9, v10);
                ground.CreateFixture(shape, 0.0f);

                shape = new EdgeShape(v10, v1);
                ground.CreateFixture(shape, 0.0f);
            }

            _body1 = null;
            _body2 = null;
            CreateBoxes();
        }

        private void CreateBoxes()
        {
            if (_body1 != null)
            {
                World.RemoveBody(_body1);
                _body1 = null;
            }

            if (_body2 != null)
            {
                World.RemoveBody(_body2);
                _body2 = null;
            }

            {
                _body1 = BodyFactory.CreateBody(World, new Vector2(8.0f, 2.6f) + _offset1, bodyType: BodyType.Dynamic);
                _body1.SleepingAllowed = false;

                FixtureFactory.AttachRectangle(0.5f, 1.0f, 1.0f, Vector2.Zero, _body1);
            }

            {
                _body2 = BodyFactory.CreateBody(World, new Vector2(8.0f, 2.6f) + _offset2, bodyType: BodyType.Dynamic);
                _body2.SleepingAllowed = false;

                FixtureFactory.AttachRectangle(0.5f, 1.0f, 1.0f, Vector2.Zero, _body2);
            }
        }

        void CreateCircles()
        {
            if (_body1 != null)
            {
                World.RemoveBody(_body1);
                _body1 = null;
            }

            if (_body2 != null)
            {
                World.RemoveBody(_body2);
                _body2 = null;
            }

            {
                _body1 = BodyFactory.CreateBody(World, new Vector2(-0.5f, 0.6f) + _offset1, bodyType: BodyType.Dynamic);
                _body1.SleepingAllowed = false;

                FixtureFactory.AttachCircle(0.5f, 1.0f, _body1);
            }

            {
                _body2 = BodyFactory.CreateBody(World, new Vector2(-0.5f, 0.6f) + _offset2, bodyType: BodyType.Dynamic);
                _body2.SleepingAllowed = false;

                FixtureFactory.AttachCircle(0.5f, 1.0f, _body2);
            }
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            DrawString("Press C to spawn circles");
            DrawString("Press B to spawn boxes");
            DrawString("Press A or D to apply force");

            base.Update(settings, gameTime);
        }

        public override void Keyboard(KeyboardManager keyboardManager)
        {
            if (keyboardManager.IsNewKeyPress(Keys.C))
                CreateCircles();
            else if (keyboardManager.IsNewKeyPress(Keys.B))
                CreateBoxes();

            if (keyboardManager.IsKeyDown(Keys.A))
            {
                _body1.ApplyForce(new Vector2(-10.0f, 0.0f));
                _body2.ApplyForce(new Vector2(-10.0f, 0.0f));
            }

            if (keyboardManager.IsKeyDown(Keys.D))
            {
                _body1.ApplyForce(new Vector2(10.0f, 0.0f));
                _body2.ApplyForce(new Vector2(10.0f, 0.0f));
            }

            base.Keyboard(keyboardManager);
        }

        internal static Test Create()
        {
            return new EdgeTest();
        }
    }
}