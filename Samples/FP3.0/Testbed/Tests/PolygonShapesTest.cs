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

using System.Collections.Generic;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.TestBed.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FarseerPhysics.TestBed.Tests
{
    public class PolygonShapesTest : Test
    {
        private int segments = 3;

        private PolygonShapesTest()
        {
            //Ground
            FixtureFactory.CreateEdge(World, new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f), 0);

            Create(0);
        }

        public override void Keyboard(KeyboardState state, KeyboardState oldState)
        {
            if (state.IsKeyDown(Keys.A) && oldState.IsKeyUp(Keys.A))
                segments++;

            if (state.IsKeyDown(Keys.S) && oldState.IsKeyUp(Keys.S) && segments > 0)
                segments--;

            if (state.IsKeyDown(Keys.D) && oldState.IsKeyUp(Keys.D))
                Create(0);
            if (state.IsKeyDown(Keys.F) && oldState.IsKeyUp(Keys.F))
                Create(1);
        }

        private void Create(int type)
        {
            Body body = BodyFactory.CreateBody(World);
            body.BodyType = BodyType.Dynamic;
            body.Position = new Vector2(0, 30);
            body.Awake = true;

            switch (type)
            {
                default:
                    List<Vertices> verts = PolygonTools.CreateRoundedRectangle(10, 10, 2.5F, 2.5F, segments);
                    for (int i = 0; i < verts.Count; i++)
                        body.CreateFixture(new PolygonShape(verts[i]), 10);
                    break;
                case 1:
                    List<Vertices> verts2 = PolygonTools.CreateCapsule(10, 2, (int) MathHelper.Max(segments, 1), 3,
                                                                       (int) MathHelper.Max(segments, 1));
                    for (int i = 0; i < verts2.Count; i++)
                        body.CreateFixture(new PolygonShape(verts2[i]), 10);
                    break;
            }
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            base.Update(settings, gameTime);
            DebugView.DrawString(50, TextLine,
                                 "Segments: " + segments +
                                 "\nPress: 'A' to increase segments, 'S' decrease segments\n'D' to create rectangle. 'F' to create capsule.");
            TextLine += 15;
        }

        internal static Test Create()
        {
            return new PolygonShapesTest();
        }
    }
}