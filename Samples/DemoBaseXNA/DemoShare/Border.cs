using System.Collections.Generic;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.DemoBaseXNA.DemoShare
{
    public class Border
    {
        public Border(World world, float width, float height, float borderWidth)
        {
            List<Vertices> borders = new List<Vertices>(4);

            //Bottom
            borders.Add(PolygonTools.CreateRectangle(width, borderWidth, new Vector2(0, height), 0));

            //Left
            borders.Add(PolygonTools.CreateRectangle(borderWidth, height, new Vector2(-width, 0), 0));

            //Top
            borders.Add(PolygonTools.CreateRectangle(width, borderWidth, new Vector2(0, -height), 0));

            //Right
            borders.Add(PolygonTools.CreateRectangle(borderWidth, height, new Vector2(width, 0), 0));

            List<Fixture> fixtures = FixtureFactory.CreateCompoundPolygon(world, borders, 1);

            foreach (Fixture t in fixtures)
            {
                t.CollisionCategories = CollisionCategory.All;
                t.CollidesWith = CollisionCategory.All;
            }
        }
    }
}