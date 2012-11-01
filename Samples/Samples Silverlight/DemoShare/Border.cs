using System.Collections.Generic;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.DemoShare
{
    public class Border
    {
        public Border(World world, float width, float height, float borderWidth)
        {
            List<Vertices> borders = new List<Vertices>(4);

            borders.Add(PolygonTools.CreateRectangle(width, borderWidth, new Vector2(0, (height / 2)), 0));
            borders.Add(PolygonTools.CreateRectangle(borderWidth, height, new Vector2(-(width / 2), 0), 0));
            borders.Add(PolygonTools.CreateRectangle(width, borderWidth, new Vector2(0, -height / 2), 0));
            borders.Add(PolygonTools.CreateRectangle(borderWidth, height, new Vector2((width / 2), 0), 0));

            Body body = BodyFactory.CreateCompoundPolygon(world, borders, 1);

            foreach (Fixture t in body.FixtureList)
            {
                t.CollisionCategories = Category.All;
                t.CollidesWith = Category.All;
            }
        }
    }
}