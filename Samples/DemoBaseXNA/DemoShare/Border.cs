using System;
using System.Collections.Generic;
using FarseerPhysics.Common;
using FarseerPhysics.DebugViews;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.DemoBaseXNA.DemoShare
{
    public class Border
    {
        private Body _anchor;
        private World _world;

        public Border(World world, float width, float height, float borderWidth)
        {
            _world = world;

            CreateBorder(width, height, borderWidth);
        }

        private void CreateBorder(float width, float height, float borderWidth)
        {
            width = Math.Abs(width);
            height = Math.Abs(height);

            _anchor = new Body(_world);
            List<Vertices> borders = new List<Vertices>(4);

            //Bottom
            borders.Add(PolygonTools.CreateRectangle(width, borderWidth, new Vector2(0, height), 0));

            //Left
            borders.Add(PolygonTools.CreateRectangle(borderWidth, height, new Vector2(-width, 0), 0));

            //Top
            borders.Add(PolygonTools.CreateRectangle(width, borderWidth, new Vector2(0, -height), 0));

            //Right
            borders.Add(PolygonTools.CreateRectangle(borderWidth, height, new Vector2(width, 0), 0));

            DebugMaterial material = new DebugMaterial(MaterialType.Pavement)
                                         {
                                             Color = Color.LightGray,
                                             Scale = 8f
                                         };
            List<Fixture> fixtures = FixtureFactory.CreateCompoundPolygon(borders, 1, _anchor, material);

            foreach (Fixture t in fixtures)
            {
                t.CollisionFilter.CollisionCategories = Category.All;
                t.CollisionFilter.CollidesWith = Category.All;
            }
        }

        public void ResetBorder(float width, float height, float borderWidth)
        {
            _world.RemoveBody(_anchor);
            _world.ProcessChanges();

            CreateBorder(width, height, borderWidth);
        }
    }
}