using System;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerPhysics.SamplesFramework
{
    public class Camera2D
    {
        private static GraphicsDevice _graphics;

        private Matrix _view;
        private Matrix _batchView;
        private Matrix _projection;

        private Vector2 _currentPosition;
        private Vector2 _targetPosition;
        private Vector2 _minPosition;
        private Vector2 _maxPosition;
        private Vector2 _translateCenter;

        private float _currentRotation;
        private float _targetRotation;
        private float _minRotation;
        private float _maxRotation;

        private Body _trackingBody;
        private bool _positionTracking;
        private bool _rotationTracking;

        /// <summary>
        /// The constructor for the Camera2D class.
        /// </summary>
        /// <param name="graphics"></param>
        public Camera2D(GraphicsDevice graphics)
        {
            _graphics = graphics;
            _projection = Matrix.CreateOrthographicOffCenter(0f, ConvertUnits.ToSimUnits(_graphics.Viewport.Width),
                                                             ConvertUnits.ToSimUnits(_graphics.Viewport.Height), 0f, 0f, 1f);
            _view = Matrix.Identity;
            _batchView = Matrix.Identity;

            _translateCenter = new Vector2(ConvertUnits.ToSimUnits(_graphics.Viewport.Width / 2f),
                                           ConvertUnits.ToSimUnits(_graphics.Viewport.Height / 2f));

            ResetCamera();
        }

        public Matrix View
        {
            get { return _batchView; }
        }

        public Matrix SimView
        {
            get { return _view; }
        }

        public Matrix SimProjection
        {
            get { return _projection; }
        }

        /// <summary>
        /// The current position of the camera.
        /// </summary>
        public Vector2 Position
        {
            get { return ConvertUnits.ToDisplayUnits(_currentPosition); }
            set
            {
                _targetPosition = ConvertUnits.ToSimUnits(value);
                if (_minPosition != _maxPosition)
                {
                    Vector2.Clamp(ref _targetPosition, ref _minPosition, ref _maxPosition, out  _targetPosition);
                }
            }
        }

        /// <summary>
        /// The furthest up, and the furthest left the camera can go.
        /// if this value equals maxPosition, then no clamping will be 
        /// applied (unless you override that function).
        /// </summary>
        public Vector2 MinPosition
        {
            get { return ConvertUnits.ToDisplayUnits(_minPosition); }
            set { _minPosition = ConvertUnits.ToSimUnits(value); }
        }

        /// <summary>
        /// the furthest down, and the furthest right the camera will go.
        /// if this value equals minPosition, then no clamping will be 
        /// applied (unless you override that function).
        /// </summary>
        public Vector2 MaxPosition
        {
            get { return ConvertUnits.ToDisplayUnits(_maxPosition); }
            set { _maxPosition = ConvertUnits.ToSimUnits(value); }
        }

        /// <summary>
        /// The current rotation of the camera in radians.
        /// </summary>
        public float Rotation
        {
            get { return _currentRotation; }
            set
            {
                _targetRotation = value % MathHelper.TwoPi;
                if (_minRotation != _maxRotation)
                {
                    _targetRotation = MathHelper.Clamp(_targetRotation, _minRotation, _maxRotation);
                }
            }
        }

        /// <summary>
        /// Gets or sets the minimum rotation in radians.
        /// </summary>
        /// <value>The min rotation.</value>
        public float MinRotation
        {
            get { return _minRotation; }
            set { _minRotation = MathHelper.Clamp(value, -MathHelper.Pi, 0f); }
        }

        /// <summary>
        /// Gets or sets the maximum rotation in radians.
        /// </summary>
        /// <value>The max rotation.</value>
        public float MaxRotation
        {
            get { return _maxRotation; }
            set { _maxRotation = MathHelper.Clamp(value, 0f, MathHelper.Pi); }
        }

        /// <summary>
        /// the body that this camera is currently tracking. 
        /// Null if not tracking any.
        /// </summary>
        public Body TrackingBody
        {
            get { return _trackingBody; }
            set
            {
                _trackingBody = value;
                if (_trackingBody != null)
                {
                    _positionTracking = true;
                }
            }
        }
        public bool EnablePositionTracking
        {
            get { return _positionTracking; }
            set
            {
                if (value == true && _trackingBody != null)
                {
                    _positionTracking = true;
                }
                else
                {
                    _positionTracking = false;
                }
            }
        }

        public bool EnableRotationTracking
        {
            get { return _rotationTracking; }
            set
            {
                if (value == true && _trackingBody != null)
                {
                    _rotationTracking = true;
                }
                else
                {
                    _rotationTracking = false;
                }
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
            Position += amount;
            _positionTracking = false;
            _rotationTracking = false;
        }

        public void RotateCamera(float amount)
        {
            Rotation += amount;
            _positionTracking = false;
            _rotationTracking = false;
        }

        /// <summary>
        /// Resets the camera to default values.
        /// </summary>
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
            Vector3 translateCenter = new Vector3(_translateCenter, 0f);
            Vector3 translateBody = new Vector3(-_currentPosition, 0f);

            _view = Matrix.CreateTranslation(translateBody) *
                    matRotation *
                    Matrix.CreateTranslation(translateCenter);

            translateCenter = ConvertUnits.ToDisplayUnits(translateCenter);
            translateBody = ConvertUnits.ToDisplayUnits(translateBody);

            _batchView = Matrix.CreateTranslation(translateBody) *
                         matRotation *
                         Matrix.CreateTranslation(translateCenter);
        }

        /// <summary>
        /// Moves the camera forward one timestep.
        /// </summary>
        public void Update(GameTime gameTime)
        {
            if (_trackingBody != null)
            {
                if (_positionTracking)
                {
                    _targetPosition = _trackingBody.Position;
                    if (_minPosition != _maxPosition)
                    {
                        Vector2.Clamp(ref _targetPosition, ref _minPosition, ref _maxPosition, out  _targetPosition);
                    }
                }
                if (_rotationTracking)
                {
                    _targetRotation = -_trackingBody.Rotation % MathHelper.TwoPi;
                    if (_minRotation != _maxRotation)
                    {
                        _targetRotation = MathHelper.Clamp(_targetRotation, _minRotation, _maxRotation);
                    }
                }
            }
            Vector2 _delta = _targetPosition - _currentPosition;
            float _distance = _delta.Length();
            if (_distance > 0f)
            {
                _delta /= _distance;
            }
            float _inertia;
            if (_distance < 10f)
            {
                _inertia = (float)Math.Pow(_distance / 10, 2.0);
            }
            else
            {
                _inertia = 1f;
            }

            float _rotDelta = _targetRotation - _currentRotation;

            float _rotInertia;
            if (Math.Abs(_rotDelta) < 5f)
            {
                _rotInertia = (float)Math.Pow(_rotDelta / 5.0, 2.0);
            }
            else
            {
                _rotInertia = 1f;
            }
            if (Math.Abs(_rotDelta) > 0f)
            {
                _rotDelta /= Math.Abs(_rotDelta);
            }

            _currentPosition += 100f * _delta * _inertia * (float)gameTime.ElapsedGameTime.TotalSeconds;
            _currentRotation += 80f * _rotDelta * _rotInertia * (float)gameTime.ElapsedGameTime.TotalSeconds;

            SetView();
        }

        public Vector2 ConvertScreenToWorld(Vector2 location)
        {
            Vector3 t = new Vector3(location, 0);

            t = _graphics.Viewport.Unproject(t, _projection, _view, Matrix.Identity);

            return new Vector2(t.X, t.Y);
        }

        public Vector2 ConvertWorldToScreen(Vector2 location)
        {
            Vector3 t = new Vector3(location, 0);

            t = _graphics.Viewport.Project(t, _projection, _view, Matrix.Identity);

            return new Vector2(t.X, t.Y);
        }
    }
}