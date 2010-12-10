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

using System;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.TestBed.Framework;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.TestBed.Tests
{
    public class EdgeShapeBenchmark : Test
    {
        private EdgeShapeBenchmark()
        {
            // Ground body
            {
                Body ground = BodyFactory.CreateBody(World);

                float x1 = -20.0f;
                float y1 = 2.0f * (float)Math.Cos(x1 / 10.0f * (float)Math.PI);
                for (int i = 0; i < 80; ++i)
                {
                    float x2 = x1 + 0.5f;
                    float y2 = 2.0f * (float)Math.Cos(x2 / 10.0f * (float)Math.PI);

                    EdgeShape shape = new EdgeShape(new Vector2(x1, y1), new Vector2(x2, y2));
                    ground.CreateFixture(shape);

                    x1 = x2;
                    y1 = y2;
                }
            }

            const float w = 1.0f;
            const float t = 2.0f;
            float b = w / (2.0f + (float)Math.Sqrt(t));
            float s = (float)Math.Sqrt(t) * b;

            Vertices vertices = new Vertices(8);
            vertices.Add(new Vector2(0.5f * s, 0.0f));
            vertices.Add(new Vector2(0.5f * w, b));
            vertices.Add(new Vector2(0.5f * w, b + s));
            vertices.Add(new Vector2(0.5f * s, w));
            vertices.Add(new Vector2(-0.5f * s, w));
            vertices.Add(new Vector2(-0.5f * w, b + s));
            vertices.Add(new Vector2(-0.5f * w, b));
            vertices.Add(new Vector2(-0.5f * s, 0.0f));

            _polyShape = new PolygonShape(20);
            _polyShape.Set(vertices);
        }

        private int _count;
        private PolygonShape _polyShape;

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            _count++;

            if (_count < 50)
            {
                const float x = 0;
                const float y = 15;

                Body body = BodyFactory.CreateBody(World);

                body.Position = new Vector2(x, y);
                body.BodyType = BodyType.Dynamic;

                Fixture fixture = body.CreateFixture(_polyShape);
                fixture.Friction = 0.3f;
            }

            base.Update(settings, gameTime);
        }

        internal static Test Create()
        {
            return new EdgeShapeBenchmark();
        }
    }
}