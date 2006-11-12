using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using Microsoft.Xna.Framework;

using FarseerGames.FarseerXNAPhysics.Mathematics;

namespace FarseerGames.FarseerXNAPhysics.Dynamics {
    public class RevoluteJoint : Joint {
        private Matrix matrix;
        private Vector2 localAnchor1;
        private Vector2 localAnchor2;

        private Vector2 r1;
        private Vector2 r2;

        private Vector2 bias;
        private float relaxation;
        private Vector2 accumulatedImpulse;

        public RevoluteJoint(Body body1, Body body2, Vector2 anchor) {
            this._body1 = body1;
            this._body2 = body2;

            //Matrix  rotation1

            localAnchor1 = body1.GetLocalPosition(anchor);
            localAnchor2 = body2.GetLocalPosition(anchor);

            accumulatedImpulse = Vector2.Zero;
            relaxation = 1f;            
        }

        public override void PreStep(float inverseDt) {
            r1 = Vector2.TransformNormal(localAnchor1, _body1.BodyMatrix);
            r2 = Vector2.TransformNormal(localAnchor2, _body2.BodyMatrix);

            Matrix K1 = new Matrix();
            K1.M11 = _body1.InverseMass + _body2.InverseMass;
            K1.M12 = 0;
            K1.M21 = 0;
            K1.M22 = _body1.InverseMass + _body2.InverseMass;

            Matrix K2 = new Matrix();
            K2.M11 = _body1.InverseMomentOfInertia * r1.Y * r1.Y;
            K2.M12 = -_body1.InverseMomentOfInertia * r1.X*r1.Y;
            K2.M21 = -_body1.InverseMomentOfInertia * r1.X*r1.Y;
            K2.M22 = _body1.InverseMomentOfInertia * r1.X*r1.X;

            Matrix K3 = new Matrix();
            K3.M11 = _body2.InverseMomentOfInertia * r2.Y * r2.Y;
            K3.M12 = -_body2.InverseMomentOfInertia * r2.X * r2.Y;
            K3.M21 = -_body2.InverseMomentOfInertia * r2.X * r2.Y;
            K3.M22 = _body2.InverseMomentOfInertia * r2.X * r2.X;

            Matrix K = K1 + K2 + K3;

            matrix = MatrixInvert2D(K);

            Vector2 position1 = _body1.Position + r1;
            Vector2 position2 = _body2.Position + r2;
            Vector2 difference = position2 - position1;
            bias = -.1f * inverseDt * difference;

            accumulatedImpulse *= relaxation;

            _body1.LinearVelocity -= _body1.InverseMass * accumulatedImpulse;
            _body1.AngularVelocity -= _body1.InverseMomentOfInertia * Calculator.Cross(r1, accumulatedImpulse);

            _body2.LinearVelocity += _body2.InverseMass * accumulatedImpulse;
            _body2.AngularVelocity += _body2.InverseMomentOfInertia * Calculator.Cross(r2, accumulatedImpulse);
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
            Vector2 dv = _body2.LinearVelocity + Calculator.Cross(_body2.AngularVelocity, r2) - _body1.LinearVelocity - Calculator.Cross(_body1.AngularVelocity, r1);
            Vector2 dvBias = -dv + bias;
            Vector2 impulse = Vector2.Transform(dvBias, matrix);

            _body1.LinearVelocity -= _body1.InverseMass * impulse;
            _body1.AngularVelocity -= _body1.InverseMomentOfInertia * Calculator.Cross(r1, impulse);

            _body2.LinearVelocity += _body2.InverseMass * impulse;
            _body2.AngularVelocity += _body2.InverseMomentOfInertia * Calculator.Cross(r2, impulse);

            accumulatedImpulse += impulse;

        }
    }
}
