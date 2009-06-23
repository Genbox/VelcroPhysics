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

// Contributed by caspin.

using System;
using System.Collections.Generic;
using System.Text;

using Box2DX.Common;
using Box2DX.Collision;
using Box2DX.Dynamics;

namespace TestBed
{
	public delegate bool ContactCBDelegate(ContactPoint cp1 ,ContactPoint cp2);

	public class ContactCB : Test
	{
		Body _ball;
		Body _bullet;
		Shape _ball_shape;
		Dictionary<MyContactPoint, ContactCBDelegate> _set = new Dictionary<MyContactPoint,ContactCBDelegate>();
		Queue<string> _strings = new Queue<string>();

		public ContactCB()
		{
			PolygonDef sd = new PolygonDef();
			sd.Friction = 0;
			sd.VertexCount = 3;

			sd.Vertices[0].Set(10, 10);
			sd.Vertices[1].Set(9, 7);
			sd.Vertices[2].Set(10, 0);
			_world.GetGroundBody().CreateShape(sd);

			sd.Vertices[0].Set(9, 7);
			sd.Vertices[1].Set(8, 0);
			sd.Vertices[2].Set(10, 0);
			_world.GetGroundBody().CreateShape(sd);

			sd.Vertices[0].Set(9, 7);
			sd.Vertices[1].Set(8, 5);
			sd.Vertices[2].Set(8, 0);
			_world.GetGroundBody().CreateShape(sd);

			sd.Vertices[0].Set(8, 5);
			sd.Vertices[1].Set(7, 4);
			sd.Vertices[2].Set(8, 0);
			_world.GetGroundBody().CreateShape(sd);

			sd.Vertices[0].Set(7, 4);
			sd.Vertices[1].Set(5, 0);
			sd.Vertices[2].Set(8, 0);
			_world.GetGroundBody().CreateShape(sd);

			sd.Vertices[0].Set(7, 4);
			sd.Vertices[1].Set(5, 3);
			sd.Vertices[2].Set(5, 0);
			_world.GetGroundBody().CreateShape(sd);

			sd.Vertices[0].Set(5, 3);
			sd.Vertices[1].Set(2, 2);
			sd.Vertices[2].Set(5, 0);
			_world.GetGroundBody().CreateShape(sd);

			sd.Vertices[0].Set(2, 2);
			sd.Vertices[1].Set(0, 0);
			sd.Vertices[2].Set(5, 0);
			_world.GetGroundBody().CreateShape(sd);

			sd.Vertices[0].Set(2, 2);
			sd.Vertices[1].Set(-2, 2);
			sd.Vertices[2].Set(0, 0);
			_world.GetGroundBody().CreateShape(sd);

			sd.Vertices[2].Set(-2, 2);
			sd.Vertices[1].Set(0, 0);
			sd.Vertices[0].Set(-5, 0);
			_world.GetGroundBody().CreateShape(sd);

			sd.Vertices[2].Set(-5, 3);
			sd.Vertices[1].Set(-2, 2);
			sd.Vertices[0].Set(-5, 0);
			_world.GetGroundBody().CreateShape(sd);

			sd.Vertices[2].Set(-7, 4);
			sd.Vertices[1].Set(-5, 3);
			sd.Vertices[0].Set(-5, 0);
			_world.GetGroundBody().CreateShape(sd);

			sd.Vertices[2].Set(-7, 4);
			sd.Vertices[1].Set(-5, 0);
			sd.Vertices[0].Set(-8, 0);
			_world.GetGroundBody().CreateShape(sd);

			sd.Vertices[2].Set(-8, 5);
			sd.Vertices[1].Set(-7, 4);
			sd.Vertices[0].Set(-8, 0);
			_world.GetGroundBody().CreateShape(sd);

			sd.Vertices[2].Set(-9, 7);
			sd.Vertices[1].Set(-8, 5);
			sd.Vertices[0].Set(-8, 0);
			_world.GetGroundBody().CreateShape(sd);

			sd.Vertices[2].Set(-9, 7);
			sd.Vertices[1].Set(-8, 0);
			sd.Vertices[0].Set(-10, 0);
			_world.GetGroundBody().CreateShape(sd);

			sd.Vertices[2].Set(-10, 10);
			sd.Vertices[1].Set(-9, 7);
			sd.Vertices[0].Set(-10, 0);
			_world.GetGroundBody().CreateShape(sd);

			sd.SetAsBox(.5f, 6, new Vec2(10.5f, 6), 0);
			_world.GetGroundBody().CreateShape(sd);

			sd.SetAsBox(.5f, 6, new Vec2(-10.5f, 6), 0);
			_world.GetGroundBody().CreateShape(sd);

			BodyDef bd = new BodyDef();
			bd.Position.Set(9.5f, 60);
			Body _ball = _world.CreateBody(bd);

			PolygonDef cd = new PolygonDef();
			cd.VertexCount = 8;
			float w = 1.0f;
			float b = w / (2.0f + (float)System.Math.Sqrt(2.0f));
			float s = (float)System.Math.Sqrt(2.0f) * b;
			cd.Vertices[0].Set(0.5f * s, 0.0f);
			cd.Vertices[1].Set(0.5f * w, b);
			cd.Vertices[2].Set(0.5f * w, b + s);
			cd.Vertices[3].Set(0.5f * s, w);
			cd.Vertices[4].Set(-0.5f * s, w);
			cd.Vertices[5].Set(-0.5f * w, b + s);
			cd.Vertices[6].Set(-0.5f * w, b);
			cd.Vertices[7].Set(-0.5f * s, 0.0f);
			cd.Density = 1.0f;

			_ball_shape = _ball.CreateShape(cd);
			_ball.SetMassFromShapes();
		}

		/*public bool key_comp(MyContactPoint lhs, MyContactPoint rhs)
		{
			if( lhs.shape1 < rhs.shape1 ) return true;
			if( lhs.shape1 == rhs.shape1 && lhs.shape2 < rhs.shape2 ) return true;
			if( lhs.shape1 == rhs.shape1 && lhs.shape2 == rhs.shape2 && lhs.id.Key < rhs.id.Key ) return true;
			return false;
		}*/

		public override void Step(Settings settings)
		{
			base.Step(settings);
			string oss = string.Empty;

			for (int i = 0; i < _pointCount; ++i)
			{
				switch (_points[i].state)
				{
					case ContactState.ContactAdded:
					{
						oss = string.Empty;
						_set.Add(_points[i], null);
						//if( ! m_set.insert( m_points[i] ).second )
						//{
						//	oss << "ERROR ";
						//}
						//else
						//{
						//	oss << "      ";
						//}
						oss += "added:   " + _points[i].shape1.ToString() + " -> " + _points[i].shape2.ToString();
						oss += " : " + _points[i].id.Key.ToString();
						_strings.Enqueue(oss);						
						break;
					}
					case ContactState.ContactRemoved:
					{
						oss = string.Empty;
						if (!_set.ContainsKey(_points[i]))
						{
							oss += "ERROR ";
						}
						else
						{
							oss += "      ";
						}
						oss += "removed: " + _points[i].shape1.ToString() + " -> " + _points[i].shape2.ToString();
						oss += " : " + _points[i].id.Key.ToString();
						_strings.Enqueue(oss);						
						_set.Remove( _points[i] );
						break;
					}
					case ContactState.ContactPersisted:
					{
						oss = string.Empty;
						if(!_set.ContainsKey(_points[i]))
						{
							oss += "ERROR persist: " + _points[i].shape1.ToString() + " -> ";
							oss += _points[i].shape2.ToString() + " : " + _points[i].id.Key;
							_strings.Enqueue(oss);							
						}
						break;
					}
				}
			}

			while (_strings.Count > 15)
			{
				_strings.Dequeue();
			}			

			foreach (string s in _strings)
			{
				OpenGLDebugDraw.DrawString(5, _textLine, s);
				_textLine += 15;
			}
		}

		public static Test Create()
		{
			return new ContactCB();
		}
	}
}