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
    public class Pyramid : Test
    {
        public const int _count = 20;

        public Pyramid()
        {
            {
                BodyDef bd = new BodyDef();
                Body ground = _world.CreateBody(bd);

                PolygonShape shape = new PolygonShape();
                shape.SetAsEdge(new Vec2(-40.0f, 0.0f), new Vec2(40.0f, 0.0f));
                ground.CreateFixture(shape, 0);
            }

            {
                float a = 0.5f;
                PolygonShape shape = new PolygonShape();
                shape.SetAsBox(a, a);

                Vec2 x = new Vec2(-7.0f, 0.75f);
                Vec2 y;
                Vec2 deltaX = new Vec2(0.5625f, 1.25f);
                Vec2 deltaY = new Vec2(1.125f, 0.0f);

                for (int i = 0; i < _count; ++i)
                {
                    y = x;

                    for (int j = i; j < _count; ++j)
                    {
                        BodyDef bd = new BodyDef();
                        bd.Position = y;
                        Body body = _world.CreateBody(bd);
                        body.CreateFixture(shape, 5.0f);

                        y += deltaY;
                    }

                    x += deltaX;
                }
            }
        }

        public static Test Create()
        {
            return new Pyramid();
        }
    }
}
