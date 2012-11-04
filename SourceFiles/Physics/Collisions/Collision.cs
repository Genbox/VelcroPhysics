using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using FarseerPhysics.Collision;
using FarseerPhysics.Common;

namespace FarseerPhysics.Physics.Collisions
{
    public enum CollisionEvent
    {
        None,
        Inside,
        Enter,
        Exit
    }

    public struct Feature
    {
        /// <summary>
        /// Distance from the feature (negative if inside)
        /// </summary>
        public float Distance;

        /// <summary>
        /// Normal of the feature
        /// </summary>
        public Vector2 Normal;

        /// <summary>
        /// Target position on the feature
        /// </summary>
        public Vector2 Position;

        /// <summary>
        /// Event generated
        /// </summary>
        public CollisionEvent Event;

        public static Feature Empty
        {
            get
            {
                return new Feature
                {
                    Distance = float.PositiveInfinity,
                    Normal = Vector2.Zero,
                    Position = Vector2.Zero,
                    Event = CollisionEvent.None
                };
            }
        }
    }

    public class CollisionDefinition
    {
        /// <summary>
        /// Slip is the amount of friction between the collision and the fluid
        /// </summary>
        public float Slip;

        /// <summary>
        /// Toggles the sticky force for this collision
        /// </summary>
        public bool IsSticky;

        /// <summary>
        /// Distance from which sticky force acts
        /// </summary>
        public float StickDistance;

        /// <summary>
        /// Force used for gettings particles to stick to the surface
        /// </summary>
        public float StickForce;

        /// <summary>
        /// User data (use this for your own instead of the fixture's one)
        /// </summary>
        public object UserData;
    }

    public abstract class Collision
    {
        public RigidBody RigidBody { get; protected set; }

        public Fixture Fixture { get; protected set; }

        protected CollisionDefinition definition;

        protected AABB aabb;
        public AABB AABB
        {
            get { return aabb; }
        }

        internal bool added;

        /// <summary>
        /// True if the fixture's user data was not a CollisionDefinition
        /// </summary>
        public bool RigidOnly { get; protected set; }

        /// <summary>
        /// Properties of the collision, you can edit them at runtime freely
        /// (except vertices from the PolygonCollision)
        /// </summary>
        public CollisionDefinition Definition
        {
            get { return definition; }
        }

        protected Collision(RigidBody rb, Fixture f)
        {
            RigidBody = rb;
            Fixture = f;

            RigidOnly = !(f.UserData is CollisionDefinition);
            if (!RigidOnly)
            {
                definition = f.UserData as CollisionDefinition;
            }

            Fixture.UserData = this;

            Transform xform = new Transform();
            xform.SetIdentity();

            f.Shape.ComputeAABB(out aabb, ref xform, 0);
            aabb.LowerBound -= new Vector2(1.0f, 1.0f);
            aabb.UpperBound += new Vector2(1.0f, 1.0f);
        }

        public void Reset()
        {
            added = false;
        }

        public abstract bool Intersect(ref Vector2 point, ref Vector2 previousPoint, out Feature result);
    }
}