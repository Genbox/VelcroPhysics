using System;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Mathematics;

namespace FarseerGames.FarseerPhysics.Controllers
{
    public class WaveController : Controller, IFluidContainer
    {
        private AABB _aabb;
        private float _aabbMin = float.MaxValue;
        private bool _goingUp = true;
        private Vector2 _pointVector;
        private Vector2 _position;

        private float[] _resultWave;
        private float _singleWaveWidth;
        private float _timePassed;
        public Vector2 _vectorFarWaveEdge;
        public Vector2 _vectorNearWaveEdge;
        public Vector2 _vectorPoint;

        private Vector2 _waveEdgeVector;
        private float _waveGeneratorCount;

        public WaveController()
        {
            Frequency = .18f;
            DampningCoefficient = .95f;
        }


        /// <summary>
        /// Top left position of wave area
        /// </summary>
        public Vector2 Position
        {
            get { return _position; }
            set { _position = value; }
        }

        public float Width { get; set; }
        public float Height { get; set; }
        public int NodeCount { get; set; }
        public float DampningCoefficient { get; set; }
        public float[] CurrentWave { get; private set; }
        public float[] PreviousWave { get; private set; }
        public float[] XPosition { get; set; }
        public float WaveGeneratorMax { get; set; }
        public float WaveGeneratorMin { get; set; }
        public float WaveGeneratorStep { get; set; }
        public float Frequency { get; set; }

        #region IFluidContainer Members

        public bool Intersect(AABB aabb)
        {
            return AABB.Intersect(aabb, _aabb);
        }

        public bool Contains(ref Vector2 vector)
        {
            int index = (int) Math.Floor((vector.X - XPosition[0])/_singleWaveWidth);

            //handle the boundry conditions
            if (index > NodeCount - 2) index = NodeCount - 2;
            if (index < 0) index = 0;

            _vectorNearWaveEdge.X = XPosition[index];
            _vectorNearWaveEdge.Y = _position.Y + CurrentWave[index];

            _vectorFarWaveEdge.X = XPosition[index + 1];
            _vectorFarWaveEdge.Y = _position.Y + CurrentWave[index + 1];

            _vectorPoint = vector;

            _waveEdgeVector.X = XPosition[index + 1] - XPosition[index];
            _waveEdgeVector.Y = CurrentWave[index + 1] - CurrentWave[index];

            _pointVector.X = vector.X - XPosition[index];
            _pointVector.Y = vector.Y - (_position.Y + CurrentWave[index]);

            float perpDot;
            Calculator.Cross(ref _waveEdgeVector, ref _pointVector, out perpDot);

            if (perpDot < 0)
            {
                return false;
            }

            return true;
        }

        #endregion

        /// <summary>
        /// Create a disturbance in the water surface that will create waves.
        /// </summary>
        /// <param name="node">Which node to change the hieght of</param>
        /// <param name="offset">The amount to move the node up or down (negative values moves the node up, positive moves it down)</param>
        public void Disturb(int node, float offset)
        {
            CurrentWave[node] = CurrentWave[node] + offset;
        }

        public void Initialize()
        {
            XPosition = new float[NodeCount];
            CurrentWave = new float[NodeCount];
            PreviousWave = new float[NodeCount];
            _resultWave = new float[NodeCount];

            for (int i = 0; i < NodeCount; i++)
            {
                XPosition[i] = MathHelper.Lerp(_position.X, _position.X + Width, (float) i/(NodeCount - 1));
                CurrentWave[i] = 0;
                PreviousWave[i] = 0;
                _resultWave[i] = 0;
            }

            _aabb = new AABB(_position, new Vector2(_position.X + Width, _position.Y + Height));
            _singleWaveWidth = Width/(NodeCount - 1);
        }

        public override void Update(float dt)
        {
            if (_timePassed < Frequency)
            {
                _timePassed += dt;
                return;
            }

            _timePassed = 0;
            _aabbMin = float.MaxValue;
            _aabb.min.Y = _aabbMin;
            for (int i = 1; i < NodeCount - 1; i++)
            {
                _resultWave[i] = (CurrentWave[i - 1] + CurrentWave[i + 1]) - PreviousWave[i];
                _resultWave[i] = _resultWave[i]*DampningCoefficient;

                //keep track of aabb min value                
                if (_resultWave[i] + _position.Y < _aabbMin)
                {
                    _aabbMin = _resultWave[i] + _position.Y;
                }
            }
            _aabb.min.Y = _aabbMin;
            CurrentWave.CopyTo(PreviousWave, 0);
            _resultWave.CopyTo(CurrentWave, 0);

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
            CurrentWave[CurrentWave.Length - 1] = ConvertUnits.ToSimUnits(_waveGeneratorCount);
        }

        public override void Validate()
        {
            //just do nothing for now. will revisit later.
        }
    }
}