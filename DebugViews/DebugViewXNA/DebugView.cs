using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace FarseerPhysics.DebugViewXNA
{
    [Flags]
    public enum DebugViewFlags
    {
	    Shape			= (1 << 0), ///< draw shapes
	    Joint			= (1 << 1), ///< draw joint connections
	    AABB			= (1 << 2), ///< draw axis aligned bounding boxes
	    Pair			= (1 << 3), ///< draw broad-phase pairs
	    CenterOfMass	= (1 << 4), ///< draw center of mass frame
    };

    /// Implement and register this class with a World to provide debug drawing of physics
    /// entities in your game.
    public abstract class DebugView
    {
	    public DebugDrawFlags Flags { get; set; }
    	
	    /// Append flags to the current flags.
	    public void AppendFlags(DebugDrawFlags flags)
        {
            Flags |= flags;
        }

	    /// Clear flags from the current flags.
	    public  void ClearFlags(DebugDrawFlags flags)
        {
            Flags &= ~flags;
        }

	    /// Draw a closed polygon provided in CCW order.
	    public abstract void DrawPolygon(ref FixedArray8<Vector2> vertices, int count, Color color);

	    /// Draw a solid closed polygon provided in CCW order.
        public abstract void DrawSolidPolygon(ref FixedArray8<Vector2> vertices, int count, Color color);

	    /// Draw a circle.
        public abstract void DrawCircle(Vector2 center, float radius, Color color);
    	
	    /// Draw a solid circle.
        public abstract void DrawSolidCircle(Vector2 center, float radius, Vector2 axis, Color color);
    	
	    /// Draw a line segment.
        public abstract void DrawSegment(Vector2 p1, Vector2 p2, Color color);

	    /// Draw a transform. Choose your own length scale.
	    /// @param xf a transform.
        public abstract void DrawTransform(ref Transform xf);

        /// Call this to draw shapes and other debug draw data.
        public void DrawDebugData()
        {
            if (DebugDraw == null)
            {
                return;
            }

            DebugDrawFlags flags = DebugDraw.Flags;

            if ((flags & DebugDrawFlags.Shape) == DebugDrawFlags.Shape)
            {
                for (Body b = _bodyList; b != null; b = b.GetNext())
                {
                    Transform xf;
                    b.GetTransform(out xf);
                    for (Fixture f = b.GetFixtureList(); f != null; f = f.GetNext())
                    {
                        if (b.IsActive() == false)
                        {
                            DrawShape(f, xf, new Color(0.5f, 0.5f, 0.3f));
                        }
                        else if (b.GetType() == BodyType.Static)
                        {
                            DrawShape(f, xf, new Color(0.5f, 0.9f, 0.5f));
                        }
                        else if (b.GetType() == BodyType.Kinematic)
                        {
                            DrawShape(f, xf, new Color(0.5f, 0.5f, 0.9f));
                        }
                        else if (b.IsAwake() == false)
                        {
                            DrawShape(f, xf, new Color(0.6f, 0.6f, 0.6f));
                        }
                        else
                        {
                            DrawShape(f, xf, new Color(0.9f, 0.7f, 0.7f));
                        }
                    }
                }
            }

            if ((flags & DebugDrawFlags.Joint) == DebugDrawFlags.Joint)
            {
                for (Joint j = _jointList; j != null; j = j.GetNext())
                {
                    DrawJoint(j);
                }
            }

            if ((flags & DebugDrawFlags.Pair) == DebugDrawFlags.Pair)
            {
                Color color = new Color(0.3f, 0.9f, 0.9f);
                for (Contact c = _contactManager._contactList; c != null; c = c.GetNext())
                {
                    Fixture fixtureA = c.GetFixtureA();
                    Fixture fixtureB = c.GetFixtureB();

                    AABB aabbA;
                    AABB aabbB;
                    fixtureA.GetAABB(out aabbA);
                    fixtureB.GetAABB(out aabbB);

                    Vector2 cA = aabbA.GetCenter();
                    Vector2 cB = aabbB.GetCenter();

                    DebugDraw.DrawSegment(cA, cB, color);
                }
            }

            if ((flags & DebugDrawFlags.AABB) == DebugDrawFlags.AABB)
            {
                Color color = new Color(0.9f, 0.3f, 0.9f);
                BroadPhase bp = _contactManager._broadPhase;

                for (Body b = _bodyList; b != null; b = b.GetNext())
                {
                    if (b.IsActive() == false)
                    {
                        continue;
                    }

                    for (Fixture f = b.GetFixtureList(); f != null; f = f.GetNext())
                    {
                        AABB aabb;
                        bp.GetFatAABB(f._proxyId, out aabb);
                        FixedArray8<Vector2> vs = new FixedArray8<Vector2>();
                        vs[0] = new Vector2(aabb.lowerBound.X, aabb.lowerBound.Y);
                        vs[1] = new Vector2(aabb.upperBound.X, aabb.lowerBound.Y);
                        vs[2] = new Vector2(aabb.upperBound.X, aabb.upperBound.Y);
                        vs[3] = new Vector2(aabb.lowerBound.X, aabb.upperBound.Y);

                        DebugDraw.DrawPolygon(ref vs, 4, color);
                    }
                }
            }

            if ((flags & DebugDrawFlags.CenterOfMass) == DebugDrawFlags.CenterOfMass)
            {
                for (Body b = _bodyList; b != null; b = b.GetNext())
                {
                    Transform xf;
                    b.GetTransform(out xf);
                    xf.Position = b.GetWorldCenter();
                    DebugDraw.DrawTransform(ref xf);
                }
            }
        }

        void DrawJoint(Joint joint)
        {
            Body b1 = joint.GetBodyA();
            Body b2 = joint.GetBodyB();
            Transform xf1, xf2;
            b1.GetTransform(out xf1);
            b2.GetTransform(out xf2);
            Vector2 x1 = xf1.Position;
            Vector2 x2 = xf2.Position;
            Vector2 p1 = joint.GetAnchorA();
            Vector2 p2 = joint.GetAnchorB();

            Color color = new Color(0.5f, 0.8f, 0.8f);

            switch (joint.JointType)
            {
                case JointType.Distance:
                    DebugDraw.DrawSegment(p1, p2, color);
                    break;

                case JointType.Pulley:
                    {
                        PulleyJoint pulley = (PulleyJoint)joint;
                        Vector2 s1 = pulley.GetGroundAnchorA();
                        Vector2 s2 = pulley.GetGroundAnchorB();
                        DebugDraw.DrawSegment(s1, p1, color);
                        DebugDraw.DrawSegment(s2, p2, color);
                        DebugDraw.DrawSegment(s1, s2, color);
                    }
                    break;

                case JointType.Mouse:
                    // don't draw this
                    break;

                default:
                    DebugDraw.DrawSegment(x1, p1, color);
                    DebugDraw.DrawSegment(p1, p2, color);
                    DebugDraw.DrawSegment(x2, p2, color);
                    break;
            }
        }

        void DrawShape(Fixture fixture, Transform xf, Color color)
        {
            switch (fixture.ShapeType)
            {
                case ShapeType.Circle:
                    {
                        CircleShape circle = (CircleShape)fixture.GetShape();

                        Vector2 center = MathUtils.Multiply(ref xf, circle._p);
                        float radius = circle._radius;
                        Vector2 axis = xf.R.col1;

                        DebugDraw.DrawSolidCircle(center, radius, axis, color);
                    }
                    break;

                case ShapeType.Polygon:
                    {
                        PolygonShape poly = (PolygonShape)fixture.GetShape();
                        int vertexCount = poly._vertexCount;
                        Debug.Assert(vertexCount <= Settings.b2_maxPolygonVertices);
                        FixedArray8<Vector2> vertices = new FixedArray8<Vector2>();

                        for (int i = 0; i < vertexCount; ++i)
                        {
                            vertices[i] = MathUtils.Multiply(ref xf, poly._vertices[i]);
                        }

                        DebugDraw.DrawSolidPolygon(ref vertices, vertexCount, color);
                    }
                    break;
            }
        }
    }
}