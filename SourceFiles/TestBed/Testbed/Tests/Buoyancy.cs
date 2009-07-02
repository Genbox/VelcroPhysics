/*
  Box2DX Copyright (c) 2008 Ihar Kalasouski http://code.google.com/p/box2dx
  Box2D original C++ version Copyright (c) 2006-2008 Erin Catto http://www.gphysics.com

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

using System;
using System.Collections.Generic;
using System.Text;

using Box2DX.Common;
using Box2DX.Collision;
using Box2DX.Dynamics;
using Box2DX.Dynamics.Controllers;

namespace TestBed
{
	public class Buoyancy : Test
	{
        private BuoyancyController _bc;

		public Buoyancy()
		{
            BuoyancyControllerDef bcd = new BuoyancyControllerDef();

            bcd.Offset = 15;
            bcd.Normal.Set(0, 1);
            bcd.Density = 2;
            bcd.LinearDrag = 2;
            bcd.AngularDrag = 1;
            
            _bc = new BuoyancyController(bcd);
			_world.AddController(_bc);

            Body ground = null;
		    {
			    PolygonDef sd = new PolygonDef();
			    sd.SetAsBox(50.0f, 10.0f);

			    BodyDef bd = new BodyDef();
			    bd.Position.Set(0.0f, -10.0f);
			    ground = _world.CreateBody(bd);
			    ground.CreateFixture(sd);
		    }

		    {
			    PolygonDef sd = new PolygonDef();
			    sd.SetAsBox(0.5f, 0.125f);
			    sd.Density = 2.0f;
			    sd.Friction = 0.2f;


			    RevoluteJointDef jd = new RevoluteJointDef();
			    const int numPlanks = 30;

			    Body prevBody = ground;
			    for (int i = 0; i < numPlanks; ++i)
			    {
				    BodyDef bd = new BodyDef();
				    bd.Position.Set(-14.5f + 1.0f * i, 5.0f);
				    Body body = _world.CreateBody(bd);
				    body.CreateFixture(sd);
				    body.SetMassFromShapes();

				    Vec2 anchor = new Vec2(-15.0f + 1.0f * i, 5.0f);
				    jd.Initialize(prevBody, body, anchor);
				    _world.CreateJoint(jd);

				    prevBody = body;

				    _bc.AddBody(body);
			    }

			    Vec2 anchor2 = new Vec2(-15.0f + 1.0f * numPlanks, 5.0f);
			    jd.Initialize(prevBody, ground, anchor2);
			    _world.CreateJoint(jd);
		    }

			for (int i = 0; i < 2; ++i)
			{
				PolygonDef sd = new PolygonDef();
				sd.VertexCount = 3;
				sd.Vertices[0].Set(-0.5f, 0.0f);
				sd.Vertices[1].Set(0.5f, 0.0f);
				sd.Vertices[2].Set(0.0f, 1.5f);
				sd.Density = 1.0f;

				BodyDef bd = new BodyDef();
				bd.Position.Set(-8.0f + 8.0f * i, 12.0f);
				Body body = _world.CreateBody(bd);
                body.CreateFixture(sd);
				body.SetMassFromShapes();

				_bc.AddBody(body);
			}

			for (int i = 0; i < 3; ++i)
			{
				CircleDef sd = new CircleDef();
				sd.Radius = 0.5f;
				sd.Density = 1.0f;

				BodyDef bd = new BodyDef();
				bd.Position.Set(-6.0f + 6.0f * i, 10.0f);
				Body body = _world.CreateBody(bd);
                body.CreateFixture(sd);
				body.SetMassFromShapes();

				_bc.AddBody(body);
			}

            //Static shapes work too:
            {
                float[] loop1 = 
                {
                    0.063134534f,8.3695248f,
                    0.94701801f,9.3165428f,
                    0.0f,9.0640047f,
                    -0.12626907f,10.326695f,
                    1.4520943f,11.77879f,
                    2.2728432f,10.137292f,
                    2.3991123f,11.147444f,
                    3.5986685f,10.958041f,
                    3.9143411f,7.3593722f,
                    4.1668793f,9.4428119f,
                    5.4295699f,9.3165428f,
                    6.2503189f,8.3063903f,
                    6.6922606f,10.137292f,
                    4.9876282f,9.8216191f,
                    4.7350901f,10.958041f,
                    7.2604714f,11.652521f,
                    10.732871f,11.147444f,
                    10.480333f,10.642368f,
                    10.732871f,9.8216191f,
                    11.55362f,9.4428119f,
                    12.374369f,9.3796773f,
                    13.005714f,9.8216191f,
                    13.195118f,10.38983f,
                    13.005714f,10.768637f,
                    12.626907f,10.894906f,
                    12.753176f,11.526252f,
                    13.573925f,11.715655f,
                    14.836616f,11.399982f,
                    16.351844f,10.768637f,
                    17.867073f,11.399982f,
                    17.803939f,10.263561f,
                    17.361997f,8.3063903f,
                    17.803939f,8.1801212f,
                    18.056477f,9.5059464f,
                    18.182746f,11.336848f,
                    18.561553f,11.210579f,
                    18.561553f,9.6322155f,
                    18.561553f,7.7381795f,
                    18.687822f,5.5284708f,
                    19.382302f,5.6547398f,
                    19.066629f,8.1801212f,
                    19.003495f,10.263561f,
                    19.066629f,11.463117f,
                    19.887378f,11.841924f,
                    20.708127f,11.273713f,
                    21.0238f,10.011023f,
                    20.708127f,7.2962377f,
                    21.086934f,6.2860852f,
                    21.150069f,3.7607038f,
                    20.392455f,2.5611476f,
                    18.624688f,2.5611476f,
                    20.771262f,2.1192059f,
                    20.771262f,0.22516988f,
                    18.624688f,-0.2799064f,
                    13.826463f,0.16203534f,
                    14.015867f,1.7403987f,
                    13.195118f,2.1823404f,
                    12.626907f,1.5509951f,
                    12.879445f,0.85651522f,
                    12.626907f,0.35143895f,
                    10.543467f,1.298457f,
                    11.490485f,3.9501074f,
                    13.889598f,3.6344347f,
                    13.889598f,2.9399549f,
                    14.584077f,3.8869729f,
                    11.932427f,5.2127981f,
                    9.7227183f,4.0132419f,
                    10.796005f,3.5081657f,
                    9.7858528f,3.2556275f,
                    10.796005f,2.4980131f,
                    7.9549513f,1.7403987f,
                    9.6595837f,1.424726f,
                    9.217642f,0.66711162f,
                    8.270624f,-0.090502792f,
                    7.0079333f,0.85651522f,
                    6.1240498f,-0.15363733f,
                    6.1240498f,3.192493f,
                    5.6821081f,2.4348786f,
                    4.9876282f,2.1192059f,
                    4.1037447f,1.8666678f,
                    3.0304576f,1.8666678f,
                    2.0834396f,2.245475f,
                    1.6414979f,2.6242822f,
                    1.3258252f,3.5081657f,
                    1.2626907f,0.47770802f,
                    0.63134534f,0.035766276f,
                    0.063134534f,0.98278429f
                };
			
                float[] loop2 = 
                {
                    8.270624f,6.1598161f,
                    8.270624f,5.3390672f,
                    8.7757003f,5.086529f,
                    9.4701801f,5.5284708f,
                    9.217642f,6.033547f,
                    8.7757003f,6.4123542f
                };
			
                Vec2[] b2Loop1 = new Vec2[87];
                Vec2[] b2Loop2 = new Vec2[6];
			
                for (int i = 86; i >= 0; i--) {
                    b2Loop1[86 - i].Set(loop1[i*2] + 10.0f, loop1[i*2 + 1] + 1.0f);
                }
                //for (int32 i = 0; i < 87; i++) {
                //	b2Loop1[i].Set(loop1[i*2] + 10.0f, loop1[i*2 + 1] + 1.0f);
                //}
			
                for (int i = 0; i < 6; i++) {
                    b2Loop2[i].Set(loop2[i*2], loop2[i*2 + 1]);
                }
			
                BodyDef bd = new BodyDef();
                bd.Position.Set( 0.0f, 0.0f );
                Body body = _world.CreateBody(bd);
			
                CircleDef weight = new CircleDef();
                weight.Filter.MaskBits = 0x0000;
                weight.Density = 100.0f;
                weight.Radius = 0.5f;
                weight.LocalPosition.Set(8.9f, 5.75f);
                body.CreateFixture(weight);
			
                EdgeChainDef edgeDef = new EdgeChainDef();
                edgeDef.vertexCount = 6;
                edgeDef.vertices = b2Loop2;
                edgeDef.CreateEdgeChain(body, edgeDef);
			
                body.SetMassFromShapes();

			

                _bc.AddBody(body);
			
			
			
                body = _world.CreateBody(bd);
                weight.Radius = 1.0f;
                weight.LocalPosition.Set(20.5f, 7.0f);
                body.CreateFixture(weight);
			
                edgeDef.vertexCount = 87;
                edgeDef.vertices = b2Loop1;
                edgeDef.CreateEdgeChain(body, edgeDef);
			
                body.SetMassFromShapes();

                _bc.AddBody(body);
            }
		}

		public static Test Create()
		{
			return new Buoyancy();
		}
	}
}