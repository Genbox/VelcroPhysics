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
        public event SpringDelegate SpringUpdated;

        private Vector2 _attachPoint1;
        private Vector2 _attachPoint2;
        private Body _body1;
        private Body _body2;
        private float _restLength;

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
            //if either of the springs's connected bodies are disposed then dispose the spring.
            if (_body1.IsDisposed || _body2.IsDisposed)
            {
                Dispose();
            }
        }

        public override void Update(float dt)
        {
            base.Update(dt);

            //If both bodies can't move. Don't apply forces to them.
            if (_body1.isStatic && _body2.isStatic)
                return;

            if (!_body1.Enabled && !_body2.Enabled)
                return;

            //F = -{s(L-r) + d[(v1-v2).L]/l}L/l   : s=spring const, d = dampning const, L=difference vector (p1-p2), l = difference magnitude, r = rest length,
            _body1.GetWorldPosition(ref _attachPoint1, out _worldPoint1);
            _body2.GetWorldPosition(ref _attachPoint2, out _worldPoint2);

            //Get the difference between the two attachpoints
            Vector2.Subtract(ref _worldPoint1, ref _worldPoint2, out _difference);
            float differenceMagnitude = _difference.Length();

            //If already close to rest length then return
            if (differenceMagnitude < _epsilon)
            {
                return;
            }

            //Calculate spring force
            SpringError = differenceMagnitude - _restLength;
            Vector2.Normalize(ref _difference, out _differenceNormalized);
            _springForce = SpringConstant * SpringError; //kX

            //Calculate relative velocity
            _body1.GetVelocityAtLocalPoint(ref _attachPoint1, out _velocityAtPoint1);
            _body2.GetVelocityAtLocalPoint(ref _attachPoint2, out _velocityAtPoint2);
            Vector2.Subtract(ref _velocityAtPoint1, ref _velocityAtPoint2, out _relativeVelocity);

            //Calculate dampning force
            Vector2.Dot(ref _relativeVelocity, ref _difference, out _temp);
            _dampningForce = DampingConstant * _temp / differenceMagnitude; //bV     

            //Calculate final force (spring + dampning)
            Vector2.Multiply(ref _differenceNormalized, -(_springForce + _dampningForce), out _force);

            bool changed = false;

            if (_force != Vector2.Zero)
            {
                if (!_body1.IsStatic)
                {
                    _body1.ApplyForceAtLocalPoint(ref _force, ref _attachPoint1);
                    changed = true;                  
                }

                if (!_body2.IsStatic)
                {
                    Vector2.Multiply(ref _force, -1, out _force);
                    _body2.ApplyForceAtLocalPoint(ref _force, ref _attachPoint2);
                    changed = true;
                }
            }

            if (changed)
            {
                if (SpringUpdated != null)
                    SpringUpdated(this, _body1, _body2);
            }

        }

        #region ApplyForce variables

        private const float _epsilon = .00001f;
        private float _dampningForce;
        private Vector2 _differenceNormalized;
        private Vector2 _force;
        private float _springForce;
        private float _temp;
        private Vector2 _difference = Vector2.Zero;
        private Vector2 _relativeVelocity = Vector2.Zero;
        private Vector2 _velocityAtPoint1 = Vector2.Zero;
        private Vector2 _velocityAtPoint2 = Vector2.Zero;
        private Vector2 _worldPoint1 = Vector2.Zero;
        private Vector2 _worldPoint2 = Vector2.Zero;
        #endregion
    }
}