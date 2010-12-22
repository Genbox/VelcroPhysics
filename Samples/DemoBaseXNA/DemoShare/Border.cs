using System;
using System.Collections.Generic;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.DebugViews;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.DemoBaseXNA.DemoShare
{
    public class Border
    {
        private Body _anchor;
        private World _world;
        private float _borderWidth;

        public Border(World world, float borderWidth)
        {
            _world = world;
            _borderWidth = borderWidth;
            _anchor = new Body(_world);
            List<Vertices> borders = new List<Vertices>(4);

            //Top
            borders.Add(PolygonTools.CreateRectangle(58.5f, _borderWidth, new Vector2(0, 50f), 0));

            //Left
            borders.Add(PolygonTools.CreateRectangle(_borderWidth, 37.5f, new Vector2(-58.5f, 12.5f), 0));

            //Bottom
            borders.Add(PolygonTools.CreateRectangle(58.5f, _borderWidth, new Vector2(0, -25f), 0));

            //Right
            borders.Add(PolygonTools.CreateRectangle(_borderWidth, 37.5f, new Vector2(58.5f, 12.5f), 0));

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
    }
}