/*
* Box2D.XNA port of Box2D:
* Copyright (c) 2009 Brandon Furtwangler, Nathan Furtwangler
*
* Original source Box2D:
* Copyright (c) 2006-2009 Erin Catto http://www.gphysics.com 
* 
* This software is provided 'as-is', without any express or implied 
* warranty.  In no event will the authors be held liable for any damages 
* arising from the use of this software. 
* Permission is granted to anyone to use this software for any purpose, 
* including commercial applications, and to alter it and redistribute it 
* freely, subject to the following restrictions: 
* 1. The origin of this software must not be misrepresented; you must not 
* claim that you wrote the original software. If you use this software 
* in a product, an acknowledgment in the product documentation would be 
* appreciated but is not required. 
* 2. Altered source versions must be plainly marked as such, and must not be 
* misrepresented as being the original software. 
* 3. This notice may not be removed or altered from any source distribution. 
*/

using System;
using Microsoft.Xna.Framework;

namespace FarseerPhysics
{
    [Flags]
    public enum DebugViewFlags
    {
        Shape = (1 << 0), ///< draw shapes
        Joint = (1 << 1), ///< draw joint connections
        AABB = (1 << 2), ///< draw axis aligned bounding boxes
        Pair = (1 << 3), ///< draw broad-phase pairs
        CenterOfMass = (1 << 4), ///< draw center of mass frame
    };

    /// Implement and register this class with a World to provide debug drawing of physics
    /// entities in your game.
    public abstract class DebugView
    {
        public DebugView(World world)
        {
            World = world;
        }

        public World World
        {
            get;
            set;
        }

        public DebugViewFlags Flags { get; set; }

        /// Append flags to the current flags.
        public void AppendFlags(DebugViewFlags flags)
        {
            Flags |= flags;
        }

        /// Clear flags from the current flags.
        public void ClearFlags(DebugViewFlags flags)
        {
            Flags &= ~flags;
        }

        /// Draw a closed polygon provided in CCW order.
        public abstract void DrawPolygon(ref FixedArray8<Vector2> vertices, int count, float red, float blue, float green);

        /// Draw a solid closed polygon provided in CCW order.
        public abstract void DrawSolidPolygon(ref FixedArray8<Vector2> vertices, int count, float red, float blue, float green);

        /// Draw a circle.
        public abstract void DrawCircle(Vector2 center, float radius, float red, float blue, float green);

        /// Draw a solid circle.
        public abstract void DrawSolidCircle(Vector2 center, float radius, Vector2 axis, float red, float blue, float green);

        /// Draw a line segment.
        public abstract void DrawSegment(Vector2 p1, Vector2 p2, float red, float blue, float green);

        /// Draw a transform. Choose your own length scale.
        /// @param xf a transform.
        public abstract void DrawTransform(ref Transform xf);
    }
}
