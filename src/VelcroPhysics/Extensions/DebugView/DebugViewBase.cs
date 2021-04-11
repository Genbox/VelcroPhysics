/*
* Velcro Physics:
* Copyright (c) 2017 Ian Qvist
*/

using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Shared;
using Microsoft.Xna.Framework;

namespace Genbox.VelcroPhysics.Extensions.DebugView
{
    /// <summary>Implement and register this class with a World to provide debug drawing of physics entities in your game.</summary>
    public abstract class DebugViewBase
    {
        protected DebugViewBase(World world)
        {
            World = world;
        }

        protected World World { get; }

        /// <summary>Gets or sets the debug view flags.</summary>
        /// <value>The flags.</value>
        public DebugViewFlags Flags { get; set; }

        /// <summary>Append flags to the current flags.</summary>
        /// <param name="flags">The flags.</param>
        public void AppendFlags(DebugViewFlags flags)
        {
            Flags |= flags;
        }

        /// <summary>Remove flags from the current flags.</summary>
        /// <param name="flags">The flags.</param>
        public void RemoveFlags(DebugViewFlags flags)
        {
            Flags &= ~flags;
        }

        /// <summary>Draw a closed polygon provided in CCW order.</summary>
        public abstract void DrawPolygon(Vector2[] vertices, int count, Color color, bool closed = true);

        /// <summary>Draw a solid closed polygon provided in CCW order.</summary>
        public abstract void DrawSolidPolygon(Vector2[] vertices, int count, Color color, bool outline = true);

        /// <summary>Draw a circle.</summary>
        public abstract void DrawCircle(Vector2 center, float radius, Color color);

        /// <summary>Draw a solid circle.</summary>
        public abstract void DrawSolidCircle(Vector2 center, float radius, Vector2 axis, Color color);

        /// <summary>Draw a line segment.</summary>
        public abstract void DrawSegment(Vector2 start, Vector2 end, Color color);

        /// <summary>Draw a transform. Choose your own length scale.</summary>
        /// <param name="transform">The transform.</param>
        public abstract void DrawTransform(ref Transform transform);
    }
}