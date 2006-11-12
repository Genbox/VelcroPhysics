using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using Microsoft.Xna.Framework;

namespace FarseerGames.FarseerXNAPhysics.Dynamics {
    public class LinearSpring : Spring {
        private Vector2 attachPoint1;
        private Vector2 attachPoint2;

        private float springConstant;
        private float dampningConstant;
        private float restLength;
        
        public LinearSpring(Body body1, Vector2 attachPoint1, Body body2, Vector2 attachPoint2, float springConstant, float dampningConstant) {
            _body1 = body1;
            _body2 = body2;
            this.attachPoint1 = attachPoint1;
            this.attachPoint2 = attachPoint2;
            this.springConstant = springConstant;
            this.dampningConstant = dampningConstant;
            Vector2 difference = body2.GetWorldPosition(attachPoint2) - body1.GetWorldPosition(attachPoint1);
            restLength = difference.Length();
        }

        public override void Update(float dt) {
            //calculate and apply spring force
            //F = -kX - bV
            Vector2 worldPoint1 = _body1.GetWorldPosition(attachPoint1);
            Vector2 worldPoint2 = _body2.GetWorldPosition(attachPoint2);

            Vector2 velocityAtPoint1 = _body1.GetVelocityAtPoint(attachPoint1);
            Vector2 velocityAtPoint2 = _body2.GetVelocityAtPoint(attachPoint2);

            Vector2 relativeVelocity = Vector2.Subtract(velocityAtPoint2, velocityAtPoint1);

            Vector2 difference = worldPoint2 - worldPoint1 +dt * relativeVelocity / 2;



            float stretch = difference.Length() - restLength;


            if (difference.Length() > 0) {
                difference.Normalize();
            }

            Vector2 force;

            if (!_body1.IsStatic) {
                force = springConstant * Vector2.Multiply(difference, stretch) + Vector2.Multiply(relativeVelocity, dampningConstant);
                _body1.ApplyForceAtLocalPoint(force, attachPoint1);
            }

            if (!_body2.IsStatic) {
                force = -springConstant * Vector2.Multiply(difference, stretch) - Vector2.Multiply(relativeVelocity, dampningConstant);
                _body2.ApplyForceAtLocalPoint(force, attachPoint2);
            }
        }
    }
}
