using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using Microsoft.Xna.Framework;

namespace FarseerGames.FarseerXNAPhysics.Dynamics {
    public class LinearSpring : ISpring {
        private RigidBody rigidBody1;
        private RigidBody rigidBody2;
        private Vector2 attachPoint1;
        private Vector2 attachPoint2;

        private float springConstant;
        private float dampningConstant;
        private float restLength;
        
        public LinearSpring(RigidBody rigidBody1, Vector2 attachPoint1, RigidBody rigidBody2, Vector2 attachPoint2, float springConstant, float dampningConstant) {
            this.rigidBody1 = rigidBody1;
            this.rigidBody2 = rigidBody2;
            this.attachPoint1 = attachPoint1;
            this.attachPoint2 = attachPoint2;
            this.springConstant = springConstant;
            this.dampningConstant = dampningConstant;
            Vector2 difference = rigidBody2.GetWorldPosition(attachPoint2) - rigidBody1.GetWorldPosition(attachPoint1);
            restLength = difference.Length();
        }

        public void Update(float dt) {
            //calculate and apply spring force
            //F = -kX - bV
            Vector2 worldPoint1 = rigidBody1.GetWorldPosition(attachPoint1);
            Vector2 worldPoint2 = rigidBody2.GetWorldPosition(attachPoint2);

            Vector2 velocityAtPoint1 = rigidBody1.GetVelocityAtPoint(attachPoint1);
            Vector2 velocityAtPoint2 = rigidBody2.GetVelocityAtPoint(attachPoint2);

            Vector2 relativeVelocity = Vector2.Subtract(velocityAtPoint2, velocityAtPoint1);

            Vector2 difference = worldPoint2 - worldPoint1 +dt * relativeVelocity / 2;



            float stretch = difference.Length() - restLength;


            if (difference.Length() > 0) {
                difference.Normalize();
            }

            Vector2 force;

            if (!rigidBody1.IsStatic) {
                force = springConstant * Vector2.Multiply(difference, stretch) + Vector2.Multiply(relativeVelocity, dampningConstant);
                rigidBody1.ApplyForceAtLocalPoint(force, attachPoint1);
            }

            if (!rigidBody2.IsStatic) {
                force = -springConstant * Vector2.Multiply(difference, stretch) - Vector2.Multiply(relativeVelocity, dampningConstant);
                rigidBody2.ApplyForceAtLocalPoint(force, attachPoint2);
            }
        }
    }
}
