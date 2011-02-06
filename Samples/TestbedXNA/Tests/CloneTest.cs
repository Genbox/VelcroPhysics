using System.Collections.Generic;
using FarseerPhysics.Common;
using FarseerPhysics.Common.Decomposition;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.TestBed.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerPhysics.TestBed.Tests
{
    public class CloneTest : Test
    {
        private CloneTest()
        {
            //Ground
            FixtureFactory.CreateEdge(World, new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));

            Fixture box = FixtureFactory.CreateRectangle(World, 5, 5, 5);
            box.Restitution = 0.8f;
            box.Friction = 0.9f;
            box.Body.BodyType = BodyType.Dynamic;
            box.Body.Position = new Vector2(10, 10);
            box.Body.SleepingAllowed = false;
            box.Body.LinearDamping = 1;
            box.Body.AngularDamping = 0.5f;
            box.Body.AngularVelocity = 0.5f;
            box.Body.LinearVelocity = new Vector2(0, 10);

            Fixture boxClone1 = box.DeepClone();
            //Swiching the body type to static will reset all forces. This will affect the next clone.
            boxClone1.Body.BodyType = BodyType.Static;
            boxClone1.Body.Position += new Vector2(-10, 0);

            Fixture boxClone2 = boxClone1.DeepClone();
            boxClone2.Body.BodyType = BodyType.Dynamic;
            boxClone2.Body.Position += new Vector2(-10, 0);
        }

        public override void Initialize()
        {
            Texture2D polygonTexture = GameInstance.Content.Load<Texture2D>("Texture");
            uint[] data = new uint[polygonTexture.Width * polygonTexture.Height];
            polygonTexture.GetData(data);

            Vertices verts = PolygonTools.CreatePolygon(data, polygonTexture.Width);

            Vector2 scale = new Vector2(0.07f, -0.07f);
            verts.Scale(ref scale);

            Vector2 centroid = -verts.GetCentroid();
            verts.Translate(ref centroid);

            List<Fixture> compund = FixtureFactory.CreateCompoundPolygon(World, BayazitDecomposer.ConvexPartition(verts), 1);
            compund[0].Body.Position = new Vector2(-25, 30);

            Body b = compund[0].Body.DeepClone();
            b.Position = new Vector2(20, 30);
            b.BodyType = BodyType.Dynamic;

            base.Initialize();
        }

        public static CloneTest Create()
        {
            return new CloneTest();
        }
    }
}