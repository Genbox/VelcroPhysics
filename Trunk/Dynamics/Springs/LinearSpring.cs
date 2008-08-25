using System;
using FarseerGames.FarseerPhysics.Mathematics;

namespace FarseerGames.FarseerPhysics.Dynamics.Springs
{
    public class LinearSpring : Controller
    {
        private const float _epsilon = .00001f;
        private float _dampningForce;
        private Vector2 _difference = Vector2.Zero;
        private Vector2 _differenceNormalized = Vector2.Zero;
        private Vector2 _force;
        private Vector2 _relativeVelocity = Vector2.Zero;

        private float _springForce;
        private float _temp;
        private Vector2 _velocityAtPoint1 = Vector2.Zero;
        private Vector2 _velocityAtPoint2 = Vector2.Zero;
        private Vector2 _worldPoint1 = Vector2.Zero;
        private Vector2 _worldPoint2 = Vector2.Zero;
        internal Vector2 attachPoint1;
        internal Vector2 attachPoint2;

        public LinearSpring()
        {
            Breakpoint = float.MaxValue;
        }

        public LinearSpring(Body body1, Vector2 attachPoint1, Body body2, Vector2 attachPoint2, float springConstant,
                            float dampningConstant)
        {
            Breakpoint = float.MaxValue;
            Body1 = body1;
            Body2 = body2;
            this.attachPoint1 = attachPoint1;
            this.attachPoint2 = attachPoint2;
            SpringConstant = springConstant;
            DampningConstant = dampningConstant;
            _difference = body2.GetWorldPosition(attachPoint2) - body1.GetWorldPosition(attachPoint1);
            RestLength = _difference.Length();
        }

        public Body Body1 { get; set; }
        public Body Body2 { get; set; }

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

        public float SpringConstant { get; set; }
        public float DampningConstant { get; set; }
        public float RestLength { get; set; }
        public float Breakpoint { get; set; }
        public float SpringError { get; private set; }
        public event EventHandler<EventArgs> Broke;

        public override void Validate()
        {
            //if either of the joint's connected bodies are disposed then dispose the joint.
            if (Body1.IsDisposed || Body2.IsDisposed)
            {
                Dispose();
            }
        }

        public override void Update(float dt)
        {
            if (Enabled && Math.Abs(SpringError) > Breakpoint)
            {
                Enabled = false;
                if (Broke != null) Broke(this, new EventArgs());
            }

            if (IsDisposed)
            {
                return;
            }
            //calculate and apply spring force
            //F = -{s(L-r) + d[(v1-v2).L]/l}L/l   : s=spring const, d = dampning const, L=difference vector (p1-p2), l = difference magnitude, r = rest length,
            Body1.GetWorldPosition(ref attachPoint1, out _worldPoint1);
            Body2.GetWorldPosition(ref attachPoint2, out _worldPoint2);
            Vector2.Subtract(ref _worldPoint1, ref _worldPoint2, out _difference);
            float differenceMagnitude = _difference.Length();
            if (differenceMagnitude < _epsilon)
            {
                return;
            } //if already close to rest length then return

            //calculate spring force
            SpringError = differenceMagnitude - RestLength;
            Vector2.Normalize(ref _difference, out _differenceNormalized);
            _springForce = SpringConstant*SpringError; //kX

            //calculate relative velocity
            Body1.GetVelocityAtLocalPoint(ref attachPoint1, out _velocityAtPoint1);
            Body2.GetVelocityAtLocalPoint(ref attachPoint2, out _velocityAtPoint2);
            Vector2.Subtract(ref _velocityAtPoint1, ref _velocityAtPoint2, out _relativeVelocity);

            //calculate dampning force
            Vector2.Dot(ref _relativeVelocity, ref _difference, out _temp);
            _dampningForce = DampningConstant*_temp/differenceMagnitude; //bV     

            //calculate final force (spring + dampning)
            Vector2.Multiply(ref _differenceNormalized, -(_springForce + _dampningForce), out _force);

            if (!Body1.IsStatic)
            {
                Body1.ApplyForceAtLocalPoint(ref _force, ref attachPoint1);
            }

            if (!Body2.IsStatic)
            {
                Vector2.Multiply(ref _force, -1, out _force);
                Body2.ApplyForceAtLocalPoint(ref _force, ref attachPoint2);
            }
        }

        //Note: Redundant
        //protected override void Dispose(bool disposing)
        //{
        //    base.Dispose(disposing);
        //}
    }
}