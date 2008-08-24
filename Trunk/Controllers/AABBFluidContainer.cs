using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Mathematics;

namespace FarseerGames.FarseerPhysics.Controllers
{
    public class AABBFluidContainer : IFluidContainer
    {
        public AABBFluidContainer()
        {
        }

        public AABBFluidContainer(AABB aabb)
        {
            AABB = aabb;
        }

        public AABB AABB { get; set; }

        #region IFluidContainer Members

        public bool Intersect(AABB aabb)
        {
            return AABB.Intersect(aabb, AABB);
        }

        public bool Contains(ref Vector2 vector)
        {
            return AABB.Contains(vector);
        }

        #endregion
    }
}