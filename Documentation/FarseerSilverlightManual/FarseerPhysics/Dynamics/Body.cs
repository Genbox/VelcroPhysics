using System;
#if (XNA)
using Microsoft.Xna.Framework;
#else
using FarseerGames.FarseerPhysics.Mathematics;
#endif

namespace FarseerGames.FarseerPhysics.Dynamics
{
    /// <summary>
    /// The Body handles all the physics of motion: position, velocity, acceleration
    /// forces, torques, etc...</para>
    /// <para>The Body is integrated every timestep (which should be fixed) by the <see cref="PhysicsSimulator">PhysicsSimulator</see> in the following manner:
    /// Set forces and torques (gravity, springs, user input...)->Apply forces and torques (updating velocity only)->Update positions and Rotations</para>
    /// <para>In technical terms, this is known as Symplectic Euler Integration because
    /// velocity is updated prior to position (The reverse of simple Euler Integration)</para>
    /// </summary>
    public class Body : IDisposable
    {
        #region Delegates

        public delegate void UpdatedEventHandler(ref Vector2 position, ref float rotation);

        #endregion

        private bool _isQuadraticDragEnabled;
        private float _linearDragCoefficient = .001f; //tuned for a body of mass 1
        private float _momentOfInertia = 1; //1 unit square
        private float _previousAngularVelocity;
        private Vector2 _previousLinearVelocity = Vector2.Zero;
        private Vector2 _previousPosition = Vector2.Zero;
        private float _previousRotation;
        private float _quadraticDragCoefficient = .001f;
        private int _revolutions;
        private float _rotationalDragCoefficient = .001f; //tuned for a 1.28m X 1.28m rectangle with mass = 1
        private Vector2 _tempVelocity = Vector2.Zero;
        private float _torque;

        internal float angularVelocity;
        internal float angularVelocityBias;
        internal bool enabled = true;
        internal Vector2 force = Vector2.Zero;
        internal bool ignoreGravity;
        internal Vector2 impulse = Vector2.Zero;
        internal float inverseMass = 1;
        internal float inverseMomentOfInertia = 1;
        internal bool isStatic;
        internal Vector2 linearVelocity = Vector2.Zero;
        internal Vector2 linearVelocityBias = Vector2.Zero;
        internal float mass = 1;
        internal Vector2 position = Vector2.Zero;
        internal float rotation;
        internal float totalRotation;

        //shouldn't need this. commenting out but keeping incase needed in the future.
        //private float linearDragVelocityThreshhold = .000001f;

        public bool IsDisposed;

        public event EventHandler<EventArgs> Disposed;
        public UpdatedEventHandler Updated;

        public Body()
        {
        }

        /// <summary>
        /// Body constructor that makes a copy of another body
        /// </summary>
        /// <param name="body"></param>
        public Body(Body body)
        {
            Mass = body.Mass;
            MomentOfInertia = body.MomentOfInertia;
            LinearDragCoefficient = body.LinearDragCoefficient;
            RotationalDragCoefficient = body.RotationalDragCoefficient;
        }

        #region Added by Daniel Pramel 08/17/08

        /// <summary>
        /// Returns or sets how long (ms) the body is below the MinimumVelocity.
        /// If this time is greater than the InactivityControllers "MaxIdleTime", it will be deactivated
        /// </summary>
        public float IdleTime;

        /// <summary>
        /// Returns or sets whether the body can be deactivated by the InactivityController or not
        /// </summary>
        public bool IsAutoIdle;

        /// <summary>
        /// Returns or sets the minimum velocity. If the bodys velocity is below this value, it can
        /// be deactivated
        /// </summary>
        public float MinimumVelocity = 55;

        /// <summary>
        /// Returns whether the body is below the minimum velocity or not
        /// </summary>
        public bool Moves
        {
            get { return linearVelocity.Length() >= 55; }
        }

        #endregion

        /// <summary>
        ///Sets whether or not the body will take part in the simulation.
        /// If not enabled, the body will remain in the internal list of bodies but it will not be updated.
        /// </summary>
        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

        /// <summary>
        /// The mass of the Body
        /// </summary>
        /// <exception cref="ArgumentException">Mass cannot be 0</exception>
        public float Mass
        {
            get { return mass; }
            set
            {
                if (value == 0)
                {
                    throw new ArgumentException("Mass cannot be 0", "value");
                }
                mass = value;
                if (isStatic)
                {
                    inverseMass = 0;
                }
                else
                {
                    inverseMass = 1f / value;
                }
            }
        }

        /// <summary>
        /// The inverse of the mass of the body (1/Mass)
        /// </summary>
        public float InverseMass
        {
            get { return inverseMass; }
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
        /// <exception cref="ArgumentException">Moment of inertia cannot be 0 or less</exception>
        public float MomentOfInertia
        {
            get { return _momentOfInertia; }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentException("Moment of inertia cannot be 0 or less", "value");
                }
                _momentOfInertia = value;
                if (isStatic)
                {
                    inverseMomentOfInertia = 0;
                }
                else
                {
                    inverseMomentOfInertia = 1f / value;
                }
            }
        }

        /// <summary>
        /// The inverse of the moment of inertia of the body (1/MomentOfInertia)
        /// </summary>
        public float InverseMomentOfInertia
        {
            get { return inverseMomentOfInertia; }
        }

        /// <summary>
        /// Indicates this body is fixed within the world and will not move no matter what forces are applied.
        /// <para>Bodies that represent land or world borders are a good examples of static bodies.</para>
        /// </summary>
        public bool IsStatic
        {
            get { return isStatic; }
            set
            {
                isStatic = value;
                if (isStatic)
                {
                    inverseMass = 0;
                    inverseMomentOfInertia = 0;
                }
                else
                {
                    inverseMass = 1f / mass;
                    inverseMomentOfInertia = 1f / _momentOfInertia;
                }
            }
        }

        /// <summary>
        /// Linear drag can be thought of as "air drag". The LinearDragCoefficient controls how much linear (non-rotational) drag
        /// a body is subject to. 
        /// <para>Linear drag causes a force to be applied to the body always in the direction opposite its velocity vector.
        /// The magnitude of this force is the _speed of the body squared multiplied by its LinearDragCoefficent. 
        /// <c>force = velocity*velocity*LinearDragCoeficcent</c></para>
        /// </summary>
        public float LinearDragCoefficient
        {
            get { return _linearDragCoefficient; }
            set { _linearDragCoefficient = value; }
        }
        
        /// <summary>
        /// Gets or sets the quadratic drag coefficient.
        /// </summary>
        /// <value>The quadratic drag coefficient.</value>
        public float QuadraticDragCoefficient
        {
            get { return _quadraticDragCoefficient; }
            set { _quadraticDragCoefficient = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this body is quadratic drag enabled.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this body is quadratic drag enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsQuadraticDragEnabled
        {
            get { return _isQuadraticDragEnabled; }
            set { _isQuadraticDragEnabled = value; }
        }

        /// <summary>
        /// Gets or sets the rotational drag coefficient.
        /// </summary>
        /// <value>The rotational drag coefficient.</value>
        public float RotationalDragCoefficient
        {
            get { return _rotationalDragCoefficient; }
            set { _rotationalDragCoefficient = value; }
        }

        /// <summary>
        /// Gets or sets the position.
        /// </summary>
        /// <value>The position.</value>
        public Vector2 Position
        {
            get { return position; }
            set
            {
                position = value;
                if (Updated != null)
                {
                    Updated(ref position, ref rotation);
                }
            }
        }

        /// <summary>
        /// Gets the revolutions relative to the original state of the body
        /// </summary>
        /// <value>The revolutions.</value>
        public int Revolutions
        {
            get { return _revolutions; }
        }

        /// <summary>
        /// Gets or sets the rotation.
        /// </summary>
        /// <value>The rotation.</value>
        public float Rotation
        {
            get { return rotation; }
            set
            {
                rotation = value;
                while (rotation > MathHelper.TwoPi)
                {
                    rotation -= MathHelper.TwoPi;
                    ++_revolutions;
                }
                while (rotation <= 0)
                {
                    rotation += MathHelper.TwoPi;
                    --_revolutions;
                }
                totalRotation = rotation + _revolutions * MathHelper.TwoPi;

                if (Updated != null)
                {
                    Updated(ref position, ref rotation);
                }
            }
        }

        /// <summary>
        ///Returns the total rotation of a body.
        ///If a body spins around 10 times then TotalRotation wold return 2 * Pi * 10.
        /// This property is mostly intended for internal use by the angle joints and springs but it could be useful in some situataions for game related things.
        /// This property is read-only 
        /// </summary>
        /// <value>The total rotation.</value>
        public float TotalRotation
        {
            get { return totalRotation; }
        }

        /// <summary>
        /// Gets or sets the linear velocity.
        /// </summary>
        /// <value>The linear velocity.</value>
        public Vector2 LinearVelocity
        {
            get { return linearVelocity; }
            set { linearVelocity = value; }
        }

        /// <summary>
        /// The rate at which a body is rotating 
        /// </summary>
        /// <value>The angular velocity.</value>
        public float AngularVelocity
        {
            get { return angularVelocity; }
            set { angularVelocity = value; }
        }

        /// <summary>
        /// The total amount of force that will be applied to the body in the upcoming loop.
        /// The Force is cleared at the end of every update call, so this value should only be called just prior to calling update.
        /// This property is read-only. 
        /// </summary>
        /// <value>The force.</value>
        public Vector2 Force
        {
            get { return force; }
        }

        /// <summary>
        /// The total amount of torque that will be applied to the body in the upcoming loop.
        /// The Torque is cleared at the end of every update call, so this value should only be called just prior to calling update.
        /// Torque can be thought of as the rotational analog of a force.
        /// This property is read-only. 
        /// </summary>
        /// <value>The torque.</value>
        public float Torque
        {
            get { return _torque; }
        }

        /// <summary>
        /// Gets or sets the tag. A tag is used to attach a custom object to the Body.
        /// </summary>
        /// <value>The tag.</value>
        public Object Tag { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this body ignores gravity.
        /// </summary>
        /// <value><c>true</c> if it ignores gravity; otherwise, <c>false</c>.</value>
        public bool IgnoreGravity
        {
            get { return ignoreGravity; }
            set { ignoreGravity = value; }
        }

        /// <summary>
        /// Returns a unit vector that represents the local X direction of a body converted to world coordinates.
        /// This property is read-only 
        /// </summary>
        /// <value>The X vector in world coordinates.</value>
        public Vector2 XVectorInWorldCoordinates
        {
            get
            {
                //Matrix bodyMatrix = BodyMatrix;
                _bodyMatrixTemp = GetBodyMatrix();
                return new Vector2(_bodyMatrixTemp.Right.X, _bodyMatrixTemp.Right.Y);
            }
        }

        /// <summary>
        /// Returns a unit vector that represents the local Y direction of a body converted to world coordinates.
        /// This property is read-only 
        /// </summary>
        /// <value>The Y vector in world coordinates.</value>
        public Vector2 YVectorInWorldCoordinates
        {
            get
            {
                //Matrix bodyMatrix = GetBodyMatrix();
                _bodyMatrixTemp = GetBodyMatrix();
                return new Vector2(_bodyMatrixTemp.Up.X, _bodyMatrixTemp.Up.Y);
            }
        }

        #region GetWorldPosition variables

        private Vector2 _worldPositionTemp = Vector2.Zero;

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        /// <summary>
        /// This method will reset position and motion properties. This method is useful
        /// when creating pools of re-usable bodies. It could be used when releasing bodies
        /// back into the pool.
        /// </summary>
        public void ResetDynamics()
        {
            linearVelocity.X = 0;
            linearVelocity.Y = 0;
            _previousLinearVelocity.X = 0;
            _previousLinearVelocity.Y = 0;
            angularVelocity = 0;
            _previousAngularVelocity = 0;
            position.X = 0;
            position.Y = 0;
            _previousPosition.X = 0;
            _previousPosition.Y = 0;
            rotation = 0;
            _revolutions = 0;
            totalRotation = 0;
            force.X = 0;
            force.Y = 0;
            _torque = 0;
            impulse.X = 0;
            impulse.Y = 0;
            _linearDrag.X = 0;
            _linearDrag.Y = 0;
            _rotationalDrag = 0;
        }

        public Matrix GetBodyMatrix()
        {
            Matrix.CreateTranslation(position.X, position.Y, 0, out _translationMatrixTemp);
            Matrix.CreateRotationZ(rotation, out _rotationMatrixTemp);
            Matrix.Multiply(ref _rotationMatrixTemp, ref _translationMatrixTemp, out _bodyMatrixTemp);
            return _bodyMatrixTemp;
        }

        public void GetBodyMatrix(out Matrix bodyMatrix)
        {
            Matrix.CreateTranslation(position.X, position.Y, 0, out _translationMatrixTemp);
            Matrix.CreateRotationZ(rotation, out _rotationMatrixTemp);
            Matrix.Multiply(ref _rotationMatrixTemp, ref _translationMatrixTemp, out bodyMatrix);
        }

        public Matrix GetBodyRotationMatrix()
        {
            Matrix.CreateRotationZ(rotation, out _rotationMatrixTemp);
            return _rotationMatrixTemp;
        }

        public void GetBodyRotationMatrix(out Matrix rotationMatrix)
        {
            Matrix.CreateRotationZ(rotation, out rotationMatrix);
        }

        public Vector2 GetWorldPosition(Vector2 localPosition)
        {
            GetBodyMatrix(out _bodyMatrixTemp);
            Vector2.Transform(ref localPosition, ref _bodyMatrixTemp, out _worldPositionTemp);
            return _worldPositionTemp;
        }

        public void GetWorldPosition(ref Vector2 localPosition, out Vector2 worldPosition)
        {
            GetBodyMatrix(out _bodyMatrixTemp);
            Vector2.Transform(ref localPosition, ref _bodyMatrixTemp, out worldPosition);
        }

        public Vector2 GetLocalPosition(Vector2 worldPosition)
        {
            GetBodyRotationMatrix(out _rotationMatrixTemp);
            Matrix.Transpose(ref _rotationMatrixTemp, out _rotationMatrixTemp);
            Vector2.Subtract(ref worldPosition, ref position, out _localPositionTemp);
            Vector2.Transform(ref _localPositionTemp, ref _rotationMatrixTemp, out _localPositionTemp);
            return _localPositionTemp;
        }

        public void GetLocalPosition(ref Vector2 worldPosition, out Vector2 localPosition)
        {
            GetBodyRotationMatrix(out _rotationMatrixTemp);
            Matrix.Transpose(ref _rotationMatrixTemp, out _rotationMatrixTemp);
            Vector2.Subtract(ref worldPosition, ref position, out localPosition);
            Vector2.Transform(ref localPosition, ref _rotationMatrixTemp, out localPosition);
        }

        public Vector2 GetVelocityAtLocalPoint(Vector2 localPoint)
        {
            GetVelocityAtLocalPoint(ref localPoint, out _tempVelocity);
            return _tempVelocity;
        }

        public void GetVelocityAtLocalPoint(ref Vector2 localPoint, out Vector2 velocity)
        {
            GetWorldPosition(ref localPoint, out _r1);
            Vector2.Subtract(ref _r1, ref position, out _r1);
            GetVelocityAtWorldOffset(ref _r1, out velocity);
        }

        public Vector2 GetVelocityAtWorldPoint(Vector2 worldPoint, Vector2 velocity)
        {
            GetVelocityAtWorldPoint(ref worldPoint, out velocity);
            return velocity;
        }

        public void GetVelocityAtWorldPoint(ref Vector2 worldPoint, out Vector2 velocity)
        {
            Vector2.Subtract(ref worldPoint, ref position, out _r1);
            GetVelocityAtWorldOffset(ref _r1, out velocity);
        }

        //for offset, think _r1!
#if(XNA)
        private Vector2 _velocityTemp = Vector2.Zero;
#endif
        public void GetVelocityAtWorldOffset(ref Vector2 offset, out Vector2 velocity)
        {
#if(XNA)
            //required by xbox
            velocity = _velocityTemp;
#endif

            #region INLINED: Calculator.Cross(ref angularVelocity, ref offset, out velocity);

            velocity.X = -angularVelocity * offset.Y;
            velocity.Y = angularVelocity * offset.X;

            #endregion

            #region INLINED: Vector2.Add(ref linearVelocity, ref velocity, out velocity);

            velocity.X = velocity.X + linearVelocity.X;
            velocity.Y = velocity.Y + linearVelocity.Y;

            #endregion
        }

        public void GetVelocityBiasAtWorldOffset(ref Vector2 offset, out Vector2 velocityBias)
        {
#if(XNA)
            //required by xbox
            velocityBias = _velocityTemp;
#endif

            #region INLINED: Calculator.Cross(ref angularVelocityBias, ref offset, out velocityBias);

            velocityBias.X = -angularVelocityBias * offset.Y;
            velocityBias.Y = angularVelocityBias * offset.X;

            #endregion

            #region INLINED: Vector2.Add(ref linearVelocityBias, ref velocityBias, out velocityBias);

            velocityBias.X = velocityBias.X + linearVelocityBias.X;
            velocityBias.Y = velocityBias.Y + linearVelocityBias.Y;

            #endregion
        }

        /// <summary>
        /// Adds a force to the body. Takes a Vector2 as parameter 
        /// </summary>
        /// <param name="force">The force.</param>
        public void ApplyForce(Vector2 force)
        {
            #region INLINE:  Vector2.Add(ref this.force, ref force, out this.force);

            this.force.X = this.force.X + force.X;
            this.force.Y = this.force.Y + force.Y;

            #endregion
        }

        /// <summary>
        /// Applies a force to the body.
        /// </summary>
        /// <param name="force">The force.</param>
        public void ApplyForce(ref Vector2 force)
        {
            #region INLINE: Vector2.Add(ref this.force, ref force, out this.force);

            this.force.X = this.force.X + force.X;
            this.force.Y = this.force.Y + force.Y;

            #endregion
        }

        public void ApplyForceAtLocalPoint(Vector2 force, Vector2 point)
        {
            //calculate _torque (2D cross product point X force)
            GetWorldPosition(ref point, out _diff);
            Vector2.Subtract(ref _diff, ref position, out _diff);

            float torque = _diff.X * force.Y - _diff.Y * force.X;

            //add to _torque
            _torque += torque;

            // add linear force
            this.force.X += force.X;
            this.force.Y += force.Y;
        }

        public void ApplyForceAtLocalPoint(ref Vector2 force, ref Vector2 point)
        {
            //calculate _torque (2D cross product point X force)
            GetWorldPosition(ref point, out _diff);
            Vector2.Subtract(ref _diff, ref position, out _diff);

            float torque = _diff.X * force.Y - _diff.Y * force.X;

            //add to _torque
            _torque += torque;

            // add linear force
            this.force.X += force.X;
            this.force.Y += force.Y;
        }

        public void ApplyForceAtWorldPoint(ref Vector2 force, ref Vector2 point)
        {
            Vector2.Subtract(ref point, ref position, out _diff);

            float torque = _diff.X * force.Y - _diff.Y * force.X;

            //add to _torque
            _torque += torque;

            // add linear force
            this.force.X += force.X;
            this.force.Y += force.Y;
        }

        public void ApplyForceAtWorldPoint(Vector2 force, Vector2 point)
        {
            Vector2.Subtract(ref point, ref position, out _diff);

            float torque = _diff.X * force.Y - _diff.Y * force.X;

            //add to _torque
            _torque += torque;

            // add linear force
            this.force.X += force.X;
            this.force.Y += force.Y;
        }

        /// <summary>
        /// Clears the force of the body.
        /// </summary>
        public void ClearForce()
        {
            force.X = 0;
            force.Y = 0;
        }

        /// <summary>
        /// Adds a torque to the body. Takes a float as parameter 
        /// </summary>
        /// <param name="torque">The torque.</param>
        public void ApplyTorque(float torque)
        {
            _torque += torque;
        }

        /// <summary>
        /// Clears the torque of the body.
        /// </summary>
        public void ClearTorque()
        {
            _torque = 0;
        }

        /// <summary>
        /// Stores all applied impulses so that they can be applied at the same time
        /// by the PhysicsSimulator.
        /// </summary>
        /// <param name="impulse"></param>
        public void ApplyImpulse(Vector2 impulse)
        {
            #region INLINE: Vector2.Multiply(ref impulse, inverseMass, out _dv);

            _dv.X = impulse.X * inverseMass;
            _dv.Y = impulse.Y * inverseMass;

            #endregion

            #region INLINE: Vector2.Add(ref _dv, ref linearVelocity, out linearVelocity);

            this.impulse.X += _dv.X + linearVelocity.X;
            this.impulse.Y += _dv.Y + linearVelocity.Y;

            #endregion
        }

        /// <summary>
        /// Stores all applied impulses so that they can be applied at the same time
        /// by the PhysicsSimulator.
        /// </summary>
        /// <param name="impulse"></param>
        public void ApplyImpulse(ref Vector2 impulse)
        {
            #region INLINE: Vector2.Multiply(ref impulse, inverseMass, out _dv);

            _dv.X = impulse.X * inverseMass;
            _dv.Y = impulse.Y * inverseMass;

            #endregion

            #region INLINE: Vector2.Add(ref _dv, ref linearVelocity, out linearVelocity);

            this.impulse.X += _dv.X + linearVelocity.X;
            this.impulse.Y += _dv.Y + linearVelocity.Y;

            #endregion
        }

        /// <summary>
        /// Applys then clears all the external impulses that were accumulated
        /// </summary>
        internal void ApplyImpulses()
        {
            ApplyImmediateImpulse(ref impulse);
            impulse.X = 0;
            impulse.Y = 0;
        }

        /// <summary>
        /// used internally only by the joints and arbiter.
        /// </summary>
        /// <param name="impulse"></param>
        internal void ApplyImmediateImpulse(ref Vector2 impulse)
        {
            #region INLINE: Vector2.Multiply(ref impulse, inverseMass, out _dv);

            _dv.X = impulse.X * inverseMass;
            _dv.Y = impulse.Y * inverseMass;

            #endregion

            #region INLINE: Vector2.Add(ref _dv, ref linearVelocity, out linearVelocity);

            linearVelocity.X = _dv.X + linearVelocity.X;
            linearVelocity.Y = _dv.Y + linearVelocity.Y;

            #endregion
        }

        public void ClearImpulse()
        {
            impulse.X = 0;
            impulse.Y = 0;
        }

        public void ApplyAngularImpulse(float impulse)
        {
            angularVelocity += impulse * inverseMomentOfInertia;
        }

        public void ApplyAngularImpulse(ref float impulse)
        {
            angularVelocity += impulse * inverseMomentOfInertia;
        }

        private void ApplyDrag()
        {
            #region INLINE: _speed = linearVelocity.Length();

            //requrired for quadratic drag. 
            if (_isQuadraticDragEnabled)
            {
                float num = (linearVelocity.X * linearVelocity.X) + (linearVelocity.Y * linearVelocity.Y);
                _speed = (float)Math.Sqrt(num);
                //Vector2 quadraticDrag = Vector2.Zero;
                //Vector2 totalDrag = Vector2.Zero;
            }

            #endregion

            //if (_speed > linearDragVelocityThreshhold) {

            #region INLINE: Vector2.Multiply(ref linearVelocity, -_linearDragCoefficient, out _linearDrag);

            _linearDrag.X = -linearVelocity.X * _linearDragCoefficient;
            _linearDrag.Y = -linearVelocity.Y * _linearDragCoefficient;

            #endregion

            //quadratic drag for "fast" moving objects. Must be enabled first.
            if (_isQuadraticDragEnabled)
            {
                #region INLINE: Vector2.Multiply(ref linearVelocity, -_quadraticDragCoefficient * _speed, out _quadraticDrag);

                _quadraticDrag.X = -_quadraticDragCoefficient * _speed * linearVelocity.X;
                _quadraticDrag.Y = -_quadraticDragCoefficient * _speed * linearVelocity.Y;

                #endregion

                #region INLINE: Vector2.Add(ref _linearDrag, ref _quadraticDrag, out _totalDrag);

                _totalDrag.X = _linearDrag.X + _quadraticDrag.X;
                _totalDrag.Y = _linearDrag.Y + _quadraticDrag.Y;

                #endregion

                ApplyForce(ref _totalDrag);
            }
            else
            {
                ApplyForce(ref _linearDrag);
            }
            //}

            _rotationalDrag = angularVelocity * angularVelocity * Math.Sign(angularVelocity);
            _rotationalDrag *= -_rotationalDragCoefficient;
            ApplyTorque(_rotationalDrag);
        }

        internal void IntegrateVelocity(float dt)
        {
            if (isStatic)
            {
                return;
            }
            //linear
            ApplyDrag();

            #region INLINE: Vector2.Multiply(ref force, inverseMass, out _acceleration);

            _acceleration.X = force.X * inverseMass;
            _acceleration.Y = force.Y * inverseMass;

            #endregion

            #region INLINE: Vector2.Multiply(ref _acceleration, dt, out _dv);

            _dv.X = _acceleration.X * dt;
            _dv.Y = _acceleration.Y * dt;

            #endregion

            _previousLinearVelocity = linearVelocity;

            #region INLINE: Vector2.Add(ref _previousLinearVelocity, ref _dv, out linearVelocity);

            linearVelocity.X = _previousLinearVelocity.X + _dv.X;
            linearVelocity.Y = _previousLinearVelocity.Y + _dv.Y;

            #endregion

            //angular
            _dw = _torque * inverseMomentOfInertia * dt;
            _previousAngularVelocity = angularVelocity;
            angularVelocity = _previousAngularVelocity + _dw;
        }

        internal void IntegratePosition(float dt)
        {
            if (isStatic)
            {
                return;
            }

            //linear

            #region INLINE: Vector2.Add(ref linearVelocity, ref linearVelocityBias, out _bodylinearVelocity);

            _bodylinearVelocity.X = linearVelocity.X + linearVelocityBias.X;
            _bodylinearVelocity.Y = linearVelocity.Y + linearVelocityBias.Y;

            #endregion

            #region INLINE: Vector2.Multiply(ref _bodylinearVelocity, dt, out _dp);

            _dp.X = _bodylinearVelocity.X * dt;
            _dp.Y = _bodylinearVelocity.Y * dt;

            #endregion

            _previousPosition = position;

            #region INLINE: Vector2.Add(ref _previousPosition, ref _dp, out position);

            position.X = _previousPosition.X + _dp.X;
            position.Y = _previousPosition.Y + _dp.Y;

            #endregion

            linearVelocityBias.X = 0;
            linearVelocityBias.Y = 0;

            //angular
            _bodyAngularVelocity = angularVelocity + angularVelocityBias;
            _rotationChange = _bodyAngularVelocity * dt;
            _previousRotation = rotation;
            rotation = _previousRotation + _rotationChange;

            //clamp rotation to 0 <= rotation <2Pi
            while (rotation > MathHelper.TwoPi)
            {
                rotation -= MathHelper.TwoPi;
                ++_revolutions;
            }
            while (rotation <= 0)
            {
                rotation += MathHelper.TwoPi;
                --_revolutions;
            }
            totalRotation = rotation + _revolutions * MathHelper.TwoPi;
            angularVelocityBias = 0; //reset angVelBias to zero

            if (Updated != null)
            {
                Updated(ref position, ref rotation);
            }
        }

        internal void ApplyImpulseAtWorldOffset(ref Vector2 impulse, ref Vector2 offset)
        {
            #region INLINE: Vector2.Multiply(ref impulse, inverseMass, out _dv);

            _dv.X = impulse.X * inverseMass;
            _dv.Y = impulse.Y * inverseMass;

            #endregion

            #region INLINE: Vector2.Add(ref _dv, ref linearVelocity, out linearVelocity);

            linearVelocity.X = _dv.X + linearVelocity.X;
            linearVelocity.Y = _dv.Y + linearVelocity.Y;

            #endregion

            #region INLINE: Calculator.Cross(ref offset, ref impulse, out angularImpulse);

            float angularImpulse = offset.X * impulse.Y - offset.Y * impulse.X;

            #endregion

            angularImpulse *= inverseMomentOfInertia;
            angularVelocity += angularImpulse;
        }

        internal void ApplyBiasImpulseAtWorldOffset(ref Vector2 impulseBias, ref Vector2 offset)
        {
            #region INLINE: Vector2.Multiply(ref impulseBias, inverseMass, out _dv);

            _dv.X = impulseBias.X * inverseMass;
            _dv.Y = impulseBias.Y * inverseMass;

            #endregion

            #region INLINE: Vector2.Add(ref _dv, ref linearVelocityBias, out linearVelocityBias);

            linearVelocityBias.X = _dv.X + linearVelocityBias.X;
            linearVelocityBias.Y = _dv.Y + linearVelocityBias.Y;

            #endregion

            #region INLINE: Calculator.Cross(ref offset, ref impulseBias, out angularImpulseBias);

            float angularImpulseBias = offset.X * impulseBias.Y - offset.Y * impulseBias.X;

            #endregion

            angularImpulseBias *= inverseMomentOfInertia;
            angularVelocityBias += angularImpulseBias;
        }

        protected virtual void Dispose(bool disposing)
        {
            IsDisposed = true;

            if (Disposed != null)
            {
                Disposed(this, EventArgs.Empty);
            }
        }

        #region IntegeratePosition variables

        private float _bodyAngularVelocity;
        private Vector2 _bodylinearVelocity;
        private Vector2 _dp;
        private float _rotationChange;

        #endregion

        #region IntegerateVelocity variables

        private Vector2 _acceleration = Vector2.Zero;
        private Vector2 _dv = Vector2.Zero; //change in linear velocity
        private float _dw; //change in angular velocity 

        #endregion

        #region ApplyDrag variables

        //private Vector2 dragDirection = Vector2.Zero;
        private Vector2 _linearDrag = Vector2.Zero;
        private Vector2 _quadraticDrag = Vector2.Zero;
        private float _rotationalDrag;
        private float _speed;
        private Vector2 _totalDrag = Vector2.Zero;

        #endregion

        #region ApplyForceAtLocalPoint variables

        private Vector2 _diff;

        #endregion

        #region GetVelocityAtPoint variables

        private Vector2 _r1 = Vector2.Zero;

        #endregion

        #region GetLocalPosition variables

        private Vector2 _localPositionTemp = Vector2.Zero;

        #endregion

        #region GetBodyMatrix variables

        private Matrix _bodyMatrixTemp = Matrix.Identity;
        private Matrix _rotationMatrixTemp = Matrix.Identity;
        private Matrix _translationMatrixTemp = Matrix.Identity;

        #endregion
    }
}