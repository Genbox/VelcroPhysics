/*
* Velcro Physics:
* Copyright (c) 2017 Ian Qvist
* 
* Original source Box2D:
* Copyright (c) 2006-2011 Erin Catto http://www.box2d.org 
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

using Genbox.VelcroPhysics.Collision.Filtering;
using Genbox.VelcroPhysics.Collision.Shapes;
using Genbox.VelcroPhysics.Definitions;
using Genbox.VelcroPhysics.Definitions.Joints;
using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Factories;
using Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Framework;
using Genbox.VelcroPhysics.Shared;
using Microsoft.Xna.Framework;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Tests
{
    /// <summary>This is a test of collision filtering. There is a triangle, a box, and a circle. There are 6 shapes. 3 large
    /// and 3 small. The 3 small ones always collide. The 3 large ones never collide. The boxes don't collide with triangles
    /// (except if both are small).</summary>
    internal class CollisionFilteringTest : Test
    {
        private const short _smallGroup = 1;
        private const short _largeGroup = -1;

        private const Category _triangleCategory = (Category)0x0002;
        private const Category _boxCategory = (Category)0x0004;
        private const Category _circleCategory = (Category)0x0008;

        private const Category _triangleMask = (Category)0xFFFF;
        private const Category _boxMask = (Category)(0xFFFF ^ (int)_triangleCategory);
        private const Category _circleMask = (Category)0xFFFF;

        private CollisionFilteringTest()
        {
            //Ground
            BodyFactory.CreateEdge(World, new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));

            // Ground body
            {
                EdgeShape shape = new EdgeShape(new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));

                FixtureDef sd = new FixtureDef();
                sd.Shape = shape;
                sd.Friction = 0.3f;

                BodyDef bd = new BodyDef();
                Body ground = BodyFactory.CreateFromDef(World, bd);
                ground.AddFixture(sd);
            }

            // Small triangle
            Vertices vertices = new Vertices(3);
            vertices.Add(new Vector2(-1.0f, 0.0f));
            vertices.Add(new Vector2(1.0f, 0.0f));
            vertices.Add(new Vector2(0.0f, 2.0f));

            PolygonShape polygon = new PolygonShape(vertices, 1.0f);

            FixtureDef triangleShapeDef = new FixtureDef();
            triangleShapeDef.Shape = polygon;

            triangleShapeDef.Filter.Group = _smallGroup;
            triangleShapeDef.Filter.Category = _triangleCategory;
            triangleShapeDef.Filter.CategoryMask = _triangleMask;

            BodyDef triangleBodyDef = new BodyDef();
            triangleBodyDef.Type = BodyType.Dynamic;
            triangleBodyDef.Position = new Vector2(-5.0f, 2.0f);

            Body body1 = BodyFactory.CreateFromDef(World, triangleBodyDef);
            body1.AddFixture(triangleShapeDef);

            // Large triangle (recycle definitions)
            vertices[0] *= 2.0f;
            vertices[1] *= 2.0f;
            vertices[2] *= 2.0f;
            polygon.Vertices = vertices;
            triangleShapeDef.Filter.Group = _largeGroup;
            triangleBodyDef.Position = new Vector2(-5.0f, 6.0f);
            triangleBodyDef.FixedRotation = true; // look at me!

            Body body2 = BodyFactory.CreateFromDef(World, triangleBodyDef);
            body2.AddFixture(triangleShapeDef);

            {
                BodyDef bd = new BodyDef();
                bd.Type = BodyType.Dynamic;
                bd.Position = new Vector2(-5.0f, 10.0f);
                Body body = BodyFactory.CreateFromDef(World, bd);

                PolygonShape p = new PolygonShape(1.0f);
                p.SetAsBox(0.5f, 1.0f);
                body.AddFixture(p);

                PrismaticJointDef jd = new PrismaticJointDef();
                jd.BodyA = body2;
                jd.BodyB = body;
                jd.EnableLimit = true;
                jd.LocalAnchorA = new Vector2(0.0f, 4.0f);
                jd.LocalAnchorB = Vector2.Zero;
                jd.LocalAxisA = new Vector2(0.0f, 1.0f);
                jd.LowerTranslation = -1.0f;
                jd.UpperTranslation = 1.0f;

                JointFactory.CreateFromDef(World, jd);
            }

            // Small box
            polygon.SetAsBox(1.0f, 0.5f);
            FixtureDef boxShapeDef = new FixtureDef();
            boxShapeDef.Shape = polygon;
            boxShapeDef.Restitution = 0.1f;

            boxShapeDef.Filter.Group = _smallGroup;
            boxShapeDef.Filter.Category = _boxCategory;
            boxShapeDef.Filter.CategoryMask = _boxMask;

            BodyDef boxBodyDef = new BodyDef();
            boxBodyDef.Type = BodyType.Dynamic;
            boxBodyDef.Position = new Vector2(0.0f, 2.0f);

            Body body3 = BodyFactory.CreateFromDef(World, boxBodyDef);
            body3.AddFixture(boxShapeDef);

            // Large box (recycle definitions)
            polygon.SetAsBox(2.0f, 1.0f);
            boxShapeDef.Filter.Group = _largeGroup;
            boxBodyDef.Position = new Vector2(0.0f, 6.0f);

            Body body4 = BodyFactory.CreateFromDef(World, boxBodyDef);
            body4.AddFixture(boxShapeDef);

            // Small circle
            CircleShape circle = new CircleShape(1.0f, 1.0f);

            FixtureDef circleShapeDef = new FixtureDef();
            circleShapeDef.Shape = circle;

            circleShapeDef.Filter.Group = _smallGroup;
            circleShapeDef.Filter.Category = _circleCategory;
            circleShapeDef.Filter.CategoryMask = _circleMask;

            BodyDef circleBodyDef = new BodyDef();
            circleBodyDef.Type = BodyType.Dynamic;
            circleBodyDef.Position = new Vector2(5.0f, 2.0f);

            Body body5 = BodyFactory.CreateFromDef(World, circleBodyDef);
            body5.AddFixture(circleShapeDef);

            // Large circle
            circle.Radius *= 2.0f;
            circleShapeDef.Filter.Group = _largeGroup;
            circleBodyDef.Position = new Vector2(5.0f, 6.0f);

            Body body6 = BodyFactory.CreateFromDef(World, circleBodyDef);
            body6.AddFixture(circleShapeDef);
        }

        internal static Test Create()
        {
            return new CollisionFilteringTest();
        }
    }
}