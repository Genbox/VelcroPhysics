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
using Microsoft.Xna.Framework.Input;

namespace FarseerPhysics.TestBed.Tests
{
    public class ShapeEditingTest : Test
    {
        private ShapeEditingTest()
        {
            Body ground = World.Add();

            Vertices edge = PolygonTools.CreateEdge(new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));
            PolygonShape shape = new PolygonShape(edge, 0);
            ground.CreateFixture(shape);

            _body = World.Add();
            _body.BodyType = BodyType.Dynamic;
            _body.Position = new Vector2(0.0f, 10.0f);

            Vertices box = PolygonTools.CreateRectangle(4.0f, 4.0f);
            PolygonShape shape2 = new PolygonShape(box, 10);
            _body.CreateFixture(shape2);

            _fixture2 = null;
        }

        public override void Keyboard(KeyboardState state, KeyboardState oldState)
        {
            if (state.IsKeyDown(Keys.C) && oldState.IsKeyUp(Keys.C) && _fixture2 == null)
            {
                CircleShape shape = new CircleShape(3.0f, 10);
                shape.Position = new Vector2(0.5f, -4.0f);
                _fixture2 = _body.CreateFixture(shape);
                _body.Awake = true;
            }

            if (state.IsKeyDown(Keys.D) && oldState.IsKeyUp(Keys.D) && _fixture2 != null)
            {
                _body.DestroyFixture(_fixture2);
                _fixture2 = null;
                _body.Awake = true;
            }
        }

        public override void Update(Framework.Settings settings, GameTime gameTime)
        {
            base.Update(settings, gameTime);
            DebugView.DrawString(50, TextLine, "Press: (c) create a shape, (d) destroy a shape.");
            TextLine += 15;
        }

        internal static Test Create()
        {
            return new ShapeEditingTest();
        }

        private Body _body;
        private Fixture _fixture2;
    }
}