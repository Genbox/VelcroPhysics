using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;

using FarseerGames.FarseerXNAPhysics.Mathematics;

namespace FarseerGames.FarseerXNAPhysics.Collisions {
    public sealed class GridPoint {
        private Vector2 _position;
        private float _distance;
        private Vector2 _normal = Vector2.Zero;
        private bool _isPrimed = false;

        internal GridPoint(float x, float y) {
            _position = new Vector2(x, y);
        }

        internal GridPoint(Vector2 position) {
            _position = position;
        }

        internal Vector2 Position {
            get { return _position; }
        }

        public float Distance {
            get { return _distance; }
        }

        internal bool IsOutside {
            get {
                if (_distance > 0) {
                    return true;
                }
                else {
                    return false;
                }
            }
        }

        public Vector2 GetWorldPosition(Matrix matrix) {
            Vector2 worldPosition = Vector2.Transform(_position, matrix);
            return worldPosition;
        }

        public Vector2 GetWorldNormal(Matrix matrix) {
            Vector2 worldNormal = Vector2.Transform(_normal, matrix);
            return worldNormal;
        }

        internal void Prime(Geometry geometry) {
            //Calculate signed distance and normal
            if (_isPrimed) return;
            Feature nearestFeature = geometry.GetNearestFeature(_position);
            
            //determine if inside or outside of geometry.
            Vector2 pointToFeaturePosition = Vector2.Subtract(_position, nearestFeature.Position);
            float dot = Vector2.Dot(pointToFeaturePosition, nearestFeature.Normal);

            if (dot < 0) {
                _distance = -nearestFeature.Distance;
            }
            else {
                _distance = nearestFeature.Distance;
            }
            _isPrimed = true;
        }
    }
}
