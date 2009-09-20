/*
  Box2DX Copyright (c) 2008 Ihar Kalasouski http://code.google.com/p/box2dx
  Box2D original C++ version Copyright (c) 2006-2007 Erin Catto http://www.gphysics.com

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
    public class VerticalStack : Test
    {
        Body _bullet;
        const int _columCount = 5;
        const int _rowCount = 16;

        public VerticalStack()
        {
            {
                BodyDef bd = new BodyDef();
                Body ground = _world.CreateBody(bd);

                PolygonShape shape = new PolygonShape();
                shape.SetAsEdge(new Vec2(-40.0f, 0.0f), new Vec2(40.0f, 0.0f));
                ground.CreateFixture(shape, 0);

                shape.SetAsEdge(new Vec2(20.0f, 0.0f), new Vec2(20.0f, 20.0f));
                ground.CreateFixture(shape, 0);
            }

            float[] xs = { 0.0f, -10.0f, -5.0f, 5.0f, 10.0f };

            for (int j = 0; j < _columCount; ++j)
            {
                PolygonShape shape = new PolygonShape();
                shape.SetAsBox(0.5f, 0.5f);

                FixtureDef fd = new FixtureDef();
                fd.Shape = shape;
                fd.Density = 1.0f;
                fd.Friction = 0.3f;

                for (int i = 0; i < _rowCount; ++i)
                {
                    BodyDef bd = new BodyDef();

                    float x = 0.0f;
                    //float32 x = RandomFloat(-0.02f, 0.02f);
                    //float32 x = i % 2 == 0 ? -0.025f : 0.025f;
                    bd.Position.Set(xs[j] + x, 0.752f + 1.54f * i);
                    Body body = _world.CreateBody(bd);

                    body.CreateFixture(fd);
                }
            }

            _bullet = null;
        }

        public override void Keyboard(System.Windows.Forms.Keys key)
        {
            switch (key)
            {
                case System.Windows.Forms.Keys.S:
                    if (_bullet != null)
                    {
                        _world.DestroyBody(_bullet);
                        _bullet = null;
                    }
                    {
                        CircleShape shape = new CircleShape();
                        shape._radius = 0.25f;

                        FixtureDef fd = new FixtureDef();
                        fd.Shape = shape;
                        fd.Density = 20.0f;
                        fd.Restitution = 0.05f;

                        BodyDef bd = new BodyDef();
                        bd.IsBullet = true;
                        bd.Position.Set(-31.0f, 5.0f);

                        _bullet = _world.CreateBody(bd);
                        _bullet.CreateFixture(fd);

                        _bullet.SetLinearVelocity(new Vec2(400.0f, 0.0f));
                    }
                    break;
            }
        }

        public override void Step(Settings settings)
        {
            base.Step(settings);
            OpenGLDebugDraw.DrawString(5, _textLine, "Press: (,) to launch a bullet.");
            _textLine += 15;
        }

        public static Test Create()
        {
            return new VerticalStack();
        }
    }
}
