using System.Diagnostics;
using FarseerPhysics.Common;
using FarseerPhysics.Common.PolygonManipulation;
using FarseerPhysics.Testbed.Framework;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Testbed.Tests
{
    public class SimplificationTest : Test
    {
        private Vertices _upperLeft;
        private Vertices _upperRight;
        private Vertices _lowerLeft;
        private Vertices _lowerRight;
        private Vertices _twoShape;

        public override void Initialize()
        {
            base.Initialize();

            GameInstance.ViewCenter = Vector2.Zero;

            _twoShape = new Vertices
            {
                new Vector2(5.510646f,-6.312136f),
                new Vector2(5.510646f,-9.534955f),
                new Vector2(-6.016356f,-9.534955f),
                new Vector2(-6.016356f,-6.837597f),
                new Vector2(-1.933609f,-0.7320573f),
                new Vector2(-0.6714239f,1.242431f),
                new Vector2(0.07130214f,2.498066f),
                new Vector2(0.4939168f,3.344996f),
                new Vector2(0.7957863f,4.093408f),
                new Vector2(0.9769094f,4.743301f),
                new Vector2(1.037288f,5.294677f),
                new Vector2(0.9643505f,5.967545f),
                new Vector2(0.7455474f,6.44485f),
                new Vector2(0.5806286f,6.610869f),
                new Vector2(0.3776243f,6.729462f),
                new Vector2(0.1365345f,6.80062f),
                new Vector2(-0.1426414f,6.824349f),
                new Vector2(-0.4218241f,6.798073f),
                new Vector2(-0.6629166f,6.719252f),
                new Vector2(-0.8659183f,6.587883f),
                new Vector2(-1.030829f,6.403981f),
                new Vector2(-1.158469f,6.141973f),
                new Vector2(-1.249639f,5.776335f),
                new Vector2(-1.32257f,4.734189f),
                new Vector2(-1.32257f,2.935948f),
                new Vector2(-6.016356f,2.935948f),
                new Vector2(-6.016356f,3.624884f),
                new Vector2(-5.970973f,5.045072f),
                new Vector2(-5.834826f,6.129576f),
                new Vector2(-5.710837f,6.586056f),
                new Vector2(-5.520398f,7.0389f),
                new Vector2(-5.263501f,7.488094f),
                new Vector2(-4.940154f,7.933653f),
                new Vector2(-4.556844f,8.350358f),
                new Vector2(-4.120041f,8.71307f),
                new Vector2(-3.629755f,9.02178f),
                new Vector2(-3.085981f,9.276493f),
                new Vector2(-2.487104f,9.475718f),
                new Vector2(-1.8315f,9.618026f),
                new Vector2(-1.119165f,9.703418f),
                new Vector2(-0.3501012f,9.731889f),
                new Vector2(1.117107f,9.644661f),
                new Vector2(1.779295f,9.535644f),
                new Vector2(2.393876f,9.383026f),
                new Vector2(2.960846f,9.186799f),
                new Vector2(3.480206f,8.946972f),
                new Vector2(3.951957f,8.663539f),
                new Vector2(4.376098f,8.336502f),
                new Vector2(5.076675f,7.592458f),
                new Vector2(5.577088f,6.755733f),
                new Vector2(5.877342f,5.82633f),
                new Vector2(5.977431f,4.804249f),
                new Vector2(5.921109f,3.981021f),
                new Vector2(5.752138f,3.134446f),
                new Vector2(5.470524f,2.264521f),
                new Vector2(5.076274f,1.371247f),
                new Vector2(4.406482f,0.2123121f),
                new Vector2(3.298271f,-1.454563f),
                new Vector2(1.751642f,-3.629379f),
                new Vector2(-0.233405f,-6.312136f),
            };

            int beforeCount = _twoShape.Count;
            _twoShape.AddRange(_twoShape); //Duplicate the points
            _twoShape = SimplifyTools.MergeIdenticalPoints(_twoShape);

            Debug.Assert(beforeCount == _twoShape.Count); //The merge should have removed all duplicate points.

            const int xOffset = 18;
            const int yOffset = 18;

            _upperLeft = new Vertices(_twoShape);
            _upperLeft.Translate(new Vector2(-xOffset, yOffset));
            _upperLeft = SimplifyTools.ReduceByArea(_upperLeft, 0.1f);

            _upperRight = new Vertices(_twoShape);
            _upperRight.Translate(new Vector2(xOffset, yOffset));
            _upperRight = SimplifyTools.ReduceByNth(_upperRight, 3);

            _lowerLeft = new Vertices(_twoShape);
            _lowerLeft.Translate(new Vector2(-xOffset, -yOffset));
            _lowerLeft = SimplifyTools.ReduceByDistance(_lowerLeft, 0.5f);

            _lowerRight = new Vertices(_twoShape);
            _lowerRight.Translate(new Vector2(xOffset, -yOffset));
            _lowerRight = SimplifyTools.DouglasPeuckerSimplify(_lowerRight, 0.5f);
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            DrawString(string.Format("Center ({0}): Original polygon", _twoShape.Count));
            
            DrawString(string.Format("Upper left ({0}): Simplified by removing points with an area of below 0.1", _upperLeft.Count));
            
            DrawString(string.Format("Upper right ({0}): Simplified by removing every 3 point", _upperRight.Count));
            
            DrawString(string.Format("Lower left ({0}): Simplified by removing points with a distance of less than 1", _lowerLeft.Count));
            
            DrawString(string.Format("Lower right ({0}): Simplified with Douglas Peucker", _lowerRight.Count));

            DebugView.BeginCustomDraw(ref GameInstance.Projection, ref GameInstance.View);

            DrawVertices(_twoShape);
            DrawVertices(_upperLeft);
            DrawVertices(_upperRight);
            DrawVertices(_lowerLeft);
            DrawVertices(_lowerRight);

            DebugView.EndCustomDraw();

            base.Update(settings, gameTime);
        }

        private void DrawVertices(Vertices vertices)
        {
            if (vertices.Count >= 1)
            {
                DebugView.DrawPolygon(vertices.ToArray(), vertices.Count, Color.Red);

                foreach (Vector2 vector2 in vertices)
                {
                    DebugView.DrawPoint(vector2, 0.1f, Color.Yellow);
                }
            }
        }

        public static Test Create()
        {
            return new SimplificationTest();
        }
    }
}