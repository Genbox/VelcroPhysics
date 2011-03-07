using System.Collections.Generic;
using FarseerPhysics.DebugViews;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerPhysics.SamplesFramework
{
    public enum ObjectType
    {
        Circle,
        Rectangle,
        Gear,
        Star
    }

    public class Objects
    {
        private Category _collidesWith;
        private Category _collisionCategories;
        private List<Body> _bodies;
        private Sprite _object;
        private PhysicsGameScreen _screen;

        public Objects(World world, PhysicsGameScreen screen, Vector2 startPosition, Vector2 endPosition, int count, float radius, ObjectType type)
        {
            _bodies = new List<Body>(count);
            CollidesWith = Category.All;
            CollisionCategories = Category.All;

            for (int i = 0; i < count; ++i)
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

            for (int i = 0; i < _bodies.Count; ++i)
            {
                Body body = _bodies[i];
                body.BodyType = BodyType.Dynamic;
                body.Position = Vector2.Lerp(startPosition, endPosition, i / (float)(count - 1));
                body.Restitution = .7f;
                body.Friction = .2f;
                body.CollisionCategories = CollisionCategories;
                body.CollidesWith = CollidesWith;
            }

            _screen = screen;

            //GFX
            AssetCreator _creator = _screen.ScreenManager.Assets;
            switch (type)
            {
                case ObjectType.Circle:
                    _object = new Sprite(_creator.CircleTexture(radius, MaterialType.Dots, Color.DarkRed, 0.8f));
                    break;
                case ObjectType.Rectangle:
                    _object = new Sprite(_creator.TextureFromVertices(PolygonTools.CreateRectangle(radius / 2f, radius / 2f),
                                                                      MaterialType.Dots, Color.Blue, 0.8f));
                    break;
                case ObjectType.Star:
                    _object = new Sprite(_creator.TextureFromVertices(PolygonTools.CreateGear(radius, 10, 0f, 1f),
                                                                      MaterialType.Dots, Color.Yellow, 0.8f));
                    break;
                case ObjectType.Gear:
                    _object = new Sprite(_creator.TextureFromVertices(PolygonTools.CreateGear(radius, 10, 100f, 1f),
                                                                      MaterialType.Dots, Color.DarkGreen, 0.8f));
                    break;
            }
        }

        public void Draw()
        {
            SpriteBatch _batch = _screen.ScreenManager.SpriteBatch;

            for (int i = 0; i < _bodies.Count; ++i)
            {
                _batch.Draw(_object.texture, ConvertUnits.ToDisplayUnits(_bodies[i].Position), null,
                            Color.White, _bodies[i].Rotation, _object.origin, 1f, SpriteEffects.None, 0f);
            }
        }

        public Category CollisionCategories
        {
            get { return _collisionCategories; }
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
            get { return _collidesWith; }
            set
            {
                _collidesWith = value;

                foreach (Body body in _bodies)
                {
                    body.CollidesWith = _collidesWith;
                }
            }
        }
    }
}