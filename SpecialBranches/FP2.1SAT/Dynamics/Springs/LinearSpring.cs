#if (XNA)
using Microsoft.Xna.Framework;
#else
using FarseerGames.FarseerPhysics.Mathematics;
#endif

namespace FarseerGames.FarseerPhysics.Dynamics.Springs
{
    /// <summary>
    /// Attaches 2 bodies with a spring. Works kind of like a rubber band.
    /// </summary>
    public class LinearSpring : Spring
    {
        private const float _epsilon = .00001f;
        private Vector2 _attachPoint1;
        private Vector2 _attachPoint2;
        private Body _body1;
        private Body _body2;

        private float _dampningForce;
        private Vector2 _difference = Vector2.Zero;
        private Vector2 _differenceNormalized = Vector2.Zero;
        private Vector2 _force;
        private Vector2 _relativeVelocity = Vector2.Zero;
        private float _restLength;

        private float _springForce;
        private float _temp;
        private Vector2 _velocityAtPoint1 = Vector2.Zero;
        private Vector2 _velocityAtPoint2 = Vector2.Zero;
        private Vector2 _worldPoint1 = Vector2.Zero;
        private Vector2 _worldPoint2 = Vector2.Zero;

        public LinearSpring()
        {
        }

        public LinearSpring(Body body1, Vector2 attachPoint1, Body body2, Vector2 attachPoint2, float springConstant,
                            float dampingConstant)
        {
            _body1 = body1;
            _body2 = body2;
            _attachPoint1 = attachPoint1;
            _attachPoint2 = attachPoint2;
            SpringConstant = springConstant;
            DampingConstant = dampingConstant;
            _difference = body2.GetWorldPosition(attachPoint2) - body1.GetWorldPosition(attachPoint1);
            _restLength = _difference.Length();
        }

        /// <summary>
        /// Gets or sets the fist body.
        /// </summary>
        /// <Value>The body1.</Value>
        public Body Body1
        {
            get { return _body1; }
            set { _body1 = value; }
        }

        /// <summary>
        /// Gets or sets the second body.
        /// </summary>
        /// <Value>The body2.</Value>
        public Body Body2
        {
            get { return _body2; }
            set { _body2 = value; }
        }

        /// <summary>
        /// Gets or sets the fist attach point.
        /// </summary>
        /// <Value>The attach point1.</Value>
        public Vector2 AttachPoint1
        {
            get { return _attachPoint1; }
            set { _attachPoint1 = value; }
        }

        /// <summary>
        /// Gets or sets the second attach point.
        /// </summary>
        /// <Value>The attach point2.</Value>
        public Vector2 AttachPoint2
        {
            get { return _attachPoint2; }
            set { _attachPoint2 = value; }
        }

        /// <summary>
        /// Gets or sets the length of the rest.
        /// </summary>
        /// <Value>The length of the rest.</Value>
        public float RestLength
        {
            get { return _restLength; }
            set { _restLength = value; }
        }

        public override void Validate()
        {
            //if either of the joint's connected bodies are disposed then dispose the joint.
            if (_body1.IsDisposed || _body2.IsDisposed)
            {
                Dispose();
            }
        }

        public override void Update(float dt)
        {
            base.Update(dt);

            if (IsDisposed)
                return;

            //calculate and apply spring _force
            //F = -{s(L-r) + d[(v1-v2).L]/l}L/l   : s=spring const, d = dampning const, L=_difference vector (p1-p2), l = _difference magnitude, r = rest length,
            _body1.GetWorldPosition(ref _attachPoint1, out _worldPoint1);
            _body2.GetWorldPosition(ref _attachPoint2, out _worldPoint2);
            Vector2.Subtract(ref _worldPoint1, ref _worldPoint2, out _difference);
            float differenceMagnitude = _difference.Length();
            if (differenceMagnitude < _epsilon)
            {
                return;
            } //if already close to rest length then return

            //calculate spring _force
            SpringError = differenceMagnitude - _restLength;
            Vector2.Normalize(ref _difference, out _differenceNormalized);
            _springForce = SpringConstant*SpringError; //kX

            //calculate relative velocity
            _body1.GetVelocityAtLocalPoint(ref _attachPoint1, out _velocityAtPoint1);
            _body2.GetVelocityAtLocalPoint(ref _attachPoint2, out _velocityAtPoint2);
            Vector2.Subtract(ref _velocityAtPoint1, ref _velocityAtPoint2, out _relativeVelocity);

            //calculate dampning _force
            Vector2.Dot(ref _relativeVelocity, ref _difference, out _temp);
            _dampningForce = DampingConstant*_temp/differenceMagnitude; //bV     

            //calculate final _force (spring + dampning)
            Vector2.Multiply(ref _differenceNormalized, -(_springForce + _dampningForce), out _force);

            if (!_body1.IsStatic)
            {
                _body1.ApplyForceAtLocalPoint(ref _force, ref _attachPoint1);
            }

            if (!_body2.IsStatic)
            {
                Vector2.Multiply(ref _force, -1, out _force);
                _body2.ApplyForceAtLocalPoint(ref _force, ref _attachPoint2);
            }
        }
    }
}