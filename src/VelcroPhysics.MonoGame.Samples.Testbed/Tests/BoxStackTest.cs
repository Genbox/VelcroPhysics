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

using System.Diagnostics;
using Genbox.VelcroPhysics.Collision.Shapes;
using Genbox.VelcroPhysics.Definitions;
using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Factories;
using Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Framework;
using Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Framework.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Tests
{
    internal class BoxStackTest : Test
    {
        private const int _columnCount = 1;
        private const int _rowCount = 15;

        private readonly Body[] _bodies = new Body[_rowCount * _columnCount];
        private readonly int[] _indices = new int[_rowCount * _columnCount];
        private Body _bullet;

        private BoxStackTest()
        {
            //Ground
            BodyFactory.CreateEdge(World, new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));
            BodyFactory.CreateEdge(World, new Vector2(20.0f, 0.0f), new Vector2(20.0f, 20.0f));

            float[] xs = { 0.0f, -10.0f, -5.0f, 5.0f, 10.0f };

            for (int j = 0; j < _columnCount; ++j)
            {
                PolygonShape shape = new PolygonShape(1.0f);
                shape.SetAsBox(0.5f, 0.5f);

                FixtureDef fd = new FixtureDef();
                fd.Shape = shape;
                fd.Friction = 0.3f;

                for (int i = 0; i < _rowCount; ++i)
                {
                    BodyDef bd = new BodyDef();
                    bd.Type = BodyType.Dynamic;

                    int n = j * _rowCount + i;
                    Debug.Assert(n < _rowCount * _columnCount);
                    _indices[n] = n;
                    bd.UserData = _indices[n];

                    const float x = 0.0f;
                    bd.Position = new Vector2(xs[j] + x, 0.55f + 1.1f * i);
                    Body body = BodyFactory.CreateFromDef(World, bd);

                    _bodies[n] = body;

                    body.AddFixture(shape);
                }
            }

            _bullet = null;
        }

        public override void Keyboard(KeyboardManager keyboard)
        {
            if (keyboard.IsNewKeyPress(Keys.OemComma))
            {
                if (_bullet != null)
                {
                    World.RemoveBody(_bullet);
                    _bullet = null;
                }

                {
                    CircleShape shape = new CircleShape(0.25f, 20);

                    FixtureDef fd = new FixtureDef();
                    fd.Shape = shape;
                    fd.Restitution = 0.05f;

                    BodyDef bd = new BodyDef();
                    bd.Type = BodyType.Dynamic;
                    bd.IsBullet = true;
                    bd.Position = new Vector2(-31.0f, 5.0f);

                    _bullet = BodyFactory.CreateFromDef(World, bd);
                    _bullet.AddFixture(fd);

                    _bullet.LinearVelocity = new Vector2(400.0f, 0.0f);
                }
            }

            base.Keyboard(keyboard);
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            base.Update(settings, gameTime);

            DrawString("Press: (,) to launch a bullet.");
        }

        internal static Test Create()
        {
            return new BoxStackTest();
        }
    }
}