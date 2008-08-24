using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

#if (XNA)
using Microsoft.Xna.Framework; 
#endif

namespace FarseerGames.FarseerPhysics.Mathematics {
    /// <summary>
    /// Encapsulates the logic to do bilinear interpolation
    /// .V1    .V4
    /// .V2    .V3
    /// </summary>
    public class BilinearInterpolator {
        Vector2 max;
        Vector2 min;

        float value1 = 0;
        float value2 = 0;
        float value3 = 0;
        float value4 = 0;

        float maxValue = float.MaxValue;
        float minValue = float.MinValue;

        public BilinearInterpolator()
        { }
        
        public BilinearInterpolator(Vector2 min, Vector2 max) {
            this.max = max;
            this.min = min;            
        }

        public BilinearInterpolator(Vector2 min, Vector2 max, float value1, float value2, float value3, float value4, float minValue, float maxValue)
        {
            this.min = min;
            this.max = max;
            this.value1 = value1;
            this.value2 = value2;
            this.value3 = value3;
            this.value4 = value4;
            this.minValue = minValue;
            this.maxValue = maxValue;
        }

        public Vector2 Min
        {
            get { return min; }
            set { min = value; }
        }

        public Vector2 Max
        {
            get { return max; }
            set { max = value; }
        }

        public float GetValue(Vector2 position) {
            return GetValue(position.X, position.Y);
        }

         public float GetValue(float x, float y) {
            float value;

            x = MathHelper.Clamp(x, min.X, max.X);
            y = MathHelper.Clamp(y, min.Y, max.Y);
        
            float xRatio = (x - min.X)/(max.X - min.X);
            float yRatio = (y - min.Y)/(max.Y - min.Y);

            float top = MathHelper.Lerp(value1, value4, xRatio);
            float bottom = MathHelper.Lerp(value2, value3, xRatio);

            value = MathHelper.Lerp(top, bottom, yRatio);
            value = MathHelper.Clamp(value, minValue, maxValue);
            return value;  
         }
    }
}
