﻿/*
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

using System;
using FarseerPhysics;
using FarseerPhysics.TestBed.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FarseerPhysics.TestBed.Tests
{
    public class PolyCollision : Test
    {
        public PolyCollision()
        {
            {
                _polygonA.SetAsBox(1.0f, 1.0f, new Vector2(0.0f, 0.0f), FarseerPhysics.Settings.Pi * 0.25f);
                _transformA.Set(new Vector2(0.0f, 5.0f), 0.0f);
            }

            {
                _polygonB.SetAsBox(0.25f, 0.25f);
                _positionB = new Vector2(-1.7793884f, 5.0326509f);
                _angleB = 2.2886343f;
                _transformB.Set(_positionB, _angleB);
            }
        }

        internal static Test Create()
        {
            return new PolyCollision();
        }

        public override void Step(Framework.Settings settings)
        {
            Manifold manifold = new Manifold();
            Collision.CollidePolygons(ref manifold, _polygonA, ref _transformA, _polygonB, ref _transformB);

            WorldManifold worldManifold = new WorldManifold(ref manifold, ref _transformA, _polygonA.Radius,
                                                            ref _transformB, _polygonB.Radius);

            _debugView.DrawString(50, _textLine, "point count = {0:n}", manifold._pointCount);
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

            for (int i = 0; i < manifold._pointCount; ++i)
            {
                _debugView.DrawPoint(worldManifold._points[i], 0.5f, new Color(0.9f, 0.3f, 0.3f));
            }
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
                _angleB += 0.1f * FarseerPhysics.Settings.Pi;
            }
            if (state.IsKeyDown(Keys.E))
            {
                _angleB -= 0.1f * FarseerPhysics.Settings.Pi;
            }

            _transformB.Set(_positionB, _angleB);
        }

        private PolygonShape _polygonA = new PolygonShape();
        private PolygonShape _polygonB = new PolygonShape();

        private Transform _transformA = new Transform();
        private Transform _transformB = new Transform();

        private Vector2 _positionB;
        private float _angleB;
    }
}