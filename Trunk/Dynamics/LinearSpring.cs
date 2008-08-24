using System;
using FarseerGames.FarseerPhysics.Mathematics;

namespace FarseerGames.FarseerPhysics.Dynamics
{
    public class LinearSpring : Controller
    {
        internal Vector2 attachPoint1;
        internal Vector2 attachPoint2;
        protected Body body1;
        protected Body body2;
        private float breakpoint = float.MaxValue;

        private float dampningConstant;
        private float dampningForce;
        private Vector2 difference = Vector2.Zero;
        private Vector2 differenceNormalized = Vector2.Zero;
        private float epsilon = .00001f;
        private Vector2 force;
        private Vector2 relativeVelocity = Vector2.Zero;
        private float restLength;
        private float springConstant;

        private float springError;
        private float springForce;
        private float temp;
        //Note: Cleanup, variable never used
        //private Vector2 vectorTemp2 = Vector2.Zero;
        private Vector2 velocityAtPoint1 = Vector2.Zero;
        private Vector2 velocityAtPoint2 = Vector2.Zero;
        private Vector2 worldPoint1 = Vector2.Zero;
        private Vector2 worldPoint2 = Vector2.Zero;

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
            this.springConstant = springConstant;
            this.dampningConstant = dampningConstant;
            difference = body2.GetWorldPosition(attachPoint2) - body1.GetWorldPosition(attachPoint1);
            restLength = difference.Length();
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
            get { return springConstant; }
            set { springConstant = value; }
        }

        public float DampningConstant
        {
            get { return dampningConstant; }
            set { dampningConstant = value; }
        }

        public float RestLength
        {
            get { return restLength; }
            set { restLength = value; }
        }

        public float Breakpoint
        {
            get { return breakpoint; }
            set { breakpoint = value; }
        }

        public float SpringError
        {
            get { return springError; }
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
            if (Enabled && Math.Abs(springError) > breakpoint)
            {
                Enabled = false;
                if (Broke != null) Broke(this, new EventArgs());
            }

            if (isDisposed)
            {
                return;
            }
            //calculate and apply spring force
            //F = -{s(L-r) + d[(v1-v2).L]/l}L/l   : s=spring const, d = dampning const, L=difference vector (p1-p2), l = difference magnitude, r = rest length,
            body1.GetWorldPosition(ref attachPoint1, out worldPoint1);
            body2.GetWorldPosition(ref attachPoint2, out worldPoint2);
            Vector2.Subtract(ref worldPoint1, ref worldPoint2, out difference);
            float differenceMagnitude = difference.Length();
            if (differenceMagnitude < epsilon)
            {
                return;
            } //if already close to rest length then return

            //calculate spring force
            springError = differenceMagnitude - restLength;
            Vector2.Normalize(ref difference, out differenceNormalized);
            springForce = springConstant*springError; //kX

            //calculate relative velocity
            body1.GetVelocityAtLocalPoint(ref attachPoint1, out velocityAtPoint1);
            body2.GetVelocityAtLocalPoint(ref attachPoint2, out velocityAtPoint2);
            Vector2.Subtract(ref velocityAtPoint1, ref velocityAtPoint2, out relativeVelocity);

            //calculate dampning force
            Vector2.Dot(ref relativeVelocity, ref difference, out temp);
            dampningForce = dampningConstant*temp/differenceMagnitude; //bV     

            //calculate final force (spring + dampning)
            Vector2.Multiply(ref differenceNormalized, -(springForce + dampningForce), out force);

            if (!body1.IsStatic)
            {
                body1.ApplyForceAtLocalPoint(ref force, ref attachPoint1);
            }

            if (!body2.IsStatic)
            {
                Vector2.Multiply(ref force, -1, out force);
                body2.ApplyForceAtLocalPoint(ref force, ref attachPoint2);
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}