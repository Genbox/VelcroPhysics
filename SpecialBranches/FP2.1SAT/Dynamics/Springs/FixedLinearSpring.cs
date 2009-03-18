#if (XNA)
using Microsoft.Xna.Framework;
#else
using FarseerGames.FarseerPhysics.Mathematics;
#endif

namespace FarseerGames.FarseerPhysics.Dynamics.Springs
{
    /// <summary>
    /// Fixed linear spring attaches a body to a fixed point.
    /// The linear spring is acting kind of like a rubber band.
    /// </summary>
    public class FixedLinearSpring : Spring
    {
        private Body _body;
        private Vector2 _bodyAttachPoint;
        private Vector2 _difference = Vector2.Zero;
        private float _restLength;
        private Vector2 _worldAttachPoint;

        public FixedLinearSpring()
        {
        }

        public FixedLinearSpring(Body body, Vector2 bodyAttachPoint, Vector2 worldAttachPoint, float springConstant,
                                 float dampingConstant)
        {
            _body = body;
            _bodyAttachPoint = bodyAttachPoint;
            _worldAttachPoint = worldAttachPoint;
            SpringConstant = springConstant;
            DampingConstant = dampingConstant;
            _difference = worldAttachPoint - _body.GetWorldPosition(bodyAttachPoint);
            _restLength = _difference.Length();
        }

        /// <summary>
        /// Gets or sets the body.
        /// </summary>
        /// <Value>The body.</Value>
        public Body Body
        {
            get { return _body; }
            set { _body = value; }
        }

        /// <summary>
        /// Gets or sets the position.
        /// </summary>
        /// <Value>The position.</Value>
        public Vector2 Position
        {
            get { return _worldAttachPoint; }
            set { _worldAttachPoint = value; }
        }

        /// <summary>
        /// Gets or sets the body attach point.
        /// </summary>
        /// <Value>The body attach point.</Value>
        public Vector2 BodyAttachPoint
        {
            get { return _bodyAttachPoint; }
            set { _bodyAttachPoint = value; }
        }

        /// <summary>
        /// Gets or sets the world attach point.
        /// </summary>
        /// <Value>The world attach point.</Value>
        public Vector2 WorldAttachPoint
        {
            get { return _worldAttachPoint; }
            set { _worldAttachPoint = value; }
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
            if (_body.IsDisposed)
            {
                Dispose();
            }
        }

        public override void Update(float dt)
        {
            base.Update(dt);

            if (IsDisposed)
            {
                return;
            }

            if (_body.isStatic)
            {
                return;
            }

            //calculate and apply spring _force
            //F = -{s(L-r) + d[(v1-v2).L]/l}L/l   : s=spring const, d = dampning const, L=_difference vector (p1-p2), l = _difference magnitude, r = rest length,            
            _body.GetWorldPosition(ref _bodyAttachPoint, out _bodyWorldPoint);
            Vector2.Subtract(ref _bodyWorldPoint, ref _worldAttachPoint, out _difference);
            float differenceMagnitude = _difference.Length();
            if (differenceMagnitude < _epsilon)
            {
                return;
            } //if already close to rest length then return

            //calculate spring _force (kX)
            SpringError = differenceMagnitude - _restLength;
            Vector2.Normalize(ref _difference, out _differenceNormalized);
            _springForce = SpringConstant*SpringError; //kX

            //calculate relative velocity 
            _body.GetVelocityAtLocalPoint(ref _bodyAttachPoint, out _bodyVelocity);

            //calculate dampning _force (bV)
            Vector2.Dot(ref _bodyVelocity, ref _difference, out _temp);
            _dampningForce = DampingConstant*_temp/differenceMagnitude; //bV     

            //calculate final _force (spring + dampning)
            Vector2.Multiply(ref _differenceNormalized, -(_springForce + _dampningForce), out _force);

            _body.ApplyForceAtLocalPoint(ref _force, ref _bodyAttachPoint);
        }

        #region ApplyForce variables

        private const float _epsilon = .00001f;
        private Vector2 _bodyVelocity = Vector2.Zero;
        private Vector2 _bodyWorldPoint = Vector2.Zero;
        private float _dampningForce;
        private Vector2 _differenceNormalized;
        private Vector2 _force;
        private float _springForce;
        private float _temp;

        #endregion
    }
}