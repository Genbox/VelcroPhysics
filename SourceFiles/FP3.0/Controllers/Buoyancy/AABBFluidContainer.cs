using FarseerGames.FarseerPhysics.Interfaces;
using FarseerPhysics;
using Microsoft.Xna.Framework;

namespace FarseerGames.FarseerPhysics.Controllers
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

        #region IFluidContainer Members

        public bool Intersect(ref AABB aabb)
        {
            return AABB.TestOverlap(ref aabb, ref AABB);
        }

        public bool Contains(ref Vector2 vector)
        {
            return AABB.Contains(ref vector);
        }

        #endregion
    }
}