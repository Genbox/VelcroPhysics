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
    public class LineJointTest : Test
    {
        private LineJointTest()
        {
            Body ground;
            {
                PolygonShape shape = new PolygonShape(PolygonTools.CreateEdge(new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f)), 0);

                BodyDef bd = new BodyDef();
                ground = _world.CreateBody(bd);
                ground.CreateFixture(shape);
            }

            {
                PolygonShape shape = new PolygonShape(PolygonTools.CreateBox(0.5f, 2.0f), 1);

                BodyDef bd = new BodyDef();
                bd.Type = BodyType.Dynamic;
                bd.Position = new Vector2(0.0f, 7.0f);
                Body body = _world.CreateBody(bd);
                body.CreateFixture(shape);

                LineJointDef jd = new LineJointDef();
                Vector2 axis = new Vector2(2.0f, 1.0f);
                axis.Normalize();
                jd.Initialize(ground, body, new Vector2(0.0f, 8.5f), axis);
                jd.MotorSpeed = 0.0f;
                jd.MaxMotorForce = 100.0f;
                jd.EnableMotor = true;
                jd.LowerTranslation = -4.0f;
                jd.UpperTranslation = 4.0f;
                jd.EnableLimit = true;
                _world.CreateJoint(jd);
            }
        }

        internal static Test Create()
        {
            return new LineJointTest();
        }
    }
}