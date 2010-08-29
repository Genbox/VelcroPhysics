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

using System.Collections.Generic;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.TestBed.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FarseerPhysics.TestBed.Tests
{
    public class PolygonShapesTest : Test
    {
        private int _segments = 3;

        private PolygonShapesTest()
        {
            //Ground
            FixtureFactory.CreateEdge(World, new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f), 0);

            Create(0);
        }

        public override void Keyboard(KeyboardState state, KeyboardState oldState)
        {
            if (state.IsKeyDown(Keys.A) && oldState.IsKeyUp(Keys.A))
                _segments++;

            if (state.IsKeyDown(Keys.S) && oldState.IsKeyUp(Keys.S) && _segments > 0)
                _segments--;

            if (state.IsKeyDown(Keys.D) && oldState.IsKeyUp(Keys.D))
                Create(0);
            if (state.IsKeyDown(Keys.F) && oldState.IsKeyUp(Keys.F))
                Create(1);
        }

        private void Create(int type)
        {
            Vector2 position = new Vector2(0, 30);

            switch (type)
            {
                default:
                    List<Fixture> rounded = FixtureFactory.CreateRoundedRectangle(World, 10, 10, 2.5F, 2.5F, _segments,
                                                                                  10, position);
                    rounded[0].Body.BodyType = BodyType.Dynamic;
                    break;
                case 1:
                    List<Fixture> capsule = FixtureFactory.CreateCapsule(World, 10, 2,
                                                                         (int) MathHelper.Max(_segments, 1), 3,
                                                                         (int) MathHelper.Max(_segments, 1), 10,
                                                                         position);
                    capsule[0].Body.BodyType = BodyType.Dynamic;
                    break;
            }
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            base.Update(settings, gameTime);
            DebugView.DrawString(50, TextLine,
                                 "Segments: " + _segments +
                                 "\nPress: 'A' to increase segments, 'S' decrease segments\n'D' to create rectangle. 'F' to create capsule.");
            TextLine += 15;
        }

        internal static Test Create()
        {
            return new PolygonShapesTest();
        }
    }
}