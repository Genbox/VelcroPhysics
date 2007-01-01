using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using Microsoft.Xna.Framework;

using FarseerGames.FarseerXNAPhysics.Mathematics;

namespace FarseerGames.FarseerXNAPhysics.Dynamics {
    public class AngularSpring : Spring {
        private float _springConstant;
        private float _dampningConstant;
        private float _targetAngle;

        public AngularSpring(Body body1, Body body2,float springConstant, float dampningConstant) {
            _body1 = body1;
            _body2 = body2;
            this._springConstant = springConstant;
            this._dampningConstant = dampningConstant;
            _targetAngle = _body2.TotalOrientation - _body1.TotalOrientation;
        }

        public float TargetAngle {
            get { return _targetAngle; }
            set {
                _targetAngle = value;
                if (_targetAngle > 5.5) { _targetAngle = 5.5f; }
                if (_targetAngle < -5.5f) { _targetAngle = -5.5f; }
            }
        }

        public override void Update(float dt) {
            //calculate and apply spring force
            float angle = _body2.TotalOrientation - _body1.TotalOrientation;
            float angleDifference = _body2.TotalOrientation - (_body1.TotalOrientation + _targetAngle);
            float springTorque = _springConstant * angleDifference;           

            //apply torque at anchor
            if (!_body1.IsStatic) {
                float torque1 = springTorque - _dampningConstant * _body1.AngularVelocity;
                _body1.ApplyTorque(torque1);

            }

            if (!_body2.IsStatic) {
                float torque2 = -springTorque - _dampningConstant * _body2.AngularVelocity;
                _body2.ApplyTorque(torque2);
            }
        }
    }
}
