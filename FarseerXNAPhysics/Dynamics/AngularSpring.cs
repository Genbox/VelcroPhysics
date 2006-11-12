using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using Microsoft.Xna.Framework;

using FarseerGames.FarseerXNAPhysics.Mathematics;

namespace FarseerGames.FarseerXNAPhysics.Dynamics {
    public class AngularSpring : Spring {
        private float springConstant;
        private float dampningConstant;
        private float targetAngle;

        private Vector2 body1XVector;
        private Vector2 body2XVector;

        public AngularSpring(Body body1, Body body2,float springConstant, float dampningConstant) {
            _body1 = body1;
            _body2 = body2;
            this.springConstant = springConstant;
            this.dampningConstant = dampningConstant;
                
            targetAngle = GetAngle(); 
        }

        public float TargetAngle {
            get { return targetAngle; }
            set { targetAngle = value; }
        }
	

        private float GetAngle() {
            body1XVector = _body1.UnitWorldXDirection;
            body2XVector = _body2.UnitWorldXDirection;

            //Debug.WriteLine(body1XVector.ToString() + " : " + body2XVector.ToString());            

            float angle = Calculator.ATan2(body2XVector.Y, body2XVector.X) - Calculator.ATan2(body1XVector.Y, body1XVector.X);

            if (angle > Math.PI) {
                angle -= Calculator.PiX2;         
            }

            if (angle < -Math.PI) {
                angle += Calculator.PiX2;
            }
            return angle;            
        }

        public override void Update(float dt) {


            //calculate and apply spring force
            float angleDifference = GetAngle() - targetAngle;
            float springTorque = springConstant * angleDifference;           

            //apply torque at anchor
            if (!_body1.IsStatic) {
                float torque1 = springTorque - dampningConstant * _body1.AngularVelocity;
                //Debug.WriteLine("torque1: " + torque1.ToString());
                _body1.ApplyTorque(torque1);

            }

            if (!_body2.IsStatic) {
                float torque2 = -springTorque - dampningConstant * _body2.AngularVelocity;
                //Debug.WriteLine("torque1: " + torque2.ToString());
                _body2.ApplyTorque(torque2);
            }
        }
    }
}
