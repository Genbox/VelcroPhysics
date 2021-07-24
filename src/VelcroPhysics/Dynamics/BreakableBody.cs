using System;
using System.Collections.Generic;
using Genbox.VelcroPhysics.Collision.ContactSystem;
using Genbox.VelcroPhysics.Collision.Shapes;
using Genbox.VelcroPhysics.Dynamics.Solver;
using Genbox.VelcroPhysics.Factories;
using Genbox.VelcroPhysics.Shared;
using Microsoft.Xna.Framework;

namespace Genbox.VelcroPhysics.Dynamics
{
    /// <summary>A type of body that supports multiple fixtures that can break apart.</summary>
    public class BreakableBody
    {
        private readonly World _world;
        private float[] _angularVelocitiesCache = new float[8];
        private bool _break;
        private Vector2[] _velocitiesCache = new Vector2[8];

        public BreakableBody(World world, ICollection<Vertices> parts, float density, Vector2 position = new Vector2(), float rotation = 0)
        {
            _world = world;
            _world.ContactManager.PostSolve += PostSolve;
            Parts = new List<Fixture>(parts.Count);
            MainBody = BodyFactory.CreateBody(_world, position, rotation, BodyType.Dynamic);
            Strength = 500.0f;

            foreach (Vertices part in parts)
            {
                PolygonShape polygonShape = new PolygonShape(part, density);
                Fixture fixture = MainBody.AddFixture(polygonShape);
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
                Fixture fixture = MainBody.AddFixture(part);
                Parts.Add(fixture);
            }
        }

        /// <summary>The force needed to break the body apart. Default: 500</summary>
        public float Strength { get; set; }

        public bool Broken { get; private set; }
        public Body MainBody { get; }
        public List<Fixture> Parts { get; }

        private void PostSolve(Contact contact, ContactVelocityConstraint impulse)
        {
            if (!Broken)
            {
                if (Parts.Contains(contact._fixtureA) || Parts.Contains(contact._fixtureB))
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
            if (!Broken)
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

                MainBody.RemoveFixture(oldFixture);

                Body body = BodyFactory.CreateBody(_world, MainBody.Position, MainBody.Rotation, BodyType.Dynamic, MainBody.UserData);

                Fixture newFixture = body.AddFixture(shape);
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