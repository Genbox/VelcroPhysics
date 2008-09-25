using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerPerformanceTest
{
    public class Box
    {
        private Body body;

        private Geom geom;
        private float height = 25f;

        private Texture2D texture;
        private float width = 25f;

        public Box(Game game, Vector2 position)
        {
            body = BodyFactory.Instance.CreateRectangleBody(Globals.Physics, width, height, 1);
            body.Position = position;

            // turn automatic deactivation on and specify the minimum velocity.
            // if you are using a object pool which deactivates bodies to reuse them later, you should
            // additionally set "IsAutoIdle" to false when assigning it back to the pool. 
            // Otherwise it could be reactivated by the InactivityController
            body.IsAutoIdle = true;
            body.MinimumVelocity = 25;

            geom = GeomFactory.Instance.CreateRectangleGeom(Globals.Physics, body, width, height);
            geom.FrictionCoefficient = 1;

            texture = game.Content.Load<Texture2D>("box");
        }

        public Body Body
        {
            get { return body; }
        }

        public Texture2D Texture
        {
            get { return texture; }
        }

        public Vector2 Position
        {
            get { return body.Position; }
        }

        public Vector2 Center
        {
            get { return new Vector2(width/2, height/2); }
        }
    }
}