﻿/*
* Velcro Physics:
* Copyright (c) 2017 Ian Qvist
*/

using Microsoft.Xna.Framework;
using VelcroPhysics.Dynamics;
using VelcroPhysics.Shared;

namespace VelcroPhysics.Extensions.DebugView
{
    /// Implement and register this class with a World to provide debug drawing of physics
    /// entities in your game.
    public abstract class DebugViewBase
    {
        protected DebugViewBase(World world)
        {
            World = world;
        }

        protected World World { get; }

        /// <summary>
        /// Gets or sets the debug view flags.
        /// </summary>
        /// <value>The flags.</value>
        public DebugViewFlags Flags { get; set; }

        /// <summary>
        /// Append flags to the current flags.
        /// </summary>
        /// <param name="flags">The flags.</param>
        public void AppendFlags(DebugViewFlags flags)
        {
            Flags |= flags;
        }

        /// <summary>
        /// Remove flags from the current flags.
        /// </summary>
        /// <param name="flags">The flags.</param>
        public void RemoveFlags(DebugViewFlags flags)
        {
            Flags &= ~flags;
        }

        /// <summary>
        /// Draw a closed polygon provided in CCW order.
        /// </summary>
        /// <param name="vertices">The vertices.</param>
        /// <param name="count">The vertex count.</param>
        /// <param name="red">The red value.</param>
        /// <param name="blue">The blue value.</param>
        /// <param name="green">The green value.</param>
        public abstract void DrawPolygon(Vector2[] vertices, int count, float red, float blue, float green, bool closed = true);

        /// <summary>
        /// Draw a solid closed polygon provided in CCW order.
        /// </summary>
        /// <param name="vertices">The vertices.</param>
        /// <param name="count">The vertex count.</param>
        /// <param name="red">The red value.</param>
        /// <param name="blue">The blue value.</param>
        /// <param name="green">The green value.</param>
        public abstract void DrawSolidPolygon(Vector2[] vertices, int count, float red, float blue, float green);

        /// <summary>
        /// Draw a circle.
        /// </summary>
        /// <param name="center">The center.</param>
        /// <param name="radius">The radius.</param>
        /// <param name="red">The red value.</param>
        /// <param name="blue">The blue value.</param>
        /// <param name="green">The green value.</param>
        public abstract void DrawCircle(Vector2 center, float radius, float red, float blue, float green);

        /// <summary>
        /// Draw a solid circle.
        /// </summary>
        /// <param name="center">The center.</param>
        /// <param name="radius">The radius.</param>
        /// <param name="axis">The axis.</param>
        /// <param name="red">The red value.</param>
        /// <param name="blue">The blue value.</param>
        /// <param name="green">The green value.</param>
        public abstract void DrawSolidCircle(Vector2 center, float radius, Vector2 axis, float red, float blue,
                                             float green);

        /// <summary>
        /// Draw a line segment.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <param name="red">The red value.</param>
        /// <param name="blue">The blue value.</param>
        /// <param name="green">The green value.</param>
        public abstract void DrawSegment(Vector2 start, Vector2 end, float red, float blue, float green);

        /// <summary>
        /// Draw a transform. Choose your own length scale.
        /// </summary>
        /// <param name="transform">The transform.</param>
        public abstract void DrawTransform(ref Transform transform);
    }
}