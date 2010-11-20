using System;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FarseerPhysics.DemoBaseXNA.ScreenSystem
{
    public class Camera2D
    {
        private const float TransitionSpeed = 0.01f;
        private const float SmoothingSpeed = 0.15f;
        public Matrix ProjectionMatrix;
        public Matrix ViewMatrix;

        private GraphicsDevice _graphics;
        private Vector2 _origPosition = Vector2.Zero;
        private float _origRotation;
        private float _origZoom = 1;
        private Vector2 _position = Vector2.Zero;
        private bool _positionUnset = true;
        private float _rotation;
        private bool _rotationUnset = true;
        private Vector2 _targetPosition = Vector2.Zero;
        private float _targetRotation;
        private float _targetZoom = 1;
        private float _transition;
        private bool _transitioning;
        private float _zoom = 1;
        private bool _zoomUnset = true;

        /// <summary>
        /// The constructor for the Camera2D class.
        /// </summary>
        /// <param name="graphics"></param>
        public Camera2D(GraphicsDevice graphics)
        {
            MoveRate = 1;
            ZoomRate = 0.01f;
            RotationRate = 0.01f;
            MinZoom = 0.25f;
            MaxZoom = 4;
            VerticalCameraMovement = (input, camera) => (input.RightStickPosition.Y * camera.MoveRate) * camera.Zoom;
            HorizontalCameraMovement = (input, camera) => (input.RightStickPosition.X * camera.MoveRate) * camera._zoom;
            ClampingEnabled = camera => (camera.MinPosition != camera.MaxPosition);
            ZoomOut = input => input.IsCurPress(Buttons.DPadDown);
            ZoomIn = input => input.IsCurPress(Buttons.DPadUp);
            MaxPosition = Vector2.Zero;
            MinPosition = Vector2.Zero;
            RotateLeft = input => false;
            ResetCamera = input => input.IsCurPress(Buttons.RightStick);
            RotateRight = input => false;
            _graphics = graphics;

            ProjectionMatrix = Matrix.CreateOrthographicOffCenter(-20 * _graphics.Viewport.AspectRatio,
                                                      20 * _graphics.Viewport.AspectRatio, 20, -20, 0, 1);
        }

        /// <summary>
        /// The current position of the camera.
        /// </summary>
        public Vector2 Position
        {
            get { return _position; }
            set
            {
                if (_positionUnset)
                {
                    _origPosition = value;
                    _positionUnset = false;
                }
                _position = value;
                _targetPosition = value;
            }
        }

        /// <summary>
        /// The current rotation of the camera in radians.
        /// </summary>
        public float Rotation
        {
            get { return _rotation; }
            set
            {
                if (_rotationUnset)
                {
                    _origRotation = value;
                    _rotationUnset = false;
                }
                _rotation = value;
                _targetRotation = value;
            }
        }

        /// <summary>
        /// The current zoom of the camera. This is a value indicating 
        /// how far zoomed in or out the camera is. To get the actual 
        /// current size of the camera view, see CurSize.
        /// </summary>
        public float Zoom
        {
            get { return _zoom; }
            set
            {
                if (_zoomUnset)
                {
                    _origZoom = value;
                    _zoomUnset = false;
                }
                _zoom = value;
                _targetZoom = value;
            }
        }

        /// <summary>
        /// the furthest zoomed in the camera can be. Larger numbers 
        /// are further zoomed in.
        /// </summary>
        public float MaxZoom { get; set; }

        /// <summary>
        /// The futhest zoomed out that the camera can be. Smaller numbers 
        /// are further zoomed out.
        /// </summary>
        public float MinZoom { get; set; }

        /// <summary>
        /// The amount that the camera rotates (in radians) in one timestep. 
        /// </summary>
        public float RotationRate { get; set; }

        /// <summary>
        /// The amount that the camera zooms in or out in one timestep.
        /// </summary>
        public float ZoomRate { get; set; }

        /// <summary>
        /// The rate at which the camera moves in one timestep.
        /// </summary>
        public float MoveRate { get; set; }

        public Vector2 ScreenCenter
        {
            get
            {
                return new Vector2(_graphics.Viewport.Width / 2f,
                                   _graphics.Viewport.Height / 2f);
            }
        }

        public int ScreenWidth
        {
            get { return _graphics.Viewport.Width; }
        }

        public int ScreenHeight
        {
            get { return _graphics.Viewport.Height; }
        }

        /// <summary>
        /// a vector representing the current size of the camera view.
        /// Expressed as: Size * (1 / zoom).
        /// </summary>
        public Vector2 CurSize
        {
            get { return Vector2.Multiply(new Vector2(_graphics.Viewport.Width, _graphics.Viewport.Height), 1 / _zoom); }
        }

        /// <summary>
        /// The furthest up, and the furthest left the camera can go.
        /// if this value equals maxPosition, then no clamping will be 
        /// applied (unless you override that function).
        /// </summary>
        public Vector2 MinPosition { get; set; }

        /// <summary>
        /// the furthest down, and the furthest right the camera will go.
        /// if this value equals minPosition, then no clamping will be 
        /// applied (unless you override that function).
        /// </summary>
        public Vector2 MaxPosition { get; set; }

        /// <summary>
        /// the body that this camera is currently tracking. 
        /// Null if not tracking any.
        /// </summary>
        public Body TrackingBody { get; set; }

        /// <summary>
        /// a function that is called to determine if the user wants 
        /// to zoom in.
        /// </summary>
        public Func<InputHelper, bool> ZoomIn { get; set; }

        /// <summary>
        /// a function that is called to determine whether the user wants 
        /// to zoom out.
        /// </summary>
        public Func<InputHelper, bool> ZoomOut { get; set; }

        /// <summary>
        /// a function that determines whether clamping is currently enabled 
        /// for this camera.
        /// </summary>
        public Func<Camera2D, bool> ClampingEnabled { get; set; }

        /// <summary>
        /// a function that is called to determine the amount of horizontal 
        /// movement that the user is requesting that the camera be moved 
        /// by.
        /// </summary>
        public Func<InputHelper, Camera2D, float> HorizontalCameraMovement { get; set; }

        /// <summary>
        /// a function that is called to determine the amount of vertical 
        /// movement that the user is requesting that the camera be moved 
        /// by.
        /// </summary>
        public Func<InputHelper, Camera2D, float> VerticalCameraMovement { get; set; }

        /// <summary>
        /// a function that is called to determine if the user wants to 
        /// rotate the camera left.
        /// </summary>
        public Func<InputHelper, bool> RotateLeft { get; set; }

        /// <summary>
        /// a function that is called to determine if the user wants to rotate 
        /// the camera right.
        /// </summary>
        public Func<InputHelper, bool> RotateRight { get; set; }

        /// <summary>
        /// A function that is called to determine if the user is requesting 
        /// that the camera be reset to it's original parameters.
        /// </summary>
        public Func<InputHelper, bool> ResetCamera { get; set; }

        /// <summary>
        /// Moves the camera forward one timestep.
        /// </summary>
        /// <param name="input">
        /// the an InputHelper input representing the current 
        /// input state.
        /// </param>
        public void Update(InputHelper input)
        {
            ViewMatrix = Matrix.CreateScale(_zoom) * Matrix.CreateRotationZ(_rotation);

            if (!_transitioning)
            {
                if (TrackingBody == null)
                {
                    if (ClampingEnabled(this))
                        _targetPosition = Vector2.Clamp(_position + new Vector2(
                                                                        HorizontalCameraMovement(input, this),
                                                                        VerticalCameraMovement(input, this)),
                                                        MinPosition,
                                                        MaxPosition);
                    else
                        _targetPosition += new Vector2(
                            HorizontalCameraMovement(input, this),
                            VerticalCameraMovement(input, this));
                }
                else
                {
                    if (ClampingEnabled(this))
                        _targetPosition = Vector2.Clamp(
                            TrackingBody.Position,
                            MinPosition,
                            MaxPosition);
                    else
                        _targetPosition = TrackingBody.Position;
                }
                if (ZoomIn(input))
                    _targetZoom = Math.Min(MaxZoom, _zoom + ZoomRate);
                if (ZoomOut(input))
                    _targetZoom = Math.Max(MinZoom, _zoom - ZoomRate);
                //these might need to be swapped
                if (RotateLeft(input))
                    _targetRotation = (_rotation + RotationRate) % (float) (Math.PI * 2);
                if (RotateRight(input))
                    _targetRotation = (_rotation - RotationRate) % (float) (Math.PI * 2);
                if (input.IsCurPress(Buttons.RightStick))
                {
                    _transitioning = true;
                    _targetPosition = _origPosition;
                    _targetRotation = _origRotation;
                    _targetZoom = _origZoom;
                    TrackingBody = null;
                }
            }
            else if (_transition < 1)
            {
                _transition += TransitionSpeed;
            }
            if (_transition >= 1f ||
                (_position == _origPosition &&
                 _rotation == _origRotation &&
                 _zoom == _origZoom))
            {
                _transition = 0;
                _transitioning = false;
            }
            _position = Vector2.SmoothStep(_position, _targetPosition, SmoothingSpeed);
            _rotation = MathHelper.SmoothStep(_rotation, _targetRotation, SmoothingSpeed);
            _zoom = MathHelper.SmoothStep(_zoom, _targetZoom, SmoothingSpeed);
        }

        public Vector2 ConvertScreenToWorld(Vector2 location)
        {
            Vector3 t = new Vector3(location, 0);

            t = _graphics.Viewport.Unproject(t, ProjectionMatrix, ViewMatrix, Matrix.Identity);

            return new Vector2(t.X, t.Y);
        }

        public Vector2 ConvertWorldToScreen(Vector2 location)
        {
            Vector3 t = new Vector3(location, 0);

            t = _graphics.Viewport.Project(t, ProjectionMatrix, ViewMatrix, Matrix.Identity);

            return new Vector2(t.X, t.Y);
        }
    }
}