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
	public class BipedDef
	{
		public static short count = 0;
		const float k_scale = 3.0f;

		public BodyDef LFootDef = new BodyDef(), RFootDef = new BodyDef(), LCalfDef = new BodyDef(), RCalfDef = new BodyDef(),
			LThighDef = new BodyDef(), RThighDef = new BodyDef(), PelvisDef0 = new BodyDef(), PelvisDef = new BodyDef(), StomachDef = new BodyDef(),
			ChestDef = new BodyDef(), NeckDef = new BodyDef(), HeadDef = new BodyDef(), LUpperArmDef = new BodyDef(),
			RUpperArmDef = new BodyDef(), LForearmDef = new BodyDef(), RForearmDef = new BodyDef(), LHandDef = new BodyDef(),
			RHandDef = new BodyDef();

		public PolygonDef LFootPoly = new PolygonDef(), RFootPoly = new PolygonDef(), LCalfPoly = new PolygonDef(),
			RCalfPoly = new PolygonDef(), LThighPoly = new PolygonDef(), RThighPoly = new PolygonDef(),
			PelvisPoly = new PolygonDef(), StomachPoly = new PolygonDef(), ChestPoly = new PolygonDef(),
			NeckPoly = new PolygonDef(), LUpperArmPoly = new PolygonDef(), RUpperArmPoly = new PolygonDef(),
			LForearmPoly = new PolygonDef(), RForearmPoly = new PolygonDef(), LHandPoly = new PolygonDef(), RHandPoly = new PolygonDef();

		public CircleDef HeadCirc = new CircleDef();

		public RevoluteJointDef LAnkleDef = new RevoluteJointDef(), RAnkleDef = new RevoluteJointDef(),
			LKneeDef = new RevoluteJointDef(), RKneeDef = new RevoluteJointDef(), LHipDef = new RevoluteJointDef(),
			RHipDef = new RevoluteJointDef(), LowerAbsDef = new RevoluteJointDef(), UpperAbsDef = new RevoluteJointDef(),
			LowerNeckDef = new RevoluteJointDef(), UpperNeckDef = new RevoluteJointDef(), LShoulderDef = new RevoluteJointDef(),
			RShoulderDef = new RevoluteJointDef(), LElbowDef = new RevoluteJointDef(), RElbowDef = new RevoluteJointDef(),
			LWristDef = new RevoluteJointDef(), RWristDef = new RevoluteJointDef();

		public BipedDef()
		{
			SetMotorTorque(2.0f);
			SetMotorSpeed(0.0f);
			SetDensity(20.0f);
			SetRestitution(0.0f);
			SetLinearDamping(0.0f);
			SetAngularDamping(0.005f);
			SetGroupIndex(--count);
			EnableMotor();
			EnableLimit();

			DefaultVertices();
			DefaultPositions();
			DefaultJoints();

			LFootPoly.Friction = RFootPoly.Friction = 0.85f;
		}

		public void IsFast(bool b) { ;}

		public void SetGroupIndex(short i)
		{
			LFootPoly.Filter.GroupIndex = i;
			RFootPoly.Filter.GroupIndex = i;
			LCalfPoly.Filter.GroupIndex = i;
			RCalfPoly.Filter.GroupIndex = i;
			LThighPoly.Filter.GroupIndex = i;
			RThighPoly.Filter.GroupIndex = i;
			PelvisPoly.Filter.GroupIndex = i;
			StomachPoly.Filter.GroupIndex = i;
			ChestPoly.Filter.GroupIndex = i;
			NeckPoly.Filter.GroupIndex = i;
			HeadCirc.Filter.GroupIndex = i;
			LUpperArmPoly.Filter.GroupIndex = i;
			RUpperArmPoly.Filter.GroupIndex = i;
			LForearmPoly.Filter.GroupIndex = i;
			RForearmPoly.Filter.GroupIndex = i;
			LHandPoly.Filter.GroupIndex = i;
			RHandPoly.Filter.GroupIndex = i;
		}

		public void SetLinearDamping(float f)
		{
			LFootDef.LinearDamping = f;
			RFootDef.LinearDamping = f;
			LCalfDef.LinearDamping = f;
			RCalfDef.LinearDamping = f;
			LThighDef.LinearDamping = f;
			RThighDef.LinearDamping = f;
			PelvisDef0.LinearDamping = f;
			PelvisDef.LinearDamping = f;
			StomachDef.LinearDamping = f;
			ChestDef.LinearDamping = f;
			NeckDef.LinearDamping = f;
			HeadDef.LinearDamping = f;
			LUpperArmDef.LinearDamping = f;
			RUpperArmDef.LinearDamping = f;
			LForearmDef.LinearDamping = f;
			RForearmDef.LinearDamping = f;
			LHandDef.LinearDamping = f;
			RHandDef.LinearDamping = f;
		}

		public void SetAngularDamping(float f)
		{
			LFootDef.AngularDamping = f;
			RFootDef.AngularDamping = f;
			LCalfDef.AngularDamping = f;
			RCalfDef.AngularDamping = f;
			LThighDef.AngularDamping = f;
			RThighDef.AngularDamping = f;
			PelvisDef0.AngularDamping = f;
			PelvisDef.AngularDamping = f;
			StomachDef.AngularDamping = f;
			ChestDef.AngularDamping = f;
			NeckDef.AngularDamping = f;
			HeadDef.AngularDamping = f;
			LUpperArmDef.AngularDamping = f;
			RUpperArmDef.AngularDamping = f;
			LForearmDef.AngularDamping = f;
			RForearmDef.AngularDamping = f;
			LHandDef.AngularDamping = f;
			RHandDef.AngularDamping = f;
		}

		public void SetMotorTorque(float f)
		{
			LAnkleDef.MaxMotorTorque = f;
			RAnkleDef.MaxMotorTorque = f;
			LKneeDef.MaxMotorTorque = f;
			RKneeDef.MaxMotorTorque = f;
			LHipDef.MaxMotorTorque = f;
			RHipDef.MaxMotorTorque = f;
			LowerAbsDef.MaxMotorTorque = f;
			UpperAbsDef.MaxMotorTorque = f;
			LowerNeckDef.MaxMotorTorque = f;
			UpperNeckDef.MaxMotorTorque = f;
			LShoulderDef.MaxMotorTorque = f;
			RShoulderDef.MaxMotorTorque = f;
			LElbowDef.MaxMotorTorque = f;
			RElbowDef.MaxMotorTorque = f;
			LWristDef.MaxMotorTorque = f;
			RWristDef.MaxMotorTorque = f;
		}

		public void SetMotorSpeed(float f)
		{
			LAnkleDef.MotorSpeed = f;
			RAnkleDef.MotorSpeed = f;
			LKneeDef.MotorSpeed = f;
			RKneeDef.MotorSpeed = f;
			LHipDef.MotorSpeed = f;
			RHipDef.MotorSpeed = f;
			LowerAbsDef.MotorSpeed = f;
			UpperAbsDef.MotorSpeed = f;
			LowerNeckDef.MotorSpeed = f;
			UpperNeckDef.MotorSpeed = f;
			LShoulderDef.MotorSpeed = f;
			RShoulderDef.MotorSpeed = f;
			LElbowDef.MotorSpeed = f;
			RElbowDef.MotorSpeed = f;
			LWristDef.MotorSpeed = f;
			RWristDef.MotorSpeed = f;
		}

		public void SetDensity(float f)
		{
			LFootPoly.Density = f;
			RFootPoly.Density = f;
			LCalfPoly.Density = f;
			RCalfPoly.Density = f;
			LThighPoly.Density = f;
			RThighPoly.Density = f;
			PelvisPoly.Density = f;
			StomachPoly.Density = f;
			ChestPoly.Density = f;
			NeckPoly.Density = f;
			HeadCirc.Density = f;
			LUpperArmPoly.Density = f;
			RUpperArmPoly.Density = f;
			LForearmPoly.Density = f;
			RForearmPoly.Density = f;
			LHandPoly.Density = f;
			RHandPoly.Density = f;
		}

		public void SetRestitution(float f)
		{
			LFootPoly.Restitution = f;
			RFootPoly.Restitution = f;
			LCalfPoly.Restitution = f;
			RCalfPoly.Restitution = f;
			LThighPoly.Restitution = f;
			RThighPoly.Restitution = f;
			PelvisPoly.Restitution = f;
			StomachPoly.Restitution = f;
			ChestPoly.Restitution = f;
			NeckPoly.Restitution = f;
			HeadCirc.Restitution = f;
			LUpperArmPoly.Restitution = f;
			RUpperArmPoly.Restitution = f;
			LForearmPoly.Restitution = f;
			RForearmPoly.Restitution = f;
			LHandPoly.Restitution = f;
			RHandPoly.Restitution = f;
		}

		public void EnableLimit()
		{
			SetLimit(true);
		}

		public void DisableLimit()
		{
			SetLimit(false);
		}

		public void SetLimit(bool b)
		{
			LAnkleDef.EnableLimit = b;
			RAnkleDef.EnableLimit = b;
			LKneeDef.EnableLimit = b;
			RKneeDef.EnableLimit = b;
			LHipDef.EnableLimit = b;
			RHipDef.EnableLimit = b;
			LowerAbsDef.EnableLimit = b;
			UpperAbsDef.EnableLimit = b;
			LowerNeckDef.EnableLimit = b;
			UpperNeckDef.EnableLimit = b;
			LShoulderDef.EnableLimit = b;
			RShoulderDef.EnableLimit = b;
			LElbowDef.EnableLimit = b;
			RElbowDef.EnableLimit = b;
			LWristDef.EnableLimit = b;
			RWristDef.EnableLimit = b;
		}

		public void EnableMotor()
		{
			SetMotor(true);
		}

		public void DisableMotor()
		{
			SetMotor(false);
		}

		public void SetMotor(bool b)
		{
			LAnkleDef.EnableMotor = b;
			RAnkleDef.EnableMotor = b;
			LKneeDef.EnableMotor = b;
			RKneeDef.EnableMotor = b;
			LHipDef.EnableMotor = b;
			RHipDef.EnableMotor = b;
			LowerAbsDef.EnableMotor = b;
			UpperAbsDef.EnableMotor = b;
			LowerNeckDef.EnableMotor = b;
			UpperNeckDef.EnableMotor = b;
			LShoulderDef.EnableMotor = b;
			RShoulderDef.EnableMotor = b;
			LElbowDef.EnableMotor = b;
			RElbowDef.EnableMotor = b;
			LWristDef.EnableMotor = b;
			RWristDef.EnableMotor = b;
		}

		public void DefaultVertices()
		{
			{	// feet
				LFootPoly.VertexCount = RFootPoly.VertexCount = 5;
				LFootPoly.Vertices[0] = RFootPoly.Vertices[0] = k_scale * new Vec2(.033f, .143f);
				LFootPoly.Vertices[1] = RFootPoly.Vertices[1] = k_scale * new Vec2(.023f,.033f);
				LFootPoly.Vertices[2] = RFootPoly.Vertices[2] = k_scale * new Vec2(.267f,.035f);
				LFootPoly.Vertices[3] = RFootPoly.Vertices[3] = k_scale * new Vec2(.265f,.065f);
				LFootPoly.Vertices[4] = RFootPoly.Vertices[4] = k_scale * new Vec2(.117f,.143f);
			}
			{	// calves
				LCalfPoly.VertexCount = RCalfPoly.VertexCount = 4;
				LCalfPoly.Vertices[0] = RCalfPoly.Vertices[0] = k_scale * new Vec2(.089f,.016f);
				LCalfPoly.Vertices[1] = RCalfPoly.Vertices[1] = k_scale * new Vec2(.178f,.016f);
				LCalfPoly.Vertices[2] = RCalfPoly.Vertices[2] = k_scale * new Vec2(.205f,.417f);
				LCalfPoly.Vertices[3] = RCalfPoly.Vertices[3] = k_scale * new Vec2(.095f,.417f);
			}
			{	// thighs
				LThighPoly.VertexCount = RThighPoly.VertexCount = 4;
				LThighPoly.Vertices[0] = RThighPoly.Vertices[0] = k_scale * new Vec2(.137f,.032f);
				LThighPoly.Vertices[1] = RThighPoly.Vertices[1] = k_scale * new Vec2(.243f,.032f);
				LThighPoly.Vertices[2] = RThighPoly.Vertices[2] = k_scale * new Vec2(.318f,.343f);
				LThighPoly.Vertices[3] = RThighPoly.Vertices[3] = k_scale * new Vec2(.142f,.343f);
			}
			{	// pelvis
				PelvisPoly.VertexCount = 5;
				PelvisPoly.Vertices[0] = k_scale * new Vec2(.105f,.051f);
				PelvisPoly.Vertices[1] = k_scale * new Vec2(.277f,.053f);
				PelvisPoly.Vertices[2] = k_scale * new Vec2(.320f,.233f);
				PelvisPoly.Vertices[3] = k_scale * new Vec2(.112f,.233f);
				PelvisPoly.Vertices[4] = k_scale * new Vec2(.067f,.152f);
			}
			{	// stomach
				StomachPoly.VertexCount = 4;
				StomachPoly.Vertices[0] = k_scale * new Vec2(.088f,.043f);
				StomachPoly.Vertices[1] = k_scale * new Vec2(.284f,.043f);
				StomachPoly.Vertices[2] = k_scale * new Vec2(.295f,.231f);
				StomachPoly.Vertices[3] = k_scale * new Vec2(.100f,.231f);
			}
			{	// chest
				ChestPoly.VertexCount = 4;
				ChestPoly.Vertices[0] = k_scale * new Vec2(.091f,.042f);
				ChestPoly.Vertices[1] = k_scale * new Vec2(.283f,.042f);
				ChestPoly.Vertices[2] = k_scale * new Vec2(.177f,.289f);
				ChestPoly.Vertices[3] = k_scale * new Vec2(.065f,.289f);
			}
			{	// head
				HeadCirc.Radius = k_scale * .115f;
			}
			{	// neck
				NeckPoly.VertexCount = 4;
				NeckPoly.Vertices[0] = k_scale * new Vec2(.038f,.054f);
				NeckPoly.Vertices[1] = k_scale * new Vec2(.149f,.054f);
				NeckPoly.Vertices[2] = k_scale * new Vec2(.154f,.102f);
				NeckPoly.Vertices[3] = k_scale * new Vec2(.054f,.113f);
			}
			{	// upper arms
				LUpperArmPoly.VertexCount = RUpperArmPoly.VertexCount = 5;
				LUpperArmPoly.Vertices[0] = RUpperArmPoly.Vertices[0] = k_scale * new Vec2(.092f,.059f);
				LUpperArmPoly.Vertices[1] = RUpperArmPoly.Vertices[1] = k_scale * new Vec2(.159f,.059f);
				LUpperArmPoly.Vertices[2] = RUpperArmPoly.Vertices[2] = k_scale * new Vec2(.169f,.335f);
				LUpperArmPoly.Vertices[3] = RUpperArmPoly.Vertices[3] = k_scale * new Vec2(.078f,.335f);
				LUpperArmPoly.Vertices[4] = RUpperArmPoly.Vertices[4] = k_scale * new Vec2(.064f,.248f);
			}
			{	// forearms
				LForearmPoly.VertexCount = RForearmPoly.VertexCount = 4;
				LForearmPoly.Vertices[0] = RForearmPoly.Vertices[0] = k_scale * new Vec2(.082f,.054f);
				LForearmPoly.Vertices[1] = RForearmPoly.Vertices[1] = k_scale * new Vec2(.138f,.054f);
				LForearmPoly.Vertices[2] = RForearmPoly.Vertices[2] = k_scale * new Vec2(.149f,.296f);
				LForearmPoly.Vertices[3] = RForearmPoly.Vertices[3] = k_scale * new Vec2(.088f,.296f);
			}
			{	// hands
				LHandPoly.VertexCount = RHandPoly.VertexCount = 5;
				LHandPoly.Vertices[0] = RHandPoly.Vertices[0] = k_scale * new Vec2(.066f,.031f);
				LHandPoly.Vertices[1] = RHandPoly.Vertices[1] = k_scale * new Vec2(.123f,.020f);
				LHandPoly.Vertices[2] = RHandPoly.Vertices[2] = k_scale * new Vec2(.160f,.127f);
				LHandPoly.Vertices[3] = RHandPoly.Vertices[3] = k_scale * new Vec2(.127f,.178f);
				LHandPoly.Vertices[4] = RHandPoly.Vertices[4] = k_scale * new Vec2(.074f,.178f);
			}
		}

		public void DefaultJoints()
		{
			{	// ankles
				Vec2 anchor = k_scale * new Vec2(-.045f, -.75f);
				LAnkleDef.LocalAnchor1 = RAnkleDef.LocalAnchor1 = anchor - LFootDef.Position;
				LAnkleDef.LocalAnchor2 = RAnkleDef.LocalAnchor2 = anchor - LCalfDef.Position;
				LAnkleDef.ReferenceAngle = RAnkleDef.ReferenceAngle = 0.0f;
				LAnkleDef.LowerAngle = RAnkleDef.LowerAngle = -0.523598776f;
				LAnkleDef.UpperAngle = RAnkleDef.UpperAngle = 0.523598776f;
			}

			{	// knees
				Vec2 anchor = k_scale * new Vec2(-.030f, -.355f);
				LKneeDef.LocalAnchor1 = RKneeDef.LocalAnchor1 = anchor - LCalfDef.Position;
				LKneeDef.LocalAnchor2 = RKneeDef.LocalAnchor2 = anchor - LThighDef.Position;
				LKneeDef.ReferenceAngle = RKneeDef.ReferenceAngle = 0.0f;
				LKneeDef.LowerAngle = RKneeDef.LowerAngle = 0;
				LKneeDef.UpperAngle = RKneeDef.UpperAngle = 2.61799388f;
			}

			{	// hips
				Vec2 anchor = k_scale * new Vec2(.005f, -.045f);
				LHipDef.LocalAnchor1 = RHipDef.LocalAnchor1 = anchor - LThighDef.Position;
				LHipDef.LocalAnchor2 = RHipDef.LocalAnchor2 = anchor - PelvisDef.Position;
				LHipDef.ReferenceAngle = RHipDef.ReferenceAngle = 0.0f;
				LHipDef.LowerAngle = RHipDef.LowerAngle = -2.26892803f;
				LHipDef.UpperAngle = RHipDef.UpperAngle = 0;
			}

			{	// lower abs
				Vec2 anchor = k_scale * new Vec2(.035f, .135f);
				LowerAbsDef.LocalAnchor1 = anchor - PelvisDef.Position;
				LowerAbsDef.LocalAnchor2 = anchor - StomachDef.Position;
				LowerAbsDef.ReferenceAngle = 0.0f;
				LowerAbsDef.LowerAngle = -0.523598776f;
				LowerAbsDef.UpperAngle = 0.523598776f;
			}

			{	// upper abs
				Vec2 anchor = k_scale * new Vec2(.045f, .320f);
				UpperAbsDef.LocalAnchor1 = anchor - StomachDef.Position;
				UpperAbsDef.LocalAnchor2 = anchor - ChestDef.Position;
				UpperAbsDef.ReferenceAngle = 0.0f;
				UpperAbsDef.LowerAngle = -0.523598776f;
				UpperAbsDef.UpperAngle = 0.174532925f;
			}

			{	// lower neck
				Vec2 anchor = k_scale * new Vec2(-.015f, .575f);
				LowerNeckDef.LocalAnchor1 = anchor - ChestDef.Position;
				LowerNeckDef.LocalAnchor2 = anchor - NeckDef.Position;
				LowerNeckDef.ReferenceAngle = 0.0f;
				LowerNeckDef.LowerAngle = -0.174532925f;
				LowerNeckDef.UpperAngle = 0.174532925f;
			}

			{	// upper neck
				Vec2 anchor = k_scale * new Vec2(-.005f, .630f);
				UpperNeckDef.LocalAnchor1 = anchor - ChestDef.Position;
				UpperNeckDef.LocalAnchor2 = anchor - HeadDef.Position;
				UpperNeckDef.ReferenceAngle = 0.0f;
				UpperNeckDef.LowerAngle = -0.610865238f;
				UpperNeckDef.UpperAngle = 0.785398163f;
			}

			{	// shoulders
				Vec2 anchor = k_scale * new Vec2(-.015f, .545f);
				LShoulderDef.LocalAnchor1 = RShoulderDef.LocalAnchor1 = anchor - ChestDef.Position;
				LShoulderDef.LocalAnchor2 = RShoulderDef.LocalAnchor2 = anchor - LUpperArmDef.Position;
				LShoulderDef.ReferenceAngle = RShoulderDef.ReferenceAngle = 0.0f;
				LShoulderDef.LowerAngle = RShoulderDef.LowerAngle = -1.04719755f;
				LShoulderDef.UpperAngle = RShoulderDef.UpperAngle = 3.14159265f;
			}

			{	// elbows
				Vec2 anchor = k_scale * new Vec2(-.005f, .290f);
				LElbowDef.LocalAnchor1 = RElbowDef.LocalAnchor1 = anchor - LForearmDef.Position;
				LElbowDef.LocalAnchor2 = RElbowDef.LocalAnchor2 = anchor - LUpperArmDef.Position;
				LElbowDef.ReferenceAngle = RElbowDef.ReferenceAngle = 0.0f;
				LElbowDef.LowerAngle = RElbowDef.LowerAngle = -2.7925268f;
				LElbowDef.UpperAngle = RElbowDef.UpperAngle = 0;
			}

			{	// wrists
				Vec2 anchor = k_scale * new Vec2(-.010f, .045f);
				LWristDef.LocalAnchor1 = RWristDef.LocalAnchor1 = anchor - LHandDef.Position;
				LWristDef.LocalAnchor2 = RWristDef.LocalAnchor2 = anchor - LForearmDef.Position;
				LWristDef.ReferenceAngle = RWristDef.ReferenceAngle = 0.0f;
				LWristDef.LowerAngle = RWristDef.LowerAngle = -0.174532925f;
				LWristDef.UpperAngle = RWristDef.UpperAngle = 0.174532925f;
			}
		}

		public void DefaultPositions()
		{
			LFootDef.Position		= RFootDef.Position			= k_scale * new Vec2(-.122f,-.901f);
			LCalfDef.Position		= RCalfDef.Position			= k_scale * new Vec2(-.177f,-.771f);
			LThighDef.Position		= RThighDef.Position		= k_scale * new Vec2(-.217f,-.391f);
			LUpperArmDef.Position	= RUpperArmDef.Position		= k_scale * new Vec2(-.127f,.228f);
			LForearmDef.Position	= RForearmDef.Position		= k_scale * new Vec2(-.117f,-.011f);
			LHandDef.Position		= RHandDef.Position			= k_scale * new Vec2(-.112f,-.136f);
			PelvisDef0.Position									= k_scale * new Vec2(-.177f, -.101f);
			PelvisDef.Position									= k_scale * new Vec2(-.177f,-.101f);
			StomachDef.Position									= k_scale * new Vec2(-.142f,.088f);
			ChestDef.Position									= k_scale * new Vec2(-.132f,.282f);
			NeckDef.Position									= k_scale * new Vec2(-.102f,.518f);
			HeadDef.Position									= k_scale * new Vec2(.022f,.738f);
		}
	}
}
