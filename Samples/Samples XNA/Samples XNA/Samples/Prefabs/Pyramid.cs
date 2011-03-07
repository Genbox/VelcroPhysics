using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerPhysics.SamplesFramework
{
    public class Pyramid
    {
        private PhysicsGameScreen _screen;
        private Sprite _box;
        private List<Body> _boxes;

        public Pyramid(World world, PhysicsGameScreen screen, Vector2 position, int count, float density)
        {
            Vertices rect = PolygonTools.CreateRectangle(0.5f, 0.5f);
            PolygonShape shape = new PolygonShape(rect, density);

            Vector2 rowStart = position;
            rowStart.Y -= 0.5f + count * 1.1f;

            Vector2 deltaRow = new Vector2(-0.625f, 1.1f);
            float spacing = 1.25f;

            _boxes = new List<Body>();

            for (int i = 0; i < count; ++i)
            {
                Vector2 pos = rowStart;

                for (int j = 0; j < i + 1; ++j)
                {
                    Body body = BodyFactory.CreateBody(world);
                    body.BodyType = BodyType.Dynamic;
                    body.Position = pos;
                    body.CreateFixture(shape);
                    _boxes.Add(body);

                    pos.X += spacing;
                }

                rowStart += deltaRow;
            }

            _screen = screen;

            //GFX
            AssetCreator _creator = _screen.ScreenManager.Assets;
            _box = new Sprite(_creator.TextureFromVertices(rect, MaterialType.Dots, Color.SaddleBrown, 2f));
        }

        public void Draw()
        {
            SpriteBatch _batch = _screen.ScreenManager.SpriteBatch;

            for (int i = 0; i < _boxes.Count; ++i)
            {
                _batch.Draw(_box.texture, ConvertUnits.ToDisplayUnits(_boxes[i].Position), null,
                            Color.White, _boxes[i].Rotation, _box.origin, 1f, SpriteEffects.None, 0f);
            }
        }
    }
}
