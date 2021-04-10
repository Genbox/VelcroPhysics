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

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using VelcroPhysics.Collision.Distance;
using VelcroPhysics.Collision.Narrowphase;
using VelcroPhysics.Collision.Shapes;
using VelcroPhysics.Samples.Testbed.Framework;
using VelcroPhysics.Shared;
using VelcroPhysics.Utilities;

namespace VelcroPhysics.Samples.Testbed.Tests
{
    public class DistanceTest : Test
    {
        private readonly PolygonShape _polygonA;
        private readonly PolygonShape _polygonB;
        private float _angleB;
        private Vector2 _positionB;
        private Transform _transformA;
        private Transform _transformB;

        private DistanceTest()
        {
            {
                _transformA.SetIdentity();
                _transformA.p = new Vector2(0.0f, -0.2f);
                Vertices vertices = PolygonUtils.CreateRectangle(10.0f, 0.2f);
                _polygonA = new PolygonShape(vertices, 0);
            }

            {
                _positionB = new Vector2(12.017401f, 0.13678508f);
                _angleB = -0.0109265f;
                _transformB.Set(_positionB, _angleB);

                Vertices vertices = PolygonUtils.CreateRectangle(2.0f, 0.1f);
                _polygonB = new PolygonShape(vertices, 0);
            }
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            base.Update(settings, gameTime);

            DistanceInput input = new DistanceInput();
            input.ProxyA = new DistanceProxy(_polygonA, 0);
            input.ProxyB = new DistanceProxy(_polygonB, 0);
            input.TransformA = _transformA;
            input.TransformB = _transformB;
            input.UseRadii = true;
            SimplexCache cache;
            cache.Count = 0;
            DistanceOutput output;
            DistanceGJK.ComputeDistance(ref input, out output, out cache);

            DrawString("Distance = " + output.Distance);
            DrawString("Iterations = " + output.Iterations);

            DebugView.BeginCustomDraw(ref GameInstance.Projection, ref GameInstance.View);
            {
                Color color = new Color(0.9f, 0.9f, 0.9f);
                Vector2[] v = new Vector2[Settings.MaxPolygonVertices];
                for (int i = 0; i < _polygonA.Vertices.Count; ++i)
                {
                    v[i] = MathUtils.Mul(ref _transformA, _polygonA.Vertices[i]);
                }
                DebugView.DrawPolygon(v, _polygonA.Vertices.Count, color);

                for (int i = 0; i < _polygonB.Vertices.Count; ++i)
                {
                    v[i] = MathUtils.Mul(ref _transformB, _polygonB.Vertices[i]);
                }
                DebugView.DrawPolygon(v, _polygonB.Vertices.Count, color);
            }

            Vector2 x1 = output.PointA;
            Vector2 x2 = output.PointB;

            DebugView.DrawPoint(x1, 0.5f, new Color(1.0f, 0.0f, 0.0f));
            DebugView.DrawPoint(x2, 0.5f, new Color(1.0f, 0.0f, 0.0f));

            DebugView.DrawSegment(x1, x2, new Color(1.0f, 1.0f, 0.0f));
            DebugView.EndCustomDraw();
        }

        public override void Keyboard(KeyboardManager keyboardManager)
        {
            if (keyboardManager.IsNewKeyPress(Keys.A))
                _positionB.X -= 0.1f;
            if (keyboardManager.IsNewKeyPress(Keys.D))
                _positionB.X += 0.1f;
            if (keyboardManager.IsNewKeyPress(Keys.S))
                _positionB.Y -= 0.1f;
            if (keyboardManager.IsNewKeyPress(Keys.W))
                _positionB.Y += 0.1f;
            if (keyboardManager.IsNewKeyPress(Keys.Q))
                _angleB += 0.1f * Settings.Pi;
            if (keyboardManager.IsNewKeyPress(Keys.E))
                _angleB -= 0.1f * Settings.Pi;

            _transformB.Set(_positionB, _angleB);
        }

        internal static Test Create()
        {
            return new DistanceTest();
        }
    }
}