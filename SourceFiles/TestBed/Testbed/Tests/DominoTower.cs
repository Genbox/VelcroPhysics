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

    public class DominoTower : Test
    {

        float dwidth = 0.3f;
        float dheight = 1.5f;
        float ddensity;// = 10f;
        float dfriction = 0.5f;
        int baseCount = 12;

        public DominoTower()
        {
            Body ground = null;
            { // Floor
                BodyDef bd = new BodyDef();
                bd.Position = new Vec2(0.0f, -10.0f);
                ground = _world.CreateBody(bd);
                
                PolygonDef sd = new PolygonDef();
                sd.SetAsBox(50.0f, 10.0f);
                ground.CreateFixture(sd);
            }

            {
                ddensity = 10f;
                //Make bullet
                PolygonDef sd = new PolygonDef();
                sd.SetAsBox(.7f, .7f);
                sd.Density = 35f;
                sd.Friction = 0f;
                sd.Restitution = 0.85f;

                BodyDef bd = new BodyDef();
                bd.Position = new Vec2(30f, 50f);

                Body body = _world.CreateBody(bd);
                body.SetLinearVelocity(new Vec2(-25f, -25f));
                body.SetAngularVelocity(6.7f);
                body.CreateFixture(sd);
                body.SetMassFromShapes();

                sd.Density = 25f;
                bd.Position = new Vec2(-30, 25f);

                body = _world.CreateBody(bd);
                body.SetLinearVelocity(new Vec2(35f, -10f));
                body.SetAngularVelocity(-8.3f);
                body.CreateFixture(sd);
                body.SetMassFromShapes();
            }

            {

                //Make base
                for (int i = 0; i < baseCount; ++i)
                {
                    float currX = i * 1.5f * dheight - (1.5f * dheight * baseCount / 2f);
                    makeDomino(currX, dheight / 2.0f, false, _world);
                    makeDomino(currX, dheight + dwidth / 2.0f, true, _world);
                }
                //Make 'I's
                for (int j = 1; j < baseCount; ++j)
                {
                    if (j > 3) ddensity *= .8f;
                    float currY = dheight * .5f + (dheight + 2f * dwidth) * .99f * j; //y at center of 'I' structure

                    for (int i = 0; i < baseCount - j; ++i)
                    {
                        float currX = i * 1.5f * dheight - (1.5f * dheight * (baseCount - j) / 2f);// + random(-.05f, .05f);
                        ddensity *= 2.5f;
                        if (i == 0)
                        {
                            makeDomino(currX - (1.25f * dheight) + .5f * dwidth, currY - dwidth, false, _world);
                        }
                        if (i == baseCount - j - 1)
                        {
                            makeDomino(currX + (1.25f * dheight) - .5f * dwidth, currY - dwidth, false, _world);
                        }
                        ddensity /= 2.5f;
                        makeDomino(currX, currY, false, _world);
                        makeDomino(currX, currY + .5f * (dwidth + dheight), true, _world);
                        makeDomino(currX, currY - .5f * (dwidth + dheight), true, _world);
                    }
                }
            }
        }

        public void makeDomino(float x, float y, bool horizontal, World world)
        {

            PolygonDef sd = new PolygonDef();
            sd.SetAsBox(.5f * dwidth, .5f * dheight);
            sd.Density = ddensity;
            BodyDef bd = new BodyDef();
            sd.Friction = dfriction;
            sd.Restitution = 0.0f;
            bd.Position = new Vec2(x, y);
            bd.Angle = horizontal ? (float)(System.Math.PI / 2.0) : 0f;

            Body body = world.CreateBody(bd);
            body.CreateFixture(sd);
            body.SetMassFromShapes();
        }

        void Step(Settings settings)
        {

        }

        public static Test Create()
        {
            return new DominoTower();
        }
    }
}
