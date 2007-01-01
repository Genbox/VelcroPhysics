using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using Microsoft.Xna.Framework;

namespace FarseerGames.FarseerXNAPhysics.Dynamics {
    public class FixedSpring : Spring {
        private Vector2 _bodyAttachPoint;
        private Vector2 _worldAttachPoint;

        private float _springConstant;
        private float _dampningConstant;
        private float _restLength;

        public FixedSpring(Body body, Vector2 _bodyAttachPoint, Vector2 worldAttachPoint, float springConstant, float dampningConstant) {
            _body1 = body;
            _body2 = null;
            this._bodyAttachPoint = _bodyAttachPoint;
            this._worldAttachPoint = worldAttachPoint;
            this._springConstant = springConstant;
            this._dampningConstant = dampningConstant;
            Vector2 difference = worldAttachPoint - _body1.GetWorldPosition(_bodyAttachPoint);
            _restLength = difference.Length();
        }

        public Vector2 Position {
            get { return _worldAttachPoint; }
            set { _worldAttachPoint = value; }
        }

        public override void Update(float dt) {
            //calculate and apply spring force
            //F = -kX - bV
            Vector2 bodyWorldPoint = _body1.GetWorldPosition(_bodyAttachPoint);
            Vector2 bodyVelocity = _body1.GetVelocityAtPoint(_bodyAttachPoint);
            Vector2 difference = _worldAttachPoint - bodyWorldPoint - dt * bodyVelocity;

            float stretch = difference.Length() - _restLength;

            if (difference.Length() > 0) {
                difference.Normalize();
            }

            Vector2 force;

            if (!_body1.IsStatic) {
                force = _springConstant * Vector2.Multiply(difference, stretch) + Vector2.Multiply(-bodyVelocity, _dampningConstant);
                _body1.ApplyForceAtLocalPoint(force, _bodyAttachPoint);
            }
        }
    }
}
