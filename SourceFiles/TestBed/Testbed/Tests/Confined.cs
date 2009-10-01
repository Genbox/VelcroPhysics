/*
* Copyright (c) 2009 Erin Catto http://www.gphysics.com
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

using System.Windows.Forms;
using Box2DX.Collision;
using Box2DX.Common;
using Box2DX.Dynamics;

namespace TestBed.Tests
{

    public class Confined : Test
    {
        private const int _columnCount = 0;
        private const int _rowCount = 0;

        public Confined()
        {
            {
                BodyDef bd = new BodyDef();
                Body ground = _world.CreateBody(bd);

                PolygonShape shape = new PolygonShape();

                // Floor
                shape.SetAsEdge(new Vec2(-10.0f, 0.0f), new Vec2(40.0f, 0.0f));
                ground.CreateFixture(shape, 0);

                // Left wall
                shape.SetAsEdge(new Vec2(-10.0f, 0.0f), new Vec2(-10.0f, 20.0f));
                ground.CreateFixture(shape, 0);

                // Right wall
                shape.SetAsEdge(new Vec2(10.0f, 0.0f), new Vec2(10.0f, 20.0f));
                ground.CreateFixture(shape, 0);

                // Roof
                shape.SetAsEdge(new Vec2(-10.0f, 20.0f), new Vec2(10.0f, 20.0f));
                ground.CreateFixture(shape, 0);
            }

            {
                float radius = 0.5f;
                CircleShape shape = new CircleShape();
                shape._p.SetZero();
                shape._radius = radius;

                FixtureDef fd = new FixtureDef();
                fd.Shape = shape;
                fd.Density = 1.0f;
                fd.Friction = 0.1f;

                for (int j = 0; j < _columnCount; ++j)
                {
                    for (int i = 0; i < _rowCount; ++i)
                    {
                        BodyDef bd = new BodyDef();
                        bd.Position.Set(-10.0f + (2.1f * j + 1.0f + 0.01f * i) * radius, (2.0f * i + 1.0f) * radius);
                        Body body = _world.CreateBody(bd);

                        body.CreateFixture(fd);
                    }
                }
            }

            _world.Gravity = new Vec2(0, 0);
        }

        private void CreateCircle()
        {
            float radius = 0.5f;
            CircleShape shape = new CircleShape();
            shape._p.SetZero();
            shape._radius = radius;

            FixtureDef fd = new FixtureDef();
            fd.Shape = shape;
            fd.Density = 1.0f;
            fd.Friction = 0.0f;

            BodyDef bd = new BodyDef();
            bd.Position.Set(Math.Random(), (2.0f + Math.Random()) * radius);
            Body body = _world.CreateBody(bd);

            body.CreateFixture(fd);
        }

        public override void Keyboard(Keys key)
        {
            switch (key)
            {
                case Keys.C:
                    CreateCircle();
                    break;
            }
        }

        public override void Step(Settings settings)
        {
            base.Step(settings);
            OpenGLDebugDraw.DrawString(5, _textLine, "Press 'c' to create a circle.");
            _textLine += 15;
        }

        public static Test Create()
        {
            return new Confined();
        }
    };
}
