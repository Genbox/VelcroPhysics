using System;
using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Demo.ScreenSystem
{
    public class Camera2D
    {
        private const float _minZoom = 0.02f;
        private const float _maxZoom = 20f;
        private static GraphicsDevice _graphics;
        private readonly Vector2 _translateCenter;

        private Vector2 _currentPosition;

        private float _currentRotation;

        private float _currentZoom;
        private Vector2 _maxPosition;
        private float _maxRotation;
        private Vector2 _minPosition;
        private float _minRotation;
        private bool _positionTracking;
        private bool _rotationTracking;
        private Vector2 _targetPosition;
        private float _targetRotation;
        private Body _trackingBody;
        private Matrix _simProjection;
        private Matrix _view;
        private Matrix _simView;

        public Camera2D(GraphicsDevice graphics)
        {
            _graphics = graphics;
            _simProjection = Matrix.CreateOrthographicOffCenter(0f, ConvertUnits.ToSimUnits(_graphics.Viewport.Width), ConvertUnits.ToSimUnits(_graphics.Viewport.Height), 0f, 0f, 1f);
            _simView = Matrix.Identity;
            _view = Matrix.Identity;

            _translateCenter = new Vector2(ConvertUnits.ToSimUnits(_graphics.Viewport.Width / 2f), ConvertUnits.ToSimUnits(_graphics.Viewport.Height / 2f));

            ResetCamera();
        }

        public ref Matrix View => ref _view;

        public ref Matrix SimView => ref _simView;

        public ref Matrix SimProjection => ref _simProjection;

        /// <summary>The current position of the camera.</summary>
        public Vector2 Position
        {
            get => ConvertUnits.ToDisplayUnits(_currentPosition);
            set
            {
                _targetPosition = ConvertUnits.ToSimUnits(value);
                if (_minPosition != _maxPosition)
                    Vector2.Clamp(ref _targetPosition, ref _minPosition, ref _maxPosition, out _targetPosition);
            }
        }

        /// <summary>The furthest up, and the furthest left the camera can go. if this value equals maxPosition, then no clamping
        /// will be applied (unless you override that function).</summary>
        public Vector2 MinPosition
        {
            get => ConvertUnits.ToDisplayUnits(_minPosition);
            set => _minPosition = ConvertUnits.ToSimUnits(value);
        }

        /// <summary>the furthest down, and the furthest right the camera will go. if this value equals minPosition, then no
        /// clamping will be applied (unless you override that function).</summary>
        public Vector2 MaxPosition
        {
            get => ConvertUnits.ToDisplayUnits(_maxPosition);
            set => _maxPosition = ConvertUnits.ToSimUnits(value);
        }

        /// <summary>The current rotation of the camera in radians.</summary>
        public float Rotation
        {
            get => _currentRotation;
            set
            {
                _targetRotation = value % MathHelper.TwoPi;
                if (_minRotation != _maxRotation)
                    _targetRotation = MathHelper.Clamp(_targetRotation, _minRotation, _maxRotation);
            }
        }

        /// <summary>Gets or sets the minimum rotation in radians.</summary>
        /// <value>The min rotation.</value>
        public float MinRotation
        {
            get => _minRotation;
            set => _minRotation = MathHelper.Clamp(value, -MathHelper.Pi, 0f);
        }

        /// <summary>Gets or sets the maximum rotation in radians.</summary>
        /// <value>The max rotation.</value>
        public float MaxRotation
        {
            get => _maxRotation;
            set => _maxRotation = MathHelper.Clamp(value, 0f, MathHelper.Pi);
        }

        /// <summary>The current rotation of the camera in radians.</summary>
        public float Zoom
        {
            get => _currentZoom;
            set
            {
                _currentZoom = value;
                _currentZoom = MathHelper.Clamp(_currentZoom, _minZoom, _maxZoom);
            }
        }

        /// <summary>the body that this camera is currently tracking. Null if not tracking any.</summary>
        public Body TrackingBody
        {
            get => _trackingBody;
            set
            {
                _trackingBody = value;
                if (_trackingBody != null)
                    _positionTracking = true;
            }
        }

        public bool EnablePositionTracking
        {
            get => _positionTracking;
            set
            {
                if (value && _trackingBody != null)
                    _positionTracking = true;
                else
                    _positionTracking = false;
            }
        }

        public bool EnableRotationTracking
        {
            get => _rotationTracking;
            set
            {
                if (value && _trackingBody != null)
                    _rotationTracking = true;
                else
                    _rotationTracking = false;
            }
        }

        public bool EnableTracking
        {
            set
            {
                EnablePositionTracking = value;
                EnableRotationTracking = value;
            }
        }

        public void MoveCamera(Vector2 amount)
        {
            _currentPosition += amount;
            if (_minPosition != _maxPosition)
                Vector2.Clamp(ref _currentPosition, ref _minPosition, ref _maxPosition, out _currentPosition);
            _targetPosition = _currentPosition;
            _positionTracking = false;
            _rotationTracking = false;
        }

        public void RotateCamera(float amount)
        {
            _currentRotation += amount;
            if (_minRotation != _maxRotation)
                _currentRotation = MathHelper.Clamp(_currentRotation, _minRotation, _maxRotation);
            _targetRotation = _currentRotation;
            _positionTracking = false;
            _rotationTracking = false;
        }

        /// <summary>Resets the camera to default values.</summary>
        public void ResetCamera()
        {
            _currentPosition = Vector2.Zero;
            _targetPosition = Vector2.Zero;
            _minPosition = Vector2.Zero;
            _maxPosition = Vector2.Zero;

            _currentRotation = 0f;
            _targetRotation = 0f;
            _minRotation = -MathHelper.Pi;
            _maxRotation = MathHelper.Pi;

            _positionTracking = false;
            _rotationTracking = false;

            _currentZoom = 1f;

            SetView();
        }

        public void Jump2Target()
        {
            _currentPosition = _targetPosition;
            _currentRotation = _targetRotation;

            SetView();
        }

        private void SetView()
        {
            Matrix matRotation = Matrix.CreateRotationZ(_currentRotation);
            Matrix matZoom = Matrix.CreateScale(_currentZoom);
            Vector3 translateCenter = new Vector3(_translateCenter, 0f);
            Vector3 translateBody = new Vector3(-_currentPosition, 0f);

            SimView = Matrix.CreateTranslation(translateBody) *
                      matRotation *
                      matZoom *
                      Matrix.CreateTranslation(translateCenter);

            translateCenter = ConvertUnits.ToDisplayUnits(translateCenter);
            translateBody = ConvertUnits.ToDisplayUnits(translateBody);

            View = Matrix.CreateTranslation(translateBody) *
                   matRotation *
                   matZoom *
                   Matrix.CreateTranslation(translateCenter);
        }

        /// <summary>Moves the camera forward one timestep.</summary>
        public void Update(GameTime gameTime)
        {
            if (_trackingBody != null)
            {
                if (_positionTracking)
                {
                    _targetPosition = _trackingBody.Position;
                    if (_minPosition != _maxPosition)
                        Vector2.Clamp(ref _targetPosition, ref _minPosition, ref _maxPosition, out _targetPosition);
                }

                if (_rotationTracking)
                {
                    _targetRotation = -_trackingBody.Rotation % MathHelper.TwoPi;
                    if (_minRotation != _maxRotation)
                        _targetRotation = MathHelper.Clamp(_targetRotation, _minRotation, _maxRotation);
                }
            }

            Vector2 delta = _targetPosition - _currentPosition;
            float distance = delta.Length();
            if (distance > 0f)
                delta /= distance;

            float inertia;
            if (distance < 10f)
                inertia = (float)Math.Pow(distance / 10.0, 2.0);
            else
                inertia = 1f;

            float rotDelta = _targetRotation - _currentRotation;
            float rotInertia;
            if (Math.Abs(rotDelta) < 5f)
                rotInertia = (float)Math.Pow(rotDelta / 5.0, 2.0);
            else
                rotInertia = 1f;
            if (Math.Abs(rotDelta) > 0f)
                rotDelta /= Math.Abs(rotDelta);

            _currentPosition += 100f * delta * inertia * (float)gameTime.ElapsedGameTime.TotalSeconds;
            _currentRotation += 80f * rotDelta * rotInertia * (float)gameTime.ElapsedGameTime.TotalSeconds;

            SetView();
        }

        public Vector2 ConvertScreenToWorld(Vector2 location)
        {
            Vector3 t = new Vector3(location, 0);

            t = _graphics.Viewport.Unproject(t, SimProjection, SimView, Matrix.Identity);

            return new Vector2(t.X, t.Y);
        }

        public Vector2 ConvertWorldToScreen(Vector2 location)
        {
            Vector3 t = new Vector3(location, 0);

            t = _graphics.Viewport.Project(t, SimProjection, SimView, Matrix.Identity);

            return new Vector2(t.X, t.Y);
        }
    }
}