/*
 * Camera2D
 * Copyright (c) 2009-2010 Matthew Bettcher
 * 
 * This software is provided 'as-is', without any express or implied 
 * warranty.  In no event will the authors be held liable for any damages 
 * arising from the use of this software. 
 * Permission is granted to anyone to use this software for any purpose, 
 * including commercial applications, and to alter it and redistribute it 
 * freely, subject to the following restrictions: 
 * 1. The origin of this software must not be misrepresented; you must not 
 * claim that you wrote the original software. If you use this software 
 * in a product, an acknowledgment in the product documentation would be 
 * appreciated but is not required. 
 * 2. Altered source versions must be plainly marked as such, and must not be 
 * misrepresented as being the original software. 
 * 3. This notice may not be removed or altered from any source distribution.
 * 
 * TODO - 
 * 
 *   o Change transformations to use 3x3 Matrix and Vector2
 *   o Use Indexed TriangleList
 *   o Add support for sprite sheet textures
 */


using System;
using System.Collections.Generic;
using DemoBaseXNA;
using DemoBaseXNA.DrawingSystem;
using DemoBaseXNA.ScreenSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DemoBaseXNA.DrawingSystem
{
    /// <summary>
    /// Maintains Projection and View matrices for the samples.
    /// </summary>
    public class Camera2D
    {
        public GraphicsDevice GraphicsDevice;
        private bool _movingCamera;
        private Vector3 _cameraMouseOffset;
        private float _screenWidth;
        private float _screenHeight;
        private float _zoom = 1.0f;

        public Vector3 Position;

        public Matrix Projection 
        { 
            get { return Matrix.CreateOrthographic(_screenWidth * _zoom, _screenHeight * _zoom, 0, 1); }
            set { Projection = value; } 
        }

        public Matrix View { get { return Matrix.CreateTranslation(Position); } set { View = value; } }

        public Camera2D(GraphicsDevice graphicsDevice, Vector3 position, float width, float height)
        {
            GraphicsDevice = graphicsDevice;
            Position = position;
            _screenWidth = width;
            _screenHeight = height;
        }

        public void HandleInput(InputState input)
        {
#if WINDOWS
            Mouse(input.CurrentMouseState, input.LastMouseState);
#endif
        }

#if WINDOWS
        private void Mouse(MouseState state, MouseState oldState)
        {
            Vector3 p = GraphicsDevice.Viewport.Unproject(new Vector3(state.X, state.Y, 0), Projection, 
                View, Matrix.Identity);

            Vector2 position = new Vector2(p.X, p.Y);

            if (state.RightButton == ButtonState.Released && oldState.RightButton == ButtonState.Pressed)
            {
                MouseUp(position);
            }
            else if (state.RightButton == ButtonState.Pressed && oldState.RightButton == ButtonState.Released)
            {
                MouseDown(position);
            }

            MouseMove(new Vector3(position, oldState.ScrollWheelValue - state.ScrollWheelValue));
        }

        private void MouseDown(Vector2 position)
        {
            _movingCamera = true;
            _cameraMouseOffset = Position - new Vector3(position, 0);
        }

        private void MouseUp(Vector2 position)
        {
            _movingCamera = false;
        }

        private void MouseMove(Vector3 position)
        {
            if (_movingCamera)
            {
                Position = new Vector3(position.X, position.Y, 0) + _cameraMouseOffset;
                _zoom += position.Z / 1000f;
            }
        }
#endif
    }
}