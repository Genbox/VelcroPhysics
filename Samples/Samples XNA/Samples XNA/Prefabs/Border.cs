using System;
using System.Collections.Generic;
using FarseerPhysics.Common;
using FarseerPhysics.DebugViews;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerPhysics.SamplesFramework
{
    public class Border
    {
        private Body _anchor;
        private World _world;

        public Border(World world, Viewport viewport)
        {
            _world = world;

            float _halfWidth = ConvertUnits.ToSimUnits(viewport.Width) / 2f - 0.0625f;
            float _halfHeight = ConvertUnits.ToSimUnits(viewport.Height) / 2f - 0.0625f;

            Vertices borders = new Vertices(4);
            borders.Add(new Vector2(-_halfWidth, _halfHeight));
            borders.Add(new Vector2(_halfWidth, _halfHeight));
            borders.Add(new Vector2(_halfWidth, -_halfHeight));
            borders.Add(new Vector2(-_halfWidth, -_halfHeight));

            _anchor = BodyFactory.CreateLoopShape(_world, borders);
            _anchor.CollisionCategories = Category.All;
            _anchor.CollidesWith = Category.All;
        }
    }
}