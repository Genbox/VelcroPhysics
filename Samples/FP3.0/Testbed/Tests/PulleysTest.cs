﻿/*
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

namespace FarseerPhysics.TestBed.Tests
{
    public class PulleysTest : Test
    {
        private PulleysTest()
        {
            Body ground;
            {
                BodyDef bd = new BodyDef();
                ground = _world.CreateBody(bd);

                PolygonShape shape = new PolygonShape(0.0f);
                shape.SetAsEdge(new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));
                ground.CreateFixture(shape);
            }

            {
                const float a = 2.0f;
                const float b = 4.0f;
                const float y = 16.0f;
                const float L = 12.0f;

                PolygonShape shape = new PolygonShape(5);
                shape.SetAsBox(a, b);

                BodyDef bd = new BodyDef();
                bd.Type = BodyType.Dynamic;

                bd.Position = new Vector2(-10.0f, y);
                Body body1 = _world.CreateBody(bd);
                body1.CreateFixture(shape);

                bd.Position = new Vector2(10.0f, y);
                Body body2 = _world.CreateBody(bd);
                body2.CreateFixture(shape);

                PulleyJointDef pulleyDef = new PulleyJointDef();
                Vector2 anchor1 = new Vector2(-10.0f, y + b);
                Vector2 anchor2 = new Vector2(10.0f, y + b);
                Vector2 groundAnchor1 = new Vector2(-10.0f, y + b + L);
                Vector2 groundAnchor2 = new Vector2(10.0f, y + b + L);
                pulleyDef.Initialize(body1, body2, groundAnchor1, groundAnchor2, anchor1, anchor2, 2.0f);

                _joint1 = (PulleyJoint)_world.CreateJoint(pulleyDef);
            }
        }

        public override void Step(Framework.Settings settings)
        {
            base.Step(settings);

            float ratio = _joint1.GetRatio();
            float L = _joint1.GetLength1() + ratio * _joint1.GetLength2();
            _debugView.DrawString(50, _textLine, "L1 + {0:n} * L2 = {1:n}", ratio, L);
            _textLine += 15;
        }

        internal static Test Create()
        {
            return new PulleysTest();
        }

        private PulleyJoint _joint1;
    }
}