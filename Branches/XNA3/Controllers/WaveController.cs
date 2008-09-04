using System;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Interfaces;
using FarseerGames.FarseerPhysics.Mathematics;
using Microsoft.Xna.Framework;

namespace FarseerGames.FarseerPhysics.Controllers
{
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
        public Vector2 vectorFarWaveEdge;
        public Vector2 vectorNearWaveEdge;
        public Vector2 vectorPoint;

        #region properties

        public float Width
        {
            get { return _width; }
            set { _width = value; }
        }

        public float Height
        {
            get { return _height; }
            set { _height = value; }
        }

        /// <summary>
        /// Top left _position of wave area
        /// </summary>
        public Vector2 Position
        {
            get { return _position; }
            set { _position = value; }
        }

        public int NodeCount
        {
            get { return _nodeCount; }
            set { _nodeCount = value; }
        }

        public float DampningCoefficient
        {
            get { return _dampningCoefficient; }
            set { _dampningCoefficient = value; }
        }

        public float[] CurrentWave
        {
            get { return _currentWave; }
        }

        public float[] PreviousWave
        {
            get { return _previousWave; }
        }

        public float[] XPosition
        {
            get { return _xPosition; }
            set { _xPosition = value; }
        }

        public float WaveGeneratorMax
        {
            get { return _waveGeneratorMax; }
            set { _waveGeneratorMax = value; }
        }

        public float WaveGeneratorMin
        {
            get { return _waveGeneratorMin; }
            set { _waveGeneratorMin = value; }
        }

        public float WaveGeneratorStep
        {
            get { return _waveGeneratorStep; }
            set { _waveGeneratorStep = value; }
        }

        public float Frequency
        {
            get { return _frequency; }
            set { _frequency = value; }
        }

        #endregion

        #region IFluidContainer Members

        public bool Intersect(AABB aabb)
        {
            return AABB.Intersect(aabb, _aabb);
        }

        public bool Contains(ref Vector2 vector)
        {
            //try
            //{
            int index = (int) Math.Floor((vector.X - _xPosition[0])/_singleWaveWidth);

            //handle the boundry conditions
            if (index > _nodeCount - 2) index = _nodeCount - 2;
            if (index < 0) index = 0;

            vectorNearWaveEdge.X = _xPosition[index];
            vectorNearWaveEdge.Y = _position.Y + _currentWave[index];

            vectorFarWaveEdge.X = _xPosition[index + 1];
            vectorFarWaveEdge.Y = _position.Y + _currentWave[index + 1];

            vectorPoint = vector;

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
        /// Create a disturbance in the water surface that will create waves.
        /// </summary>
        /// <param name="node">Which node to change the hieght of</param>
        /// <param name="offset">The amount to move the node up or down (negative values moves the node up, positive moves it down)</param>
        public void Disturb(int node, float offset)
        {
            _currentWave[node] = _currentWave[node] + offset;
        }

        public void Initialize()
        {
            _xPosition = new float[_nodeCount];
            _currentWave = new float[_nodeCount];
            _previousWave = new float[_nodeCount];
            _resultWave = new float[_nodeCount];

            for (int i = 0; i < _nodeCount; i++)
            {
                _xPosition[i] = MathHelper.Lerp(_position.X, _position.X + _width, (float) i/(_nodeCount - 1));
                _currentWave[i] = 0;
                _previousWave[i] = 0;
                _resultWave[i] = 0;
            }

            _aabb = new AABB(_position, new Vector2(_position.X + _width, _position.Y + _height));
            _singleWaveWidth = _width/(_nodeCount - 1);
        }

        public override void Update(float dt)
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
                _resultWave[i] = _resultWave[i]*_dampningCoefficient;

                //keep track of _aabb min value                
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
            _currentWave[_currentWave.Length - 1] = ConvertUnits.ToSimUnits(_waveGeneratorCount);
        }

        public override void Validate()
        {
            //just do nothing for now. will revisit later.
        }
    }
}