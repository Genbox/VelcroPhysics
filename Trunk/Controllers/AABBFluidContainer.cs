using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Mathematics;

namespace FarseerGames.FarseerPhysics.Controllers
{
    public class AABBFluidContainer : IFluidContainer
    {
        private AABB aabb;

        public AABBFluidContainer()
        {
        }

        public AABBFluidContainer(AABB aabb)
        {
            this.aabb = aabb;
        }

        public AABB AABB
        {
            get { return aabb; }
            set { aabb = value; }
        }

        #region IFluidContainer Members

        public bool Intersect(AABB aabb)
        {
            return AABB.Intersect(aabb, this.aabb);
        }

        public bool Contains(ref Vector2 vector)
        {
            return aabb.Contains(vector);
        }

        #endregion
    }
}