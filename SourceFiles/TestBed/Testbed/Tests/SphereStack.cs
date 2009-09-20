/*
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

using Box2DX.Collision;
using Box2DX.Common;
using Box2DX.Dynamics;

namespace TestBed
{
    public class SphereStack : Test
    {
        const int _count = 10;
        Body[] _bodies = new Body[_count];

        public SphereStack()
        {
            {
                BodyDef bd = new BodyDef();
                Body ground = _world.CreateBody(bd);

                PolygonShape shape = new PolygonShape();
                shape.SetAsEdge(new Vec2(-40.0f, 0.0f), new Vec2(40.0f, 0.0f));
                ground.CreateFixture(shape, 0);
            }

            {
                CircleShape shape = new CircleShape();
                shape._radius = 1.0f;

                for (int i = 0; i < _count; ++i)
                {
                    BodyDef bd = new BodyDef();
                    bd.Position.Set(0.0f, 4.0f + 3.0f * i);

                    _bodies[i] = _world.CreateBody(bd);
                    
                    _bodies[i].CreateFixture(shape, 1.0f);

                    //m_bodies[i]->SetLinearVelocity(b2Vec2(0.0f, -100.0f));
                }
            }
        }

        override public void Step(Settings settings)
        {
            base.Step(settings);

            //for (int32 i = 0; i < e_count; ++i)
            //{
            //	printf("%g ", m_bodies[i]->GetWorldCenter().y);
            //}

            //for (int32 i = 0; i < e_count; ++i)
            //{
            //	printf("%g ", m_bodies[i]->GetLinearVelocity().y);
            //}

            //printf("\n");
        }

        public static Test Create()
        {
            return new SphereStack();
        }
    }
}