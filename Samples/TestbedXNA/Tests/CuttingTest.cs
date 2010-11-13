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
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Common.PolygonManipulation;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.TestBed.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FarseerPhysics.TestBed.Tests
{
    public class CuttingTest : Test
    {
        private const float MoveAmount = 0.1f;

        private const int Count = 20;
        private Vector2 _end = new Vector2(6, 5);
        private Vector2 _start = new Vector2(-6, 5);
        private bool _switched;

        private CuttingTest()
        {
            //Ground
            FixtureFactory.CreateEdge(World, new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));

            Vertices box = PolygonTools.CreateRectangle(0.5f, 0.5f);
            PolygonShape shape = new PolygonShape(box, 5);

            Vector2 x = new Vector2(-7.0f, 0.75f);
            Vector2 deltaX = new Vector2(0.5625f, 1.25f);
            Vector2 deltaY = new Vector2(1.125f, 0.0f);

            for (int i = 0; i < Count; ++i)
            {
                Vector2 y = x;

                for (int j = i; j < Count; ++j)
                {
                    Body body = BodyFactory.CreateBody(World);
                    body.BodyType = BodyType.Dynamic;
                    body.Position = y;
                    body.CreateFixture(shape);

                    y += deltaY;
                }

                x += deltaX;
            }
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            DebugView.DrawString(50, TextLine, "Press A,S,W,D move endpoint");
            TextLine += 15;
            DebugView.DrawString(50, TextLine, "Press Enter to cut");
            TextLine += 15;
            DebugView.DrawString(50, TextLine, "Press TAB to change endpoint");
            TextLine += 15;

            DebugView.DrawSegment(_start, _end, Color.Red);

            List<Fixture> fixtures = new List<Fixture>();
            List<Vector2> entryPoints = new List<Vector2>();
            List<Vector2> exitPoints = new List<Vector2>();

            //Get the entry points
            World.RayCast((f, p, n, fr) =>
                              {
                                  fixtures.Add(f);
                                  entryPoints.Add(p);
                                  return 1;
                              }, _start, _end);

            //Reverse the ray to get the exitpoints
            World.RayCast((f, p, n, fr) =>
                              {
                                  exitPoints.Add(p);
                                  return 1;
                              }, _end, _start);

            DebugView.DrawString(50, TextLine, "Fixtures: " + fixtures.Count);

            foreach (Vector2 entryPoint in entryPoints)
            {
                DebugView.DrawPoint(entryPoint, 0.5f, Color.Yellow);
            }

            foreach (Vector2 exitPoint in exitPoints)
            {
                DebugView.DrawPoint(exitPoint, 0.5f, Color.PowderBlue);
            }

            base.Update(settings, gameTime);
        }

        public override void Keyboard(KeyboardManager keyboardManager)
        {
            if (keyboardManager.IsNewKeyPress(Keys.Tab))
                _switched = !_switched;

            if (keyboardManager.IsNewKeyPress(Keys.Enter))
                CuttingTools.Cut(World, _start, _end, 0.001f);

            if (_switched)
            {
                if (keyboardManager.IsKeyDown(Keys.A))
                    _start.X -= MoveAmount;

                if (keyboardManager.IsKeyDown(Keys.S))
                    _start.Y -= MoveAmount;

                if (keyboardManager.IsKeyDown(Keys.W))
                    _start.Y += MoveAmount;

                if (keyboardManager.IsKeyDown(Keys.D))
                    _start.X += MoveAmount;
            }
            else
            {
                if (keyboardManager.IsKeyDown(Keys.A))
                    _end.X -= MoveAmount;

                if (keyboardManager.IsKeyDown(Keys.S))
                    _end.Y -= MoveAmount;

                if (keyboardManager.IsKeyDown(Keys.W))
                    _end.Y += MoveAmount;

                if (keyboardManager.IsKeyDown(Keys.D))
                    _end.X += MoveAmount;
            }
        }

        public override void Gamepad(GamePadState state, GamePadState oldState)
        {
            _start.X += state.ThumbSticks.Left.X / 5;
            _start.Y += state.ThumbSticks.Left.Y / 5;

            _end.X += state.ThumbSticks.Right.X / 5;
            _end.Y += state.ThumbSticks.Right.Y / 5;

            if (state.Buttons.A == ButtonState.Pressed && oldState.Buttons.A == ButtonState.Released)
                CuttingTools.Cut(World, _start, _end, 0.001f);

            base.Gamepad(state, oldState);
        }

        public static CuttingTest Create()
        {
            return new CuttingTest();
        }
    }
}