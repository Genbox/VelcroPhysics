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
	// This tests distance joints, body destruction, and joint destruction.
	public class Web : Test
	{
		public Web()
		{
			Body ground = null;
			{
				PolygonDef sd = new PolygonDef();
				sd.SetAsBox(50.0f, 10.0f);

				BodyDef bd = new BodyDef();
				bd.Position.Set(0.0f, -10.0f);
				ground = _world.CreateBody(bd);
				ground.CreateShape(sd);
			}

			{
				PolygonDef sd = new PolygonDef();
				sd.SetAsBox(0.5f, 0.5f);
				sd.Density = 5.0f;
				sd.Friction = 0.2f;

				BodyDef bd = new BodyDef();

				bd.Position.Set(-5.0f, 5.0f);
				_bodies[0] = _world.CreateBody(bd);
				_bodies[0].CreateShape(sd);
				_bodies[0].SetMassFromShapes();

				bd.Position.Set(5.0f, 5.0f);
				_bodies[1] = _world.CreateBody(bd);
				_bodies[1].CreateShape(sd);
				_bodies[1].SetMassFromShapes();

				bd.Position.Set(5.0f, 15.0f);
				_bodies[2] = _world.CreateBody(bd);
				_bodies[2].CreateShape(sd);
				_bodies[2].SetMassFromShapes();

				bd.Position.Set(-5.0f, 15.0f);
				_bodies[3] = _world.CreateBody(bd);
				_bodies[3].CreateShape(sd);
				_bodies[3].SetMassFromShapes();

				DistanceJointDef jd = new DistanceJointDef();
				Vec2 p1, p2, d;

				jd.FrequencyHz = 4.0f;
				jd.DampingRatio = 0.5f;

				jd.Body1 = ground;
				jd.Body2 = _bodies[0];
				jd.LocalAnchor1.Set(-10.0f, 10.0f);
				jd.LocalAnchor2.Set(-0.5f, -0.5f);
				p1 = jd.Body1.GetWorldPoint(jd.LocalAnchor1);
				p2 = jd.Body2.GetWorldPoint(jd.LocalAnchor2);
				d = p2 - p1;
				jd.Length = d.Length();
				_joints[0] = _world.CreateJoint(jd);

				jd.Body1 = ground;
				jd.Body2 = _bodies[1];
				jd.LocalAnchor1.Set(10.0f, 10.0f);
				jd.LocalAnchor2.Set(0.5f, -0.5f);
				p1 = jd.Body1.GetWorldPoint(jd.LocalAnchor1);
				p2 = jd.Body2.GetWorldPoint(jd.LocalAnchor2);
				d = p2 - p1;
				jd.Length = d.Length();
				_joints[1] = _world.CreateJoint(jd);

				jd.Body1 = ground;
				jd.Body2 = _bodies[2];
				jd.LocalAnchor1.Set(10.0f, 30.0f);
				jd.LocalAnchor2.Set(0.5f, 0.5f);
				p1 = jd.Body1.GetWorldPoint(jd.LocalAnchor1);
				p2 = jd.Body2.GetWorldPoint(jd.LocalAnchor2);
				d = p2 - p1;
				jd.Length = d.Length();
				_joints[2] = _world.CreateJoint(jd);

				jd.Body1 = ground;
				jd.Body2 = _bodies[3];
				jd.LocalAnchor1.Set(-10.0f, 30.0f);
				jd.LocalAnchor2.Set(-0.5f, 0.5f);
				p1 = jd.Body1.GetWorldPoint(jd.LocalAnchor1);
				p2 = jd.Body2.GetWorldPoint(jd.LocalAnchor2);
				d = p2 - p1;
				jd.Length = d.Length();
				_joints[3] = _world.CreateJoint(jd);

				jd.Body1 = _bodies[0];
				jd.Body2 = _bodies[1];
				jd.LocalAnchor1.Set(0.5f, 0.0f);
				jd.LocalAnchor2.Set(-0.5f, 0.0f); ;
				p1 = jd.Body1.GetWorldPoint(jd.LocalAnchor1);
				p2 = jd.Body2.GetWorldPoint(jd.LocalAnchor2);
				d = p2 - p1;
				jd.Length = d.Length();
				_joints[4] = _world.CreateJoint(jd);

				jd.Body1 = _bodies[1];
				jd.Body2 = _bodies[2];
				jd.LocalAnchor1.Set(0.0f, 0.5f);
				jd.LocalAnchor2.Set(0.0f, -0.5f);
				p1 = jd.Body1.GetWorldPoint(jd.LocalAnchor1);
				p2 = jd.Body2.GetWorldPoint(jd.LocalAnchor2);
				d = p2 - p1;
				jd.Length = d.Length();
				_joints[5] = _world.CreateJoint(jd);

				jd.Body1 = _bodies[2];
				jd.Body2 = _bodies[3];
				jd.LocalAnchor1.Set(-0.5f, 0.0f);
				jd.LocalAnchor2.Set(0.5f, 0.0f);
				p1 = jd.Body1.GetWorldPoint(jd.LocalAnchor1);
				p2 = jd.Body2.GetWorldPoint(jd.LocalAnchor2);
				d = p2 - p1;
				jd.Length = d.Length();
				_joints[6] = _world.CreateJoint(jd);

				jd.Body1 = _bodies[3];
				jd.Body2 = _bodies[0];
				jd.LocalAnchor1.Set(0.0f, -0.5f);
				jd.LocalAnchor2.Set(0.0f, 0.5f);
				p1 = jd.Body1.GetWorldPoint(jd.LocalAnchor1);
				p2 = jd.Body2.GetWorldPoint(jd.LocalAnchor2);
				d = p2 - p1;
				jd.Length = d.Length();
				_joints[7] = _world.CreateJoint(jd);
			}
		}

		public override void Keyboard(System.Windows.Forms.Keys key)
		{
			switch (key)
			{
				case System.Windows.Forms.Keys.B:
					for (int i = 0; i < 4; ++i)
					{
						if (_bodies[i]!=null)
						{
							_world.DestroyBody(_bodies[i]);
							_bodies[i] = null;
							break;
						}
					}
					break;

				case System.Windows.Forms.Keys.J:
					for (int i = 0; i < 8; ++i)
					{
						if (_joints[i]!=null)
						{
							_world.DestroyJoint(_joints[i]);
							_joints[i] = null;
							break;
						}
					}
					break;
			}
		}

		public override void Step(Settings settings)
		{
			base.Step(settings);

			OpenGLDebugDraw.DrawString(5, _textLine, "This demonstrates a soft distance joint.");
			_textLine += 15;
			OpenGLDebugDraw.DrawString(5, _textLine, "Press: (b) to delete a body, (j) to delete a joint");
			_textLine += 15;
		}

		public override void JointDestroyed(Joint joint)
		{
			for (int i = 0; i < 8; ++i)
			{
				if (_joints[i] == joint)
				{
					_joints[i] = null;
					break;
				}
			}
		}

		public static Test Create()
		{
			return new Web();
		}

		Body[] _bodies = new Body[4];
		Joint[] _joints = new Joint[8];
	}
}