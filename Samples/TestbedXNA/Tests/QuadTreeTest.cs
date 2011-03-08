using System;
using FarseerPhysics.Collision;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.TestBed.Framework;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.TestBed.Tests
{
    public class QuadTreeTest : Test
    {
        Vector2 worldSize;

        public override void Initialize()
        {
            Settings.VelocityIterations = 2;
            Settings.PositionIterations = 4;

            GameInstance.ViewCenter = Vector2.Zero;

            worldSize = 2 * GameInstance.ConvertScreenToWorld(GameInstance.Window.ClientBounds.Width, 0);

            //Create a World using QuadTree constructor
            World = new World(new Vector2(0.0f, -10.0f), new AABB(-worldSize / 2, worldSize / 2));
            
            //Create a World using DynamicTree constructor
            //World = new World(new Vector2(0.0f, -10.0f));

            //
            //set up border
            //

            float halfWidth = worldSize.X / 2 - 2f;
            float halfHeight = worldSize.Y / 2 - 2f;

            Vertices borders = new Vertices(4);
            borders.Add(new Vector2(-halfWidth, halfHeight));
            borders.Add(new Vector2(halfWidth, halfHeight));
            borders.Add(new Vector2(halfWidth, -halfHeight));
            borders.Add(new Vector2(-halfWidth, -halfHeight));

            Body anchor = BodyFactory.CreateLoopShape(World, borders);
            anchor.CollisionCategories = Category.All;
            anchor.CollidesWith = Category.All;

            //
            //box
            //

            Vertices bigbox = PolygonTools.CreateRectangle(3f, 3f);
            PolygonShape bigshape = new PolygonShape(bigbox, 5);

            Body bigbody = BodyFactory.CreateBody(World);
            bigbody.BodyType = BodyType.Dynamic;
            bigbody.Position = Vector2.UnitX * 25;
            bigbody.CreateFixture(bigshape);

            //
            //populate
            //
            const int rad = 12;
            const float a = 0.6f;
            const float sep = 0.000f;

            Vector2 cent = Vector2.Zero;

            for (int y = -rad; y <= +rad; y++)
            {
                int xrad = (int)Math.Round(Math.Sqrt(rad * rad - y * y));
                for (int x = -xrad; x <= +xrad; x++)
                {
                    Vector2 pos = cent + new Vector2(x * (2 * a + sep), y * (2 * a + sep));
                    Body cBody = BodyFactory.CreateCircle(World, a, 55, pos);
                    cBody.BodyType = BodyType.Dynamic;
                }
            }


            /*for (int i = 0; i < 50; i++)
            {
                Vector2 pos = new Vector2(Rand.RandomFloat(-worldSize.X / 2, worldSize.X / 2), Rand.RandomFloat(-worldSize.Y / 2, worldSize.Y / 2));
                Body cBody = BodyFactory.CreateCircle(World, 1.0f, 5f, pos);
                cBody.BodyType = BodyType.Static;
            }*/


            base.Initialize();
        }

        AABB randomAABB()
        {
            AABB aabb = new AABB();
            aabb.LowerBound.X = Rand.RandomFloat(0.0f, worldSize.X);
            aabb.LowerBound.Y = Rand.RandomFloat(0.0f, worldSize.Y);
            aabb.UpperBound.X = aabb.LowerBound.X + Rand.RandomFloat(0.0f, 2.0f);
            aabb.UpperBound.X = aabb.LowerBound.X + Rand.RandomFloat(0.0f, 2.0f);
            return aabb;
        }
        void testCycle()
        {
            for (int i = 0; i < 10000; i++)
            {
                var aabb = randomAABB();
                World.ContactManager.BroadPhase.Query(id => true, ref aabb);
            }

        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            GameInstance.ViewCenter = Vector2.Zero;

            /*DateTime start, end;

            start = DateTime.Now;
            testCycle();
            end = DateTime.Now;
            var time = (end - start).TotalMilliseconds;

            DebugView.DrawString(0, 0, time.ToString());*/

            base.Update(settings, gameTime);
        }

        internal static Test Create()
        {
            return new QuadTreeTest();
        }
    }
}
