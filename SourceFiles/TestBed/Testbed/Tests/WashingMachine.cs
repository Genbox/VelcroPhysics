/*
 * JBox2D - A Java Port of Erin Catto's Box2D
 * 
 * JBox2D homepage: http://jbox2d.sourceforge.net/ 
 * Box2D homepage: http://www.gphysics.com
 * 
 * This software is provided 'as-is', without any express or implied
 * warranty.  In no event will the authors be held liable for any damages
 * arising from the use of this software.
 * 
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 * 
 * 1. The origin of this software must not be misrepresented; you must not
 * claim that you wrote the original software. If you use this software
 * in a product, an acknowledgment in the product documentation would be
 * appreciated but is not required.
 * 2. Altered source versions must be plainly marked as such, and must not be
 * misrepresented as being the original software.
 * 3. This notice may not be removed or altered from any source distribution.
 */

using System;
using System.Collections.Generic;
using System.Text;

using Box2DX.Common;
using Box2DX.Collision;
using Box2DX.Dynamics;

namespace TestBed
{

    public class WashingMachine : Test
    {

        RevoluteJoint m_joint1;

        public WashingMachine()
        {
            Body ground = null;
            { // Floor
                BodyDef bd = new BodyDef();
                bd.Position = new Vec2(0.0f, -10.0f);
                ground = _world.CreateBody(bd);

                PolygonDef sd = new PolygonDef();
                sd.SetAsBox(50.0f, 10.0f);
                //ground.CreateFixture(sd);
            }

            {
                CircleDef cd = new CircleDef();

                BodyDef bd = new BodyDef();
                bd.Position = new Vec2(0.0f, 5.0f);
                Body body = _world.CreateBody(bd);

                int numPieces = 30;
                float radius = 13f;
                for (int i = 0; i < numPieces; i++)
                {
                    cd = new CircleDef();
                    cd.Radius = 1.2f;
                    cd.Density = 5.0f;
                    cd.Friction = 0.5f;
                    cd.Restitution = 0.5f;
                    float xPos = radius * (float)System.Math.Cos(2f * System.Math.PI * (i / (float)(numPieces)));
                    float yPos = radius * (float)System.Math.Sin(2f * System.Math.PI * (i / (float)(numPieces)));
                    //System.out.println(xPos+ " "+yPos);
                    cd.LocalPosition = new Vec2(xPos, yPos);
                    body.CreateFixture(cd);
                    body.SetMassFromShapes();
                }

                

                RevoluteJointDef rjd = new RevoluteJointDef();
                //rjd.AnchorPoint = body.m_position.clone();
                //rjd.Body1 = ground;
                //rjd.Body2 = body;
                rjd.MotorSpeed = (float)System.Math.PI / 4.0f;
                rjd.MaxMotorTorque = 50000.0f;
                rjd.EnableMotor = true;
                rjd.Initialize(ground, body, body.GetPosition());
                _world.CreateJoint(rjd);


                int loadSize = 15;
                for (int i = 0; i < loadSize; i++)
                {
                    float ang = Box2DX.Common.Math.Random(0f, 2 * (float)System.Math.PI);
                    float rad = Box2DX.Common.Math.Random(0f, .8f * radius);
                    float xPos = rad * (float)System.Math.Cos(ang);
                    float yPos = bd.Position.Y + rad * (float)System.Math.Sin(ang);
                    //Ragdoll.makeRagdoll(.3f, new Vec2(xPos, yPos), _world);
                }

                loadSize = 30;
                for (int i = 0; i < loadSize; i++)
                {
                    PolygonDef box = new PolygonDef();
                    BodyDef bod = new BodyDef();
                    box.SetAsBox(Box2DX.Common.Math.Random(.4f, .9f), Box2DX.Common.Math.Random(.4f, .9f));
                    box.Density = 5.0f;
                    box.Friction = 0.5f;
                    box.Restitution = 0.5f;
                    float ang = Box2DX.Common.Math.Random(0f, 2 * (float)System.Math.PI);
                    float rad = Box2DX.Common.Math.Random(0f, .8f * radius);
                    float xPos = rad * (float)System.Math.Cos(ang);
                    float yPos = bd.Position.Y + rad * (float)System.Math.Sin(ang);
                    
                    bod.Position = new Vec2(xPos, yPos);
                    Body body1 = _world.CreateBody(bod);
                    body1.CreateFixture(box);
                    body1.SetMassFromShapes();
                }

                for (int i = 0; i < loadSize; i++)
                {
                    CircleDef circ = new CircleDef();
                    BodyDef bod = new BodyDef();
                    circ.Radius = Box2DX.Common.Math.Random(.4f, .9f);
                    circ.Density = 5.0f;
                    circ.Friction = 0.5f;
                    circ.Restitution = 0.5f;
                    float ang = Box2DX.Common.Math.Random(0f, 2 * (float)System.Math.PI);
                    float rad = Box2DX.Common.Math.Random(0f, .8f * radius);
                    float xPos = rad * (float)System.Math.Cos(ang);
                    float yPos = bd.Position.Y + rad * (float)System.Math.Sin(ang);
                    
                    bod.Position = new Vec2(xPos, yPos);
                    Body body1 = _world.CreateBody(bod);
                    body1.CreateFixture(circ);
                    body1.SetMassFromShapes();
                }
            }
        }

        void Step(Settings settings)
        {

        }

        public static Test Create()
        {
            return new WashingMachine();
        }
    }
}
