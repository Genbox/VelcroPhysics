using Genbox.VelcroPhysics.Collision.Filtering;
using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Factories;
using Genbox.VelcroPhysics.MonoGame.Samples.Demo.MediaSystem;
using Genbox.VelcroPhysics.MonoGame.Samples.Demo.MediaSystem.Graphics;
using Genbox.VelcroPhysics.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Demo.Demos.Prefabs
{
    public class Agent
    {
        private readonly Sprite _box;
        private readonly Sprite _knob;
        private readonly float _offset;
        private Category _collidesWith;
        private Category _collisionCategories;

        public Agent(World world, Vector2 position)
        {
            _collidesWith = Category.All;
            _collisionCategories = Category.All;

            Body = BodyFactory.CreateBody(world, position);
            Body.BodyType = BodyType.Dynamic;

            //Center
            FixtureFactory.AttachCircle(0.5f, 0.5f, Body);

            //Left arm
            FixtureFactory.AttachRectangle(1.5f, 0.4f, 1f, new Vector2(-1f, 0f), Body);
            FixtureFactory.AttachCircle(0.5f, 0.5f, Body, new Vector2(-2f, 0f));

            //Right arm
            FixtureFactory.AttachRectangle(1.5f, 0.4f, 1f, new Vector2(1f, 0f), Body);
            FixtureFactory.AttachCircle(0.5f, 0.5f, Body, new Vector2(2f, 0f));

            //Top arm
            FixtureFactory.AttachRectangle(0.4f, 1.5f, 1f, new Vector2(0f, 1f), Body);
            FixtureFactory.AttachCircle(0.5f, 0.5f, Body, new Vector2(0f, 2f));

            //Bottom arm
            FixtureFactory.AttachRectangle(0.4f, 1.5f, 1f, new Vector2(0f, -1f), Body);
            FixtureFactory.AttachCircle(0.5f, 0.5f, Body, new Vector2(0f, -2f));

            //GFX
            _box = new Sprite(Managers.TextureManager.PolygonTexture(PolygonUtils.CreateRectangle(1.75f, 0.2f), Color.White, Colors.Black));
            _knob = new Sprite(Managers.TextureManager.CircleTexture(0.5f, "Square", Colors.Black, Colors.Gold, Colors.Black, 1f));

            _offset = ConvertUnits.ToDisplayUnits(2f);
        }

        public Category CollisionCategories
        {
            get => _collisionCategories;
            set
            {
                _collisionCategories = value;
                Body.CollisionCategories = value;
            }
        }

        public Category CollidesWith
        {
            get => _collidesWith;
            set
            {
                _collidesWith = value;
                Body.CollidesWith = value;
            }
        }

        public Body Body { get; }

        public void Draw(SpriteBatch batch)
        {
            //cross
            batch.Draw(_box.Image, ConvertUnits.ToDisplayUnits(Body.Position), null, Color.White, Body.Rotation, _box.Origin, 1f, SpriteEffects.None, 0f);
            batch.Draw(_box.Image, ConvertUnits.ToDisplayUnits(Body.Position), null, Color.White, Body.Rotation + MathHelper.Pi / 2f, _box.Origin, 1f, SpriteEffects.None, 0f);

            //knobs
            batch.Draw(_knob.Image, ConvertUnits.ToDisplayUnits(Body.Position), null, Color.White, Body.Rotation, _knob.Origin, 1f, SpriteEffects.None, 0f);
            batch.Draw(_knob.Image, ConvertUnits.ToDisplayUnits(Body.Position), null, Color.White, Body.Rotation, _knob.Origin + new Vector2(0f, _offset), 1f, SpriteEffects.None, 0f);
            batch.Draw(_knob.Image, ConvertUnits.ToDisplayUnits(Body.Position), null, Color.White, Body.Rotation, _knob.Origin - new Vector2(0f, _offset), 1f, SpriteEffects.None, 0f);
            batch.Draw(_knob.Image, ConvertUnits.ToDisplayUnits(Body.Position), null, Color.White, Body.Rotation, _knob.Origin + new Vector2(_offset, 0f), 1f, SpriteEffects.None, 0f);
            batch.Draw(_knob.Image, ConvertUnits.ToDisplayUnits(Body.Position), null, Color.White, Body.Rotation, _knob.Origin - new Vector2(_offset, 0f), 1f, SpriteEffects.None, 0f);
        }
    }
}