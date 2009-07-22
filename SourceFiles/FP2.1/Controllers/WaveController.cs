using System;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Interfaces;
using FarseerGames.FarseerPhysics.Mathematics;

#if (XNA)
using Microsoft.Xna.Framework;
#endif

namespace FarseerGames.FarseerPhysics.Controllers
{
    /// <summary>
    /// The WaveController simulates wave motion. It's driven by a mathematical algorithm (not physics) which dynamically 
    /// alters a polygonal shape to mimic waves.
    /// 
    /// The WaveController can be viewed as a rectangle shape but with the top of the rectangle broken into 
    /// multiple segments defined by a set of vertices. When the algorithm is operating, any disturbance in 
    /// the y-position of one of these vertices will cause a wave to ripple across the other vertices.
    /// 
    /// The speed and shape of the wave depend on a number of parameters and on the number of vertices used
    /// to define the surface of the water.
    /// 
    /// The WaveController can also (but does not have to) work in conjunction with the <see cref="FluidDragController"/> in
    /// order to have fluid physics applied to any body that falls within the area defined by the WaveController.
    /// 
    /// The WaveController also implements some wave generator functionality. By default, the WaveController will
    /// just sit still until one or more of its vertices are disturbed.  The wave generator is simply a controlled
    /// means of disturbing the vertices of the WaveController.  The wave generator acts by moving the right-most 
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
    public class WaveController : Controller, IFluidContainer
    {
        private AABB _aabb;
        private float _aabbMin = float.MaxValue;
        private float[] _currentWave;
        private float _dampningCoefficient = .95f;
        private float _frequency = .18f; //seconds;
        private bool _goingUp = true;
        private float _height;
        private int _nodeCount;
        private Vector2 _pointVector;
        private Vector2 _position;

        private float[] _previousWave;
        private float[] _resultWave;
        private float _singleWaveWidth;
        private float _timePassed;

        private Vector2 _waveEdgeVector;
        private float _waveGeneratorCount;

        private float _waveGeneratorMax;
        private float _waveGeneratorMin;
        private float _waveGeneratorStep;
        private float _width;
        private float[] _xPosition;
        public Vector2 VectorFarWaveEdge;
        public Vector2 VectorNearWaveEdge;
        public Vector2 VectorPoint;

        #region properties

        /// <summary>
        /// The width of the wave area.
        /// </summary>
        public float Width
        {
            get { return _width; }
            set { _width = value; }
        }

        /// <summary>
        /// The height of the wave area. Best thought of as the depth of the water.
        /// </summary>
        public float Height
        {
            get { return _height; }
            set { _height = value; }
        }

        /// <summary>
        /// Top left position of wave area
        /// </summary>
        public Vector2 Position
        {
            get { return _position; }
            set { _position = value; }
        }

        /// <summary>
        /// How many vertices to use for the surface of the water. Determines, along with other parameters, the shape of the waves.
        /// </summary>
        public int NodeCount
        {
            get { return _nodeCount; }
            set { _nodeCount = value; }
        }

        /// <summary>
        /// Determines how quickly the waves dissipate.  A Value of zero will cause any disturbance in the water surface to ripple forever. 
        /// Values closer to 1 will cause the waves to smooth out quickly.
        /// </summary>
        public float DampingCoefficient
        {
            get { return _dampningCoefficient; }
            set { _dampningCoefficient = value; }
        }

        /// <summary>
        /// An array representing the current y-offset of each vertice from its rest position.  The "rest" position is the same as the
        /// y-component of the "Position" property of the <see cref="WaveController"/>.  These Value can be used to visually represent the wave.
        /// </summary>
        public float[] CurrentWave
        {
            get { return _currentWave; }
        }

        /// <summary>
        /// Used by the algorithm but not really needed externally.
        /// </summary>
        public float[] PreviousWave
        {
            get { return _previousWave; }
        }

        /// <summary>
        /// An array of x positions that represents the x position of the vertices that make up the wave.
        /// This can be used to create the visuals for your wave.
        /// </summary>
        public float[] XPosition
        {
            get { return _xPosition; }
            set { _xPosition = value; }
        }

        /// <summary>
        /// The max offset that you want the wave generator to move the control vertice
        /// </summary>
        public float WaveGeneratorMax
        {
            get { return _waveGeneratorMax; }
            set { _waveGeneratorMax = value; }
        }

        /// <summary>
        /// The min offset that you want the wave generator to move the control vertice
        /// </summary>
        public float WaveGeneratorMin
        {
            get { return _waveGeneratorMin; }
            set { _waveGeneratorMin = value; }
        }

        /// <summary>
        /// How many steps you want it to take for the wave generator to move the control vertices between the
        /// min and max.  The vertice will be moved every time Update(..) runs which in-turn is controlled by the 
        /// Frequency property.
        /// </summary>
        public float WaveGeneratorStep
        {
            get { return _waveGeneratorStep; }
            set { _waveGeneratorStep = value; }
        }

        /// <summary>
        /// Determines how fast the wave algorithm (NOT the wave generator) runs. The best way to understand this property 
        /// is to try some different values and watch the affect.
        /// </summary>
        public float Frequency
        {
            get { return _frequency; }
            set { _frequency = value; }
        }

        #endregion

        #region IFluidContainer Members

        public bool Intersect(ref AABB aabb)
        {
            return AABB.Intersect(ref aabb, ref  _aabb);
        }

        public bool Contains(ref Vector2 vector)
        {
            //try
            //{
            int index = (int)Math.Floor((vector.X - _xPosition[0]) / _singleWaveWidth);

            //handle the boundry conditions
            if (index > _nodeCount - 2) index = _nodeCount - 2;
            if (index < 0) index = 0;

            VectorNearWaveEdge.X = _xPosition[index];
            VectorNearWaveEdge.Y = _position.Y + _currentWave[index];

            VectorFarWaveEdge.X = _xPosition[index + 1];
            VectorFarWaveEdge.Y = _position.Y + _currentWave[index + 1];

            VectorPoint = vector;

            _waveEdgeVector.X = _xPosition[index + 1] - _xPosition[index];
            _waveEdgeVector.Y = _currentWave[index + 1] - _currentWave[index];

            _pointVector.X = vector.X - _xPosition[index];
            _pointVector.Y = vector.Y - (_position.Y + _currentWave[index]);

            float perpDot;
            Calculator.Cross(ref _waveEdgeVector, ref _pointVector, out perpDot);

            if (perpDot < 0)
            {
                return false;
            }
            return true;
            //}
            //catch (Exception ex)
            //{
            //    throw ex;
            //}
        }

        #endregion

        /// <summary>
        /// Create a disturbance in the water surface that will create waves.  The disturbance created will "ripple" across
        /// the surface of the "water" based on the parameters that define the <see cref="WaveController"/>.
        /// This could be used to create waves when something falls in the water. For this,though, you would need to determine what vertices
        /// to move and how far.
        /// </summary>
        /// <param name="x">The node to change the height of</param>
        /// <param name="offset">The amount to move the node up or down (negative values moves the node up, positive moves it down)</param>
        public void Disturb(float x, float offset)
        {
            int i = 0;

            for (i = 0; i < _nodeCount - 1; i++)
            {
                if (x >= _xPosition[i] && x <= _xPosition[i + 1])
                    _currentWave[i] = _currentWave[i] + offset;
            }
        }

        /// <summary>
        /// Initialize the wave controller.
        /// </summary>
        public void Initialize()
        {
            _xPosition = new float[_nodeCount];
            _currentWave = new float[_nodeCount];
            _previousWave = new float[_nodeCount];
            _resultWave = new float[_nodeCount];

            for (int i = 0; i < _nodeCount; i++)
            {
                _xPosition[i] = MathHelper.Lerp(_position.X, _position.X + _width, (float)i / (_nodeCount - 1));
                _currentWave[i] = 0;
                _previousWave[i] = 0;
                _resultWave[i] = 0;
            }

            Vector2 max = new Vector2(_position.X + _width, _position.Y + _height);
            _aabb = new AABB(ref _position, ref max);
            _singleWaveWidth = _width / (_nodeCount - 1);
        }

        /// <summary>
        /// Steps the wave algorithm.  The wave algorithm does not run at the same speed as the physics simulator. It runs at its
        /// own frequency set by the Frequency property.
        /// </summary>
        /// <param name="dt">The time since last update.</param>
        /// <param name="dtReal">The real time since last update.</param>
        public override void Update(float dt, float dtReal)
        {
            if (_timePassed < _frequency)
            {
                _timePassed += dt;
                return;
            }
            _timePassed = 0;

            _aabbMin = float.MaxValue;
            _aabb.min.Y = _aabbMin;
            for (int i = 1; i < _nodeCount - 1; i++)
            {
                _resultWave[i] = (_currentWave[i - 1] + _currentWave[i + 1]) - _previousWave[i];
                _resultWave[i] = _resultWave[i] * _dampningCoefficient;

                //keep track of _aabb min Value                
                if (_resultWave[i] + _position.Y < _aabbMin)
                {
                    _aabbMin = _resultWave[i] + _position.Y;
                }
            }
            _aabb.min.Y = _aabbMin;
            _currentWave.CopyTo(_previousWave, 0);
            _resultWave.CopyTo(_currentWave, 0);

            if (_goingUp)
            {
                if (_waveGeneratorCount > _waveGeneratorMax)
                {
                    _goingUp = false;
                }
                else
                {
                    _waveGeneratorCount += _waveGeneratorStep;
                }
            }
            else
            {
                if (_waveGeneratorCount < _waveGeneratorMin)
                {
                    _goingUp = true;
                }
                else
                {
                    _waveGeneratorCount -= _waveGeneratorStep;
                }
            }
            _currentWave[_currentWave.Length - 1] = _waveGeneratorCount;
        }

        public override void Validate()
        {
            //just do nothing for now. will revisit later.
        }
    }
}