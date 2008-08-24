using System;
using FarseerGames.FarseerPhysics.Mathematics;

namespace FarseerGames.FarseerPhysics.Dynamics
{
    public class FixedLinearSpring : Controller
    {
        protected Body body;

        internal Vector2 bodyAttachPoint;
        private float breakpoint = float.MaxValue;
        private float dampningConstant;
        private Vector2 difference = Vector2.Zero;
        private float restLength;
        private float springConstant;

        private float springError;
        internal Vector2 worldAttachPoint;

        public FixedLinearSpring()
        {
        }

        public FixedLinearSpring(Body body, Vector2 _bodyAttachPoint, Vector2 worldAttachPoint, float springConstant,
                                 float dampningConstant)
        {
            this.body = body;
            bodyAttachPoint = _bodyAttachPoint;
            this.worldAttachPoint = worldAttachPoint;
            this.springConstant = springConstant;
            this.dampningConstant = dampningConstant;
            difference = worldAttachPoint - this.body.GetWorldPosition(_bodyAttachPoint);
            restLength = difference.Length();
        }

        public Body Body
        {
            get { return body; }
            set { body = value; }
        }

        public Vector2 Position
        {
            get { return worldAttachPoint; }
            set { worldAttachPoint = value; }
        }

        public Vector2 BodyAttachPoint
        {
            get { return bodyAttachPoint; }
            set { bodyAttachPoint = value; }
        }

        public Vector2 WorldAttachPoint
        {
            get { return worldAttachPoint; }
            set { worldAttachPoint = value; }
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
            if (body.IsDisposed)
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
            if (body.isStatic)
            {
                return;
            }

            //calculate and apply spring force
            //F = -{s(L-r) + d[(v1-v2).L]/l}L/l   : s=spring const, d = dampning const, L=difference vector (p1-p2), l = difference magnitude, r = rest length,            
            body.GetWorldPosition(ref bodyAttachPoint, out bodyWorldPoint);
            Vector2.Subtract(ref bodyWorldPoint, ref worldAttachPoint, out difference);
            float differenceMagnitude = difference.Length();
            if (differenceMagnitude < epsilon)
            {
                return;
            } //if already close to rest length then return

            //calculate spring force (kX)
            springError = differenceMagnitude - restLength;
            Vector2.Normalize(ref difference, out differenceNormalized);
            springForce = springConstant*springError; //kX

            //calculate relative velocity 
            body.GetVelocityAtLocalPoint(ref bodyAttachPoint, out bodyVelocity);

            //calculate dampning force (bV)
            Vector2.Dot(ref bodyVelocity, ref difference, out temp);
            dampningForce = dampningConstant*temp/differenceMagnitude; //bV     

            //calculate final force (spring + dampning)
            Vector2.Multiply(ref differenceNormalized, -(springForce + dampningForce), out force);

            body.ApplyForceAtLocalPoint(ref force, ref bodyAttachPoint);
        }

        #region ApplyForce variables

        private Vector2 bodyVelocity = Vector2.Zero;
        private Vector2 bodyWorldPoint = Vector2.Zero;
        private float dampningForce;
        private Vector2 differenceNormalized;
        private float epsilon = .00001f;
        private Vector2 force;
        private float springForce;
        private float temp;
        private Vector2 vectorTemp1 = Vector2.Zero;
        private Vector2 vectorTemp2 = Vector2.Zero;

        #endregion
    }
}