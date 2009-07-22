using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerGames.AdvancedSamplesXNA.Demos.Demo7
{
    public class Grenade
    {
        public delegate void TimeoutDelegate(Grenade sender, Vector2 position);

        private Texture2D _texture;
        private Vector2 _origin;
        public double Timeout = 3000;
        public event TimeoutDelegate OnTimeout;
        private Vector2 _position;
        private PhysicsSimulator _physicsSimulator;

        public Grenade(Vector2 position, PhysicsSimulator physicsSimulator)
        {
            _position = position;
            _physicsSimulator = physicsSimulator;
        }

        public void Load(Texture2D texture)
        {
            _texture = texture;
            _origin = new Vector2(_texture.Width / 2f, _texture.Height / 2f);
            OnTimeout += Grenade_OnTimeout;
        }

        void Grenade_OnTimeout(Grenade sender, Vector2 position)
        {
            ApplyExplosion(position);
        }

        public void Draw(SpriteBatch spriteBatch, SpriteFont font)
        {
            spriteBatch.Draw(_texture, _position, null, Color.White, 0, _origin, 1f, SpriteEffects.None, 1);

            string text = ((int)Timeout / 1000).ToString();
            Vector2 origin = font.MeasureString(text);

            spriteBatch.DrawString(font, text, _position, Color.White, 0, new Vector2(origin.X / 2, 0), 1, SpriteEffects.None, 1);
        }

        public void Update(GameTime time)
        {
            Timeout -= time.ElapsedGameTime.TotalMilliseconds;

            if (Timeout <= 0)
            {
                if (OnTimeout != null)
                {
                    OnTimeout(this, _position);
                }
            }
        }

        private void ApplyExplosion(Vector2 position)
        {
            Vector2 min = Vector2.Subtract(position, new Vector2(100, 100));
            Vector2 max = Vector2.Add(position, new Vector2(100, 100));

            AABB aabb = new AABB(ref min, ref max);

            foreach (Body body in _physicsSimulator.BodyList)
            {
                if (aabb.Contains(body.Position))
                {
                    Vector2 fv = body.Position;
                    fv = Vector2.Subtract(fv, position);
                    fv.Normalize();
                    fv = Vector2.Multiply(fv, 50000);
                    body.ApplyForce(fv);
                }
            }
        }
    }
}
