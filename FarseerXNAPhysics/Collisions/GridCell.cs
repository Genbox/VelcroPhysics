using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using Microsoft.Xna.Framework;

using FarseerGames.FarseerXNAPhysics.Mathematics;

namespace FarseerGames.FarseerXNAPhysics.Collisions
{
    public sealed class GridCell {
        private bool _isPrimed = false;
        private bool _isOutside = false;
        private GridPoint[] _gridPoints;

        internal GridCell() {
            _gridPoints = new GridPoint[4];
        }

        internal bool IsOutside {
            get { return _isOutside; }
        }

        internal GridPoint[] GridPoints {
            get { return _gridPoints; }
        }

        internal void Prime(Geometry geometry) {
            _isOutside = true;
            if (_isPrimed) return;
            for (int i = 0; i < 4; i++) {
                _gridPoints[i].Prime(geometry);
                if (!_gridPoints[i].IsOutside) { _isOutside = false; }
            }
            _isPrimed = true;
        }

        internal Feature Evaluate(Vector2 point) {
            float top = 0;
            float bottom = 0;
            float left = 0;
            float right = 0;
            Feature feature = new Feature(point);
            //validate point is in boundry


            ComputeTopBottom(point,ref top,ref bottom);
            ComputeLeftRight(point, ref left, ref right);

            //interpolate to find distance
            feature.Distance = GetBilinearInterpolatedDistance(point,top, bottom);

            //calc gradient to find normal
            feature.Normal = new Vector2(right - left, bottom - top);
            feature.Normal.Normalize();            
            return feature;
        }

        private void ComputeTopBottom(Vector2 point, ref float top, ref float bottom){
            float denominatorXInverse = 1f / (_gridPoints[2].Position.X - _gridPoints[1].Position.X);
            float multiplierX1 = (_gridPoints[2].Position.X - point.X) * denominatorXInverse;
            float multiplierX2 = (point.X - _gridPoints[1].Position.X) * denominatorXInverse;

             bottom = _gridPoints[1].Distance * multiplierX1 + _gridPoints[2].Distance * multiplierX2;
             top = _gridPoints[0].Distance * multiplierX1 + _gridPoints[3].Distance * multiplierX2;
        }

        private void ComputeLeftRight(Vector2 point, ref float left, ref float right) {
            float denominatorYInverse = 1f / (_gridPoints[1].Position.Y - _gridPoints[0].Position.Y);
            float multiplierY1 = (_gridPoints[1].Position.Y - point.Y) * denominatorYInverse;
            float multiplierY2 = (point.Y - _gridPoints[0].Position.Y) * denominatorYInverse;

            right = _gridPoints[3].Distance * multiplierY1 + _gridPoints[2].Distance * multiplierY2;
            left = _gridPoints[0].Distance * multiplierY1 + _gridPoints[1].Distance * multiplierY2;
        }

        private float GetBilinearInterpolatedDistance(Vector2 point,float top, float bottom ) {
            float denominatorYInverse = 1f / (_gridPoints[1].Position.Y - _gridPoints[0].Position.Y);
            float distance = (point.Y - _gridPoints[0].Position.Y) * denominatorYInverse * bottom +
                             (_gridPoints[1].Position.Y - point.Y) * denominatorYInverse * top;
            return distance;
        }

    }
}
