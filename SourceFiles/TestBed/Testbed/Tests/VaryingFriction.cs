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

namespace TestBed
{
    public class VaryingFriction : Test
    {
        public VaryingFriction()
	    {
		    {
			    PolygonDef sd = new  PolygonDef();;
			    sd.SetAsBox(50.0f, 20.0f);

			    BodyDef bd = new  BodyDef() ;
			    bd.Position.Set(0.0f, -20.0f);
			    Body ground = _world.CreateBody(bd);
			    ground.CreateShape(sd);
		    }

		    {
			    PolygonDef sd = new  PolygonDef();;
			    sd.SetAsBox(13.0f, 0.25f);

			    BodyDef bd = new  BodyDef() ;
			    bd.Position.Set(-4.0f, 22.0f);
			    bd.Angle = -0.25f;

			    Body ground = _world.CreateBody(bd);
			    ground.CreateShape(sd);
		    }

		    {
			    PolygonDef sd = new  PolygonDef();;
			    sd.SetAsBox(0.25f, 1.0f);

			    BodyDef bd = new  BodyDef() ;
			    bd.Position.Set(10.5f, 19.0f);

			    Body ground = _world.CreateBody(bd);
			    ground.CreateShape(sd);
		    }

		    {
			    PolygonDef sd = new  PolygonDef();;
			    sd.SetAsBox(13.0f, 0.25f);

			    BodyDef bd = new  BodyDef() ;
			    bd.Position.Set(4.0f, 14.0f);
			    bd.Angle = 0.25f;

			    Body ground = _world.CreateBody(bd);
			    ground.CreateShape(sd);
		    }

		    {
			    PolygonDef sd = new  PolygonDef();;
			    sd.SetAsBox(0.25f, 1.0f);

			    BodyDef bd = new  BodyDef() ;
			    bd.Position.Set(-10.5f, 11.0f);

			    Body ground = _world.CreateBody(bd);
			    ground.CreateShape(sd);
		    }

		    {
			    PolygonDef sd = new  PolygonDef();;
			    sd.SetAsBox(13.0f, 0.25f);

			    BodyDef bd = new  BodyDef() ;
			    bd.Position.Set(-4.0f, 6.0f);
			    bd.Angle = -0.25f;

			    Body ground = _world.CreateBody(bd);
			    ground.CreateShape(sd);
		    }

		    {
			    PolygonDef sd = new  PolygonDef();;
			    sd.SetAsBox(0.5f, 0.5f);
			    sd.Density = 25.0f;

			    float[] friction = new float[5]{0.75f, 0.5f, 0.35f, 0.1f, 0.0f};

			    for (int i = 0; i < 5; ++i)
			    {
				    BodyDef bd = new  BodyDef() ;
				    bd.Position.Set(-15.0f + 4.0f * i, 28.0f);
				    Body body = _world.CreateBody(bd);

				    sd.Friction = friction[i];
				    body.CreateShape(sd);
				    body.SetMassFromShapes();
			    }
		    }
        }

        public static Test Create()
        {
            return new VaryingFriction();
        }
    }
}