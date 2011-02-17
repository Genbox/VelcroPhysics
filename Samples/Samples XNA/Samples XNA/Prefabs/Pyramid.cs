using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.SamplesFramework
{
    public class Pyramid
    {
        public Pyramid(World world, Vector2 position, int count, float density)
        {
            Vertices box = PolygonTools.CreateRectangle(0.5f, 0.5f);
            PolygonShape shape = new PolygonShape(box, density);

            Vector2 rowStart = position;
            rowStart.Y -= 0.5f + count * 1.1f;

            Vector2 deltaRow = new Vector2(-0.625f, 1.1f);
            float spacing = 1.25f;

            for (int i = 0; i < count; ++i)
            {
                Vector2 pos = rowStart;

                for (int j = 0; j < i + 1; ++j)
                {
                    Body body = BodyFactory.CreateBody(world);
                    body.BodyType = BodyType.Dynamic;
                    body.Position = pos;
                    body.CreateFixture(shape);

                    pos.X += spacing;
                }

                rowStart += deltaRow;
            }
        }
    }
}
