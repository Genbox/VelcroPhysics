using Genbox.VelcroPhysics.Collision.Shapes;
using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Factories;
using Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Framework;
using Genbox.VelcroPhysics.Templates;
using Microsoft.Xna.Framework;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Tests
{
    public class TemplatesTest : Test
    {
        private TemplatesTest()
        {
            BodyTemplate b1 = new BodyTemplate();
            b1.Type = BodyType.Dynamic;
            b1.LinearVelocity = new Vector2(15.0f, 0.0f);

            Body body = BodyFactory.CreateFromTemplate(World, b1);

            //CircleShapeTemplate circle = new CircleShapeTemplate();
            //circle.Density = 1f;
            //circle.Radius = 2f;

            Shape circle = new CircleShape(2f, 2f);

            FixtureTemplate f1 = new FixtureTemplate();
            f1.Shape = circle;

            FixtureFactory.CreateFromTemplate(body, f1);
        }

        public static Test Create()
        {
            return new TemplatesTest();
        }
    }
}