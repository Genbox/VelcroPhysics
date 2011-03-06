using FarseerPhysics.DebugViews;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Common;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerPhysics.SamplesFramework
{
    public class Agent
    {
        private Body _agentBody;
        private Category _collidesWith;
        private PhysicsGameScreen _screen;
        private Sprite _box;
        private Sprite _knob;
        private float _offset;

        private Category _collisionCategories;

        public Agent(World world, PhysicsGameScreen screen, Vector2 position)
        {
            _collidesWith = Category.All;
            _collisionCategories = Category.All;

            _agentBody = BodyFactory.CreateBody(world, position);
            _agentBody.BodyType = BodyType.Dynamic;

            //Center
            FixtureFactory.AttachCircle(0.5f, 0.5f, _agentBody);

            //Left arm
            FixtureFactory.AttachRectangle(1.5f, 0.4f, 1f, new Vector2(-1f, 0f), _agentBody);
            FixtureFactory.AttachCircle(0.5f, 0.5f, _agentBody, new Vector2(-2f, 0f));

            //Right arm
            FixtureFactory.AttachRectangle(1.5f, 0.4f, 1f, new Vector2(1f, 0f), _agentBody);
            FixtureFactory.AttachCircle(0.5f, 0.5f, _agentBody, new Vector2(2f, 0f));

            //Top arm
            FixtureFactory.AttachRectangle(0.4f, 1.5f, 1f, new Vector2(0f, 1f), _agentBody);
            FixtureFactory.AttachCircle(0.5f, 0.5f, _agentBody, new Vector2(0f, 2f));

            //Bottom arm
            FixtureFactory.AttachRectangle(0.4f, 1.5f, 1f, new Vector2(0f, -1f), _agentBody);
            FixtureFactory.AttachCircle(0.5f, 0.5f, _agentBody, new Vector2(0f, -2f));

            _screen = screen;

            //Gfx
            AssetCreator _creator = _screen.ScreenManager.Assets;
            _box = new Sprite(_creator.TextureFromVertices(PolygonTools.CreateRectangle(1.75f, 0.2f),
                                                                   MaterialType.Blank, Color.White, 1f));
            _knob = new Sprite(_creator.CircleTexture(0.5f, MaterialType.Blank, Color.Orange, 1f));
            _offset = ConvertUnits.ToDisplayUnits(2f);
        }

        public void Draw()
        {
            SpriteBatch _batch = _screen.ScreenManager.SpriteBatch;
            //cross
            _batch.Draw(_box.texture, ConvertUnits.ToDisplayUnits(_agentBody.Position), null,
                        Color.White, _agentBody.Rotation, _box.origin, 1f, SpriteEffects.None, 0f);
            _batch.Draw(_box.texture, ConvertUnits.ToDisplayUnits(_agentBody.Position), null,
                        Color.White, _agentBody.Rotation + MathHelper.Pi / 2f, _box.origin, 1f, SpriteEffects.None, 0f);
            //knobs
            _batch.Draw(_knob.texture, ConvertUnits.ToDisplayUnits(_agentBody.Position), null,
                        Color.White, _agentBody.Rotation, _knob.origin, 1f, SpriteEffects.None, 0f);
            _batch.Draw(_knob.texture, ConvertUnits.ToDisplayUnits(_agentBody.Position), null,
                        Color.White, _agentBody.Rotation, _knob.origin + new Vector2(0f, _offset), 1f, SpriteEffects.None, 0f);
            _batch.Draw(_knob.texture, ConvertUnits.ToDisplayUnits(_agentBody.Position), null,
                        Color.White, _agentBody.Rotation, _knob.origin - new Vector2(0f, _offset), 1f, SpriteEffects.None, 0f);
            _batch.Draw(_knob.texture, ConvertUnits.ToDisplayUnits(_agentBody.Position), null,
                        Color.White, _agentBody.Rotation, _knob.origin + new Vector2(_offset, 0f), 1f, SpriteEffects.None, 0f);
            _batch.Draw(_knob.texture, ConvertUnits.ToDisplayUnits(_agentBody.Position), null,
                        Color.White, _agentBody.Rotation, _knob.origin - new Vector2(_offset, 0f), 1f, SpriteEffects.None, 0f);
        }

        public Category CollisionCategories
        {
            get { return _collisionCategories; }
            set
            {
                _collisionCategories = value;
                Body.CollisionCategories = value;
            }
        }

        public Category CollidesWith
        {
            get { return _collidesWith; }
            set
            {
                _collidesWith = value;
                Body.CollidesWith = value;
            }
        }

        public Body Body
        {
            get { return _agentBody; }
        }
    }
}