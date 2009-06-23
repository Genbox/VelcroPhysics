/*
  Box2DX Copyright (c) 2008 Ihar Kalasouski http://code.google.com/p/box2dx
  Box2D original C++ version Copyright (c) 2006-2007 Erin Catto http://www.gphysics.com

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
	// Ragdoll class thanks to darkzerox.
	public class Biped: IDisposable
	{
		public Biped(World w, Vec2 position)				
		{
			_world = w;

			BipedDef def = new BipedDef();
			BodyDef bd = new BodyDef();

			// create body parts
			bd = def.LFootDef;
			bd.Position += position;
			LFoot = w.CreateBody(bd);
			LFoot.CreateShape(def.LFootPoly);
			LFoot.SetMassFromShapes();

			bd = def.RFootDef;
			bd.Position += position;
			RFoot = w.CreateBody(bd);
			RFoot.CreateShape(def.RFootPoly);
			RFoot.SetMassFromShapes();

			bd = def.LCalfDef;
			bd.Position += position;
			LCalf = w.CreateBody(bd);
			LCalf.CreateShape(def.LCalfPoly);
			LCalf.SetMassFromShapes();

			bd = def.RCalfDef;
			bd.Position += position;
			RCalf = w.CreateBody(bd);
			RCalf.CreateShape(def.RCalfPoly);
			RCalf.SetMassFromShapes();

			bd = def.LThighDef;
			bd.Position += position;
			LThigh = w.CreateBody(bd);
			LThigh.CreateShape(def.LThighPoly);
			LThigh.SetMassFromShapes();

			bd = def.RThighDef;
			bd.Position += position;
			RThigh = w.CreateBody(bd);
			RThigh.CreateShape(def.RThighPoly);
			RThigh.SetMassFromShapes();

			bd = def.PelvisDef0;
			bd.Position += position;
			Pelvis = w.CreateBody(bd);
			Pelvis.CreateShape(def.PelvisPoly);
			Pelvis.SetMassFromShapes();

			bd = def.PelvisDef;
			bd.Position += position;
			Stomach = w.CreateBody(bd);
			Stomach.CreateShape(def.StomachPoly);
			Stomach.SetMassFromShapes();

			bd = def.ChestDef;
			bd.Position += position;
			Chest = w.CreateBody(bd);
			Chest.CreateShape(def.ChestPoly);
			Chest.SetMassFromShapes();

			bd = def.NeckDef;
			bd.Position += position;
			Neck = w.CreateBody(bd);
			Neck.CreateShape(def.NeckPoly);
			Neck.SetMassFromShapes();

			bd = def.HeadDef;
			bd.Position += position;
			Head = w.CreateBody(bd);
			Head.CreateShape(def.HeadCirc);
			Head.SetMassFromShapes();

			bd = def.LUpperArmDef;
			bd.Position += position;
			LUpperArm = w.CreateBody(bd);
			LUpperArm.CreateShape(def.LUpperArmPoly);
			LUpperArm.SetMassFromShapes();

			bd = def.RUpperArmDef;
			bd.Position += position;
			RUpperArm = w.CreateBody(bd);
			RUpperArm.CreateShape(def.RUpperArmPoly);
			RUpperArm.SetMassFromShapes();

			bd = def.LForearmDef;
			bd.Position += position;
			LForearm = w.CreateBody(bd);
			LForearm.CreateShape(def.LForearmPoly);
			LForearm.SetMassFromShapes();

			bd = def.RForearmDef;
			bd.Position += position;
			RForearm = w.CreateBody(bd);
			RForearm.CreateShape(def.RForearmPoly);
			RForearm.SetMassFromShapes();

			bd = def.LHandDef;
			bd.Position += position;
			LHand = w.CreateBody(bd);
			LHand.CreateShape(def.LHandPoly);
			LHand.SetMassFromShapes();

			bd = def.RHandDef;
			bd.Position += position;
			RHand = w.CreateBody(bd);
			RHand.CreateShape(def.RHandPoly);
			RHand.SetMassFromShapes();
			
			// link body parts
			def.LAnkleDef.Body1		= LFoot;
			def.LAnkleDef.Body2		= LCalf;
			def.RAnkleDef.Body1		= RFoot;
			def.RAnkleDef.Body2		= RCalf;
			def.LKneeDef.Body1		= LCalf;
			def.LKneeDef.Body2		= LThigh;
			def.RKneeDef.Body1		= RCalf;
			def.RKneeDef.Body2		= RThigh;
			def.LHipDef.Body1		= LThigh;
			def.LHipDef.Body2		= Pelvis;
			def.RHipDef.Body1		= RThigh;
			def.RHipDef.Body2		= Pelvis;
			def.LowerAbsDef.Body1	= Pelvis;
			def.LowerAbsDef.Body2	= Stomach;
			def.UpperAbsDef.Body1	= Stomach;
			def.UpperAbsDef.Body2	= Chest;
			def.LowerNeckDef.Body1	= Chest;
			def.LowerNeckDef.Body2	= Neck;
			def.UpperNeckDef.Body1	= Chest;
			def.UpperNeckDef.Body2	= Head;
			def.LShoulderDef.Body1	= Chest;
			def.LShoulderDef.Body2	= LUpperArm;
			def.RShoulderDef.Body1	= Chest;
			def.RShoulderDef.Body2	= RUpperArm;
			def.LElbowDef.Body1		= LForearm;
			def.LElbowDef.Body2		= LUpperArm;
			def.RElbowDef.Body1		= RForearm;
			def.RElbowDef.Body2		= RUpperArm;
			def.LWristDef.Body1		= LHand;
			def.LWristDef.Body2		= LForearm;
			def.RWristDef.Body1		= RHand;
			def.RWristDef.Body2		= RForearm;
			
			// create joints
			LAnkle		= (RevoluteJoint)w.CreateJoint(def.LAnkleDef);
			RAnkle		= (RevoluteJoint)w.CreateJoint(def.RAnkleDef);
			LKnee		= (RevoluteJoint)w.CreateJoint(def.LKneeDef);
			RKnee		= (RevoluteJoint)w.CreateJoint(def.RKneeDef);
			LHip		= (RevoluteJoint)w.CreateJoint(def.LHipDef);
			RHip		= (RevoluteJoint)w.CreateJoint(def.RHipDef);
			LowerAbs	= (RevoluteJoint)w.CreateJoint(def.LowerAbsDef);
			UpperAbs	= (RevoluteJoint)w.CreateJoint(def.UpperAbsDef);
			LowerNeck	= (RevoluteJoint)w.CreateJoint(def.LowerNeckDef);
			UpperNeck	= (RevoluteJoint)w.CreateJoint(def.UpperNeckDef);
			LShoulder	= (RevoluteJoint)w.CreateJoint(def.LShoulderDef);
			RShoulder	= (RevoluteJoint)w.CreateJoint(def.RShoulderDef);
			LElbow		= (RevoluteJoint)w.CreateJoint(def.LElbowDef);
			RElbow		= (RevoluteJoint)w.CreateJoint(def.RElbowDef);
			LWrist		= (RevoluteJoint)w.CreateJoint(def.LWristDef);
			RWrist		= (RevoluteJoint)w.CreateJoint(def.RWristDef);
		}

		public void Dispose()
		{
			_world.DestroyBody(LFoot);
			_world.DestroyBody(RFoot);
			_world.DestroyBody(LCalf);
			_world.DestroyBody(RCalf);
			_world.DestroyBody(LThigh);
			_world.DestroyBody(RThigh);
			_world.DestroyBody(Pelvis);
			_world.DestroyBody(Stomach);
			_world.DestroyBody(Chest);
			_world.DestroyBody(Neck);
			_world.DestroyBody(Head);
			_world.DestroyBody(LUpperArm);
			_world.DestroyBody(RUpperArm);
			_world.DestroyBody(LForearm);
			_world.DestroyBody(RForearm);
			_world.DestroyBody(LHand);
			_world.DestroyBody(RHand);
		}

		World _world;

		Body				LFoot, RFoot, LCalf, RCalf, LThigh, RThigh,
							Pelvis, Stomach, Chest, Neck, Head,
							LUpperArm, RUpperArm, LForearm, RForearm, LHand, RHand;

		RevoluteJoint		LAnkle, RAnkle, LKnee, RKnee, LHip, RHip, 
							LowerAbs, UpperAbs, LowerNeck, UpperNeck,
							LShoulder, RShoulder, LElbow, RElbow, LWrist, RWrist;
	}
}
