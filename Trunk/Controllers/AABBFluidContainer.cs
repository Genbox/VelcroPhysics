using System;
#if (XNA)
using Microsoft.Xna.Framework;
#endif
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Mathematics;

namespace FarseerGames.FarseerPhysics.Controllers
{
    public class AABBFluidContainer : IFluidContainer
    {
        AABB aabb;

        public AABBFluidContainer() { }

        public AABBFluidContainer(AABB aabb)
        {
            this.aabb = aabb;
        }

        public AABB AABB
        {
            get { return aabb; }
            set { aabb = value; }
        }

        public bool Intersect(AABB aabb)
        {
            return AABB.Intersect(aabb, this.aabb);
        }

        public bool Contains(ref Vector2 vector)
        {
            return this.aabb.Contains(vector);
        }
    }
}
