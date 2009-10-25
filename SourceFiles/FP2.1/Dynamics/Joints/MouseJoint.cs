using FarseerGames.FarseerPhysics.Mathematics;

#if (XNA)
using Microsoft.Xna.Framework;
#else
using FarseerGames.FarseerPhysics.Mathematics;
#endif

namespace FarseerGames.FarseerPhysics.Dynamics.Joints
{
    /// <summary>
    /// Creates a revolute joint between 2 bodies.
    /// Can be used as wheels on a car.
    /// </summary>
    public class MouseJoint : Joint
    {
        //public event JointDelegate JointUpdated;

        private Vector2 _localAnchor;
        private Vector2 _impulse;

        private Mat22 _mass;		// effective mass for point-to-point constraint.
        private Vector2 _C;			// position error
        private float _maxForce;
        private float _frequencyHz;
        private float _dampingRatio;
        private float _beta;
        private float _gamma;

        public MouseJoint(Body body, Vector2 target, float maxForce, float frequency, float damping)
        {
            Body = body;
            Target = target;

            body.GetLocalPosition(ref target, out _localAnchor);

            _maxForce = maxForce;
            _impulse = Vector2.Zero;

            _frequencyHz = frequency;
            _dampingRatio = damping;

            _beta = 0.0f;
            _gamma = 0.0f;
        }

        /// <summary>
        /// Gets or sets the first body.
        /// </summary>
        /// <Value>The body1.</Value>
        public Body Body { get; set; }

        /// <summary>
        /// Gets or sets the anchor.
        /// </summary>
        /// <Value>The anchor.</Value>
        public Vector2 Target { get; set; }


        /// <summary>
        /// Validates this instance.
        /// </summary>
        public override void Validate()
        {
            if (Body.IsDisposed)
            {
                Dispose();
            }
        }

        /// <summary>
        /// Calculates all the work needed before updating the joint.
        /// </summary>
        /// <param name="inverseDt">The inverse dt.</param>
        public override void PreStep(float inverseDt, float dt)
        {
            if (Body.isStatic)
                return;

            if (!Body.Enabled)
                return;

            Body b = Body;

            float mass = b.Mass;

            // Frequency
            float omega = 2.0f * MathHelper.Pi * _frequencyHz;

            // Damping coefficient
            float d = 2.0f * mass * _dampingRatio * omega;

            // Spring stiffness
            float k = mass * (omega * omega);

            // magic formulas
            // gamma has units of inverse mass.
            // beta has units of inverse time.
            _gamma = 1.0f / (dt * (d + dt * k));
            _beta = inverseDt * k * _gamma;

            // Compute the effective mass matrix.
            //TODO
            Vector2 r = Vector2.Multiply(_localAnchor - b.position, b.rotation);

            // K    = [(1/m1 + 1/m2) * eye(2) - skew(r1) * invI1 * skew(r1) - skew(r2) * invI2 * skew(r2)]
            //      = [1/m1+1/m2     0    ] + invI1 * [r1.y*r1.y -r1.x*r1.y] + invI2 * [r1.y*r1.y -r1.x*r1.y]
            //        [    0     1/m1+1/m2]           [-r1.x*r1.y r1.x*r1.x]           [-r1.x*r1.y r1.x*r1.x]
            float invMass = b.inverseMass;
            float invI = b.inverseMomentOfInertia;

            Mat22 K1 = new Mat22();
            K1.Col1.X = invMass; K1.Col2.X = 0.0f;
            K1.Col1.Y = 0.0f; K1.Col2.Y = invMass;

            Mat22 K2 = new Mat22();
            K2.Col1.X = invI * r.Y * r.Y; K2.Col2.X = -invI * r.X * r.Y;
            K2.Col1.Y = -invI * r.X * r.Y; K2.Col2.Y = invI * r.X * r.X;

            Mat22 K = K1 + K2;
            K.Col1.X += _gamma;
            K.Col2.Y += _gamma;

            _mass = K.GetInverse();

            //TODO
            _C = b.position + r - Target;

            // Cheat with some damping
            b.AngularVelocity *= 0.98f;

            // Warm starting.
            _impulse *= inverseDt;
            _impulse *= dt * inverseDt;
            b.LinearVelocity += invMass * _impulse;
            b.AngularVelocity += invI * Calculator.Cross(ref r, ref _impulse);
        }

        /// <summary>
        /// Updates this instance.
        /// </summary>
        public override void Update(float dt)
        {
            base.Update();

            if (Body.isStatic)
                return;

            if (!Body.Enabled)
                return;

            Body b = Body;

            //TODO
            Vector2 r = Vector2.Multiply(_localAnchor - b.position, b.rotation);

            // Cdot = v + cross(w, r)
            Vector2 Cdot = b.LinearVelocity + Calculator.Cross(b.AngularVelocity, ref r);

            Vector2 v = -(Cdot + _beta * _C + _gamma * _impulse);
            Vector2 impulse = new Vector2(_mass.Col1.X * v.X + _mass.Col2.X * v.Y, _mass.Col1.Y * v.X + _mass.Col2.Y * v.Y);

            Vector2 oldImpulse = _impulse;
            _impulse += impulse;

            float maxImpulse = dt * _maxForce;
            if (_impulse.LengthSquared() > maxImpulse * maxImpulse)
            {
                _impulse *= maxImpulse / _impulse.Length();
            }
            impulse = _impulse - oldImpulse;

            b.LinearVelocity += b.inverseMass * impulse;
            b.AngularVelocity += b.inverseMomentOfInertia * Calculator.Cross(ref r, ref impulse);


            //if (_impulse != Vector2.Zero)
            //{

            //if (JointUpdated != null)
            //    JointUpdated(this, _body1, _body2);
            //}
        }

        public struct Mat22
        {
            public Vector2 Col1, Col2;

            /// <summary>
            /// Construct this matrix using scalars.
            /// </summary>
            public Mat22(float a11, float a12, float a21, float a22)
            {
                Col1.X = a11; Col1.Y = a21;
                Col2.X = a12; Col2.Y = a22;
            }

            /// <summary>
            /// Initialize this matrix using columns.
            /// </summary>
            public void Set(Vector2 c1, Vector2 c2)
            {
                Col1 = c1;
                Col2 = c2;
            }

            /// <summary>
            /// Compute the inverse of this matrix, such that inv(A) * A = identity.
            /// </summary>
            public Mat22 GetInverse()
            {
                float a = Col1.X, b = Col2.X, c = Col1.Y, d = Col2.Y;
                Mat22 B = new Mat22();
                float det = a * d - b * c;
                det = 1.0f / det;
                B.Col1.X = det * d; B.Col2.X = -det * b;
                B.Col1.Y = -det * c; B.Col2.Y = det * a;
                return B;
            }

            public static Mat22 operator +(Mat22 A, Mat22 B)
            {
                Mat22 C = new Mat22();
                C.Set(A.Col1 + B.Col1, A.Col2 + B.Col2);
                return C;
            }
        }
    }
}