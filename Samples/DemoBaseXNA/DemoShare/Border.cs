using System;
using System.Collections.Generic;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.DemoBaseXNA.ScreenSystem;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.DemoBaseXNA.DemoShare
{
    public class Border
    {
        private Body _anchor;
        private World _world;
        private float _borderWidth;

        public Border(World world, float width, float height, float borderWidth)
        {
            _world = world;
            _borderWidth = borderWidth;
            _anchor = new Body(_world);
            ResetBorder(Math.Abs(width), Math.Abs(height));
        }

        public void ResetBorder(float width, float height)
        {
            while (_anchor.FixtureList.Count > 0)
            {
                _anchor.DestroyFixture(_anchor.FixtureList[0]);
            }
            List<Vertices> borders = new List<Vertices>(4);

            //Bottom
            borders.Add(PolygonTools.CreateRectangle(width, _borderWidth, new Vector2(0, height), 0));

            //Left
            borders.Add(PolygonTools.CreateRectangle(_borderWidth, height, new Vector2(-width, 0), 0));

            //Top
            borders.Add(PolygonTools.CreateRectangle(width, _borderWidth, new Vector2(0, -height), 0));

            //Right
            borders.Add(PolygonTools.CreateRectangle(_borderWidth, height, new Vector2(width, 0), 0));

            DemoMaterial material = new DemoMaterial(MaterialType.Dots)
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
    }
}