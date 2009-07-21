using DemoBaseXNA.DrawingSystem;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DemoBaseXNA.DemoShare
{
    public class Agent
    {
        private Body _agentBody;
        private Vector2 _agentCrossBeamOrigin;
        private Texture2D _agentCrossBeamTexture;
        private Geom[] _agentGeom;
        private Vector2 _agentOrigin;
        private Texture2D _agentTexture;

        private Vector2 _position;

        public Agent(Vector2 position)
        {
            CollidesWith = CollisionCategory.All;
            CollisionCategory = CollisionCategory.All;
            _position = position;
        }

        public Body Body
        {
            get { return _agentBody; }
        }

        public CollisionCategory CollisionCategory { get; set; }

        public CollisionCategory CollidesWith { get; set; }

        public void ApplyForce(Vector2 force)
        {
            _agentBody.ApplyForce(force);
        }

        public void ApplyTorque(float torque)
        {
            _agentBody.ApplyTorque(torque);
        }

        public void Load(GraphicsDevice graphicsDevice, PhysicsSimulator physicsSimulator)
        {
            _agentTexture = DrawingHelper.CreateCircleTexture(graphicsDevice, 16, Color.Gold, Color.Black);
            _agentOrigin = new Vector2(_agentTexture.Width/2f, _agentTexture.Height/2f);

            _agentCrossBeamTexture = DrawingHelper.CreateRectangleTexture(graphicsDevice, 16, 120, Color.DarkGray,
                                                                          Color.Black);
            _agentCrossBeamOrigin = new Vector2(_agentCrossBeamTexture.Width/2f, _agentCrossBeamTexture.Height/2f);

            _agentBody = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, 80, 80, 5);
            _agentBody.Position = _position;

            _agentGeom = new Geom[7];
            _agentGeom[0] = GeomFactory.Instance.CreateCircleGeom(physicsSimulator, _agentBody, 16, 10,
                                                                  new Vector2(-40, -40), 0);
            _agentGeom[0].RestitutionCoefficient = .4f;
            _agentGeom[0].FrictionCoefficient = .2f;
            _agentGeom[0].CollisionGroup = 1;
            _agentGeom[0].CollisionCategories = CollisionCategory;
            _agentGeom[0].CollidesWith = CollidesWith;
            _agentGeom[1] = GeomFactory.Instance.CreateGeom(physicsSimulator, _agentBody, _agentGeom[0],
                                                            new Vector2(-40, 40), 0);
            _agentGeom[2] = GeomFactory.Instance.CreateGeom(physicsSimulator, _agentBody, _agentGeom[0],
                                                            new Vector2(40, -40), 0);
            _agentGeom[3] = GeomFactory.Instance.CreateGeom(physicsSimulator, _agentBody, _agentGeom[0],
                                                            new Vector2(40, 40), 0);
            _agentGeom[4] = GeomFactory.Instance.CreateGeom(physicsSimulator, _agentBody, _agentGeom[0],
                                                            new Vector2(0, 0),
                                                            0);

            _agentGeom[5] = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, _agentBody, 16, 120, Vector2.Zero,
                                                                     MathHelper.PiOver4);
            _agentGeom[5].CollisionGroup = 1;
            _agentGeom[5].CollisionCategories = CollisionCategory;
            _agentGeom[5].CollidesWith = CollidesWith;

            _agentGeom[6] = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, _agentBody, 16, 120, Vector2.Zero,
                                                                     -MathHelper.PiOver4);
            _agentGeom[6].CollisionGroup = 1;
            _agentGeom[6].CollisionCategories = CollisionCategory;
            _agentGeom[6].CollidesWith = CollidesWith;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 5; i < 7; i++)
            {
                spriteBatch.Draw(_agentCrossBeamTexture, _agentGeom[i].Position, null, Color.White,
                                 _agentGeom[i].Rotation,
                                 _agentCrossBeamOrigin, 1, SpriteEffects.None, 0f);
            }
            for (int i = 0; i < 5; i++)
            {
                spriteBatch.Draw(_agentTexture, _agentGeom[i].Position, null, Color.White, _agentGeom[i].Rotation,
                                 _agentOrigin, 1, SpriteEffects.None, 0f);
            }
        }
    }
}