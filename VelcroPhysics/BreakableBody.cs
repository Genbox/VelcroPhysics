using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using VelcroPhysics.Collision.ContactSystem;
using VelcroPhysics.Collision.Shapes;
using VelcroPhysics.Dynamics.Solver;
using VelcroPhysics.Factories;
using VelcroPhysics.Shared;

namespace VelcroPhysics.Dynamics
{
    /// <summary>
    /// A type of body that supports multiple fixtures that can break apart.
    /// </summary>
    public class BreakableBody
    {
        private float[] _angularVelocitiesCache = new float[8];
        private bool _break;
        private Vector2[] _velocitiesCache = new Vector2[8];
        private readonly World _world;

        /// <summary>
        /// The force needed to break the body apart.
        /// Default: 500
        /// </summary>
        public float Strength { get; set; }

        public BreakableBody(World world, IEnumerable<Vertices> vertices, float density, Vector2 position = new Vector2(), float rotation = 0)
        {
            _world = world;
            _world.ContactManager.PostSolve += PostSolve;
            Parts = new List<Fixture>(8);
            MainBody = BodyFactory.CreateBody(_world, position, rotation, BodyType.Dynamic);
            Strength = 500.0f;

            foreach (Vertices part in vertices)
            {
                PolygonShape polygonShape = new PolygonShape(part, density);
                Fixture fixture = MainBody.CreateFixture(polygonShape);
                Parts.Add(fixture);
            }
        }

        public BreakableBody(World world, IEnumerable<Shape> shapes, Vector2 position = new Vector2(), float rotation = 0)
        {
            _world = world;
            _world.ContactManager.PostSolve += PostSolve;
            MainBody = BodyFactory.CreateBody(_world, position, rotation, BodyType.Dynamic);
            Parts = new List<Fixture>(8);

            foreach (Shape part in shapes)
            {
                Fixture fixture = MainBody.CreateFixture(part);
                Parts.Add(fixture);
            }
        }

        public bool Broken { get; private set; }
        public Body MainBody { get; }
        public List<Fixture> Parts { get; }

        private void PostSolve(Contact contact, ContactVelocityConstraint impulse)
        {
            if (!Broken)
            {
                if (Parts.Contains(contact.FixtureA) || Parts.Contains(contact.FixtureB))
                {
                    float maxImpulse = 0.0f;
                    int count = contact.Manifold.PointCount;

                    for (int i = 0; i < count; ++i)
                    {
                        maxImpulse = Math.Max(maxImpulse, impulse.Points[i].NormalImpulse);
                    }

                    if (maxImpulse > Strength)
                    {
                        // Flag the body for breaking.
                        _break = true;
                    }
                }
            }
        }

        public void Update()
        {
            if (_break)
            {
                Decompose();
                Broken = true;
                _break = false;
            }

            // Cache velocities to improve movement on breakage.
            if (Broken == false)
            {
                //Enlarge the cache if needed
                if (Parts.Count > _angularVelocitiesCache.Length)
                {
                    _velocitiesCache = new Vector2[Parts.Count];
                    _angularVelocitiesCache = new float[Parts.Count];
                }

                //Cache the linear and angular velocities.
                for (int i = 0; i < Parts.Count; i++)
                {
                    _velocitiesCache[i] = Parts[i].Body.LinearVelocity;
                    _angularVelocitiesCache[i] = Parts[i].Body.AngularVelocity;
                }
            }
        }

        private void Decompose()
        {
            //Unsubsribe from the PostSolve delegate
            _world.ContactManager.PostSolve -= PostSolve;

            for (int i = 0; i < Parts.Count; i++)
            {
                Fixture oldFixture = Parts[i];

                Shape shape = oldFixture.Shape.Clone();
                object userData = oldFixture.UserData;

                MainBody.DestroyFixture(oldFixture);

                Body body = BodyFactory.CreateBody(_world, MainBody.Position, MainBody.Rotation, BodyType.Dynamic, MainBody.UserData);

                Fixture newFixture = body.CreateFixture(shape);
                newFixture.UserData = userData;
                Parts[i] = newFixture;

                body.AngularVelocity = _angularVelocitiesCache[i];
                body.LinearVelocity = _velocitiesCache[i];
            }

            _world.RemoveBody(MainBody);
            _world.RemoveBreakableBody(this);
        }

        public void Break()
        {
            _break = true;
        }
    }
}