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

using FarseerPhysics.TestBed.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FarseerPhysics.TestBed.Tests
{
    public class DistanceTest : Test
    {
        private DistanceTest()
        {
            {
                _transformA.SetIdentity();
                _transformA.Position = new Vector2(0.0f, -0.2f);
                _polygonA.SetAsBox(10.0f, 0.2f);
            }

            {
                _positionB = new Vector2(12.017401f, 0.13678508f);
                _angleB = -0.0109265f;
                _transformB.Set(_positionB, _angleB);

                _polygonB.SetAsBox(2.0f, 0.1f);
            }
        }

        internal static Test Create()
        {
            return new DistanceTest();
        }

        public override void Step(Framework.Settings settings)
        {
            base.Step(settings);

            DistanceInput input = new DistanceInput();
            input.ProxyA.Set(_polygonA);
            input.ProxyB.Set(_polygonB);
            input.TransformA = _transformA;
            input.TransformB = _transformB;
            input.UseRadii = true;
            SimplexCache cache;
            cache.Count = 0;
            DistanceOutput output;
            Distance.ComputeDistance(out output, out cache, ref input);

            _debugView.DrawString(50, _textLine, "distance = {0:n}", output.Distance);
            _textLine += 15;

            _debugView.DrawString(50, _textLine, "iterations = {0:n}", output.Iterations);
            _textLine += 15;

            {
                Color color = new Color(0.9f, 0.9f, 0.9f);
                FixedArray8<Vector2> v = new FixedArray8<Vector2>();
                for (int i = 0; i < _polygonA.VertexCount; ++i)
                {
                    v[i] = MathUtils.Multiply(ref _transformA, _polygonA.Vertices[i]);
                }
                _debugView.DrawPolygon(ref v, _polygonA.VertexCount, color);

                for (int i = 0; i < _polygonB.VertexCount; ++i)
                {
                    v[i] = MathUtils.Multiply(ref _transformB, _polygonB.Vertices[i]);
                }
                _debugView.DrawPolygon(ref v, _polygonB.VertexCount, color);
            }

            Vector2 x1 = output.PointA;
            Vector2 x2 = output.PointB;


            _debugView.DrawPoint(x1, 0.5f, new Color(1.0f, 0.0f, 0.0f));
            _debugView.DrawPoint(x2, 0.5f, new Color(1.0f, 0.0f, 0.0f));

            _debugView.DrawSegment(x1, x2, new Color(1.0f, 1.0f, 0.0f));
        }

        public override void Keyboard(KeyboardState state, KeyboardState oldState)
        {
            if (state.IsKeyDown(Keys.A))
            {
                _positionB.X -= 0.1f;
            }
            if (state.IsKeyDown(Keys.D))
            {
                _positionB.X += 0.1f;
            }
            if (state.IsKeyDown(Keys.S))
            {
                _positionB.Y -= 0.1f;
            }
            if (state.IsKeyDown(Keys.W))
            {
                _positionB.Y += 0.1f;
            }
            if (state.IsKeyDown(Keys.Q))
            {
                _angleB += 0.1f * Settings.Pi;
            }
            if (state.IsKeyDown(Keys.E))
            {
                _angleB -= 0.1f * Settings.Pi;
            }

            _transformB.Set(_positionB, _angleB);
        }

        private Vector2 _positionB = Vector2.Zero;
        private float _angleB;

        private Transform _transformA;
        private Transform _transformB;
        private PolygonShape _polygonA = new PolygonShape();
        private PolygonShape _polygonB = new PolygonShape();
    }
}