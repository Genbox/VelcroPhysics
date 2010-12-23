using System.Collections.Generic;
using System.Text;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.DebugViews;
using FarseerPhysics.DemoBaseXNA;
using FarseerPhysics.DemoBaseXNA.ScreenSystem;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.AdvancedSamplesXNA
{
    internal class Demo2Screen : PhysicsGameScreen, IDemoScreen
    {
        #region IDemoScreen Members

        public string GetTitle()
        {
            return "Demo2: Path generator";
        }

        public string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            return sb.ToString();
        }

        #endregion

        public override void LoadContent()
        {
            World = new World(new Vector2(0, -9.82f));
            base.LoadContent();

            /*
             * Bridge
             */

            DebugMaterial circleMaterial = new DebugMaterial(MaterialType.Blank)
            {
                Color = Color.LightGray
            };

            DebugMaterial defaultMaterial = new DebugMaterial(MaterialType.Blank)
            {
                Color = Color.WhiteSmoke
            };

            //We make a path using 2 points.
            Path bridgePath = new Path();
            bridgePath.Add(new Vector2(-15, -10));
            bridgePath.Add(new Vector2(15, -10));
            bridgePath.Closed = false;

            Vertices box = PolygonTools.CreateRectangle(0.125f, 0.5f);
            PolygonShape shape = new PolygonShape(box, 20);

            List<Body> bridgeBodies = PathManager.EvenlyDistributeShapesAlongPath(World, bridgePath, shape, BodyType.Dynamic, 29, defaultMaterial);

            foreach (var bridgeBody in bridgeBodies)
            {
                //bridgeBody.BodyType = BodyType.Static;
            }

            //Attach the first and last fixtures to the world
            JointFactory.CreateFixedRevoluteJoint(World, bridgeBodies[0], new Vector2(0, 0.5f), bridgeBodies[0].Position);
            JointFactory.CreateFixedRevoluteJoint(World, bridgeBodies[bridgeBodies.Count - 1], new Vector2(0, -0.5f), bridgeBodies[bridgeBodies.Count - 1].Position);

            PathManager.AttachBodiesWithRevoluteJoint(World, bridgeBodies, new Vector2(0, 0.5f), new Vector2(0, -0.5f), false, true);

            /*
             * Soft body
             */

            //We make a rectangular path.
            Path rectanglePath = new Path();
            rectanglePath.Add(new Vector2(-6, 6));
            rectanglePath.Add(new Vector2(-6, -6));
            rectanglePath.Add(new Vector2(6, -6));
            rectanglePath.Add(new Vector2(6, 6));
            rectanglePath.Closed = true;

            //Creating two shapes. A circle to form the circle and a rectangle to stabilize the soft body.
            List<Shape> shapes = new List<Shape>(2);
            shapes.Add(new PolygonShape(PolygonTools.CreateRectangle(0.5f, 0.5f, new Vector2(-0.1f, 0), 0), 1));
            shapes.Add(new CircleShape(0.5f, 1));

            //We distribute the shapes in the rectangular path.
            List<Body> bodies = PathManager.EvenlyDistributeShapesAlongPath(World, rectanglePath, shapes, BodyType.Dynamic, 30, circleMaterial);

            //Attach the bodies together with revolute joints. The rectangular form will converge to a circular form.
            PathManager.AttachBodiesWithRevoluteJoint(World, bodies, new Vector2(0, 0.5f), new Vector2(0, -0.5f), true,
                                                      true);
        }
    }
}