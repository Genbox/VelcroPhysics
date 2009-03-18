using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Factories;
using FarseerGames.GettingStarted.DrawingSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerGames.GettingStarted.Demos.DemoShare
{
    public class Agent
    {
        private Body _agentBody;
        private Vector2 _agentCrossBeamOrigin;
        private Texture2D _agentCrossBeamTexture;
        private Geom[] _agentGeom;
        private Vector2 _agentOrigin;
        private Texture2D _agentTexture;

        private CollisionCategory _collidesWith = CollisionCategory.All;
        private CollisionCategory _collisionCategory = CollisionCategory.All;
        private Vector2 _position;

        public Agent(Vector2 position)
        {
            _position = position;
        }

        public Body Body
        {
            get { return _agentBody; }
        }

        public CollisionCategory CollisionCategory
        {
            get { return _collisionCategory; }
            set { _collisionCategory = value; }
        }

        public CollisionCategory CollidesWith
        {
            get { return _collidesWith; }
            set { _collidesWith = value; }
        }

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
            _agentGeom[0].CollisionCategories = _collisionCategory;
            _agentGeom[0].CollidesWith = _collidesWith;
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
            _agentGeom[5].CollisionCategories = _collisionCategory;
            _agentGeom[5].CollidesWith = _collidesWith;

            _agentGeom[6] = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, _agentBody, 16, 120, Vector2.Zero,
                                                                     -MathHelper.PiOver4);
            _agentGeom[6].CollisionGroup = 1;
            _agentGeom[6].CollisionCategories = _collisionCategory;
            _agentGeom[6].CollidesWith = _collidesWith;
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