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

using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.Testbed.Framework;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Testbed.Tests
{
    public class EdgeTest : Test
    {
        private Fixture _circleFixture;

        private EdgeTest()
        {
            {
                Body ground = BodyFactory.CreateBody(World);

                Vector2 v1 = new Vector2(-10.0f, 0.0f);
                Vector2 v2 = new Vector2(-7.0f, -2.0f);
                Vector2 v3 = new Vector2(-4.0f, 0.0f);
                Vector2 v4 = Vector2.Zero;
                Vector2 v5 = new Vector2(4.0f, 0.0f);
                Vector2 v6 = new Vector2(7.0f, 2.0f);
                Vector2 v7 = new Vector2(10.0f, 0.0f);

                EdgeShape shape = new EdgeShape(v1, v2);
                shape.HasVertex3 = true;
                shape.Vertex3 = v3;
                ground.CreateFixture(shape);

                shape.Set(v2, v3);
                shape.HasVertex0 = true;
                shape.HasVertex3 = true;
                shape.Vertex0 = v1;
                shape.Vertex3 = v4;
                ground.CreateFixture(shape);

                shape.Set(v3, v4);
                shape.HasVertex0 = true;
                shape.HasVertex3 = true;
                shape.Vertex0 = v2;
                shape.Vertex3 = v5;
                ground.CreateFixture(shape);

                shape.Set(v4, v5);
                shape.HasVertex0 = true;
                shape.HasVertex3 = true;
                shape.Vertex0 = v3;
                shape.Vertex3 = v6;
                ground.CreateFixture(shape);

                shape.Set(v5, v6);
                shape.HasVertex0 = true;
                shape.HasVertex3 = true;
                shape.Vertex0 = v4;
                shape.Vertex3 = v7;
                ground.CreateFixture(shape);

                shape.Set(v6, v7);
                shape.HasVertex0 = true;
                shape.Vertex0 = v5;
                ground.CreateFixture(shape);
            }

            {
                Body body = BodyFactory.CreateBody(World, new Vector2(-0.5f, 0.6f));
                body.BodyType = BodyType.Dynamic;
                body.SleepingAllowed = false;

                CircleShape shape = new CircleShape(0.5f, 1);
                _circleFixture = body.CreateFixture(shape);
            }

            {
                Body body = BodyFactory.CreateBody(World, new Vector2(1.0f, 0.6f));
                body.BodyType = BodyType.Dynamic;
                body.SleepingAllowed = false;

                PolygonShape shape = new PolygonShape(1);
                shape.Vertices = PolygonTools.CreateRectangle(0.5f, 0.5f);

                body.CreateFixture(shape);
            }
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            DrawString("Rotation: " + _circleFixture.Body.Rotation);
            
            DrawString("Revolutions: " + _circleFixture.Body.Revolutions);

            base.Update(settings, gameTime);
        }

        internal static Test Create()
        {
            return new EdgeTest();
        }
    }
}