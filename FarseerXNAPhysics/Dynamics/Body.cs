using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using Microsoft.Xna.Framework;

using FarseerGames.FarseerXNAPhysics.Mathematics;

namespace FarseerGames.FarseerXNAPhysics.Dynamics {
    /// <summary>
    /// <para>
    /// Body is at the core of the Rigid Body System.  It is a <see cref="RigidBody">RigidBody</see>
    /// without the geometry.</para>
    /// <para>
    /// The Body handles all the physics of motion: position, velocity, acceleration
    /// forces, torques, etc...</para>
    ///<list type="number">
    ///<listheader>
    /// The Body is integrated every timestep (which should be fixed) by the <see cref="PhysicsSimulator">PhysicsSimulator</see> in the following manner:</listheader> 
    /// <item>-Set forces and torques (gravity, springs, user input...)</item>
    /// <item>-Apply forces and torques (updating velocity only)</item>
    /// <item>-Update positions and orientations</item>
    ///</list>
    /// <para>In technical terms, this is known as Symplectic Euler Integration because
    /// velocity is updated prior to position (The reverse of simple Euler Integration)</para>
    /// </summary>
    public class Body : IIsDisposable  {
        private float _mass = 1;
        private float _inverseMass = 1;
        private float _momentOfInertia = 1; //1 unit square ;
        private float _inverseMomentOfInertia = 1;
        private Vector2 _position = Vector2.Zero;
        private float _orientation = 0;
        private Vector2 _linearVelocity = Vector2.Zero;
        private float _angularVelocity = 0;
        private Vector2 _previousPosition = Vector2.Zero;
        private float _previousOrientation = 0;
        private Vector2 _previousLinearVelocity = Vector2.Zero;
        private float _previousAngularVelocity = 0;
        private Vector2 _linearVelocityBias = Vector2.Zero;
        private float _angularVelocityBias = 0;
        private Vector2 _force = Vector2.Zero;
        //private Vector2 _constantForce = Vector2.Zero;
        private float _torque = 0;
        private float _restitutionCoefficient = 0f;
        private float _frictionCoefficient = 0f;
        private float _linearDragCoefficient = 0f;
        private float _linearDragVelocityThreshhold = .000000001f;
        private float _rotationalDragCoefficient = .1f;

        private bool _isStatic = false;

        /// <summary>
        /// The mass of the Body
        /// </summary>
        public float Mass {
            get { return _mass; }
            set {
                if (value == 0) { throw new Exception("Mass cannot be 0"); }
                _mass = value;
                if (_isStatic) {
                    _inverseMass = 0;
                }
                else {
                    _inverseMass = 1f / value;
                }
            }
        }

        /// <summary>
        /// The inverse of the mass of the body (1/Mass)
        /// </summary>
        public float InverseMass {
            get { return _inverseMass; }
        }

        /// <summary>
        /// The moment of inertia of the body. 
        /// <para>The moment of intertia of a body in 2d is a scalar value that represents how
        /// difficult (or not difficult) it is to rotate a body about the center of mass.</para>
        /// <para>The moment of inertia is varies by the shape of the body.  For basic shapes like
        /// circles and rectangles, forumulas exist for computing the moment of interia based on
        /// the shape properties (radius of the circle, or length and width of the rectangle)</para>
        /// <para>For bodies that are not basic, it is usually good enough to estimate the moment of
        /// intertia by picking a basic shape that is close to the same shape. It is also possible
        /// using more advance calculus techniques to compute the actual moment of intertia of 
        /// non-basic shapes.</para>
        /// </summary>
        public float MomentOfInertia {
            get { return _momentOfInertia; }
            set {
                if (value == 0) { throw new Exception("Mass cannot be 0"); }
                _momentOfInertia = value;
                if (_isStatic) {
                    _inverseMomentOfInertia = 0;
                }
                else {
                    _inverseMomentOfInertia = 1f / value;
                }
            }
        }

        /// <summary>
        /// The inverse of the moment of inertia of the body (1/MomentOfInertia)
        /// </summary>
        public float InverseMomentOfInertia {
            get { return _inverseMomentOfInertia; }
        }

        /// <summary>
        /// Indicates this body is fixed within the world and will not move no matter what forces are applied.
        /// <para>Bodies that represent land or world borders are a good examples of static bodies.</para>
        /// </summary>
        public bool IsStatic {
            get { return _isStatic; }
            set {
                _isStatic = value;
                if (_isStatic) {
                    _inverseMass = 0;
                    _inverseMomentOfInertia = 0;
                }
                else {
                    _inverseMass = 1f / _mass;
                    _inverseMomentOfInertia = 1f / _momentOfInertia;
                }
            }
        }

        /// <summary>
        /// The coefficient of restitution of the body.
        /// <para>This parameter controls how bouncy an object is when it collides with other
        /// bodies. Valid values range from 0 to 1 inclusive.  1 implies 100% restitution (think of a really good bouncing)
        /// 0 implies no restitution (think a ball of clay)</para>
        /// <para>NOTE: In this engine, he RestitutionCoefficient is not the only thing that can cause "bounce"
        /// Because of the way collision response is handle (allowing some penetration between objects) fast moving
        /// objects may bounce even when their RestitutionCoefficent is set to zero. Hopefully in future versions this
        /// can be fixed or at least minamized</para>
        /// </summary>
        public float RestitutionCoefficient {
            get { return _restitutionCoefficient; }
            set { _restitutionCoefficient = value; }
        }	

        /// <summary>
        /// Controls the amount of friction a body has when in contact with another body. A value of zero implies
        /// no friction.
        /// </summary>
        public float FrictionCoefficient {
            get { return _frictionCoefficient; }
            set { _frictionCoefficient = value; }
        }

        /// <summary>
        /// Linear drag can be thought of as "air drag". The LinearDragCoefficient controls how much linear (non-rotational) drag
        /// a body is subject to. 
        /// <para>Linear drag causes a force to be applied to the body always in the direction opposite its velocity vector.
        /// The magnitude of this force is the speed of the body squared multiplied by its LinearDragCoefficent. 
        /// <c>force = velocity*velocity*LinearDragCoeficcent</c></para>
        /// </summary>
        public float LinearDragCoefficient {
            get { return _linearDragCoefficient ; }
            set { _linearDragCoefficient  = value; }
        }

        public float RotationalDragCoefficient {
            get { return _rotationalDragCoefficient; }
            set { _rotationalDragCoefficient = value; }
        }	

        public virtual Vector2 Position {
            get { return _position; }
            set { _position = value; }
        }

        public virtual float Orientation {
            get { return _orientation; }
            set {
                _orientation = value;
                if (_orientation > Calculator.PiX2) {
                    _orientation = _orientation % Calculator.PiX2;
                }
                if (_orientation < -Calculator.PiX2) {
                    _orientation = _orientation % Calculator.PiX2;
                }
            }
        }

        public Vector2 LinearVelocity {
            get { return _linearVelocity; }
            set { _linearVelocity = value; }
        }

        public Vector2 LinearVelocityBias {
            get { return _linearVelocityBias; }
            set { _linearVelocityBias = value; }
        }

        public float AngularVelocity {
            get { return _angularVelocity; }
            set { _angularVelocity = value; }
        }

        public float AngularVelocityBias {
            get { return _angularVelocityBias ; }
            set { _angularVelocityBias  = value; }
        }	

        public Vector2 Force {
            get { return _force; }
        }

        public float Torque {
            get { return _torque; }
        }

        public Vector2 Acceleration {
            get {
                return Vector2.Multiply(_force, 1f / _mass);
            }
        }

        public float AngularAcceleration {
            get { return _torque / _momentOfInertia; }
        }

        //public Vector2 ConstantForce {
        //    get { return _constantForce; }
        //    set { _constantForce = value; }
        //}

        public Matrix BodyMatrix {
            get {
                Matrix translationMatrix = Matrix.CreateTranslation(_position.X, _position.Y, 0);
                Matrix rotationMatrix = Matrix.CreateRotationZ(_orientation);
                return Matrix.Multiply(rotationMatrix,translationMatrix);
            }
        }

        public Matrix BodyRotationMatrix {
            get {
                Matrix rotationMatrix = Matrix.CreateRotationZ(_orientation);
                return rotationMatrix;
            }
        }

        public Vector2 UnitWorldXDirection{
            get {
                Matrix bodyMatrix = BodyMatrix;
                return new Vector2(bodyMatrix.Right.X, bodyMatrix.Right.Y); 
            }
        }

        public Vector2 UnitWorldYDirection {
            get {
                Matrix bodyMatrix = BodyMatrix;
                return new Vector2(bodyMatrix.Up.X, bodyMatrix.Up.Y);
            }
        
        }

        public Vector2 GetWorldPosition(Vector2 localPosition) {
            Vector2 retVector =  Vector2.Transform(localPosition, BodyMatrix);
            return retVector;
        }

        public Vector2 GetLocalPosition(Vector2 worldPosition) {
            Matrix rotationMatrix = BodyRotationMatrix;
            rotationMatrix = Matrix.Transpose(rotationMatrix);
            Vector2 localPosition = worldPosition - Position;
            localPosition = Vector2.Transform(localPosition, rotationMatrix);
            return localPosition;
        }

        public Vector2 GetVelocityAtPoint(Vector2 localPoint) {
            Vector2 velocity = LinearVelocity + AngularVelocity * (GetWorldPosition(localPoint) - Position);
            return velocity;
        }

        private void ApplyDrag() {
            float speed = _linearVelocity.Length();
            if (speed > _linearDragVelocityThreshhold) {
                Vector2 dragDirection = _linearVelocity;
                dragDirection.Normalize();
                dragDirection = -dragDirection;

                Vector2 linearDrag = _linearVelocity * _linearVelocity;
                linearDrag = Vector2.Multiply(linearDrag, _linearDragCoefficient * dragDirection);
                ApplyForce(linearDrag);
            }
            float rotationalDrag = _angularVelocity * _angularVelocity * Math.Sign(_angularVelocity);

            rotationalDrag *= -_rotationalDragCoefficient;
            ApplyTorque(rotationalDrag);
        }

        public virtual void IntegrateVelocity(float dt) {
            if (_isStatic) { return; }
            Vector2 dv; //change in linear velocity
            float dw; //change in angular velocity 
            //linear
            ApplyDrag();

            dv = Vector2.Multiply(this.Acceleration, dt);
            _previousLinearVelocity = _linearVelocity;
            _linearVelocity = Vector2.Add(_previousLinearVelocity, dv);

            //angular
            dw = AngularAcceleration * dt;
            _previousAngularVelocity = _angularVelocity;
            _angularVelocity = _previousAngularVelocity += dw;
        }

        public virtual void IntegratePosition(float dt) {
            if (_isStatic) { return; }
            Vector2 dp;
            float orientationChange;
            //linear

            Vector2 linearVelocity = Vector2.Add(_linearVelocity, _linearVelocityBias);
            dp = Vector2.Multiply(linearVelocity, dt);

            _previousPosition = _position;
            _position = Vector2.Add(_previousPosition, dp);

            _linearVelocityBias = Vector2.Zero; //reset velocityBias to zero

            //angular
            float angularVelocity = _angularVelocity + _angularVelocityBias;

            orientationChange = angularVelocity * dt;
            _previousOrientation = _orientation;
            Orientation = _previousOrientation + orientationChange;

            _angularVelocityBias = 0; //reset angVelBias to zero
        }

        public void ApplyForce(Vector2 force) {
            _force = Vector2.Add(_force,force);
        }

        public void ApplyForceAtLocalPoint(Vector2 force, Vector2 point) {
            //calculate torque (2D cross product point X force)
            //Vector2 torqueVector = new Vector2(-point.Y, point.X);

            //torqueVector = Calculator.Project(force, torqueVector);

            Vector2 diff = GetWorldPosition(point) - Position;

            float torque = diff.X * force.Y - diff.Y * force.X;

            //add to torque
           _torque += torque;

            //also add linear force
            ApplyForce(force);           
        }

        public void ClearForce() { 
            _force = Vector2.Zero;
        }

        public void ApplyTorque(float torque) {
             _torque += torque;
        }

        public void ClearTorque() {
            _torque = 0;
        }

        protected bool isDisposed = false;
        public bool IsDisposed {
            get { return isDisposed; }
        }
        
        public void Dispose() {            
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            //subclasses can override incase they need to dispose of resources
            //otherwise do nothing.
            if (!isDisposed) {
                if (disposing) { };
                isDisposed = true;
            }            
        }
     }
}
