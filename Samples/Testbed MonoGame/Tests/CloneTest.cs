using FarseerPhysics.Common;
using FarseerPhysics.Common.Decomposition;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.Testbed.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerPhysics.Testbed.Tests
{
    public class CloneTest : Test
    {
        private CloneTest()
        {
            //Ground
            BodyFactory.CreateEdge(World, new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));

            Body box = BodyFactory.CreateRectangle(World, 5, 5, 5);
            box.Restitution = 0.8f;
            box.Friction = 0.9f;
            box.BodyType = BodyType.Dynamic;
            box.Position = new Vector2(10, 10);
            box.SleepingAllowed = false;
            box.LinearDamping = 1;
            box.AngularDamping = 0.5f;
            box.AngularVelocity = 0.5f;
            box.LinearVelocity = new Vector2(0, 10);

            //This clones the body and all attached fixtures
            Body boxClone1 = box.DeepClone();

            //Swiching the body type to static will reset all forces. This will affect the next clone.
            boxClone1.BodyType = BodyType.Static;
            boxClone1.Position += new Vector2(-10, 0);

            Body boxClone2 = boxClone1.DeepClone();
            boxClone2.BodyType = BodyType.Dynamic;
            boxClone2.Position += new Vector2(-10, 0);
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

            Body compund = BodyFactory.CreateCompoundPolygon(World, Triangulate.ConvexPartition(verts, TriangulationAlgorithm.Bayazit), 1);
            compund.Position = new Vector2(-25, 30);

            Body b = compund.DeepClone();
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