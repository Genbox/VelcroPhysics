using System.Collections.Generic;
using FarseerGames.FarseerPhysics.Mathematics;
using FarseerGames.FarseerPhysics.Dynamics;
#if (XNA)
using Microsoft.Xna.Framework;
#endif

namespace FarseerGames.FarseerPhysics.Controllers
{
    /// <summary>
    /// This class offers a way to simulate planetary gravity.
    /// You can get 2 types of gravity: Distance Squared and Linear.
    /// Distance Squared is the realistic method to apply gravity.
    /// Linear is a simpler method of calculating the strength but will not be very realistic (or significantly impact performance).
    /// you can also specify a maximum strength and radius.
    /// </summary>
    public class GravityController : Controller
    {
        private GravityType _gravityType = GravityType.Linear;
        private float _strength;
        private PhysicsSimulator _simulator;
        private List<Vector2> _pointList;
        private List<Body> _bodyList;
        private float _radius;

        /// <summary>
        /// Initializes the GravityController.
        /// </summary>
        /// <param name="simulator">The physicsSimulator used by this controller.</param>
        /// <param name="bodies">The bodies that you want to generate gravity.</param>
        /// <param name="points">The points that you want to generate gravity.</param>
        /// <param name="strength">the strength of gravity (the gravity strength when two bodies are on the same spot)</param>
        /// <param name="radius">the maximum distance that can be between 2 bodies before it will stop trying to apply gravity between them.</param>
        public GravityController(PhysicsSimulator simulator, List<Body> bodies, List<Vector2> points, float strength, float radius)
        {
            _simulator = simulator;
            _strength = strength;

            if (_gravityType == GravityType.DistanceSquared)
                _strength *= 100;

            _radius = radius;
            _pointList = points;
            _bodyList = bodies;
        }

        /// <summary>
        /// Initializes the GravityController.
        /// </summary>
        /// <param name="simulator">The physicsSimulator used by this controller.</param>
        /// <param name="bodies">The bodies that you want to generate gravity.</param>
        /// <param name="strength">the strength of gravity (the gravity strength when two bodies are on the same spot)</param>
        /// <param name="radius">the maximum distance that can be between 2 bodies before it will stop trying to apply gravity between them.</param>
        public GravityController(PhysicsSimulator simulator, List<Body> bodies, float strength, float radius)
        {
            _simulator = simulator;
            _strength = strength;

            if (_gravityType == GravityType.DistanceSquared)
                _strength *= 100;

            _radius = radius;
            _bodyList = bodies;
        }

        /// <summary>
        /// Initializes the GravityController.
        /// </summary>
        /// <param name="simulator">The physicsSimulator used by this controller.</param>
        /// <param name="points">The points that you want to generate gravity.</param>
        /// <param name="strength">the strength of gravity (the gravity strength when two bodies are on the same spot)</param>
        /// <param name="radius">the maximum distance that can be between 2 bodies before it will stop trying to apply gravity between them.</param>
        public GravityController(PhysicsSimulator simulator, List<Vector2> points, float strength, float radius)
        {
            _simulator = simulator;
            _strength = strength;

            if (_gravityType == GravityType.DistanceSquared)
                _strength *= 100;

            _radius = radius;
            _pointList = points;
        }

        public GravityType GravityType
        {
            get { return _gravityType; }
            set { _gravityType = value; }
        }

        /// <summary>
        /// The list of points that act as gravity generators.
        /// </summary>
        public List<Vector2> PointList
        {
            get { return _pointList; }
            set { _pointList = value; }
        }

        /// <summary>
        /// The list of bodies that act as gravity generators.
        /// </summary>
        public List<Body> BodyList
        {
            get { return _bodyList; }
            set { _bodyList = value; }
        }

        public override void Validate()
        {
            //Do nothing
        }

        public override void Update(float dt, float dtReal)
        {
            foreach (Body body in _simulator.BodyList)
            {
                if (body.IsDisposed || !body.Enabled)
                    continue;

                if (_bodyList != null)
                    foreach (Body body2 in _bodyList)
                    {
                        if (body == body2 || (body.isStatic && body2.isStatic) || body2.IsDisposed || !body2.Enabled)
                            continue;

                        Vector2 difference = body2.position - body.position;
                        Vector2 differenceNormal = difference;
                        differenceNormal.Normalize();

                        float distance = difference.Length();
                        if (distance > _radius)
                            continue;

                        Vector2 force = Vector2.Multiply(differenceNormal, _strength);
                        if (_gravityType == GravityType.DistanceSquared)
                        {
                            force = Vector2.Divide(force, distance * distance);
                        }
                        else if (_gravityType == GravityType.Linear)
                        {
                            force = Vector2.Divide(force, distance);
                        }
                        body.ApplyForce(ref force);
                        body2.ApplyForce(ref force);
                    }

                if (_pointList != null)
                    foreach (Vector2 anchor in _pointList)
                    {
                        Vector2 difference = anchor - body.position;
                        Vector2 differenceNormal = difference;
                        differenceNormal.Normalize();

                        float distance = difference.Length();
                        if (distance > _radius)
                            continue;

                        Vector2 force = Vector2.Multiply(differenceNormal, _strength);
                        if (_gravityType == GravityType.DistanceSquared)
                        {
                            force = Vector2.Divide(force, distance * distance);
                        }
                        else if (_gravityType == GravityType.Linear)
                        {
                            force = Vector2.Divide(force, distance);
                        }

                        body.ApplyForce(ref force);
                    }
            }
        }
    }
}
