using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

#if (XNA)
using Microsoft.Xna.Framework; 
#endif

using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Mathematics;

namespace FarseerGames.FarseerPhysics.Dynamics {
    public class InteractiveAngleJoint {
        private float angle1;
        private float angle2;
        private float angle3;
        private float angle4;
        private float angleMin;
        private float angleMax;
        private CircularInterpolator circularInterpolator;
        RevoluteJoint revoluteJoint;
        AngleJoint angleJoint;
        float softness;
        Body body1;
        Body body2;
        Vector2 anchor;

        public InteractiveAngleJoint(float angle1, float angle2, float angle3, float angle4, float angleMin, float angleMax, float softness, Vector2 anchor, Body body1, Body body2) {
            this.angle1 = angle1;
            this.angle2 = angle2;
            this.angle3 = angle3;
            this.angle4 = angle4;
            this.angleMin = angleMin;
            this.angleMax = angleMax;
            this.softness = softness;
            this.body1 = body1;
            this.body2 = body2;
            this.anchor = anchor;

            this.revoluteJoint = new RevoluteJoint(body1, body2, anchor);
            revoluteJoint.Softness = softness;
            this.angleJoint = new AngleJoint(body1, body2);
            circularInterpolator = new CircularInterpolator(angle1, angle2, angle3, angle4, angleMin, angleMax);
        }

        public RevoluteJoint RevoluteJoint {
            get { return revoluteJoint; }
            set { revoluteJoint = value; }
        }

        public AngleJoint AngleJoint {
            get { return angleJoint; }
            set { angleJoint = value; }
        }

        public void LoadPhysicsContent(PhysicsSimulator physicsSimulator) {
            physicsSimulator.Add(revoluteJoint);
            physicsSimulator.Add(angleJoint);
        }

        public void UnloadPhysicsContent(PhysicsSimulator physicsSimulator) {
            physicsSimulator.Remove(revoluteJoint);
            physicsSimulator.Remove(angleJoint);
        }

        public void CalculateJoint(float x, float y) {
            float angle = circularInterpolator.GetValue(x, y);
            angleJoint.TargetAngle = angle;
        }
    }
}
