using System;
using System.Collections.Generic;
using FarseerPhysics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DemoBaseXNA.DrawingSystem
{
    public class GraphRenderHelper
    {
        private GraphicsDevice _graphicsDevice;
        private LineRenderHelper _line;
        private List<float> _graphValues;
        private int _valuesToGraph;
        private float _min; 
        private float _max; 
        private Color _color;

        public GraphRenderHelper(GraphicsDevice graphicsDevice, int valuesToGraph, float min, float max, Color color)
        {
            _graphicsDevice = graphicsDevice;
            _valuesToGraph = valuesToGraph;
            _min = min;
            _max = max;
            _color = color;

            _graphValues = new List<float>(_valuesToGraph + 1);
            _line = new LineRenderHelper(valuesToGraph * 2, _graphicsDevice);
        }

        public void UpdateGraph(float value)
        {
            _graphValues.Add(value);

            if (_graphValues.Count > _valuesToGraph + 1)
                _graphValues.RemoveAt(0);
        }

        public void Render(Rectangle destRect)
        {
            float x = (float)destRect.X;
            float deltaX = (float)destRect.Width / (float)_valuesToGraph;
            float yScale = (float)destRect.Bottom - (float)destRect.Top;
            
            // we must have at least 2 values to start rendering
            if (_graphValues.Count > 2)
            {
                // start at last value (newest value added)
                // continue until no values are left
                for (int i = _graphValues.Count - 1; i > 0; i--)
                {
                    float y1 = (float)destRect.Bottom - ((_graphValues[i] / (_max - _min)) * yScale);
                    float y2 = (float)destRect.Bottom - ((_graphValues[i-1] / (_max - _min)) * yScale);

                    _line.Submit(new Vector3(MathHelper.Clamp(x, (float)destRect.Left, (float)destRect.Right), MathHelper.Clamp(y1, (float)destRect.Top, (float)destRect.Bottom), 0),
                        new Vector3(MathHelper.Clamp(x + deltaX, (float)destRect.Left, (float)destRect.Right), MathHelper.Clamp(y2, (float)destRect.Top, (float)destRect.Bottom), 0), _color);

                    x += deltaX;
                }
            }
            // we create our own projection based on the viewport size
            // this keeps our graph in screen space
            _line.Render(_graphicsDevice, Matrix.CreateOrthographicOffCenter(0, _graphicsDevice.Viewport.Width, _graphicsDevice.Viewport.Height, 0, 0, 1), Matrix.Identity);
            _line.Clear();
        }
    }
}