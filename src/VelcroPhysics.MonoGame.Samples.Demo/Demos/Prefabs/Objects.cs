using System.Collections.Generic;
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
    public class Objects
    {
        private readonly List<Body> _bodies;
        private readonly Sprite _object;
        private Category _collidesWith;
        private Category _collisionCategories;

        public Objects(World world, Vector2 startPosition, Vector2 endPosition, int count, float radius, ObjectType type)
        {
            _bodies = new List<Body>(count);
            CollidesWith = Category.All;
            CollisionCategories = Category.All;

            // Physics
            for (int i = 0; i < count; i++)
            {
                switch (type)
                {
                    case ObjectType.Circle:
                        _bodies.Add(BodyFactory.CreateCircle(world, radius, 1f));
                        break;
                    case ObjectType.Rectangle:
                        _bodies.Add(BodyFactory.CreateRectangle(world, radius, radius, 1f));
                        break;
                    case ObjectType.Star:
                        _bodies.Add(BodyFactory.CreateGear(world, radius, 10, 0f, 1f, 1f));
                        break;
                    case ObjectType.Gear:
                        _bodies.Add(BodyFactory.CreateGear(world, radius, 10, 100f, 1f, 1f));
                        break;
                }
            }

            for (int i = 0; i < _bodies.Count; i++)
            {
                Body body = _bodies[i];
                body.BodyType = BodyType.Dynamic;
                body.Position = Vector2.Lerp(startPosition, endPosition, i / (float)(count - 1));
                body.Restitution = 0.7f;
                body.Friction = 0.2f;
            }

            //GFX
            switch (type)
            {
                case ObjectType.Circle:
                    _object = new Sprite(Managers.TextureManager.CircleTexture(radius, Colors.Gold, Colors.Grey));
                    break;
                case ObjectType.Rectangle:
                    _object = new Sprite(Managers.TextureManager.PolygonTexture(PolygonUtils.CreateRectangle(radius / 2f, radius / 2f), Colors.Red, Colors.Grey));
                    break;
                case ObjectType.Star:
                    _object = new Sprite(Managers.TextureManager.PolygonTexture(PolygonUtils.CreateGear(radius, 10, 0f, 1f), Colors.Brown, Colors.Black));
                    break;
                case ObjectType.Gear:
                    _object = new Sprite(Managers.TextureManager.PolygonTexture(PolygonUtils.CreateGear(radius, 10, 100f, 1f), Colors.Orange, Colors.Grey));
                    break;
            }
        }

        public Category CollisionCategories
        {
            get => _collisionCategories;
            set
            {
                _collisionCategories = value;

                foreach (Body body in _bodies)
                {
                    body.CollisionCategories = _collisionCategories;
                }
            }
        }

        public Category CollidesWith
        {
            get => _collidesWith;
            set
            {
                _collidesWith = value;

                foreach (Body body in _bodies)
                {
                    body.CollidesWith = _collidesWith;
                }
            }
        }

        public void Draw(SpriteBatch batch)
        {
            foreach (Body body in _bodies)
            {
                batch.Draw(_object.Image, ConvertUnits.ToDisplayUnits(body.Position), null, Color.White, body.Rotation, _object.Origin, 1f, SpriteEffects.None, 0f);
            }
        }
    }
}