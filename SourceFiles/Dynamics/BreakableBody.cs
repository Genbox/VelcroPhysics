/*
* Farseer Physics Engine based on Box2D.XNA port:
* Copyright (c) 2010 Ian Qvist
* 
* Box2D.XNA port of Box2D:
* Copyright (c) 2009 Brandon Furtwangler, Nathan Furtwangler
*
* Original source Box2D:
* Copyright (c) 2006-2009 Erin Catto http://www.gphysics.com 
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
*/

using System;
using System.Collections.Generic;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Dynamics
{
    /// <summary>
    /// A type of body that supports multiple fixtures that can break apart.
    /// </summary>
    public class BreakableBody
    {
        public bool Broken;
        public Body MainBody;
        public List<Fixture> Parts = new List<Fixture>(8);
        public float Strength = 500.0f;
        private float[] _angularVelocitiesCache = new float[8];
        private bool _break;
        private Vector2[] _velocitiesCache = new Vector2[8];
        private World _world;

        public BreakableBody(List<Vertices> vertices, World world, float density)
        {
            _world = world;
            MainBody = _world.CreateBody();
            MainBody.BodyType = BodyType.Dynamic;

            foreach (Vertices part in vertices)
            {
                PolygonShape polygonShape = new PolygonShape(part);
                Fixture fixture = MainBody.CreateFixture(polygonShape, density);
                fixture.PostSolve += PostSolve;
                Parts.Add(fixture);
            }
        }

        private void PostSolve(ContactConstraint contactConstraint)
        {
            if (!Broken)
            {
                float maxImpulse = 0.0f;
                for (int i = 0; i < contactConstraint.Manifold.PointCount; ++i)
                {
                    maxImpulse = Math.Max(maxImpulse, contactConstraint.Manifold.Points[0].NormalImpulse);
                    maxImpulse = Math.Max(maxImpulse, contactConstraint.Manifold.Points[1].NormalImpulse);
                }

                if (maxImpulse > Strength)
                {
                    // Flag the body for breaking.
                    _break = true;
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
            for (int i = 0; i < Parts.Count; i++)
            {
                Fixture fixture = Parts[i];

                //Unsubsribe from the PostSolve delegate
                fixture.PostSolve -= PostSolve;

                Shape shape = fixture.Shape.Clone();

                MainBody.DestroyFixture(fixture);

                Body body = BodyFactory.CreateBody(_world);
                body.BodyType = BodyType.Dynamic;
                body.Position = MainBody.Position;
                body.Rotation = MainBody.Rotation;

                body.CreateFixture(shape, fixture.Shape.Density);

                body.AngularVelocity = _angularVelocitiesCache[i];
                body.LinearVelocity = _velocitiesCache[i];
            }
        }

        public void Break()
        {
            _break = true;
        }
    }
}