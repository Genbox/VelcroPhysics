using System;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerPhysics.DemoBaseXNA.ScreenSystem
{
    public class Camera2D
    {
        const float TargetPositionEpsilon = 0.01f;
        public float SmoothingSpeed = 0.15f;
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
                _position = Vector2.Clamp(value, MinPosition, MaxPosition);

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
                value = Vector2.Clamp(value, MinPosition, MaxPosition);
                _targetPosition = value;
            }
        }

        public Vector2 TargetPositionTolerance { get; set; }

        public bool EnableTargetTracking { get; set; }

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
            MinPosition = new Vector2(-25f, -25f);

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
            MinPosition = new Vector2(-25f, -25f);

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
            if (!EnableTargetTracking)
                return;

            // Do logic on the position using local variables so the property assignment & it's related clamping/resize 
            // only occurs once and only then if the value changed.
            Vector2 currentPosition = Position;

            if (TrackingBody != null)
                currentPosition = TrackingBody.Position;

            _targetXPositionReached = Math.Abs(TargetPosition.X - currentPosition.X) < TargetPositionTolerance.X;
            _targetYPositionReached = Math.Abs(TargetPosition.Y - currentPosition.Y) < TargetPositionTolerance.Y;

            if (_targetXPositionReached == false)
            {
                if (TargetPosition.X > currentPosition.X)
                {
                    var goal = TargetPosition.X - TargetPositionTolerance.X;
                    if (goal < currentPosition.X)
                    {
                        _targetXPositionReached = true;
                    }
                    else
                    {
                        currentPosition.X = MathHelper.SmoothStep(currentPosition.X, goal, SmoothingSpeed);
                        _targetXPositionReached = currentPosition.X >= goal - TargetPositionEpsilon;
                    }
                }
                else if (TargetPosition.X < currentPosition.X)
                {
                    var goal = TargetPosition.X + TargetPositionTolerance.X;
                    if (goal > currentPosition.X)
                    {
                        _targetXPositionReached = true;
                    }
                    else
                    {
                        currentPosition.X = MathHelper.SmoothStep(currentPosition.X, goal, SmoothingSpeed);
                        _targetXPositionReached = currentPosition.X <= goal + TargetPositionEpsilon;
                    }
                }
            }

            if (_targetYPositionReached == false)
            {
                if (TargetPosition.Y > currentPosition.Y)
                {
                    float goal = TargetPosition.Y - TargetPositionTolerance.Y;
                    if (goal < currentPosition.Y)
                    {
                        _targetYPositionReached = true;
                    }
                    else
                    {
                        currentPosition.Y = MathHelper.SmoothStep(currentPosition.Y, goal, SmoothingSpeed);
                        _targetYPositionReached = currentPosition.Y >= goal - TargetPositionEpsilon;
                    }
                }
                else if (TargetPosition.Y < currentPosition.Y)
                {
                    float goal = TargetPosition.Y + TargetPositionTolerance.Y;
                    if (goal > currentPosition.Y)
                    {
                        _targetYPositionReached = true;
                    }
                    else
                    {
                        currentPosition.Y = MathHelper.SmoothStep(currentPosition.Y, goal, SmoothingSpeed);
                        _targetYPositionReached = currentPosition.Y <= goal + TargetPositionEpsilon;
                    }
                }
            }

            // Only update the property if the value changed.
            if (!currentPosition.Equals(Position))
            {
                Position = currentPosition;
            }

            // Do logic on the rotation using local variables so the property assignment & it's related clamping/resize 
            // only occurs once and only then if the value changed.
            var currentRotation = _rotation;
            if (_targetRotationReached == false)
            {
                float value;

                if (_targetRotation > currentRotation)
                {
                    value = Math.Min(MaxRotation, currentRotation + RotationRate);
                    currentRotation = MathHelper.SmoothStep(currentRotation, value, SmoothingSpeed);

                    if (currentRotation >= _targetRotation)
                        _targetRotationReached = true;
                }
                else if (_targetRotation < currentRotation)
                {
                    value = Math.Max(MinRotation, currentRotation - RotationRate);
                    currentRotation = MathHelper.SmoothStep(currentRotation, value, SmoothingSpeed);

                    if (currentRotation <= _targetRotation)
                        _targetRotationReached = true;
                }
            }

            // Only update the property if the value changed.
            if (currentRotation != _rotation)
            {
                Rotation = currentRotation;
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