/*
  Box2DX Copyright (c) 2008 Ihar Kalasouski http://code.google.com/p/box2dx
  Box2D original C++ version Copyright (c) 2008 Erin Catto http://www.gphysics.com

  This software is provided 'as-is', without any express or implied
  warranty.  In no event will the authors be held liable for any damages
  arising from the use of this software.

  Permission is granted to anyone to use this software for any purpose,
  including commercial applications, and to alter it and redistribute it
  freely, subject to the following restrictions:

  1. The origin of this software must not be misrepresented; you must not
     claim that you wrote the original software. If you use this software
     in a product, an acknowledgment in the product documentation would be
     appreciated but is not required.
  2. Altered source versions must be plainly marked as such, and must not be
     misrepresented as being the original software.
  3. This notice may not be removed or altered from any source distribution.
*/

using Box2DX.Collision;
using Box2DX.Common;
using Box2DX.Dynamics;

namespace TestBed
{
    public class ShapeEditing : Test
    {
        Body _body;
        Fixture _fixture1;
        Fixture _fixture2;

        public ShapeEditing()
        {
            {
                BodyDef bd = new BodyDef();
                Body ground = _world.CreateBody(bd);

                PolygonShape shape = new PolygonShape();
                shape.SetAsEdge(new Vec2(-40.0f, 0.0f), new Vec2(40.0f, 0.0f));
                ground.CreateFixture(shape, 0);
            }

            BodyDef bodydef = new BodyDef();
            bodydef.Position.Set(0.0f, 10.0f);
            _body = _world.CreateBody(bodydef);

            {
                PolygonShape shape = new PolygonShape();
                shape.SetAsBox(4.0f, 4.0f, new Vec2(0.0f, 0.0f), 0.0f);
                _fixture1 = _body.CreateFixture(shape, 10.0f);

                _fixture2 = null;
            }
        }

        public override void Keyboard(System.Windows.Forms.Keys key)
        {
            switch (key)
            {
                case System.Windows.Forms.Keys.C:
                    if (_fixture2 == null)
                    {
                        CircleShape shape = new CircleShape();
                        shape._radius = 3.0f;
                        shape._p.Set(0.5f, -4.0f);
                        _fixture2 = _body.CreateFixture(shape, 10.0f);
                        _body.WakeUp();
                    }
                    break;

                case System.Windows.Forms.Keys.D:
                    if (_fixture2 != null)
                    {
                        _body.DestroyFixture(ref _fixture2);
                        _fixture2 = null;
                        _body.WakeUp();
                    }
                    break;
            }
        }

        public override void Step(Settings settings)
        {
            base.Step(settings);
            OpenGLDebugDraw.DrawString(5, _textLine, "Press: (c) create a shape, (d) destroy a shape.");
            _textLine += 15;
        }

        public static Test Create()
        {
            return new ShapeEditing();
        }
    }
}