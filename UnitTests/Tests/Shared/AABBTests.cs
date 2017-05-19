using Microsoft.Xna.Framework;
using VelcroPhysics.Shared;
using Xunit;

namespace UnitTests.Tests.Shared
{
    public class AABBTests
    {
        [Fact]
        public void TestOverlap()
        {
            {
                AABB bb1 = new AABB(new Vector2(-2, -3), new Vector2(-1, 0));
                Assert.True(AABB.TestOverlap(ref bb1, ref bb1));
            }
            {
                Vector2 vec = new Vector2(-2, -3);
                AABB bb1 = new AABB(vec, vec);
                Assert.True(AABB.TestOverlap(ref bb1, ref bb1));
            }
            {
                AABB bb1 = new AABB(new Vector2(-2, -3), new Vector2(-1, 0));
                AABB bb2 = new AABB(new Vector2(-1, -1), new Vector2(1, 2));
                Assert.True(AABB.TestOverlap(ref bb1, ref bb2));
            }
            {
                AABB bb1 = new AABB(new Vector2(-99, -3), new Vector2(-1, 0));
                AABB bb2 = new AABB(new Vector2(76, -1), new Vector2(-2, 2));
                Assert.True(AABB.TestOverlap(ref bb1, ref bb2));
            }
            {
                AABB bb1 = new AABB(new Vector2(-20, -3), new Vector2(-18, 0));
                AABB bb2 = new AABB(new Vector2(-1, -1), new Vector2(1, 2));
                Assert.False(AABB.TestOverlap(ref bb1, ref bb2));
            }
            {
                AABB bb1 = new AABB(new Vector2(-2, -3), new Vector2(-1, 0));
                AABB bb2 = new AABB(new Vector2(-1, +1), new Vector2(1, 2));
                Assert.False(AABB.TestOverlap(ref bb1, ref bb2));
            }
            {
                AABB bb1 = new AABB(new Vector2(-2, +3), new Vector2(-1, 0));
                AABB bb2 = new AABB(new Vector2(-1, -1), new Vector2(0, -2));
                Assert.False(AABB.TestOverlap(ref bb1, ref bb2));
            }
        }
    }
}
