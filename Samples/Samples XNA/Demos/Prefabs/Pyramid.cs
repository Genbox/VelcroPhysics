using System.Collections.Generic;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.Samples.MediaSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerPhysics.Samples.Demos.Prefabs
{
    public class Pyramid
    {
        private Sprite _box;
        private List<Body> _boxes;

        public Pyramid(World world, Vector2 position, int count, float density)
        {
            Vertices rect = PolygonTools.CreateRectangle(0.5f, 0.5f);
            PolygonShape shape = new PolygonShape(rect, density);

            Vector2 rowStart = position;
            rowStart.Y -= 0.5f + count * 1.1f;

            Vector2 deltaRow = new Vector2(-0.625f, 1.1f);
            const float spacing = 1.25f;

            // Physics
            _boxes = new List<Body>();

            for (int i = 0; i < count; i++)
            {
                Vector2 pos = rowStart;

                for (int j = 0; j < i + 1; j++)
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

            //GFX
            _box = new Sprite(ContentWrapper.PolygonTexture(rect, "Square", ContentWrapper.Blue, ContentWrapper.Gold, ContentWrapper.Black, 1f));
        }

        public void Draw(SpriteBatch batch)
        {
            foreach (Body body in _boxes)
            {
                batch.Draw(_box.Image, ConvertUnits.ToDisplayUnits(body.Position), null, Color.White, body.Rotation, _box.Origin, 1f, SpriteEffects.None, 0f);
            }
        }
    }
}