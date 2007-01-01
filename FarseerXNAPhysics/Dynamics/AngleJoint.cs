using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using Microsoft.Xna.Framework;

using FarseerGames.FarseerXNAPhysics.Mathematics;

namespace FarseerGames.FarseerXNAPhysics.Dynamics {
    public class AngleJoint : Joint {
        private float _biasFactor = .1f;
        private float _bias;
        private float _relaxation;
        private float _accumulatedAngularImpulse;
        //private float oldAccumulatedAngularImpulse;
        private float _targetAngle;

        public AngleJoint(Body body1, Body body2) {
            this._body1 = body1;
            this._body2 = body2;

            _accumulatedAngularImpulse = 0;
            _relaxation = .85f;
            _targetAngle = _body2.TotalOrientation - _body1.TotalOrientation;
        }

        public float TargetAngle {
            get { return _targetAngle; }
            set { _targetAngle = value; }
        }

        public override void PreStep(float inverseDt) {
            float difference;
            difference = (_body2.TotalOrientation - _body1.TotalOrientation) - _targetAngle;

            _bias = -_biasFactor * inverseDt * difference;

            _accumulatedAngularImpulse *= _relaxation;

            _body1.AngularVelocity -= _body1.InverseMomentOfInertia * _accumulatedAngularImpulse;
            _body2.AngularVelocity += _body2.InverseMomentOfInertia * _accumulatedAngularImpulse;
        }

        public override void Update() {
            float angularImpulse;
            angularImpulse = (_bias - _body2.AngularVelocity + _body1.AngularVelocity) / (_body1.InverseMomentOfInertia  + _body2.InverseMomentOfInertia);
            _accumulatedAngularImpulse += angularImpulse;

            _body1.AngularVelocity -= _body1.InverseMomentOfInertia * angularImpulse;
            _body2.AngularVelocity += _body2.InverseMomentOfInertia * angularImpulse;
        }
    }
}
