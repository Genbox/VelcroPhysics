using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using Microsoft.Xna.Framework;

using FarseerGames.FarseerXNAPhysics.Mathematics;

namespace FarseerGames.FarseerXNAPhysics.Dynamics {
    public class RevoluteJoint : Joint {
        private Matrix _matrix;
        private Vector2 _localAnchor1;
        private Vector2 _localAnchor2;

        private Vector2 _r1;
        private Vector2 _r2;

        private float _biasFactor = .1f;
        private Vector2 _bias;
        private float _relaxation;
        private Vector2 _accumulatedImpulse;

        public RevoluteJoint(Body body1, Body body2, Vector2 anchor) {
            this._body1 = body1;
            this._body2 = body2;

            //Matrix  rotation1

            _localAnchor1 = body1.GetLocalPosition(anchor);
            _localAnchor2 = body2.GetLocalPosition(anchor);

            _accumulatedImpulse = Vector2.Zero;
            _relaxation = 1f;            
        }

        public override void PreStep(float inverseDt) {
            _r1 = Vector2.TransformNormal(_localAnchor1, _body1.BodyMatrix);
            _r2 = Vector2.TransformNormal(_localAnchor2, _body2.BodyMatrix);

            Matrix K1 = new Matrix();
            K1.M11 = _body1.InverseMass + _body2.InverseMass;
            K1.M12 = 0;
            K1.M21 = 0;
            K1.M22 = _body1.InverseMass + _body2.InverseMass;

            Matrix K2 = new Matrix();
            K2.M11 = _body1.InverseMomentOfInertia * _r1.Y * _r1.Y;
            K2.M12 = -_body1.InverseMomentOfInertia * _r1.X*_r1.Y;
            K2.M21 = -_body1.InverseMomentOfInertia * _r1.X*_r1.Y;
            K2.M22 = _body1.InverseMomentOfInertia * _r1.X*_r1.X;

            Matrix K3 = new Matrix();
            K3.M11 = _body2.InverseMomentOfInertia * _r2.Y * _r2.Y;
            K3.M12 = -_body2.InverseMomentOfInertia * _r2.X * _r2.Y;
            K3.M21 = -_body2.InverseMomentOfInertia * _r2.X * _r2.Y;
            K3.M22 = _body2.InverseMomentOfInertia * _r2.X * _r2.X;

            Matrix K = K1 + K2 + K3;

            _matrix = MatrixInvert2D(K);

            Vector2 position1 = _body1.Position + _r1;
            Vector2 position2 = _body2.Position + _r2;
            Vector2 difference = position2 - position1;
            _bias = -_biasFactor * inverseDt * difference;

            _accumulatedImpulse *= _relaxation;

            _body1.LinearVelocity -= _body1.InverseMass * _accumulatedImpulse;
            _body1.AngularVelocity -= _body1.InverseMomentOfInertia * Calculator.Cross(_r1, _accumulatedImpulse);

            _body2.LinearVelocity += _body2.InverseMass * _accumulatedImpulse;
            _body2.AngularVelocity += _body2.InverseMomentOfInertia * Calculator.Cross(_r2, _accumulatedImpulse);
        }

        public Matrix MatrixInvert2D(Matrix matrix) {
            float a = matrix.M11, b = matrix.M12, c = matrix.M21, d = matrix.M22;
            Matrix B = new Matrix();
            float det = a * d - b * c;
            Debug.Assert(det != 0.0f);
            det = 1.0f / det;
            B.M11 =  det * d;	B.M12 = -det * b;
            B.M21 = -det * c;	B.M22 =  det * a;
            return B;
        }

        public override void Update() {
            Vector2 dv = _body2.LinearVelocity + Calculator.Cross(_body2.AngularVelocity, _r2) - _body1.LinearVelocity - Calculator.Cross(_body1.AngularVelocity, _r1);
            Vector2 dvBias = -dv + _bias;
            Vector2 impulse = Vector2.Transform(dvBias, _matrix);

            _body1.LinearVelocity -= _body1.InverseMass * impulse;
            _body1.AngularVelocity -= _body1.InverseMomentOfInertia * Calculator.Cross(_r1, impulse);

            _body2.LinearVelocity += _body2.InverseMass * impulse;
            _body2.AngularVelocity += _body2.InverseMomentOfInertia * Calculator.Cross(_r2, impulse);

            _accumulatedImpulse += impulse;

        }

    }
}
