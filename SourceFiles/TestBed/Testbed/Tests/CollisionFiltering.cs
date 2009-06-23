/*
  Box2DX Copyright (c) 2008 Ihar Kalasouski http://code.Google.Com/p/box2dx
  Box2D original C++ version Copyright (c) 2006-2008 Erin Catto http://www.Gphysics.Com

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
	public class CollisionFiltering : Test
	{
		// This is a test of collision filtering.
		// There is a triangle, a box, and a circle.
		// There are 6 shapes. 3 large and 3 small.
		// The 3 small ones always collide.
		// The 3 large ones never collide.
		// The boxes don't collide with triangles (except if both are small).
		const short k_smallGroup = 1;
		const short k_largeGroup = -1;

		const ushort k_defaultCategory = 0x0001;
		const ushort k_triangleCategory = 0x0002;
		const ushort k_boxCategory = 0x0004;
		const ushort k_circleCategory = 0x0008;

		const ushort k_triangleMask = 0xFFFF;
		readonly ushort k_boxMask = 0xFFFF ^ k_triangleCategory;
		const ushort k_circleMask = 0xFFFF;

		public CollisionFiltering()
		{
			// Ground body
			{
				PolygonDef sd = new PolygonDef();
				sd.SetAsBox(50.0f, 10.0f);
				sd.Friction = 0.3f;

				BodyDef bd = new BodyDef();
				bd.Position.Set(0.0f, -10.0f);

				Body ground = _world.CreateBody(bd);
				ground.CreateShape(sd);
			}

			// Small triangle
			PolygonDef triangleShapeDef = new PolygonDef();
			triangleShapeDef.VertexCount = 3;
			triangleShapeDef.Vertices[0].Set(-1.0f, 0.0f);
			triangleShapeDef.Vertices[1].Set(1.0f, 0.0f);
			triangleShapeDef.Vertices[2].Set(0.0f, 2.0f);
			triangleShapeDef.Density = 1.0f;

			triangleShapeDef.Filter.GroupIndex = k_smallGroup;
			triangleShapeDef.Filter.CategoryBits = k_triangleCategory;
			triangleShapeDef.Filter.MaskBits = k_triangleMask;

			BodyDef triangleBodyDef = new BodyDef();
			triangleBodyDef.Position.Set(-5.0f, 2.0f);

			Body body1 = _world.CreateBody(triangleBodyDef);
			body1.CreateShape(triangleShapeDef);
			body1.SetMassFromShapes();

			// Large triangle (recycle definitions)
			triangleShapeDef.Vertices[0] *= 2.0f;
			triangleShapeDef.Vertices[1] *= 2.0f;
			triangleShapeDef.Vertices[2] *= 2.0f;
			triangleShapeDef.Filter.GroupIndex = k_largeGroup;
			triangleBodyDef.Position.Set(-5.0f, 6.0f);
			triangleBodyDef.FixedRotation = true; // look at me!

			Body body2 = _world.CreateBody(triangleBodyDef);
			body2.CreateShape(triangleShapeDef);
			body2.SetMassFromShapes();

			// Small box
			PolygonDef boxShapeDef = new PolygonDef();
			boxShapeDef.SetAsBox(1.0f, 0.5f);
			boxShapeDef.Density = 1.0f;

			boxShapeDef.Filter.GroupIndex = k_smallGroup;
			boxShapeDef.Filter.CategoryBits = k_boxCategory;
			boxShapeDef.Filter.MaskBits = k_boxMask;

			BodyDef boxBodyDef = new BodyDef();
			boxBodyDef.Position.Set(0.0f, 2.0f);

			Body body3 = _world.CreateBody(boxBodyDef);
			body3.CreateShape(boxShapeDef);
			body3.SetMassFromShapes();

			// Large box (recycle definitions)
			boxShapeDef.SetAsBox(2.0f, 1.0f);
			boxShapeDef.Filter.GroupIndex = k_largeGroup;
			boxBodyDef.Position.Set(0.0f, 6.0f);

			Body body4 = _world.CreateBody(boxBodyDef);
			body4.CreateShape(boxShapeDef);
			body4.SetMassFromShapes();

			// Small circle
			CircleDef circleShapeDef = new CircleDef();
			circleShapeDef.Radius = 1.0f;
			circleShapeDef.Density = 1.0f;

			circleShapeDef.Filter.GroupIndex = k_smallGroup;
			circleShapeDef.Filter.CategoryBits = k_circleCategory;
			circleShapeDef.Filter.MaskBits = k_circleMask;

			BodyDef circleBodyDef = new BodyDef();
			circleBodyDef.Position.Set(5.0f, 2.0f);

			Body body5 = _world.CreateBody(circleBodyDef);
			body5.CreateShape(circleShapeDef);
			body5.SetMassFromShapes();

			// Large circle
			circleShapeDef.Radius *= 2.0f;
			circleShapeDef.Filter.GroupIndex = k_largeGroup;
			circleBodyDef.Position.Set(5.0f, 6.0f);

			Body body6 = _world.CreateBody(circleBodyDef);
			body6.CreateShape(circleShapeDef);
			body6.SetMassFromShapes();
		}

		public static Test Create()
		{
			return new CollisionFiltering();
		}
	}
}