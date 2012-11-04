using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using FarseerPhysics.Collision.Shapes;
using System.Diagnostics;

namespace FarseerPhysics.Physics.Collisions
{
    public class CircleCollision : Collision
    {
        private CircleShape _circle;

        public CircleCollision(RigidBody rb, Fixture f)
            : base(rb, f)
        {
            Debug.Assert(f.ShapeType == ShapeType.Circle);
            _circle = f.Shape as CircleShape;
        }

        public override bool Intersect(ref Vector2 point, ref Vector2 previousPoint, out Feature result)
        {
            if (aabb.Contains(ref point))
            {
                Vector2 toCenter = _circle.Position - point;
                float distanceToCenter = toCenter.Length();
                toCenter.Normalize();

                float distanceToBorder = distanceToCenter - _circle.Radius;

                result = new Feature
                {
                    Distance = distanceToBorder,
                    Normal = toCenter,
                    Position = point + toCenter * distanceToBorder,
                };

                return true;
            }

            result = Feature.Empty;
            return false;
        }
    }
}