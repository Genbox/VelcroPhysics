using System;
using System.Xml.Serialization;
using FarseerGames.FarseerPhysics.Dynamics.Joints;
using FarseerGames.FarseerPhysics.Dynamics.Springs;
using FarseerGames.FarseerPhysics.Interfaces;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Controllers;

#if (XNA)
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
#else
using FarseerGames.FarseerPhysics.Mathematics;
#endif

namespace FarseerGames.FarseerPhysics.Dynamics
{
    public delegate bool FixedJointDelegate(Joint sender, Body body);
    public delegate bool JointDelegate(Joint sender, Body body1, Body body2);
    public delegate bool FixedSpringDelegate(Spring sender, Body body);
    public delegate bool SpringDelegate(Spring sender, Body body1, Body body2);

    public delegate void UpdatedEventHandler(ref Vector2 position, ref float rotation);

    /// <summary>
    /// The Body handles all the physics of motion: position, velocity, acceleration
    /// forces, torques, etc...</para>
    /// <para>The Body is integrated every time step (which should be fixed) by the <see cref="PhysicsSimulator"/> in the following manner:
    /// Set forces and torques (gravity, springs, user input...)->Apply forces and torques (updating velocity only)->Update positions and Rotations</para>
    /// <para>In technical terms, this is known as Symplectic Euler Integration because
    /// velocity is updated prior to position (The reverse of simple Euler Integration)</para>
    /// </summary>
    public sealed class Body : IIsDisposable
    {
        private float _momentOfInertia = 1; //1 unit square
        private float _previousAngularVelocity;
        private Vector2 _previousLinearVelocity = Vector2.Zero;
        private Vector2 _previousPosition = Vector2.Zero;
        private float _previousRotation;
        private int _revolutions;
        private float _torque;
        private bool _isDisposed;

        internal Vector2 impulse = Vector2.Zero;
        internal float inverseMass = 1;
        internal float inverseMomentOfInertia = 1;
        internal float totalRotation;
        internal float rotation;
        internal Vector2 linearVelocityBias = Vector2.Zero;
        internal float mass = 1;
        internal Vector2 position = Vector2.Zero;
        internal float angularVelocityBias;
        internal Vector2 force = Vector2.Zero;
        internal bool isStatic;

        /// <summary>
        /// The rate at which a body is rotating 
        /// </summary>
        /// <Value>The angular velocity.</Value>
        public float AngularVelocity;

        /// <summary>
        ///Sets whether or not the body will take part in the simulation.
        /// If not enabled, the body will remain in the internal list of bodies but it will not be updated.
        /// </summary>
        public bool Enabled = true;

        /// <summary>
        /// Gets or sets a Value indicating whether this body ignores gravity.
        /// </summary>
        /// <Value><c>true</c> if it ignores gravity; otherwise, <c>false</c>.</Value>
        public bool IgnoreGravity;

        /// <summary>
        /// Gets or sets a Value indicating whether this body is quadratic drag enabled.
        /// </summary>
        /// <Value>
        /// 	<c>true</c> if this body is quadratic drag enabled; otherwise, <c>false</c>.
        /// </Value>
        public bool IsQuadraticDragEnabled;

        /// <summary>
        /// The linear drag coefficient is the amount of drag a body has.
        /// Linear drag is the drag applied when the body travels in a straight line.
        /// Default is 0.001f - tuned for a body of mass 1
        /// </summary>
        public float LinearDragCoefficient = .001f;

        /// <summary>
        /// Gets or sets the linear velocity.
        /// </summary>
        /// <Value>The linear velocity.</Value>
        public Vector2 LinearVelocity = Vector2.Zero;

        /// <summary>
        /// Gets or sets the quadratic drag coefficient.
        /// </summary>
        /// <Value>The quadratic drag coefficient.</Value>
        public float QuadraticDragCoefficient = .001f;

        /// <summary>
        /// Gets or sets the rotational drag coefficient.
        /// </summary>
        /// <Value>The rotational drag coefficient.</Value>
        public float RotationalDragCoefficient = .001f; //tuned for a 1.28m X 1.28m rectangle with mass = 1

#if(XNA)
        [ContentSerializerIgnore]
#endif
        [XmlIgnore]
        /// <summary>
        /// Gets or sets the tag. A tag is used to attach a custom object to the Body.
        /// </summary>
        /// <Value>The custom object</Value>
        public object Tag;

#if(XNA)
        [ContentSerializerIgnore]
#endif
        [XmlIgnore]
        /// <summary>
        /// If the position or rotation of the body changes, this event will be fired.
        /// </summary>
        public UpdatedEventHandler Updated;

        /// <summary>
        /// Fires when body gets disposed
        /// </summary>
        public event EventHandler<EventArgs> Disposed;

        //NOTE: shouldn't need this. commenting out but keeping incase needed in the future.
        //private float linearDragVelocityThreshhold = .000001f;

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
            IsQuadraticDragEnabled = body.IsQuadraticDragEnabled;
            QuadraticDragCoefficient = body.QuadraticDragCoefficient;
            Enabled = body.Enabled;
            Tag = body.Tag;
            IgnoreGravity = body.IgnoreGravity;
            IsStatic = body.isStatic;
        }

        #region Added by Daniel Pramel 08/17/08

        /// <summary>
        /// Returns or sets how long (ms) the body is below the <see cref="MinimumVelocity"/>.
        /// If this time is greater than the InactivityControllers "MaxIdleTime", it will be deactivated
        /// </summary>
        public float IdleTime;

        /// <summary>
        /// Returns or sets whether the body can be deactivated by the <see cref="InactivityController"/> or not
        /// </summary>
        public bool IsAutoIdle;

        /// <summary>
        /// Returns or sets the minimum velocity. If the body's velocity is below this Value, it can
        /// be deactivated
        /// </summary>
        public float MinimumVelocity = 55;

        /// <summary>
        /// Returns whether the body is below the minimum velocity or not
        /// </summary>
        public bool Moves
        {
            get { return LinearVelocity.Length() >= MinimumVelocity; }
        }

        #endregion

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
                    throw new ArgumentException("Mass cannot be 0", "value");

                mass = value;

                if (isStatic)
                    inverseMass = 0;
                else
                    inverseMass = 1f / value;
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
        /// <para>The moment of inertia of a body in 2D is a scalar Value that represents how
        /// difficult (or not difficult) it is to rotate a body about the center of mass.</para>
        /// <para>The moment of inertia varies by the shape of the body. For basic shapes like
        /// circles and rectangles, formulas exist for computing the moment of inertia based on
        /// the shape properties (radius of the circle, or length and width of the rectangle)</para>
        /// <para>For bodies that are not basic, it is usually good enough to estimate the moment of
        /// inertia by picking a basic shape that is close to the same shape. It is also possible
        /// using more advance calculus techniques to compute the actual moment of inertia of 
        /// non-basic shapes.</para>
        /// The <see cref="Vertices"/> class has the ability of calculating the MOI (Moment of Inertia) from a polygon shape.
        /// </summary>
        /// <exception cref="ArgumentException">Moment of inertia cannot be 0 or less</exception>
        public float MomentOfInertia
        {
            get { return _momentOfInertia; }
            set
            {
                if (value <= 0)
                    throw new ArgumentException("Moment of inertia cannot be 0 or less", "value");

                _momentOfInertia = value;

                if (isStatic)
                    inverseMomentOfInertia = 0;
                else
                    inverseMomentOfInertia = 1f / value;
            }
        }

        /// <summary>
        /// The inverse of the moment of inertia of the body (1/<see cref="MomentOfInertia"/>)
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
        /// Gets or sets the position.
        /// </summary>
        /// <Value>The position.</Value>
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
        /// <Value>The revolutions.</Value>
        public int Revolutions
        {
            get { return _revolutions; }
        }

        /// <summary>
        /// Gets or sets the rotation.
        /// </summary>
        /// <Value>The rotation.</Value>
        public float Rotation
        {
            get { return rotation; }
            set
            {
                rotation = value;

                // Calculate floating point remainder of rotation
                int z = (int)(rotation / MathHelper.TwoPi);
                if (rotation < 0)
                    z--;
                rotation = (rotation - z * MathHelper.TwoPi);

                _revolutions += z;
                totalRotation = rotation + _revolutions * MathHelper.TwoPi;

                if (Updated != null)
                {
                    Updated(ref position, ref rotation);
                }
            }
        }

        /// <summary>
        /// Returns the total rotation of a body.
        /// If a body spins around 10 times then TotalRotation wold return 2 * Pi * 10.
        /// This property is mostly intended for internal use by the angle joints and springs but it could be useful in some situations for game related things.
        /// This property is read-only 
        /// </summary>
        /// <Value>The total rotation.</Value>
        public float TotalRotation
        {
            get { return totalRotation; }
        }

        /// <summary>
        /// The total amount of force that will be applied to the body in the upcoming loop.
        /// The force is cleared at the end of every update call, so this Value should only be called just prior to calling update.
        /// This property is read-only. 
        /// </summary>
        /// <Value>The force.</Value>
        public Vector2 Force
        {
            get { return force; }
        }

        /// <summary>
        /// The total amount of torque that will be applied to the body in the upcoming loop.
        /// The Torque is cleared at the end of every update call, so this Value should only be called just prior to calling update.
        /// Torque can be thought of as the rotational analog of a force.
        /// This property is read-only. 
        /// </summary>
        /// <Value>The torque.</Value>
        public float Torque
        {
            get { return _torque; }
        }

        /// <summary>
        /// Returns a unit vector that represents the local X direction of a body converted to world coordinates.
        /// This property is read-only 
        /// </summary>
        /// <Value>The X vector in world coordinates.</Value>
        public Vector2 XVectorInWorldCoordinates
        {
            get
            {
                GetBodyMatrix(out _bodyMatrixTemp);
                return new Vector2(_bodyMatrixTemp.Right.X, _bodyMatrixTemp.Right.Y);
            }
        }

        /// <summary>
        /// Returns a unit vector that represents the local Y direction of a body converted to world coordinates.
        /// This property is read-only 
        /// </summary>
        /// <Value>The Y vector in world coordinates.</Value>
        public Vector2 YVectorInWorldCoordinates
        {
            get
            {
                GetBodyMatrix(out _bodyMatrixTemp);
                return new Vector2(_bodyMatrixTemp.Up.X, _bodyMatrixTemp.Up.Y);
            }
        }

        /// <summary>
        /// This method will reset position and motion properties. This method is useful
        /// when creating pools of re-usable bodies. It could be used when releasing bodies
        /// back into the pool.
        /// </summary>
        public void ResetDynamics()
        {
            LinearVelocity.X = 0;
            LinearVelocity.Y = 0;
            _previousLinearVelocity.X = 0;
            _previousLinearVelocity.Y = 0;
            AngularVelocity = 0;
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

            //Update the geometry with the new values (reset values)
            if (Updated != null)
            {
                Updated(ref position, ref rotation);
            }
        }

        /// <summary>
        /// Gets the body matrix.
        /// It's a combination of the translation and rotation matrix.
        /// </summary>
        /// <returns></returns>
        public Matrix GetBodyMatrix()
        {
            Matrix.CreateTranslation(position.X, position.Y, 0, out _translationMatrixTemp);
            Matrix.CreateRotationZ(rotation, out _rotationMatrixTemp);
            Matrix.Multiply(ref _rotationMatrixTemp, ref _translationMatrixTemp, out _bodyMatrixTemp);
            return _bodyMatrixTemp;
        }

        /// <summary>
        /// Gets the body matrix.
        /// It's a combination of the translation and rotation matrix.
        /// </summary>
        /// <returns></returns>
        public void GetBodyMatrix(out Matrix bodyMatrix)
        {
            Matrix.CreateTranslation(position.X, position.Y, 0, out _translationMatrixTemp);
            Matrix.CreateRotationZ(rotation, out _rotationMatrixTemp);
            Matrix.Multiply(ref _rotationMatrixTemp, ref _translationMatrixTemp, out bodyMatrix);
        }

        /// <summary>
        /// Gets the body rotation matrix.
        /// </summary>
        /// <returns></returns>
        public Matrix GetBodyRotationMatrix()
        {
            Matrix.CreateRotationZ(rotation, out _rotationMatrixTemp);
            return _rotationMatrixTemp;
        }

        /// <summary>
        /// Gets the body rotation matrix.
        /// </summary>
        /// <returns></returns>
        public void GetBodyRotationMatrix(out Matrix rotationMatrix)
        {
            Matrix.CreateRotationZ(rotation, out rotationMatrix);
        }

        /// <summary>
        /// Gets the body's position in world coordinates from local coordinates.
        /// </summary>
        /// <param name="localPosition">The local position of the body</param>
        /// <returns>The world position of the body</returns>
        public Vector2 GetWorldPosition(Vector2 localPosition)
        {
            GetBodyMatrix(out _bodyMatrixTemp);
            Vector2.Transform(ref localPosition, ref _bodyMatrixTemp, out _worldPositionTemp);
            return _worldPositionTemp;
        }

        /// <summary>
        /// Gets the body's position in world coordinates from local coordinates.
        /// </summary>
        /// <param name="localPosition">The local position of the body</param>
        /// <param name="worldPosition">The world position.</param>
        public void GetWorldPosition(ref Vector2 localPosition, out Vector2 worldPosition)
        {
            GetBodyMatrix(out _bodyMatrixTemp);
            Vector2.Transform(ref localPosition, ref _bodyMatrixTemp, out worldPosition);
        }

        /// <summary>
        /// Gets the body's position in local coordinates from world coordinates.
        /// </summary>
        /// <param name="worldPosition">The world position of the body</param>
        /// <returns>The local position of the body</returns>
        public Vector2 GetLocalPosition(Vector2 worldPosition)
        {
            GetBodyRotationMatrix(out _rotationMatrixTemp);
            Matrix.Transpose(ref _rotationMatrixTemp, out _rotationMatrixTemp);
            Vector2.Subtract(ref worldPosition, ref position, out _localPositionTemp);
            Vector2.Transform(ref _localPositionTemp, ref _rotationMatrixTemp, out _localPositionTemp);
            return _localPositionTemp;
        }

        /// <summary>
        /// Gets the body's position in local coordinates from world coordinates.
        /// </summary>
        /// <param name="worldPosition">The world position of the body</param>
        /// <param name="localPosition">The local position.</param>
        public void GetLocalPosition(ref Vector2 worldPosition, out Vector2 localPosition)
        {
            GetBodyRotationMatrix(out _rotationMatrixTemp);
            Matrix.Transpose(ref _rotationMatrixTemp, out _rotationMatrixTemp);
            Vector2.Subtract(ref worldPosition, ref position, out localPosition);
            Vector2.Transform(ref localPosition, ref _rotationMatrixTemp, out localPosition);
        }

        /// <summary>
        /// Gets the velocity at a local point.
        /// </summary>
        /// <param name="localPoint">The local point.</param>
        /// <returns>The velocity at the point</returns>
        public Vector2 GetVelocityAtLocalPoint(Vector2 localPoint)
        {
            GetVelocityAtLocalPoint(ref localPoint, out _tempVelocity);
            return _tempVelocity;
        }

        /// <summary>
        /// Gets the velocity at a local point.
        /// </summary>
        /// <param name="localPoint">The local point.</param>
        /// <param name="velocity">The velocity.</param>
        public void GetVelocityAtLocalPoint(ref Vector2 localPoint, out Vector2 velocity)
        {
            GetWorldPosition(ref localPoint, out _r1);
            Vector2.Subtract(ref _r1, ref position, out _r1);
            GetVelocityAtWorldOffset(ref _r1, out velocity);
        }

        /// <summary>
        /// Gets the velocity at a world point.
        /// </summary>
        /// <param name="worldPoint">The world point.</param>
        /// <param name="velocity">The velocity.</param>
        public void GetVelocityAtWorldPoint(ref Vector2 worldPoint, out Vector2 velocity)
        {
            Vector2.Subtract(ref worldPoint, ref position, out _r1);
            GetVelocityAtWorldOffset(ref _r1, out velocity);
        }

#if(XNA)
        private Vector2 _velocityTemp = Vector2.Zero;
#endif

        /// <summary>
        /// Gets the velocity at world offset.
        /// </summary>
        /// <param name="offset">The offset.</param>
        /// <param name="velocity">The velocity.</param>
        public void GetVelocityAtWorldOffset(ref Vector2 offset, out Vector2 velocity)
        {
#if(XNA)
            //required by xbox
            velocity = _velocityTemp;
#endif

            #region INLINED: Calculator.Cross(ref AngularVelocity, ref offset, out velocity);

            velocity.X = -AngularVelocity * offset.Y;
            velocity.Y = AngularVelocity * offset.X;

            #endregion

            #region INLINED: Vector2.Add(ref linearVelocity, ref velocity, out velocity);

            velocity.X = velocity.X + LinearVelocity.X;
            velocity.Y = velocity.Y + LinearVelocity.Y;

            #endregion
        }

        /// <summary>
        /// Gets the velocity bias at world offset.
        /// </summary>
        /// <param name="offset">The offset.</param>
        /// <param name="velocityBias">The velocity bias.</param>
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
        /// Adds force to the body.
        /// </summary>
        /// <param name="amount">The amount.</param>
        public void ApplyForce(Vector2 amount)
        {
            #region INLINE:  Vector2.Add(ref this.force, ref amount, out this.force);

            force.X = force.X + amount.X;
            force.Y = force.Y + amount.Y;

            #endregion
        }

        /// <summary>
        /// Applies a force to the body.
        /// </summary>
        /// <param name="amount">The amount.</param>
        public void ApplyForce(ref Vector2 amount)
        {
            #region INLINE: Vector2.Add(ref this.force, ref amount, out this.force);

            force.X = force.X + amount.X;
            force.Y = force.Y + amount.Y;

            #endregion
        }

        /// <summary>
        /// Applies force at a local point.
        /// </summary>
        /// <param name="amount">The amount.</param>
        /// <param name="point">The point.</param>
        public void ApplyForceAtLocalPoint(Vector2 amount, Vector2 point)
        {
            //calculate _torque (2D cross product point X force)
            GetWorldPosition(ref point, out _diff);
            Vector2.Subtract(ref _diff, ref position, out _diff);

            float torque = _diff.X * amount.Y - _diff.Y * amount.X;

            //add to _torque
            _torque += torque;

            // add linear force
            force.X += amount.X;
            force.Y += amount.Y;
        }

        /// <summary>
        /// Applies force at a local point.
        /// </summary>
        /// <param name="amount">The amount.</param>
        /// <param name="point">The point.</param>
        public void ApplyForceAtLocalPoint(ref Vector2 amount, ref Vector2 point)
        {
            //calculate _torque (2D cross product point X force)
            GetWorldPosition(ref point, out _diff);
            Vector2.Subtract(ref _diff, ref position, out _diff);

            float torque = _diff.X * amount.Y - _diff.Y * amount.X;

            //add to _torque
            _torque += torque;

            // add linear force
            force.X += amount.X;
            force.Y += amount.Y;
        }

        /// <summary>
        /// Applies force at a world point.
        /// </summary>
        /// <param name="amount">The amount.</param>
        /// <param name="point">The point.</param>
        public void ApplyForceAtWorldPoint(Vector2 amount, Vector2 point)
        {
            Vector2.Subtract(ref point, ref position, out _diff);

            float torque = _diff.X * amount.Y - _diff.Y * amount.X;

            //add to _torque
            _torque += torque;

            // add linear force
            force.X += amount.X;
            force.Y += amount.Y;
        }

        /// <summary>
        /// Applies force at a world point.
        /// </summary>
        /// <param name="amount">The amount.</param>
        /// <param name="point">The point.</param>
        public void ApplyForceAtWorldPoint(ref Vector2 amount, ref Vector2 point)
        {
            Vector2.Subtract(ref point, ref position, out _diff);

            float torque = _diff.X * amount.Y - _diff.Y * amount.X;

            //add to _torque
            _torque += torque;

            // add linear force
            force.X += amount.X;
            force.Y += amount.Y;
        }

        /// <summary>
        /// Clears the force of the body.
        /// This method gets called after each update.
        /// </summary>
        public void ClearForce()
        {
            force.X = 0;
            force.Y = 0;
        }

        /// <summary>
        /// Adds a torque to the body.
        /// </summary>
        /// <param name="torque">The torque.</param>
        public void ApplyTorque(float torque)
        {
            _torque += torque;
        }

        /// <summary>
        /// Clears the torque of the body.
        /// This method gets called after each update.
        /// </summary>
        public void ClearTorque()
        {
            _torque = 0;
        }

        /// <summary>
        /// Stores all applied impulses so that they can be applied at the same time
        /// by the <see cref="PhysicsSimulator"/>.
        /// </summary>
        /// <param name="amount"></param>
        public void ApplyImpulse(Vector2 amount)
        {
            //Applying second fix from: http://www.codeplex.com/FarseerPhysics/WorkItem/View.aspx?WorkItemId=20155
            //#region INLINE: Vector2.Multiply(ref impulse, inverseMass, out _dv);
            //_dv.X = amount.X * inverseMass;
            //_dv.Y = amount.Y * inverseMass;
            //#endregion

            #region INLINE: Vector2.Add(ref amount, ref impulse, out impulse);

            impulse.X += amount.X;
            impulse.Y += amount.Y;

            #endregion
        }

        /// <summary>
        /// Stores all applied impulses so that they can be applied at the same time
        /// by the <see cref="PhysicsSimulator"/>.
        /// </summary>
        /// <param name="amount"></param>
        public void ApplyImpulse(ref Vector2 amount)
        {
            //Applying second fix from: http://www.codeplex.com/FarseerPhysics/WorkItem/View.aspx?WorkItemId=20155
            //#region INLINE: Vector2.Multiply(ref impulse, inverseMass, out _dv);
            //_dv.X = amount.X * inverseMass;
            //_dv.Y = amount.Y * inverseMass;
            //#endregion

            #region INLINE: Vector2.Add(ref amount, ref impulse, out impulse);

            impulse.X += amount.X;
            impulse.Y += amount.Y;

            #endregion
        }

        /// <summary>
        /// Applies then clears all the external impulses that were accumulated
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
        /// <param name="amount"></param>
        internal void ApplyImmediateImpulse(ref Vector2 amount)
        {
            if (isStatic)
                return;

            #region INLINE: Vector2.Multiply(ref impulse, inverseMass, out _dv);

            _dv.X = amount.X * inverseMass;
            _dv.Y = amount.Y * inverseMass;

            #endregion

            #region INLINE: Vector2.Add(ref _dv, ref linearVelocity, out linearVelocity);

            LinearVelocity.X = _dv.X + LinearVelocity.X;
            LinearVelocity.Y = _dv.Y + LinearVelocity.Y;

            #endregion
        }

        /// <summary>
        /// Clears the impulse of the body.
        /// </summary>
        public void ClearImpulse()
        {
            impulse.X = 0;
            impulse.Y = 0;
        }

        /// <summary>
        /// Applies angular impulse.
        /// </summary>
        /// <param name="amount">The amount.</param>
        public void ApplyAngularImpulse(float amount)
        {
            AngularVelocity += amount * inverseMomentOfInertia;
        }

        /// <summary>
        /// Applies angular impulse.
        /// </summary>
        /// <param name="amount">The amount.</param>
        public void ApplyAngularImpulse(ref float amount)
        {
            AngularVelocity += amount * inverseMomentOfInertia;
        }

        /// <summary>
        /// Use internally by the engine to integrate the velocity.
        /// </summary>
        /// <param name="dt">The delta time.</param>
        internal void IntegrateVelocity(float dt)
        {
            //Calculate linear drag
            #region INLINE: Vector2.Multiply(ref linearVelocity, -_linearDragCoefficient, out _linearDrag);

            _linearDrag.X = -LinearVelocity.X * LinearDragCoefficient;
            _linearDrag.Y = -LinearVelocity.Y * LinearDragCoefficient;

            #endregion

            //Quadratic drag for "fast" moving objects. Must be enabled first.
            if (IsQuadraticDragEnabled)
            {
                float num = (LinearVelocity.X * LinearVelocity.X) + (LinearVelocity.Y * LinearVelocity.Y);
                _speed = (float)Math.Sqrt(num);

                _linearDrag.X = _linearDrag.X + (-_speed * QuadraticDragCoefficient * Math.Sign(LinearVelocity.X));
                _linearDrag.Y = _linearDrag.Y + (-_speed * QuadraticDragCoefficient * Math.Sign(LinearVelocity.Y));
            }

            ApplyForce(ref _linearDrag);

            //Calculate rotational drag and apply it as torque
            _rotationalDrag = AngularVelocity * AngularVelocity * Math.Sign(AngularVelocity);
            _rotationalDrag *= -RotationalDragCoefficient;
            ApplyTorque(_rotationalDrag);

            //Calculate acceleration
            #region INLINE: Vector2.Multiply(ref force, inverseMass, out _acceleration);

            _acceleration.X = force.X * inverseMass;
            _acceleration.Y = force.Y * inverseMass;

            #endregion

            #region INLINE: Vector2.Multiply(ref _acceleration, dt, out _dv);

            _dv.X = _acceleration.X * dt;
            _dv.Y = _acceleration.Y * dt;

            #endregion

            _previousLinearVelocity = LinearVelocity;

            #region INLINE: Vector2.Add(ref _previousLinearVelocity, ref _dv, out linearVelocity);

            LinearVelocity.X = _previousLinearVelocity.X + _dv.X;
            LinearVelocity.Y = _previousLinearVelocity.Y + _dv.Y;

            #endregion

            //Angular velocity
            _dw = _torque * inverseMomentOfInertia * dt;
            _previousAngularVelocity = AngularVelocity;
            AngularVelocity = _previousAngularVelocity + _dw;
        }

        /// <summary>
        /// Use internally by the engine to integrate the position.
        /// </summary>
        /// <param name="dt">The delta time.</param>
        internal void IntegratePosition(float dt)
        {
            //
            //Linear
            //

            //Linear velocity
            _tempLinearVelocity.X = LinearVelocity.X + linearVelocityBias.X;
            _tempLinearVelocity.Y = LinearVelocity.Y + linearVelocityBias.Y;

            //Position change
            _tempDeltaPosition.X = _tempLinearVelocity.X * dt;
            _tempDeltaPosition.Y = _tempLinearVelocity.Y * dt;

            if (_tempDeltaPosition != Vector2.Zero)
            {
                //Save old position
                _previousPosition = position;

                //Apply position change
                position.X = _previousPosition.X + _tempDeltaPosition.X;
                position.Y = _previousPosition.Y + _tempDeltaPosition.Y;
                _changed = true;
            }

            //Reset linear velocity bias
            linearVelocityBias.X = 0;
            linearVelocityBias.Y = 0;

            //
            //Angular
            //

            //Angular velocity
            _tempAngularVelocity = AngularVelocity + angularVelocityBias;

            //Rotation change
            _tempDeltaRotation = _tempAngularVelocity * dt;

            if (_tempDeltaRotation != 0f)
            {
                //Save old rotation
                _previousRotation = rotation;

                //Apply rotation change
                rotation = _previousRotation + _tempDeltaRotation;
                _changed = true;

                //Calculate floating point remainder of rotation
                int z = (int)(rotation / MathHelper.TwoPi);
                if (rotation < 0)
                    z--;
                rotation = (rotation - z * MathHelper.TwoPi);

                _revolutions += z;

                totalRotation = rotation + _revolutions * MathHelper.TwoPi;
            }

            //Reset angular velocity bias
            angularVelocityBias = 0;

            if (_changed)
            {
                if (Updated != null)
                {
                    Updated(ref position, ref rotation);
                }
            }
        }

        /// <summary>
        /// Applies a impulse at the specified world offset.
        /// </summary>
        /// <param name="amount">The amount.</param>
        /// <param name="offset">The offset.</param>
        internal void ApplyImpulseAtWorldOffset(ref Vector2 amount, ref Vector2 offset)
        {
            #region INLINE: Vector2.Multiply(ref impulse, inverseMass, out _dv);

            _dv.X = amount.X * inverseMass;
            _dv.Y = amount.Y * inverseMass;

            #endregion

            #region INLINE: Vector2.Add(ref _dv, ref linearVelocity, out linearVelocity);

            LinearVelocity.X = _dv.X + LinearVelocity.X;
            LinearVelocity.Y = _dv.Y + LinearVelocity.Y;

            #endregion

            #region INLINE: Calculator.Cross(ref offset, ref impulse, out angularImpulse);

            float angularImpulse = offset.X * amount.Y - offset.Y * amount.X;

            #endregion

            angularImpulse *= inverseMomentOfInertia;
            AngularVelocity += angularImpulse;
        }

        /// <summary>
        /// Applies a bias impulse at the specified world offset.
        /// </summary>
        /// <param name="impulseBias">The impulse bias.</param>
        /// <param name="offset">The offset.</param>
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

        #region IDisposable Members

        public void Dispose()
        {
            IsDisposed = true;

            if (Disposed != null)
            {
                Disposed(this, EventArgs.Empty);
            }

            GC.SuppressFinalize(this);
        }

        #endregion

        #region GetWorldPosition variables

        private Vector2 _worldPositionTemp = Vector2.Zero;

        #endregion

        #region IntegeratePosition variables

        private float _tempAngularVelocity;
        private Vector2 _tempLinearVelocity;
        private Vector2 _tempDeltaPosition;
        private float _tempDeltaRotation;
        private bool _changed;

        #endregion

        #region IntegerateVelocity variables

        private Vector2 _acceleration = Vector2.Zero;
        private Vector2 _dv = Vector2.Zero; //change in linear velocity
        private float _dw; //change in angular velocity 

        #endregion

        #region ApplyDrag variables

        private Vector2 _linearDrag = Vector2.Zero;
        private float _rotationalDrag;
        private float _speed;

        #endregion

        #region ApplyForceAtLocalPoint variables

        private Vector2 _diff;
        private Vector2 _tempVelocity = Vector2.Zero;

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

        #region IIsDisposable Members

#if(XNA)
        [ContentSerializerIgnore]
#endif
        [XmlIgnore]
        public bool IsDisposed
        {
            get { return _isDisposed; }
            set { _isDisposed = value; }
        }

        #endregion
    }
}