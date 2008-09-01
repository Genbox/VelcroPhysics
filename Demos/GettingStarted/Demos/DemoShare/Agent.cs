using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysicsDemos.DrawingSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerGames.FarseerPhysicsDemos.Demos.DemoShare
{
    public class Agent
    {
        private readonly Vector2 position;
        private Body agentBody;
        private Vector2 agentCrossBeamOrigin;
        private Texture2D agentCrossBeamTexture;
        private Geom[] agentGeom;
        private Vector2 agentOrigin;
        private Texture2D agentTexture;

        private Enums.CollisionCategories collidesWith = Enums.CollisionCategories.All;
        private Enums.CollisionCategories collisionCategory = Enums.CollisionCategories.All;

        public Agent(Vector2 position)
        {
            this.position = position;
        }

        public Body Body
        {
            get { return agentBody; }
        }

        public Enums.CollisionCategories CollisionCategory
        {
            get { return collisionCategory; }
            set { collisionCategory = value; }
        }

        public Enums.CollisionCategories CollidesWith
        {
            get { return collidesWith; }
            set { collidesWith = value; }
        }

        public void ApplyForce(Vector2 force)
        {
            agentBody.ApplyForce(force);
        }

        public void ApplyTorque(float torque)
        {
            agentBody.ApplyTorque(torque);
        }


        public void Load(GraphicsDevice graphicsDevice, PhysicsSimulator physicsSimulator)
        {
            agentTexture = DrawingHelper.CreateCircleTexture(graphicsDevice, 16, Color.Gold, Color.Black);
            agentOrigin = new Vector2(agentTexture.Width/2f, agentTexture.Height/2f);

            agentCrossBeamTexture = DrawingHelper.CreateRectangleTexture(graphicsDevice, 16, 120, Color.DarkGray,
                                                                         Color.Black);
            agentCrossBeamOrigin = new Vector2(agentCrossBeamTexture.Width/2f, agentCrossBeamTexture.Height/2f);

            agentBody = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, 80, 80, 5);
            agentBody.Position = position;

            agentGeom = new Geom[7];
            agentGeom[0] = GeomFactory.Instance.CreateCircleGeom(physicsSimulator, agentBody, 16, 10,
                                                                 new Vector2(-40, -40), 0);
            agentGeom[0].RestitutionCoefficient = .4f;
            agentGeom[0].FrictionCoefficient = .2f;
            agentGeom[0].CollisionGroup = 1;
            agentGeom[0].CollisionCategories = collisionCategory;
            agentGeom[0].CollidesWith = collidesWith;
            agentGeom[1] = GeomFactory.Instance.CreateGeom(physicsSimulator, agentBody, agentGeom[0],
                                                           new Vector2(-40, 40), 0);
            agentGeom[2] = GeomFactory.Instance.CreateGeom(physicsSimulator, agentBody, agentGeom[0],
                                                           new Vector2(40, -40), 0);
            agentGeom[3] = GeomFactory.Instance.CreateGeom(physicsSimulator, agentBody, agentGeom[0],
                                                           new Vector2(40, 40), 0);
            agentGeom[4] = GeomFactory.Instance.CreateGeom(physicsSimulator, agentBody, agentGeom[0], new Vector2(0, 0),
                                                           0);

            agentGeom[5] = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, agentBody, 16, 120, Vector2.Zero,
                                                                    MathHelper.PiOver4);
            agentGeom[5].CollisionGroup = 1;
            agentGeom[5].CollisionCategories = collisionCategory;
            agentGeom[5].CollidesWith = collidesWith;

            agentGeom[6] = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, agentBody, 16, 120, Vector2.Zero,
                                                                    -MathHelper.PiOver4);
            agentGeom[6].CollisionGroup = 1;
            agentGeom[6].CollisionCategories = collisionCategory;
            agentGeom[6].CollidesWith = collidesWith;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 5; i < 7; i++)
            {
                spriteBatch.Draw(agentCrossBeamTexture, agentGeom[i].Position, null, Color.White, agentGeom[i].Rotation,
                                 agentCrossBeamOrigin, 1, SpriteEffects.None, 0f);
            }
            for (int i = 0; i < 5; i++)
            {
                spriteBatch.Draw(agentTexture, agentGeom[i].Position, null, Color.White, agentGeom[i].Rotation,
                                 agentOrigin, 1, SpriteEffects.None, 0f);
            }
        }
    }
}