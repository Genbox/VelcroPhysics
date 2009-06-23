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
		private BuoyancyController _bc = new BuoyancyController();

		public Buoyancy()
		{
			BuoyancyController bc = _bc;
			_world.AddController(bc);

			bc.offset = 15;
			bc.normal.Set(0, 1);
			bc.density = 2;
			bc.linearDrag = 2;
			bc.angularDrag = 1;

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
				body.CreateShape(sd);
				body.SetMassFromShapes();

				bc.AddBody(body);
			}

			for (int i = 0; i < 3; ++i)
			{
				CircleDef sd = new CircleDef();
				sd.Radius = 0.5f;
				sd.Density = 1.0f;

				BodyDef bd = new BodyDef();
				bd.Position.Set(-6.0f + 6.0f * i, 10.0f);
				Body body = _world.CreateBody(bd);
				body.CreateShape(sd);
				body.SetMassFromShapes();

				bc.AddBody(body);
			}
		}

		public static Test Create()
		{
			return new Buoyancy();
		}
	}
}