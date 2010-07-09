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

using FarseerPhysics.Collision;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.TestBed.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FarseerPhysics.TestBed.Tests
{
    public class DistanceTest : Test
    {
        private float _angleB;
        private PolygonShape _polygonA;
        private PolygonShape _polygonB;
        private Vector2 _positionB = Vector2.Zero;
        private Transform _transformA;
        private Transform _transformB;

        private DistanceTest()
        {
            {
                _transformA.SetIdentity();
                _transformA.Position = new Vector2(0.0f, -0.2f);
                Vertices vertices = PolygonTools.CreateRectangle(10.0f, 0.2f);
                _polygonA = new PolygonShape(vertices, 0);
            }

            {
                _positionB = new Vector2(12.017401f, 0.13678508f);
                _angleB = -0.0109265f;
                _transformB.Set(_positionB, _angleB);

                Vertices vertices = PolygonTools.CreateRectangle(2.0f, 0.1f);
                _polygonB = new PolygonShape(vertices, 0);
            }
        }

        internal static Test Create()
        {
            return new DistanceTest();
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            base.Update(settings, gameTime);

            DistanceInput input = new DistanceInput();
            input.ProxyA.Set(_polygonA, 0);
            input.ProxyB.Set(_polygonB, 0);
            input.TransformA = _transformA;
            input.TransformB = _transformB;
            input.UseRadii = true;
            SimplexCache cache;
            cache.Count = 0;
            DistanceOutput output;
            Distance.ComputeDistance(out output, out cache, ref input);

            DebugView.DrawString(50, TextLine, "distance = {0:n7}", output.Distance);
            TextLine += 15;

            DebugView.DrawString(50, TextLine, "iterations = {0:n0}", output.Iterations);
            TextLine += 15;

            {
                Color color = new Color(0.9f, 0.9f, 0.9f);
                Vector2[] v = new Vector2[Settings.MaxPolygonVertices];
                for (int i = 0; i < _polygonA.Vertices.Count; ++i)
                {
                    v[i] = MathUtils.Multiply(ref _transformA, _polygonA.Vertices[i]);
                }
                DebugView.DrawPolygon(ref v, _polygonA.Vertices.Count, color);

                for (int i = 0; i < _polygonB.Vertices.Count; ++i)
                {
                    v[i] = MathUtils.Multiply(ref _transformB, _polygonB.Vertices[i]);
                }
                DebugView.DrawPolygon(ref v, _polygonB.Vertices.Count, color);
            }

            Vector2 x1 = output.PointA;
            Vector2 x2 = output.PointB;


            DebugView.DrawPoint(x1, 0.5f, new Color(1.0f, 0.0f, 0.0f));
            DebugView.DrawPoint(x2, 0.5f, new Color(1.0f, 0.0f, 0.0f));

            DebugView.DrawSegment(x1, x2, new Color(1.0f, 1.0f, 0.0f));
        }

        public override void Keyboard(KeyboardState state, KeyboardState oldState)
        {
            if (state.IsKeyDown(Keys.A) && oldState.IsKeyUp(Keys.A))
            {
                _positionB.X -= 0.1f;
            }
            if (state.IsKeyDown(Keys.D) && oldState.IsKeyUp(Keys.D))
            {
                _positionB.X += 0.1f;
            }
            if (state.IsKeyDown(Keys.S) && oldState.IsKeyUp(Keys.S))
            {
                _positionB.Y -= 0.1f;
            }
            if (state.IsKeyDown(Keys.W) && oldState.IsKeyUp(Keys.W))
            {
                _positionB.Y += 0.1f;
            }
            if (state.IsKeyDown(Keys.Q) && oldState.IsKeyUp(Keys.Q))
            {
                _angleB += 0.1f * Settings.Pi;
            }
            if (state.IsKeyDown(Keys.E) && oldState.IsKeyUp(Keys.E))
            {
                _angleB -= 0.1f * Settings.Pi;
            }

            _transformB.Set(_positionB, _angleB);
        }
    }
}