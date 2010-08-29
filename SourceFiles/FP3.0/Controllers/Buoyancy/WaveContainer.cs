using System;
using FarseerPhysics.Collision;
using FarseerPhysics.Common;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Controllers.Buoyancy
{
    /// <summary>
    /// The waveContainer simulates wave motion. It's driven by a mathematical algorithm (not physics) which dynamically 
    /// alters a polygonal shape to mimic waves.
    /// 
    /// The waveContainer can be viewed as a rectangle shape but with the top of the rectangle broken into 
    /// multiple segments defined by a set of vertices. When the algorithm is operating, any disturbance in 
    /// the y-position of one of these vertices will cause a wave to ripple across the other vertices.
    /// 
    /// The speed and shape of the wave depend on a number of parameters and on the number of vertices used
    /// to define the surface of the water.
    /// 
    /// The waveContainer can also (but does not have to) work in conjunction with the <see cref="FluidDragController"/> in
    /// order to have fluid physics applied to any body that falls within the area defined by the waveContainer.
    /// 
    /// The waveContainer also implements some wave generator functionality. By default, the waveContainer will
    /// just sit still until one or more of its vertices are disturbed.  The wave generator is simply a controlled
    /// means of disturbing the vertices of the waveContainer.  The wave generator acts by moving the right-most 
    /// vertice up and down at a rate defined by a combination of WaveGeneratorMax, WaveGeneratorMin, and WaveGeneratorStep.
    /// 
    /// You can visualize the wave generator by imagining a person holding a string that is attached to a wall. If 
    /// the person were to move their arm holding the string up and down to create a wave like motion in the string, they
    /// would be acting very similar to how the wave generator works.
    ///  
    /// 
    /// If you want the details behind the wave algorithm see the following:
    /// http://freespace.virgin.net/hugo.elias/graphics/x_water.htm
    /// http://www.gamedev.net/reference/articles/article915.asp
    ///  
    /// </summary>
    public class WaveContainer : IFluidContainer
    {
        public Vector2 VectorFarWaveEdge;
        public Vector2 VectorNearWaveEdge;
        private AABB _aabb;
        private float _aabbMin = float.MaxValue;
        private bool _goingUp = true;
        private Vector2 _pointVector;

        private float[] _resultWave;
        private float _singleWaveWidth;
        private float _timePassed;

        private Vector2 _waveEdgeVector;
        private float _waveGeneratorCount;

        public WaveContainer(Vector2 position, float width, float height, int nodeCount)
        {
            //Default values
            Frequency = .001f;
            DampingCoefficient = .98f;
            WaveGeneratorMax = 4;
            WaveGeneratorMin = 2;
            WaveGeneratorStep = 0.2f;

            //Generate nodecount from width
            if (nodeCount <= 0)
                nodeCount = (int) width * (int) (width * 0.1f);

            NodeCount = nodeCount;
            Position = position;
            Width = width;
            Height = height;

            XPosition = new float[nodeCount];
            YPosition = new float[nodeCount];
            PreviousWave = new float[nodeCount];
            _resultWave = new float[nodeCount];

            for (short i = 0; i < nodeCount; i++)
            {
                XPosition[i] = MathHelper.Lerp(position.X, position.X + width, (float) i / (nodeCount - 1));
                YPosition[i] = 0;
                PreviousWave[i] = 0;
                _resultWave[i] = 0;
            }

            Vector2 max = new Vector2(position.X + width, position.Y + height);
            _aabb = new AABB(ref position, ref max);
            _singleWaveWidth = width / (nodeCount - 1);
        }

        public WaveContainer(Vector2 position, float width, float height)
            : this(position, width, height, 0)
        {
        }

        /// <summary>
        /// The width of the wave area.
        /// </summary>
        public float Width { get; set; }

        /// <summary>
        /// The height of the wave area. Best thought of as the depth of the water.
        /// </summary>
        public float Height { get; set; }

        /// <summary>
        /// Top left position of wave area
        /// </summary>
        public Vector2 Position { get; set; }

        /// <summary>
        /// How many vertices to use for the surface of the water. Determines, along with other parameters, the shape of the waves.
        /// </summary>
        public int NodeCount { get; set; }

        /// <summary>
        /// Determines how quickly the waves dissipate.  A Value of zero will cause any disturbance in the water surface to ripple forever. 
        /// Values closer to 1 will cause the waves to smooth out quickly.
        /// </summary>
        public float DampingCoefficient { get; set; }

        /// <summary>
        /// An array representing the current y-offset of each vertice from its rest position.  The "rest" position is the same as the
        /// y-component of the "Position" property of the <see cref="WaveContainer"/>.  These Value can be used to visually represent the wave.
        /// </summary>
        public float[] YPosition { get; private set; }

        /// <summary>
        /// Used by the algorithm but not really needed externally.
        /// </summary>
        public float[] PreviousWave { get; private set; }

        /// <summary>
        /// An array of x positions that represents the x position of the vertices that make up the wave.
        /// This can be used to create the visuals for your wave.
        /// </summary>
        public float[] XPosition { get; set; }

        /// <summary>
        /// The max offset that you want the wave generator to move the control vertice
        /// </summary>
        public float WaveGeneratorMax { get; set; }

        /// <summary>
        /// The min offset that you want the wave generator to move the control vertice
        /// </summary>
        public float WaveGeneratorMin { get; set; }

        /// <summary>
        /// How many steps you want it to take for the wave generator to move the control vertices between the
        /// min and max.  The vertice will be moved every time Update(..) runs which in-turn is controlled by the 
        /// Frequency property.
        /// </summary>
        public float WaveGeneratorStep { get; set; }

        /// <summary>
        /// Determines how fast the wave algorithm (NOT the wave generator) runs. The best way to understand this property 
        /// is to try some different values and watch the affect.
        /// </summary>
        public float Frequency { get; set; }

        #region IFluidContainer Members

        public bool Intersect(ref AABB aabb)
        {
            return AABB.TestOverlap(ref aabb, ref _aabb);
        }

        public bool Contains(ref Vector2 vector)
        {
            int index = (int) Math.Floor((vector.X - XPosition[0]) / _singleWaveWidth);

            //handle the boundry conditions
            if (index > NodeCount - 2)
                index = NodeCount - 2;

            if (index < 0)
                index = 0;

            VectorNearWaveEdge.X = XPosition[index];
            VectorNearWaveEdge.Y = Position.Y + YPosition[index];

            VectorFarWaveEdge.X = XPosition[index + 1];
            VectorFarWaveEdge.Y = Position.Y + YPosition[index + 1];

            _waveEdgeVector.X = XPosition[index + 1] - XPosition[index];
            _waveEdgeVector.Y = YPosition[index + 1] - YPosition[index];

            _pointVector.X = vector.X - XPosition[index];
            _pointVector.Y = vector.Y - (Position.Y + YPosition[index]);

            float perpDot;
            MathUtils.Cross(ref _waveEdgeVector, ref _pointVector, out perpDot);

            if (perpDot < 0)
                return false;

            return true;
        }

        /// <summary>
        /// Steps the wave algorithm.  The wave algorithm does not run at the same speed as the physics simulator. It runs at its
        /// own frequency set by the Frequency property.
        /// </summary>
        public void Update(float dt)
        {
            if (_timePassed < Frequency)
            {
                _timePassed += dt;
                return;
            }
            _timePassed = 0;

            _aabbMin = float.MaxValue;
            _aabb.LowerBound.Y = _aabbMin;
            for (int i = 1; i < NodeCount - 1; i++)
            {
                _resultWave[i] = (YPosition[i - 1] + YPosition[i + 1]) - PreviousWave[i];
                _resultWave[i] = _resultWave[i] * DampingCoefficient;

                //keep track of _aabb min Value                
                if (_resultWave[i] + Position.Y < _aabbMin)
                {
                    _aabbMin = _resultWave[i] + Position.Y;
                }
            }
            _aabb.LowerBound.Y = _aabbMin;
            YPosition.CopyTo(PreviousWave, 0);
            _resultWave.CopyTo(YPosition, 0);

            if (_goingUp)
            {
                if (_waveGeneratorCount > WaveGeneratorMax)
                {
                    _goingUp = false;
                }
                else
                {
                    _waveGeneratorCount += WaveGeneratorStep;
                }
            }
            else
            {
                if (_waveGeneratorCount < WaveGeneratorMin)
                {
                    _goingUp = true;
                }
                else
                {
                    _waveGeneratorCount -= WaveGeneratorStep;
                }
            }
            YPosition[YPosition.Length - 1] = _waveGeneratorCount;
        }

        #endregion

        /// <summary>
        /// Create a disturbance in the water surface that will create waves.  The disturbance created will "ripple" across
        /// the surface of the "water" based on the parameters that define the <see cref="WaveContainer"/>.
        /// This could be used to create waves when something falls in the water. For this,though, you would need to determine what vertices
        /// to move and how far.
        /// </summary>
        /// <param name="x">The node to change the height of</param>
        /// <param name="offset">The amount to move the node up or down (negative values moves the node up, positive moves it down)</param>
        public void Disturb(float x, float offset)
        {
            int i;

            for (i = 0; i < NodeCount - 1; i++)
            {
                if (x >= XPosition[i] && x <= XPosition[i + 1])
                    YPosition[i] = YPosition[i] + offset;
            }
        }
    }
}