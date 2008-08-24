using System;
using System.Collections.Generic;
using System.Diagnostics;

#if (XNA)
using Microsoft.Xna.Framework;
#endif

using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Mathematics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;

namespace FarseerGames.FarseerPhysics.Controllers
{
    public class WaveController : Controller, IFluidContainer
    {
        private float width;
        private float height;
        private Vector2 position;
        private float singleWaveWidth;

        private int nodeCount;
        private float dampningCoefficient = .95f;
        private float frequency = .18f; //seconds;

        private float[] currentWave;
        private float[] previousWave;
        private float[] resultWave;
        private float[] xPosition;

        private float waveGeneratorMax = 0;
        private float waveGeneratorMin = 0;
        private float waveGeneratorStep = 0;
        private bool goingUp = true;
        private float waveGeneratorCount = 0;


        private AABB aabb;

        public WaveController()
        {

        }

        #region properties
        public float Width
        {
            get { return width; }
            set { width = value; }
        }

        public float Height
        {
            get { return height; }
            set { height = value; }
        }

        /// <summary>
        /// Top left position of wave area
        /// </summary>
        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }

        public int NodeCount
        {
            get { return nodeCount; }
            set { nodeCount = value; }
        }

        public float DampningCoefficient
        {
            get { return dampningCoefficient; }
            set { dampningCoefficient = value; }
        }

        public float[] CurrentWave
        {
            get { return currentWave; }
        }

        public float[] PreviousWave
        {
            get { return previousWave; }
        }

        public float[] XPosition
        {
            get { return xPosition; }
            set { xPosition = value; }
        }

        public float WaveGeneratorMax
        {
            get { return waveGeneratorMax; }
            set { waveGeneratorMax = value; }
        }

        public float WaveGeneratorMin
        {
            get { return waveGeneratorMin; }
            set { waveGeneratorMin = value; }
        }

        public float WaveGeneratorStep
        {
            get { return waveGeneratorStep; }
            set { waveGeneratorStep = value; }
        }

        public float Frequency
        {
            get { return frequency; }
            set { frequency = value; }
        }
        #endregion

        /// <summary>
        /// Create a disturbance in the water surface that will create waves.
        /// </summary>
        /// <param name="node">Which node to change the hieght of</param>
        /// <param name="offset">The amount to move the node up or down (negative values moves the node up, positive moves it down)</param>
        public void Disturb(int node, float offset)
        {
            currentWave[node] = currentWave[node] + offset;
        }
       
        public void Initialize()
        {
            xPosition = new float[nodeCount];
            currentWave = new float[nodeCount];
            previousWave = new float[nodeCount];
            resultWave = new float[nodeCount];

            for (int i = 0; i < nodeCount; i++)
            {
                xPosition[i] = MathHelper.Lerp(position.X, position.X + width, (float)i / (nodeCount - 1));
                currentWave[i] = 0;
                previousWave[i] = 0;
                resultWave[i] = 0;
            }

            aabb = new AABB(position, new Vector2(position.X + width, position.Y + height));
            singleWaveWidth = width / (nodeCount - 1);
        }

        float aabbMin = float.MaxValue;
        float timePassed = 0;
        public override void Update(float dt)
        {
            if (timePassed < frequency)
            {
                timePassed += dt;
                return;
            }
            else
            {
                timePassed = 0;
            }

            aabbMin = float.MaxValue;
            aabb.min.Y = aabbMin;
            for (int i = 1; i < nodeCount - 1; i++)
            {
                resultWave[i] = (currentWave[i - 1] + currentWave[i + 1]) - previousWave[i];
                resultWave[i] = resultWave[i] * dampningCoefficient;

                //keep track of aabb min value                
                if (resultWave[i] + position.Y < aabbMin) { aabbMin = resultWave[i] + position.Y; }
            }
            aabb.min.Y = aabbMin;
            currentWave.CopyTo(previousWave, 0);
            resultWave.CopyTo(currentWave, 0);

            if (goingUp)
            {
                if (waveGeneratorCount > waveGeneratorMax)
                {
                    goingUp = false;
                }
                else
                {
                    waveGeneratorCount += waveGeneratorStep;
                }
            }
            else
            {
                if (waveGeneratorCount < waveGeneratorMin)
                {
                    goingUp = true;
                }
                else
                {
                    waveGeneratorCount -= waveGeneratorStep;
                }
            }
            currentWave[currentWave.Length - 1] = ConvertUnits.ToSimUnits(waveGeneratorCount);
        }

        public bool Intersect(AABB aabb)
        {
            return AABB.Intersect(aabb, this.aabb);
        }

        public Vector2 vectorNearWaveEdge;
        public Vector2 vectorFarWaveEdge;
        public Vector2 vectorPoint;

        Vector2 waveEdgeVector;
        Vector2 pointVector;
        public bool Contains(ref Vector2 vector)
        {
            try
            {
                int index = (int)Math.Floor((vector.X - xPosition[0]) / singleWaveWidth);

                //handle the boundry conditions
                if (index > nodeCount - 2) index = nodeCount - 2;
                if (index < 0) index = 0;

                vectorNearWaveEdge.X = xPosition[index];
                vectorNearWaveEdge.Y = position.Y + currentWave[index];

                vectorFarWaveEdge.X = xPosition[index + 1];
                vectorFarWaveEdge.Y = position.Y + currentWave[index + 1];

                vectorPoint = vector;

                waveEdgeVector.X = xPosition[index + 1] - xPosition[index];
                waveEdgeVector.Y = currentWave[index + 1] - currentWave[index];

                pointVector.X = vector.X - xPosition[index];
                pointVector.Y = vector.Y - (position.Y + currentWave[index]);

                float perpDot;
                Calculator.Cross(ref waveEdgeVector, ref pointVector, out perpDot);

                if (perpDot < 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }


        }

        public override void Validate()
        {
            //just do nothing for now. will revisit later.
            
        }
    }
}
