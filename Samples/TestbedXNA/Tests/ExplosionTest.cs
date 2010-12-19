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

using System.Diagnostics;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.TestBed.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using FarseerPhysics.Common.PhysicsLogic;

namespace FarseerPhysics.TestBed.Tests
{
    public class ExplosionTest : Test
    {
        private const int ColumnCount = 5;
        private const int RowCount = 16;
        private Body[] _bodies = new Body[RowCount * ColumnCount];
        private Explosion _explosion;
        private float _radius;
        private float _power;
        private Vector2 _mousePos;
        private int[] _indices = new int[RowCount * ColumnCount];

        private ExplosionTest()
        {
            //Ground
            FixtureFactory.CreateEdge(World, new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));

            float[] xs = new[] { -10.0f, -5.0f, 0.0f, 5.0f, 10.0f };

            for (int j = 0; j < ColumnCount; ++j)
            {
                PolygonShape shape = new PolygonShape(1);
                shape.SetAsBox(0.5f, 0.5f);

                for (int i = 0; i < RowCount; ++i)
                {
                    int n = j * RowCount + i;
                    Debug.Assert(n < RowCount * ColumnCount);
                    _indices[n] = n;

                    const float x = 0.0f;
                    Body body = BodyFactory.CreateBody(World);
                    body.BodyType = BodyType.Dynamic;
                    body.Position = new Vector2(xs[j] + x, 0.752f + 1.54f * i);
                    body.UserData = _indices[n];
                    _bodies[n] = body;

                    Fixture fixture = body.CreateFixture(shape);
                    fixture.Friction = 0.3f;

                    //First column is unaffected by the explosion
                    if (j == 0)
                    {
                        body.PhysicsLogicFilter.IgnorePhysicsLogic(PhysicsLogicType.Explosion);
                    }
                }
            }

            _radius = 5;
            _power = 3;
            _explosion = new Explosion(World);
        }

        public override void Mouse(MouseState state, MouseState oldState)
        {
            _mousePos = GameInstance.ConvertScreenToWorld(state.X, state.Y);
            base.Mouse(state, oldState);
        }

        public override void Keyboard(KeyboardManager keyboardManager)
        {
            if (keyboardManager.IsNewKeyPress(Keys.OemComma))
            {
                _explosion.Activate(_mousePos, _radius, _power);
            }
            if (keyboardManager.IsKeyDown(Keys.A))
            {
                _radius = MathHelper.Clamp(_radius - 0.1f, 0, 20);
            }
            if (keyboardManager.IsKeyDown(Keys.S))
            {
                _radius = MathHelper.Clamp(_radius + 0.1f, 0, 20);
            }
            if (keyboardManager.IsKeyDown(Keys.D))
            {
                _power = MathHelper.Clamp(_power - 0.1f, 0, 20);
            }
            if (keyboardManager.IsKeyDown(Keys.F))
            {
                _power = MathHelper.Clamp(_power + 0.1f, 0, 20);
            }
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            base.Update(settings, gameTime);

            DebugView.DrawString(50, TextLine, "Press: (,) to explode at mouse position.");
            TextLine += 15;
            DebugView.DrawString(50, TextLine, "Press: (A) to decrease the explosion radius, (S) to increase it.");
            TextLine += 15;
            DebugView.DrawString(50, TextLine, "Press: (D) to decrease the explosion power, (F) to increase it.");
            TextLine += 15;
            // Fighting against float decimals
            float powernumber = (float)((int)(_power * 10)) / 10;
            DebugView.DrawString(50, TextLine, "Power: " + powernumber);

            Color color = new Color(0.4f, 0.7f, 0.8f);
            DebugView.DrawCircle(_mousePos, _radius, color);
        }

        internal static Test Create()
        {
            return new ExplosionTest();
        }
    }
}