using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace FarseerGames.FarseerXNAGame.Utilities {
    internal class FrameRate {
        private int _numberOfFramesToAverage = 1;
        private float[] _elapsedTime;
        private int _elapsedTimeIndex = 0;
        private bool _elapsedTimeArrayFilled = false;

        private float _framesPerSecond;
        private float _frameTime;
        private float _totalTime;

       private float _frequency = 1;

        internal FrameRate(int numberOfFramesToAverage, float calculationFrequency) {
            _frequency = calculationFrequency;
            _elapsedTime = new float[numberOfFramesToAverage];
            _numberOfFramesToAverage = numberOfFramesToAverage;
        }

        internal float FramesPerSecond {
            get{return _framesPerSecond;}
        }

        internal float FrameTime {
            get { return _frameTime; }
        }

        internal float TotalTime {
            get { return _totalTime; }
        }

        internal void Update(float elapsedTime) {
            if (elapsedTime == 0) { return; }
            _totalTime += elapsedTime;
            if (_totalTime > _frequency) {
                _elapsedTime[_elapsedTimeIndex] = elapsedTime;
                _elapsedTimeIndex += 1;
                //array index wraps back to 0 once it reaches the
                //end of the array.
                if (_elapsedTimeIndex == _numberOfFramesToAverage) {
                    _elapsedTimeIndex = 0;
                    _elapsedTimeArrayFilled = true;
                }
                _framesPerSecond = GetAverageFramesPerSecond();
                _frameTime = 1000 / _framesPerSecond;
                _totalTime = 0;
            }

        }

        //calculates the average frames per second by summing the elapsed times in the array
        //and dividing by the number of items in the array
        private float GetAverageFramesPerSecond() {
            int maxElapsedTimeIndex;
            float sum = 0;
            if (!_elapsedTimeArrayFilled) {
                maxElapsedTimeIndex = _elapsedTimeIndex;
            }
            else {
                maxElapsedTimeIndex = _numberOfFramesToAverage;
            }

            for (int i = 0; i < maxElapsedTimeIndex; i++) {
                sum += _elapsedTime[i];
            }
            return maxElapsedTimeIndex / sum;   
            
        }



    }
}
