/*
* Farseer Physics Engine:
* Copyright (c) 2012 Ian Qvist
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

using System.Diagnostics;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.Testbed.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FarseerPhysics.Testbed.Tests
{
    public class VerticalStackTest : Test
    {
        private const int ColumnCount = 5;
        private const int RowCount = 16;
        private Body[] _bodies = new Body[RowCount * ColumnCount];
        private Body _bullet;
        private int[] _indices = new int[RowCount * ColumnCount];

        private VerticalStackTest()
        {
            //Ground
            BodyFactory.CreateEdge(World, new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));
            BodyFactory.CreateEdge(World, new Vector2(20.0f, 0.0f), new Vector2(20.0f, 20.0f));

            float[] xs = new[] { 0.0f, -10.0f, -5.0f, 5.0f, 10.0f };

            for (int j = 0; j < ColumnCount; ++j)
            {
                PolygonShape shape = new PolygonShape(1);
                shape.Vertices = PolygonTools.CreateRectangle(0.5f, 0.5f);

                for (int i = 0; i < RowCount; ++i)
                {
                    int n = j * RowCount + i;
                    Debug.Assert(n < RowCount * ColumnCount);
                    _indices[n] = n;

                    const float x = 0.0f;
                    //float x = Rand.RandomFloat-0.02f, 0.02f);
                    //float x = i % 2 == 0 ? -0.025f : 0.025f;
                    Body body = BodyFactory.CreateBody(World);
                    body.BodyType = BodyType.Dynamic;
                    body.Position = new Vector2(xs[j] + x, 0.752f + 1.54f * i);
                    body.UserData = _indices[n];

                    _bodies[n] = body;

                    Fixture fixture = body.CreateFixture(shape);
                    fixture.Friction = 0.3f;
                }
            }

            _bullet = null;
        }

        public override void Keyboard(KeyboardManager keyboardManager)
        {
            if (keyboardManager.IsNewKeyPress(Keys.OemComma))
            {
                if (_bullet != null)
                {
                    World.RemoveBody(_bullet);
                    _bullet = null;
                }

                {
                    CircleShape shape = new CircleShape(0.25f, 20);

                    _bullet = BodyFactory.CreateBody(World);
                    _bullet.BodyType = BodyType.Dynamic;
                    _bullet.IsBullet = true;
                    _bullet.Position = new Vector2(-31.0f, 5.0f);

                    Fixture fixture = _bullet.CreateFixture(shape);
                    fixture.Restitution = 0.05f;

                    _bullet.LinearVelocity = new Vector2(400.0f, 0.0f);
                }
            }

            base.Keyboard(keyboardManager);
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            base.Update(settings, gameTime);

            DrawString("Press: (,) to launch a bullet.");

            //if (StepCount == 300)
            //{
            //    if (_bullet != null)
            //    {
            //        World.Remove(_bullet);
            //        _bullet = null;
            //    }

            //    {
            //        CircleShape shape = new CircleShape(0.25f, 20);

            //        _bullet = BodyFactory.CreateBody(World);
            //        _bullet.BodyType = BodyType.Dynamic;
            //        _bullet.Bullet = true;
            //        _bullet.Position = new Vector2(-31.0f, 5.0f);
            //        _bullet.LinearVelocity = new Vector2(400.0f, 0.0f);

            //        Fixture fixture = _bullet.CreateFixture(shape);
            //        fixture.Restitution = 0.05f;
            //    }
            //}

            //
        }

        internal static Test Create()
        {
            return new VerticalStackTest();
        }
    }
}