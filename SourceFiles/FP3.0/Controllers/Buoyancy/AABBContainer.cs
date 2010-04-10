using FarseerPhysics.Collision;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Controllers.Buoyancy
{
    /// <summary>
    /// TODO: Create documentation
    /// </summary>
    public class AABBFluidContainer : IFluidContainer
    {
        public AABB AABB;

        public AABBFluidContainer()
        {
        }

        public AABBFluidContainer(Vector2 min, Vector2 max)
        {
            AABB = new AABB(ref min, ref max);
        }

        public AABBFluidContainer(ref AABB aabb)
        {
            AABB = aabb;
        }

        public AABBFluidContainer(Vector2 position, float width, float height)
        {
            AABB = new AABB(width, height, position);
        }

        #region IFluidContainer Members

        public bool Intersect(ref AABB aabb)
        {
            return AABB.TestOverlap(ref aabb, ref AABB);
        }

        public bool Contains(ref Vector2 point)
        {
            return AABB.Contains(ref point);
        }

        public void Update(float dt)
        {
            //do nothing
        }

        #endregion
    }
}