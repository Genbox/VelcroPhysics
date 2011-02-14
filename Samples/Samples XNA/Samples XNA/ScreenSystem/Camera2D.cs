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
        private Matrix _projection;

        private Vector2 _currentPosition;
        private Vector2 _targetPosition;

        private float _currentRotation;
        private float _targetRotation;

        private bool _tracking;

        /// <summary>
        /// The constructor for the Camera2D class.
        /// </summary>
        /// <param name="graphics"></param>
        public Camera2D(GraphicsDevice graphics)
        {
            _graphics = graphics;
            _projection = Matrix.CreateOrthographic(ConvertUnits.ToSimUnits(_graphics.Viewport.Width),
                                                    ConvertUnits.ToSimUnits(_graphics.Viewport.Height), 0f, 1f);
            _view = Matrix.Identity;

            ResetCamera();
        }

        public Matrix View
        {
            get { return _view; }
        }

        public Matrix Projection
        {
            get { return _projection; }
        }

        /// <summary>
        /// The current position of the camera.
        /// </summary>
        public Vector2 Position
        {
            get { return _currentPosition; }
            set { _targetPosition = Vector2.Clamp(value, MinPosition, MaxPosition); }
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
        /// The current rotation of the camera in radians.
        /// </summary>
        public float Rotation
        {
            get { return _currentRotation; }
            set
            {
                _targetRotation = MathHelper.Clamp(value, MinRotation, MaxRotation);
            }
        }

        /// <summary>
        /// Gets or sets the minimum rotation in radians.
        /// </summary>
        /// <value>The min rotation.</value>
        public float MinRotation { get; set; }

        /// <summary>
        /// Gets or sets the maximum rotation in radians.
        /// </summary>
        /// <value>The max rotation.</value>
        public float MaxRotation { get; set; }

        /// <summary>
        /// the body that this camera is currently tracking. 
        /// Null if not tracking any.
        /// </summary>
        public Body TrackingBody { get; set; }
        public bool EnableTargetTracking
        {
            get { return _tracking; }
            set
            {
                if (value == true && TrackingBody != null)
                {
                    _tracking = true;
                }
                else
                {
                    _tracking = false;
                }
            }
        }

        public void MoveCamera(Vector2 amount)
        {
            _targetPosition += amount;
            _tracking = false;
        }

        public void RotateCamera(float amount)
        {
            _targetRotation += amount;
            _tracking = false;
        }

        /// <summary>
        /// Resets the camera to default values.
        /// </summary>
        public void ResetCamera()
        {
            _currentPosition = Vector2.Zero;
            _targetPosition = Vector2.Zero;
            MinPosition = Vector2.Zero;
            MaxPosition = Vector2.Zero;

            _currentRotation = 0f;
            _targetRotation = 0f;
            MinRotation = -MathHelper.Pi;
            MaxRotation = MathHelper.Pi;

            _tracking = false;

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
            _view = Matrix.CreateRotationZ(_currentRotation) *
                    Matrix.CreateTranslation(-_currentPosition.X, -_currentPosition.Y, 0f);
        }

        /// <summary>
        /// Moves the camera forward one timestep.
        /// </summary>
        public void Update(GameTime gameTime)
        {
            if (_tracking && TrackingBody != null)
            {
                _targetPosition = TrackingBody.Position;
                _targetRotation = TrackingBody.Rotation;
            }

            Vector2 _delta = _targetPosition - _currentPosition;
            float _distance = _delta.Length();
            if (_distance > 0f)
            {
                _delta /= _distance;
            }
            float _inertia;
            if (_distance < 1.5f)
            {
                _inertia = (float)Math.Pow(_distance / 1.5, 2.0);
            }
            else
            {
                _inertia = 1f;
            }

            float _rotDelta = _targetRotation - _currentRotation;

            float _rotInertia;
            if (_rotDelta < 10f)
            {
                _rotInertia = (float)Math.Pow(_rotDelta / 10.0, 2.0);
            }
            else
            {
                _rotInertia = 1f;
            }

            _currentPosition += _delta * _inertia * (float)gameTime.ElapsedGameTime.TotalSeconds;
            _currentRotation += 10f * _rotInertia * (float)gameTime.ElapsedGameTime.TotalSeconds;

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