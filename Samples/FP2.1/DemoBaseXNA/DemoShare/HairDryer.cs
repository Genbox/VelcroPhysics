using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerGames.AdvancedSamplesXNA.Demos.Demo7
{
    public class HairDryer
    {
        private Texture2D _texture;
        private Vector2 _origin;
        private Vector2 _position;
        private PhysicsSimulator _physicsSimulator;

        public HairDryer(Vector2 position, PhysicsSimulator physicsSimulator)
        {
            _position = position;
            _physicsSimulator = physicsSimulator;
        }

        public Vector2 Position
        {
            set { _position = value; }
        }

        public void Load(Texture2D texture)
        {
            _texture = texture;
            _origin = new Vector2(_texture.Width / 2f, _texture.Height / 2f);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, _position, null, Color.White, 0, _origin, 1f, SpriteEffects.None, 1);
        }

        public void Update()
        {
            ApplyWind(_position);
        }

        private void ApplyWind(Vector2 position)
        {
            Vector2 min = Vector2.Subtract(position, new Vector2(0, 100));
            Vector2 max = Vector2.Add(position, new Vector2(300, 100));

            AABB aabb = new AABB(ref min, ref max);

            foreach (Body body in _physicsSimulator.BodyList)
            {
                if (aabb.Contains(body.Position))
                {
                    body.ApplyForce(new Vector2(400,0));
                }
            }
        }
    }
}
