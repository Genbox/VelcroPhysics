﻿/*
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

namespace Box2DX.Dynamics
{
	/// <summary>
	/// A body definition holds all the data needed to construct a rigid body.
	/// You can safely re-use body definitions.
	/// </summary>
	public class BodyDef
	{
		/// <summary>
		/// This constructor sets the body definition default values.
		/// </summary>
		public BodyDef()
		{
			MassData = new MassData();
			MassData.Center.SetZero();
			MassData.Mass = 0.0f;
			MassData.I = 0.0f;
			UserData = null;
			Position = new Vec2();
			Position.Set(0.0f, 0.0f);
			Angle = 0.0f;
			LinearDamping = 0.0f;
			AngularDamping = 0.0f;
			AllowSleep = true;
			IsSleeping = false;
			FixedRotation = false;
			IsBullet = false;
		}

		/// <summary>
		/// You can use this to initialized the mass properties of the body.
		/// If you prefer, you can set the mass properties after the shapes
		/// have been added using Body.SetMassFromShapes.
		/// </summary>
		public MassData MassData;

		/// <summary>
		/// Use this to store application specific body data.
		/// </summary>
		public object UserData;

		/// <summary>
		/// The world position of the body. Avoid creating bodies at the origin
		/// since this can lead to many overlapping shapes.
		/// </summary>
		public Vec2 Position;

		/// <summary>
		/// The world angle of the body in radians.
		/// </summary>
		public float Angle;

		/// <summary>
		/// Linear damping is use to reduce the linear velocity. The damping parameter
		/// can be larger than 1.0f but the damping effect becomes sensitive to the
		/// time step when the damping parameter is large.
		/// </summary>
		public float LinearDamping;

		/// <summary>
		/// Angular damping is use to reduce the angular velocity. The damping parameter
		/// can be larger than 1.0f but the damping effect becomes sensitive to the
		/// time step when the damping parameter is large.
		/// </summary>
		public float AngularDamping;

		/// <summary>
		/// Set this flag to false if this body should never fall asleep. Note that
		/// this increases CPU usage.
		/// </summary>
		public bool AllowSleep;

		/// <summary>
		/// Is this body initially sleeping?
		/// </summary>
		public bool IsSleeping;

		/// <summary>
		/// Should this body be prevented from rotating? Useful for characters.
		/// </summary>
		public bool FixedRotation;

		/// <summary>
		/// Is this a fast moving body that should be prevented from tunneling through
		/// other moving bodies? Note that all bodies are prevented from tunneling through
		/// static bodies.
		/// @warning You should use this flag sparingly since it increases processing time.
		/// </summary>
		public bool IsBullet;
	}

	/// <summary>
	/// A rigid body.
	/// </summary>
	public class Body : IDisposable
	{

		[Flags]
		public enum BodyFlags
		{
			Frozen = 0x0002,
			Island = 0x0004,
			Sleep = 0x0008,
			AllowSleep = 0x0010,
			Bullet = 0x0020,
			FixedRotation = 0x0040
		}

		public enum BodyType
		{
			Static,
			Dynamic,
			MaxTypes
		}

		internal BodyFlags _flags;
		private BodyType _type;

		internal int _islandIndex;

		private XForm _xf;		// the body origin transform

		internal Sweep _sweep;	// the swept motion for CCD

		internal Vec2 _linearVelocity;
		internal float _angularVelocity;

		internal Vec2 _force;
		internal float _torque;

		private World _world;
		internal Body _prev;
		internal Body _next;

		internal Shape _shapeList;
		internal int _shapeCount;

		internal JointEdge _jointList;
		internal ContactEdge _contactList;

		internal Box2DX.Dynamics.Controllers.ControllerEdge _controllerList;

		internal float _mass;
		internal float _invMass;
		private float _I;
		internal float _invI;

		internal float _linearDamping;
		internal float _angularDamping;

		internal float _sleepTime;

		private object _userData;

		internal Body(BodyDef bd, World world)
		{
			Box2DXDebug.Assert(world._lock == false);

			_flags = 0;

			if (bd.IsBullet)
			{
				_flags |= BodyFlags.Bullet;
			}
			if (bd.FixedRotation)
			{
				_flags |= BodyFlags.FixedRotation;
			}
			if (bd.AllowSleep)
			{
				_flags |= BodyFlags.AllowSleep;
			}
			if (bd.IsSleeping)
			{
				_flags |= BodyFlags.Sleep;
			}

			_world = world;

			_xf.Position = bd.Position;
			_xf.R.Set(bd.Angle);

			_sweep.LocalCenter = bd.MassData.Center;
			_sweep.T0 = 1.0f;
			_sweep.A0 = _sweep.A = bd.Angle;
			_sweep.C0 = _sweep.C = Common.Math.Mul(_xf, _sweep.LocalCenter);

			_jointList = null;
			_contactList = null;
			_prev = null;
			_next = null;

			_linearDamping = bd.LinearDamping;
			_angularDamping = bd.AngularDamping;

			_force.Set(0.0f, 0.0f);
			_torque = 0.0f;

			_linearVelocity.SetZero();
			_angularVelocity = 0.0f;

			_sleepTime = 0.0f;

			_invMass = 0.0f;
			_I = 0.0f;
			_invI = 0.0f;

			_mass = bd.MassData.Mass;

			if (_mass > 0.0f)
			{
				_invMass = 1.0f / _mass;
			}

			if ((_flags & BodyFlags.FixedRotation) == 0)
			{
				_I = bd.MassData.I;
			}

			if (_I > 0.0f)
			{
				_invI = 1.0f / _I;
			}

			if (_invMass == 0.0f && _invI == 0.0f)
			{
				_type = BodyType.Static;
			}
			else
			{
				_type = BodyType.Dynamic;
			}

			_userData = bd.UserData;

			_shapeList = null;
			_shapeCount = 0;
		}

		public void Dispose()
		{
			Box2DXDebug.Assert(_world._lock == false);
			// shapes and joints are destroyed in World.Destroy
		}

		/// <summary>
		/// Creates a shape and attach it to this body.
		/// @warning This function is locked during callbacks.
		/// </summary>
		/// <param name="shapeDef">The shape definition.</param>
		/// <returns></returns>
		public Shape CreateShape(ShapeDef shapeDef)
		{
			Box2DXDebug.Assert(_world._lock == false);
			if (_world._lock == true)
			{
				return null;
			}

			Shape s = Shape.Create(shapeDef);

			s._next = _shapeList;
			_shapeList = s;
			++_shapeCount;

			s._body = this;

			// Add the shape to the world's broad-phase.
			s.CreateProxy(_world._broadPhase, _xf);

			// Compute the sweep radius for CCD.
			s.UpdateSweepRadius(_sweep.LocalCenter);

			return s;
		}

		/// <summary>
		/// Destroy a shape. This removes the shape from the broad-phase and
		/// therefore destroys any contacts associated with this shape. All shapes
		/// attached to a body are implicitly destroyed when the body is destroyed.
		/// @warning This function is locked during callbacks.
		/// </summary>
		/// <param name="shape">The shape to be removed.</param>
		public void DestroyShape(Shape shape)
		{
			Box2DXDebug.Assert(_world._lock == false);
			if (_world._lock == true)
			{
				return;
			}

			Box2DXDebug.Assert(shape.GetBody() == this);
			shape.DestroyProxy(_world._broadPhase);

			Box2DXDebug.Assert(_shapeCount > 0);
			Shape node = _shapeList;
			bool found = false;
			while (node != null)
			{
				if (node == shape)
				{
					_shapeList = shape._next;
					found = true;
					break;
				}

				node = node._next;
			}

			// You tried to remove a shape that is not attached to this body.
			Box2DXDebug.Assert(found);

			shape._body = null;
			shape._next = null;

			--_shapeCount;

			Shape.Destroy(shape);
		}

		// TODO_ERIN adjust linear velocity and torque to account for movement of center.
		/// <summary>
		/// Set the mass properties. Note that this changes the center of mass position.
		/// If you are not sure how to compute mass properties, use SetMassFromShapes.
		/// The inertia tensor is assumed to be relative to the center of mass.
		/// @param massData the mass properties.
		/// </summary>
		/// <param name="massData"></param>
		public void SetMass(MassData massData)
		{
			Box2DXDebug.Assert(_world._lock == false);
			if (_world._lock == true)
			{
				return;
			}

			_invMass = 0.0f;
			_I = 0.0f;
			_invI = 0.0f;

			_mass = massData.Mass;

			if (_mass > 0.0f)
			{
				_invMass = 1.0f / _mass;
			}

			if ((_flags & BodyFlags.FixedRotation) == 0)
			{
				_I = massData.I;
			}

			if (_I > 0.0f)
			{
				_invI = 1.0f /_I;
			}

			// Move center of mass.
			_sweep.LocalCenter = massData.Center;
			_sweep.C0 = _sweep.C = Common.Math.Mul(_xf, _sweep.LocalCenter);

			// Update the sweep radii of all child shapes.
			for (Shape s = _shapeList; s != null; s = s._next)
			{
				s.UpdateSweepRadius(_sweep.LocalCenter);
			}

			BodyType oldType = _type;
			if (_invMass == 0.0f && _invI == 0.0f)
			{
				_type = BodyType.Static;
			}
			else
			{
				_type = BodyType.Dynamic;
			}

			// If the body type changed, we need to refilter the broad-phase proxies.
			if (oldType != _type)
			{
				for (Shape s = _shapeList; s!=null; s = s._next)
				{
					s.RefilterProxy(_world._broadPhase, _xf);
				}
			}
		}

		// TODO_ERIN adjust linear velocity and torque to account for movement of center.
		/// <summary>
		/// Compute the mass properties from the attached shapes. You typically call this
		/// after adding all the shapes. If you add or remove shapes later, you may want
		/// to call this again. Note that this changes the center of mass position.
		/// </summary>
		public void SetMassFromShapes()
		{
			Box2DXDebug.Assert(_world._lock == false);
			if (_world._lock == true)
			{
				return;
			}

			// Compute mass data from shapes. Each shape has its own density.
			_mass = 0.0f;
			_invMass = 0.0f;
			_I = 0.0f;
			_invI = 0.0f;

			Vec2 center = Vec2.Zero;
			for (Shape s = _shapeList; s!=null; s = s._next)
			{
				MassData massData;
				s.ComputeMass(out massData);
				_mass += massData.Mass;
				center += massData.Mass * massData.Center;
				_I += massData.I;
			}

			// Compute center of mass, and shift the origin to the COM.
			if (_mass > 0.0f)
			{
				_invMass = 1.0f / _mass;
				center *= _invMass;
			}

			if (_I > 0.0f && (_flags & BodyFlags.FixedRotation) == 0)
			{
				// Center the inertia about the center of mass.
				_I -= _mass * Vec2.Dot(center, center);
				Box2DXDebug.Assert(_I > 0.0f);
				_invI = 1.0f / _I;
			}
			else
			{
				_I = 0.0f;
				_invI = 0.0f;
			}

			// Move center of mass.
			_sweep.LocalCenter = center;
			_sweep.C0 = _sweep.C = Common.Math.Mul(_xf, _sweep.LocalCenter);

			// Update the sweep radii of all child shapes.
			for (Shape s = _shapeList; s != null; s = s._next)
			{
				s.UpdateSweepRadius(_sweep.LocalCenter);
			}

			BodyType oldType = _type;
			if (_invMass == 0.0f && _invI == 0.0f)
			{
				_type = BodyType.Static;
			}
			else
			{
				_type = BodyType.Dynamic;
			}

			// If the body type changed, we need to refilter the broad-phase proxies.
			if (oldType != _type)
			{
				for (Shape s = _shapeList; s!=null; s = s._next)
				{
					s.RefilterProxy(_world._broadPhase, _xf);
				}
			}
		}

		/// <summary>
		/// Set the position of the body's origin and rotation (radians).
		/// This breaks any contacts and wakes the other bodies.
		/// </summary>
		/// <param name="position">The new world position of the body's origin (not necessarily
		/// the center of mass).</param>
		/// <param name="angle">The new world rotation angle of the body in radians.</param>
		/// <returns>Return false if the movement put a shape outside the world. In this case the
		/// body is automatically frozen.</returns>
		public bool SetXForm(Vec2 position, float angle)
		{
			Box2DXDebug.Assert(_world._lock == false);
			if (_world._lock == true)
			{
				return true;
			}

			if (IsFrozen())
			{
				return false;
			}

			_xf.R.Set(angle);
			_xf.Position = position;

			_sweep.C0 = _sweep.C = Common.Math.Mul(_xf, _sweep.LocalCenter);
			_sweep.A0 = _sweep.A = angle;

			bool freeze = false;
			for (Shape s = _shapeList; s != null; s = s._next)
			{
				bool inRange = s.Synchronize(_world._broadPhase, _xf, _xf);

				if (inRange == false)
				{
					freeze = true;
					break;
				}
			}

			if (freeze == true)
			{
				_flags |= BodyFlags.Frozen;
				_linearVelocity.SetZero();
				_angularVelocity = 0.0f;
				for (Shape s = _shapeList; s != null; s = s._next)
				{
					s.DestroyProxy(_world._broadPhase);
				}

				// Failure
				return false;
			}

			// Success
			_world._broadPhase.Commit();
			return true;
		}

		/// <summary>
		/// Get the body transform for the body's origin.
		/// </summary>
		/// <returns>Return the world transform of the body's origin.</returns>
		public XForm GetXForm()
		{
			return _xf;
		}

		/// <summary>
		/// Get the world body origin position.
		/// </summary>
		/// <returns>Return the world position of the body's origin.</returns>
		public Vec2 GetPosition()
		{
			return _xf.Position;
		}

		/// <summary>
		/// Get the angle in radians.
		/// </summary>
		/// <returns>Return the current world rotation angle in radians.</returns>
		public float GetAngle()
		{
			return _sweep.A;
		}

		/// <summary>
		/// Get the world position of the center of mass.
		/// </summary>
		/// <returns></returns>
		public Vec2 GetWorldCenter()
		{
			return _sweep.C;
		}

		/// <summary>
		/// Get the local position of the center of mass.
		/// </summary>
		/// <returns></returns>
		public Vec2 GetLocalCenter()
		{
			return _sweep.LocalCenter;
		}

		/// <summary>
		/// Set the linear velocity of the center of mass.
		/// </summary>
		/// <param name="v">The new linear velocity of the center of mass.</param>
		public void SetLinearVelocity(Vec2 v)
		{
			_linearVelocity = v;
		}

		/// <summary>
		/// Get the linear velocity of the center of mass.
		/// </summary>
		/// <returns>Return the linear velocity of the center of mass.</returns>
		public Vec2 GetLinearVelocity()
		{
			return _linearVelocity;
		}

		/// <summary>
		/// Set the angular velocity.
		/// </summary>
		/// <param name="omega">The new angular velocity in radians/second.</param>
		public void SetAngularVelocity(float omega)
		{
			_angularVelocity = omega;
		}

		/// <summary>
		/// Get the angular velocity.
		/// </summary>
		/// <returns>Return the angular velocity in radians/second.</returns>
		public float GetAngularVelocity()
		{
			return _angularVelocity;
		}

		/// <summary>
		/// Apply a force at a world point. If the force is not
		/// applied at the center of mass, it will generate a torque and
		/// affect the angular velocity. This wakes up the body.
		/// </summary>
		/// <param name="force">The world force vector, usually in Newtons (N).</param>
		/// <param name="point">The world position of the point of application.</param>
		public void ApplyForce(Vec2 force, Vec2 point)
		{
			if (IsSleeping())
			{
				WakeUp();
			}
			_force += force;
			_torque += Vec2.Cross(point - _sweep.C, force);
		}

		/// <summary>
		/// Apply a torque. This affects the angular velocity
		/// without affecting the linear velocity of the center of mass.
		/// This wakes up the body.
		/// </summary>
		/// <param name="torque">About the z-axis (out of the screen), usually in N-m.</param>
		public void ApplyTorque(float torque)
		{
			if (IsSleeping())
			{
				WakeUp();
			}
			_torque += torque;
		}

		/// <summary>
		/// Apply an impulse at a point. This immediately modifies the velocity.
		/// It also modifies the angular velocity if the point of application
		/// is not at the center of mass. This wakes up the body.
		/// </summary>
		/// <param name="impulse">The world impulse vector, usually in N-seconds or kg-m/s.</param>
		/// <param name="point">The world position of the point of application.</param>
		public void ApplyImpulse(Vec2 impulse, Vec2 point)
		{
			if (IsSleeping())
			{
				WakeUp();
			}
			_linearVelocity += _invMass * impulse;
			_angularVelocity += _invI * Vec2.Cross(point - _sweep.C, impulse);
		}

		/// <summary>
		/// Get the total mass of the body.
		/// </summary>
		/// <returns>Return the mass, usually in kilograms (kg).</returns>
		public float GetMass()
		{
			return _mass;
		}

		/// <summary>
		/// Get the central rotational inertia of the body.
		/// </summary>
		/// <returns>Return the rotational inertia, usually in kg-m^2.</returns>
		public float GetInertia()
		{
			return _I;
		}

		/// <summary>
		/// Get the world coordinates of a point given the local coordinates.
		/// </summary>
		/// <param name="localPoint">A point on the body measured relative the the body's origin.</param>
		/// <returns>Return the same point expressed in world coordinates.</returns>
		public Vec2 GetWorldPoint(Vec2 localPoint)
		{
			return Common.Math.Mul(_xf, localPoint);
		}

		/// <summary>
		/// Get the world coordinates of a vector given the local coordinates.
		/// </summary>
		/// <param name="localVector">A vector fixed in the body.</param>
		/// <returns>Return the same vector expressed in world coordinates.</returns>
		public Vec2 GetWorldVector(Vec2 localVector)
		{
			return Common.Math.Mul(_xf.R, localVector);
		}

		/// <summary>
		/// Gets a local point relative to the body's origin given a world point.
		/// </summary>
		/// <param name="worldPoint">A point in world coordinates.</param>
		/// <returns>Return the corresponding local point relative to the body's origin.</returns>
		public Vec2 GetLocalPoint(Vec2 worldPoint)
		{
			return Common.Math.MulT(_xf, worldPoint);
		}

		/// <summary>
		/// Gets a local vector given a world vector.
		/// </summary>
		/// <param name="worldVector">A vector in world coordinates.</param>
		/// <returns>Return the corresponding local vector.</returns>
		public Vec2 GetLocalVector(Vec2 worldVector)
		{
			return Common.Math.MulT(_xf.R, worldVector);
		}

		/// <summary>
		/// Get the world linear velocity of a world point attached to this body.
		/// </summary>
		/// <param name="worldPoint">A point in world coordinates.</param>
		/// <returns>The world velocity of a point.</returns>
		public Vec2 GetLinearVelocityFromWorldPoint(Vec2 worldPoint)
		{
			return _linearVelocity + Vec2.Cross(_angularVelocity, worldPoint - _sweep.C);
		}

		/// <summary>
		/// Get the world velocity of a local point.
		/// </summary>
		/// <param name="localPoint">A point in local coordinates.</param>
		/// <returns>The world velocity of a point.</returns>
		public Vec2 GetLinearVelocityFromLocalPoint(Vec2 localPoint)
		{
			return GetLinearVelocityFromWorldPoint(GetWorldPoint(localPoint));
		}

		/// <summary>
		/// Is this body treated like a bullet for continuous collision detection?
		/// </summary>
		/// <returns></returns>
		public bool IsBullet()
		{
			return (_flags & BodyFlags.Bullet) == BodyFlags.Bullet;
		}

		/// <summary>
		/// Should this body be treated like a bullet for continuous collision detection?
		/// </summary>
		/// <param name="flag"></param>
		public void SetBullet(bool flag)
		{
			if (flag)
			{
				_flags |= BodyFlags.Bullet;
			}
			else
			{
				_flags &= ~BodyFlags.Bullet;
			}
		}

		/// <summary>
		/// Is this body static (immovable)?
		/// </summary>
		/// <returns></returns>
		public bool IsStatic()
		{
			return _type == BodyType.Static;
		}

		/// <summary>
		/// Is this body dynamic (movable)?
		/// </summary>
		/// <returns></returns>
		public bool IsDynamic()
		{
			return _type == BodyType.Dynamic;
		}

		/// <summary>
		/// Is this body frozen?
		/// </summary>
		/// <returns></returns>
		public bool IsFrozen()
		{
			return (_flags & BodyFlags.Frozen) == BodyFlags.Frozen;
		}

		/// <summary>
		/// Is this body sleeping (not simulating).
		/// </summary>
		/// <returns></returns>
		public bool IsSleeping()
		{
			return (_flags & BodyFlags.Sleep) == BodyFlags.Sleep;
		}

		/// <summary>
		/// You can disable sleeping on this body.
		/// </summary>
		/// <param name="flag"></param>
		public void AllowSleeping(bool flag)
		{
			if (flag)
			{
				_flags |= BodyFlags.AllowSleep;
			}
			else
			{
				_flags &= ~BodyFlags.AllowSleep;
				WakeUp();
			}
		}

		/// <summary>
		/// Wake up this body so it will begin simulating.
		/// </summary>
		public void WakeUp()
		{
			_flags &= ~BodyFlags.Sleep;
			_sleepTime = 0.0f;
		}

		/// <summary>
		/// Put this body to sleep so it will stop simulating.
		/// This also sets the velocity to zero.
		/// </summary>
		public void PutToSleep()
		{
			_flags |= BodyFlags.Sleep;
			_sleepTime = 0.0f;
			_linearVelocity.SetZero();
			_angularVelocity = 0.0f;
			_force.SetZero();
			_torque = 0.0f;
		}

		/// <summary>
		/// Get the list of all shapes attached to this body.
		/// </summary>
		/// <returns></returns>
		public Shape GetShapeList()
		{
			return _shapeList;
		}

		/// <summary>
		/// Get the list of all joints attached to this body.
		/// </summary>
		/// <returns></returns>
		public JointEdge GetJointList()
		{
			return _jointList;
		}


		/// <summary>
		/// Get the next body in the world's body list.
		/// </summary>
		/// <returns></returns>
		public Body GetNext()
		{
			return _next;
		}

		/// <summary>
		/// Get the user data pointer that was provided in the body definition.
		/// </summary>
		/// <returns></returns>
		public object GetUserData()
		{
			return _userData;
		}

		/// <summary>
		/// Set the user data. Use this to store your application specific data.
		/// </summary>
		/// <param name="data"></param>
		public void SetUserData(object data) { _userData = data; }

		/// <summary>
		/// Get the parent world of this body.
		/// </summary>
		/// <returns></returns>
		public World GetWorld() { return _world; }

		internal bool SynchronizeShapes()
		{
			XForm xf1 = new XForm();
			xf1.R.Set(_sweep.A0);
			xf1.Position = _sweep.C0 - Common.Math.Mul(xf1.R, _sweep.LocalCenter);

			bool inRange = true;
			for (Shape s = _shapeList; s != null; s = s._next)
			{
				inRange = s.Synchronize(_world._broadPhase, xf1, _xf);
				if (inRange == false)
				{
					break;
				}
			}

			if (inRange == false)
			{
				_flags |= BodyFlags.Frozen;
				_linearVelocity.SetZero();
				_angularVelocity = 0.0f;
				for (Shape s = _shapeList; s != null; s = s._next)
				{
					s.DestroyProxy(_world._broadPhase);
				}

				// Failure
				return false;
			}

			// Success
			return true;
		}

		internal void SynchronizeTransform()
		{
			_xf.R.Set(_sweep.A);
			_xf.Position = _sweep.C - Common.Math.Mul(_xf.R, _sweep.LocalCenter);
		}

		// This is used to prevent connected bodies from colliding.
		// It may lie, depending on the collideConnected flag.
		internal bool IsConnected(Body other)
		{
			for (JointEdge jn = _jointList; jn != null; jn = jn.Next)
			{
				if (jn.Other == other)
					return jn.Joint._collideConnected == false;
			}

			return false;
		}

		internal void Advance(float t)
		{
			// Advance to the new safe time.
			_sweep.Advance(t);
			_sweep.C = _sweep.C0;
			_sweep.A = _sweep.A0;
			SynchronizeTransform();
		}
	}
}
