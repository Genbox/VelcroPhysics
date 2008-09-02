using System;
using FarseerGames.FarseerPhysics.Mathematics;

namespace FarseerGames.FarseerPhysics.Dynamics.Springs
{
    public class LinearSpring : Controller
    {
        private const float _epsilon = .00001f;
        private float _breakpoint = float.MaxValue;

        private float _dampningConstant;
        private float _dampningForce;
        private Vector2 _difference = Vector2.Zero;
        private Vector2 _differenceNormalized = Vector2.Zero;
        private Vector2 _force;
        private Vector2 _relativeVelocity = Vector2.Zero;
        private float _restLength;
        private float _springConstant;

        private float _springError;
        private float _springForce;
        private float _temp;
        private Vector2 _velocityAtPoint1 = Vector2.Zero;
        private Vector2 _velocityAtPoint2 = Vector2.Zero;
        private Vector2 _worldPoint1 = Vector2.Zero;
        private Vector2 _worldPoint2 = Vector2.Zero;
        internal Vector2 attachPoint1;
        internal Vector2 attachPoint2;
        protected Body body1;
        protected Body body2;

        public LinearSpring()
        {
        }

        public LinearSpring(Body body1, Vector2 attachPoint1, Body body2, Vector2 attachPoint2, float springConstant,
                            float dampningConstant)
        {
            this.body1 = body1;
            this.body2 = body2;
            this.attachPoint1 = attachPoint1;
            this.attachPoint2 = attachPoint2;
            _springConstant = springConstant;
            _dampningConstant = dampningConstant;
            _difference = body2.GetWorldPosition(attachPoint2) - body1.GetWorldPosition(attachPoint1);
            _restLength = _difference.Length();
        }

        public Body Body1
        {
            get { return body1; }
            set { body1 = value; }
        }

        public Body Body2
        {
            get { return body2; }
            set { body2 = value; }
        }

        public Vector2 AttachPoint1
        {
            get { return attachPoint1; }
            set { attachPoint1 = value; }
        }

        public Vector2 AttachPoint2
        {
            get { return attachPoint2; }
            set { attachPoint2 = value; }
        }

        public float SpringConstant
        {
            get { return _springConstant; }
            set { _springConstant = value; }
        }

        public float DampningConstant
        {
            get { return _dampningConstant; }
            set { _dampningConstant = value; }
        }

        public float RestLength
        {
            get { return _restLength; }
            set { _restLength = value; }
        }

        public float Breakpoint
        {
            get { return _breakpoint; }
            set { _breakpoint = value; }
        }

        public float SpringError
        {
            get { return _springError; }
        }

        public event EventHandler<EventArgs> Broke;

        public override void Validate()
        {
            //if either of the joint's connected bodies are disposed then dispose the joint.
            if (body1.IsDisposed || body2.IsDisposed)
            {
                Dispose();
            }
        }

        public override void Update(float dt)
        {
            if (Enabled && Math.Abs(_springError) > _breakpoint)
            {
                Enabled = false;
                if (Broke != null) Broke(this, new EventArgs());
            }

            if (isDisposed)
            {
                return;
            }
            //calculate and apply spring _force
            //F = -{s(L-r) + d[(v1-v2).L]/l}L/l   : s=spring const, d = dampning const, L=_difference vector (p1-p2), l = _difference magnitude, r = rest length,
            body1.GetWorldPosition(ref attachPoint1, out _worldPoint1);
            body2.GetWorldPosition(ref attachPoint2, out _worldPoint2);
            Vector2.Subtract(ref _worldPoint1, ref _worldPoint2, out _difference);
            float differenceMagnitude = _difference.Length();
            if (differenceMagnitude < _epsilon)
            {
                return;
            } //if already close to rest length then return

            //calculate spring _force
            _springError = differenceMagnitude - _restLength;
            Vector2.Normalize(ref _difference, out _differenceNormalized);
            _springForce = _springConstant*_springError; //kX

            //calculate relative velocity
            body1.GetVelocityAtLocalPoint(ref attachPoint1, out _velocityAtPoint1);
            body2.GetVelocityAtLocalPoint(ref attachPoint2, out _velocityAtPoint2);
            Vector2.Subtract(ref _velocityAtPoint1, ref _velocityAtPoint2, out _relativeVelocity);

            //calculate dampning _force
            Vector2.Dot(ref _relativeVelocity, ref _difference, out _temp);
            _dampningForce = _dampningConstant*_temp/differenceMagnitude; //bV     

            //calculate final _force (spring + dampning)
            Vector2.Multiply(ref _differenceNormalized, -(_springForce + _dampningForce), out _force);

            if (!body1.IsStatic)
            {
                body1.ApplyForceAtLocalPoint(ref _force, ref attachPoint1);
            }

            if (!body2.IsStatic)
            {
                Vector2.Multiply(ref _force, -1, out _force);
                body2.ApplyForceAtLocalPoint(ref _force, ref attachPoint2);
            }
        }
    }
}