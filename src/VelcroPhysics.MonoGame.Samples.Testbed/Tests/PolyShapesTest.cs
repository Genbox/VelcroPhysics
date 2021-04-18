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

using System;
using Genbox.VelcroPhysics.Collision.Shapes;
using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Factories;
using Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Framework;
using Genbox.VelcroPhysics.Shared;
using Genbox.VelcroPhysics.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Tests
{
    public class PolyShapesTest : Test
    {
        private const int MaxBodies = 256;
        private readonly Body[] _bodies = new Body[MaxBodies];
        private readonly CircleShape _circle = new CircleShape(0, 0);
        private readonly PolygonShape[] _polygons = new PolygonShape[4];
        private int _bodyIndex;

        private PolyShapesTest()
        {
            // Ground body
            BodyFactory.CreateEdge(World, new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));

            {
                Vertices vertices = new Vertices(3);
                vertices.Add(new Vector2(-0.5f, 0.0f));
                vertices.Add(new Vector2(0.5f, 0.0f));
                vertices.Add(new Vector2(0.0f, 1.5f));
                _polygons[0] = new PolygonShape(vertices, 1);
            }

            {
                Vertices vertices3 = new Vertices(3);
                vertices3.Add(new Vector2(-0.1f, 0.0f));
                vertices3.Add(new Vector2(0.1f, 0.0f));
                vertices3.Add(new Vector2(0.0f, 1.5f));
                _polygons[1] = new PolygonShape(vertices3, 1);
            }

            {
                const float w = 1.0f;
                float b = w / (2.0f + (float)Math.Sqrt(2.0));
                float s = (float)Math.Sqrt(2.0) * b;

                Vertices vertices8 = new Vertices(8);
                vertices8.Add(new Vector2(0.5f * s, 0.0f));
                vertices8.Add(new Vector2(0.5f * w, b));
                vertices8.Add(new Vector2(0.5f * w, b + s));
                vertices8.Add(new Vector2(0.5f * s, w));
                vertices8.Add(new Vector2(-0.5f * s, w));
                vertices8.Add(new Vector2(-0.5f * w, b + s));
                vertices8.Add(new Vector2(-0.5f * w, b));
                vertices8.Add(new Vector2(-0.5f * s, 0.0f));

                _polygons[2] = new PolygonShape(vertices8, 1);
            }

            {
                Vertices box = PolygonUtils.CreateRectangle(0.5f, 0.5f);
                _polygons[3] = new PolygonShape(box, 1);
            }

            {
                _circle.Radius = 0.5f;
            }

            _bodyIndex = 0;
        }

        private void Create(int index)
        {
            if (_bodies[_bodyIndex] != null)
            {
                World.RemoveBody(_bodies[_bodyIndex]);
                _bodies[_bodyIndex] = null;
            }

            _bodies[_bodyIndex] = BodyFactory.CreateBody(World);
            _bodies[_bodyIndex].BodyType = BodyType.Dynamic;

            float x = Rand.RandomFloat(-2.0f, 2.0f);

            _bodies[_bodyIndex].Position = new Vector2(x, 10.0f);
            _bodies[_bodyIndex].Rotation = Rand.RandomFloat(-MathConstants.Pi, MathConstants.Pi);

            if (index == 4)
                _bodies[_bodyIndex].AngularDamping = 0.02f;

            if (index < 4)
            {
                Fixture fixture = _bodies[_bodyIndex].CreateFixture(_polygons[index]);
                fixture.Friction = 0.3f;
            }
            else
            {
                Fixture fixture = _bodies[_bodyIndex].CreateFixture(_circle);
                fixture.Friction = 0.3f;
            }

            _bodyIndex = (_bodyIndex + 1) % MaxBodies;
        }

        private void DestroyBody()
        {
            for (int i = 0; i < MaxBodies; ++i)
            {
                if (_bodies[i] != null)
                {
                    World.RemoveBody(_bodies[i]);
                    _bodies[i] = null;
                    return;
                }
            }
        }

        public override void Keyboard(KeyboardManager keyboardManager)
        {
            if (keyboardManager.IsNewKeyPress(Keys.D1))
                Create(0);
            if (keyboardManager.IsNewKeyPress(Keys.D2))
                Create(1);
            if (keyboardManager.IsNewKeyPress(Keys.D3))
                Create(2);
            if (keyboardManager.IsNewKeyPress(Keys.D4))
                Create(3);
            if (keyboardManager.IsNewKeyPress(Keys.D5))
                Create(4);
            if (keyboardManager.IsNewKeyPress(Keys.A))
            {
                for (int i = 0; i < MaxBodies; i += 2)
                {
                    if (_bodies[i] != null)
                    {
                        bool enabled = _bodies[i].Enabled;
                        _bodies[i].Enabled = !enabled;
                    }
                }
            }
            if (keyboardManager.IsNewKeyPress(Keys.D))
                DestroyBody();

            base.Keyboard(keyboardManager);
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            base.Update(settings, gameTime);

            PolyShapesCallback callback = new PolyShapesCallback();
            callback.Circle.Radius = 2.0f;
            callback.Circle.Position = new Vector2(0.0f, 1.1f);
            callback.Transform.SetIdentity();
            callback.DebugDraw = DebugView;

            callback.Circle.ComputeAABB(ref callback.Transform, 0, out AABB aabb);

            DebugView.BeginCustomDraw(ref GameInstance.Projection, ref GameInstance.View);
            World.QueryAABB(callback.ReportFixture, ref aabb);

            Color color = new Color(0.4f, 0.7f, 0.8f);
            DebugView.DrawCircle(callback.Circle.Position, callback.Circle.Radius, color);
            DebugView.EndCustomDraw();

            DrawString("Press 1-5 to drop stuff");
            DrawString("Press a to (de)activate some bodies");
            DrawString("Press d to destroy a body");
        }

        internal static Test Create()
        {
            return new PolyShapesTest();
        }
    }
}