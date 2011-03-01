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
    public class QTBroadTest : Test
    {
        private QTBroadTest()
        {

        }
        public override void Initialize()
        {
            Settings.VelocityIterations = 2;
            Settings.PositionIterations = 4;


            GameInstance.ViewCenter = Vector2.Zero;

            Vector2 worldSize = 2 * GameInstance.ConvertScreenToWorld(GameInstance.Window.ClientBounds.Width, 0);

            //recreate World, using QT constructor
            World = new Dynamics.World(new Vector2(0.0f, -10.0f), new AABB(-worldSize / 2, worldSize / 2));
            World.JointRemoved += JointRemoved;
            World.ContactManager.PreSolve += PreSolve;
            World.ContactManager.PostSolve += PostSolve;
            World.ContactManager.BeginContact += BeginContact;
            World.ContactManager.EndContact += EndContact;

            //
            //set up border
            //

            float _halfWidth = worldSize.X / 2 - 2f;
            float _halfHeight = worldSize.Y / 2 - 2f;

            Vertices borders = new Vertices(4);
            borders.Add(new Vector2(-_halfWidth, _halfHeight));
            borders.Add(new Vector2(_halfWidth, _halfHeight));
            borders.Add(new Vector2(_halfWidth, -_halfHeight));
            borders.Add(new Vector2(-_halfWidth, -_halfHeight));

            Body _anchor = BodyFactory.CreateLoopShape(World, borders);
            _anchor.CollisionCategories = Category.All;
            _anchor.CollidesWith = Category.All;

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
            int rad = 12;

            float a = 0.6f;

            float sep = 0.000f;
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
            base.Initialize();

        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {

            GameInstance.ViewCenter = Vector2.Zero;

            base.Update(settings, gameTime);
        }

        internal static Test Create()
        {
            return new QTBroadTest();
        }
    }
}
