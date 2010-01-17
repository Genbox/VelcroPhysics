using FarseerPhysics;
using Microsoft.Xna.Framework;

namespace FarseerGames.FarseerPhysics.Interfaces
{
    public interface IFluidContainer
    {
        bool Intersect(ref AABB aabb);
        bool Contains(ref Vector2 vector);
    }
}