using System;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerPhysics.DemoBaseXNA.ScreenSystem
{
    public class Camera2D
    {
        private const float SmoothingSpeed = 0.15f;
        public static Matrix View;
        public static Matrix Projection;

        private static GraphicsDevice _graphics;
        public Action ProjectionUpdated;
        public Action ViewUpdated;
        private Vector2 _position;
        private float _rotation;
        private Vector2 _targetPosition;
        private float _targetRotation;
        private bool _targetRotationReached = true;
        private bool _targetXPositionReached = true;
        private bool _targetYPositionReached = true;
        private float _targetZoom;
        private bool _targetZoomReached = true;
        private float _zoom;

        /// <summary>
        /// The constructor for the Camera2D class.
        /// </summary>
        /// <param name="graphics"></param>
        public Camera2D(GraphicsDevice graphics)
        {
            _graphics = graphics;

            Projection = Matrix.Identity;
            View = Matrix.Identity;

            CreateProjection();
            ResetCamera();
        }

        /// <summary>
        /// The current position of the camera.
        /// </summary>
        public Vector2 Position
        {
            get { return _position; }
            set
            {
                _position = Vector2.Clamp(value, MinPosition * Zoom, MaxPosition * Zoom);

                Resize();
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
                _rotation = MathHelper.Clamp(value, MinRotation, MaxRotation);

                Resize();
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
                _zoom = MathHelper.Clamp(value, MinZoom, MaxZoom);

                Resize();
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
        /// Center of the screen
        /// </summary>
        /// <value>The screen center.</value>
        public Vector2 ScreenCenter
        {
            get
            {
                return new Vector2(_graphics.Viewport.Width / 2f,
                                   _graphics.Viewport.Height / 2f);
            }
        }

        /// <summary>
        /// Gets the width of the screen.
        /// </summary>
        /// <value>The width of the screen.</value>
        public int ScreenWidth
        {
            get { return _graphics.Viewport.Width; }
        }

        /// <summary>
        /// Gets the height of the screen.
        /// </summary>
        /// <value>The height of the screen.</value>
        public int ScreenHeight
        {
            get { return _graphics.Viewport.Height; }
        }

        /// <summary>
        /// a vector representing the current size of the camera view.
        /// Expressed as: Size * (1 / zoom).
        /// </summary>
        public Vector2 CurrentSize
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

        public float TargetRotation
        {
            get { return _targetRotation; }
            set
            {
                if (_targetRotation == value)
                    _targetRotationReached = true;
                else
                {
                    _targetRotation = value;
                    _targetRotationReached = false;
                }
            }
        }

        public float TargetZoom
        {
            get { return _targetZoom; }
            set
            {
                if (_targetZoom == value)
                    _targetZoomReached = true;
                else
                {
                    _targetZoom = value;
                    _targetZoomReached = false;
                }
            }
        }

        public Vector2 TargetPosition
        {
            get { return _targetPosition; }
            set
            {
                if (_targetPosition == value)
                {
                    _targetXPositionReached = true;
                    _targetYPositionReached = true;
                }
                else
                {
                    _targetPosition = value;
                    _targetXPositionReached = false;
                    _targetYPositionReached = false;
                }
            }
        }

        /// <summary>
        /// Gets or sets the maximum rotation in radians.
        /// </summary>
        /// <value>The max rotation.</value>
        public float MaxRotation { get; set; }

        /// <summary>
        /// Gets or sets the minimum rotation in radians.
        /// </summary>
        /// <value>The min rotation.</value>
        public float MinRotation { get; set; }

        /// <summary>
        /// Gets or sets the rate at which the tracking moves.
        /// </summary>
        /// <value>The move rate.</value>
        public Vector2 MoveRate { get; set; }

        public void ZoomIn(float amount)
        {
            Zoom += amount;
        }

        public void ZoomOut(float amount)
        {
            Zoom -= amount;
        }

        public void MoveCamera(Vector2 amount)
        {
            Position += amount;
        }

        /// <summary>
        /// Creates the projection matrix. Call this if the aspect ratio of the screen changes.
        /// </summary>
        public void CreateProjection()
        {
            // L/R/B/T
            Projection = Matrix.CreateOrthographicOffCenter(-25 * _graphics.Viewport.AspectRatio,
                                                            25 * _graphics.Viewport.AspectRatio, -25, 25, -1, 1);

            if (ProjectionUpdated != null)
                ProjectionUpdated();
        }

        /// <summary>
        /// Resets the camera to default values.
        /// </summary>
        public void ResetCamera()
        {
            ZoomRate = 0.1f;
            MoveRate = new Vector2(1f, 1f);
            RotationRate = 0.1f;
            MinZoom = 0.5f;
            MaxZoom = 2f;
            MinRotation = -(MathHelper.Pi / 2);
            MaxRotation = MathHelper.Pi / 2;
            MaxPosition = new Vector2(25f, 25f);
            MinPosition = new Vector2(-25f, 0f);

            _targetPosition = Vector2.Zero;
            _targetRotation = 0;
            _targetZoom = 1f;

            _zoom = 1f;
            _position = Vector2.Zero;
            _rotation = 0;

            Resize();
        }

        /// <summary>
        /// Resets the camera to default values.
        /// </summary>
        public void SmoothResetCamera()
        {
            ZoomRate = 0.1f;
            MoveRate = new Vector2(1f, 1f);
            RotationRate = 0.1f;
            MinZoom = 0.5f;
            MaxZoom = 2f;
            MinRotation = -(MathHelper.Pi / 2);
            MaxRotation = MathHelper.Pi / 2;
            MaxPosition = new Vector2(25f, 25f);
            MinPosition = new Vector2(-25f, 0f);

            TargetPosition = Vector2.Zero;
            TargetRotation = 0;
            TargetZoom = 1f;
        }

        private void Resize()
        {
            View = Matrix.CreateRotationZ(_rotation) * Matrix.CreateTranslation(-_position.X, -_position.Y, 0) *
                   Matrix.CreateScale(_zoom);

            if (ViewUpdated != null)
                ViewUpdated();
        }

        /// <summary>
        /// Moves the camera forward one timestep.
        /// </summary>
        public void Update()
        {
            if (TrackingBody != null)
                Position = TrackingBody.Position;

            if (_targetYPositionReached == false)
            {
                float value;

                if (TargetPosition.X > Position.X)
                {
                    value = Math.Min(MaxPosition.X * Zoom, _position.X + MoveRate.X);
                    Position = new Vector2(MathHelper.SmoothStep(_position.X, value, SmoothingSpeed), Position.Y);

                    if (Position.X >= TargetPosition.X)
                        _targetYPositionReached = true;
                }
                else if (TargetPosition.X < Position.X)
                {
                    value = Math.Max(MinPosition.X * Zoom, _position.X - MoveRate.X);
                    Position = new Vector2(MathHelper.SmoothStep(_position.X, value, SmoothingSpeed), Position.Y);

                    if (Position.X <= TargetPosition.X)
                        _targetYPositionReached = true;
                }
            }

            if (_targetXPositionReached == false)
            {
                float value;

                if (TargetPosition.Y > Position.Y)
                {
                    value = Math.Min(MaxPosition.Y * Zoom, _position.Y + MoveRate.Y);
                    Position = new Vector2(Position.X, MathHelper.SmoothStep(_position.Y, value, SmoothingSpeed));

                    if (Position.Y >= TargetPosition.Y)
                        _targetXPositionReached = true;
                }
                else if (TargetPosition.Y < Position.Y)
                {
                    value = Math.Max(MinPosition.Y * Zoom, _position.Y - MoveRate.Y);
                    Position = new Vector2(Position.X, MathHelper.SmoothStep(_position.Y, value, SmoothingSpeed));

                    if (Position.Y <= TargetPosition.Y)
                        _targetXPositionReached = true;
                }
            }

            if (_targetRotationReached == false)
            {
                float value;

                if (TargetRotation > Rotation)
                {
                    value = Math.Min(MaxRotation, _rotation + RotationRate);
                    Rotation = MathHelper.SmoothStep(_rotation, value, SmoothingSpeed);

                    if (Rotation >= TargetRotation)
                        _targetRotationReached = true;
                }
                else if (TargetRotation < Rotation)
                {
                    value = Math.Max(MinRotation, _rotation - RotationRate);
                    Rotation = MathHelper.SmoothStep(_rotation, value, SmoothingSpeed);

                    if (Rotation <= TargetRotation)
                        _targetRotationReached = true;
                }
            }

            if (_targetZoomReached == false)
            {
                float value;

                if (TargetZoom > Zoom)
                {
                    value = Math.Min(MaxZoom, _zoom + ZoomRate);
                    Zoom = MathHelper.SmoothStep(_zoom, value, SmoothingSpeed);

                    if (Zoom >= TargetZoom)
                        _targetZoomReached = true;
                }
                else if (TargetZoom < Zoom)
                {
                    value = Math.Max(MinZoom, _zoom - ZoomRate);
                    Zoom = MathHelper.SmoothStep(_zoom, value, SmoothingSpeed);

                    if (Zoom <= TargetZoom)
                        _targetZoomReached = true;
                }
            }
        }

        public static Vector2 ConvertScreenToWorld(Vector2 location)
        {
            Vector3 t = new Vector3(location, 0);

            t = _graphics.Viewport.Unproject(t, Projection, View, Matrix.Identity);

            return new Vector2(t.X, t.Y);
        }

        public static Vector2 ConvertWorldToScreen(Vector2 location)
        {
            Vector3 t = new Vector3(location, 0);

            t = _graphics.Viewport.Project(t, Projection, View, Matrix.Identity);

            return new Vector2(t.X, t.Y);
        }
    }
}