using VelcroPhysics.Shared;

namespace VelcroPhysics.Collision.Broadphase
{
    /// <summary>
    /// A node in the dynamic tree. The client does not interact with this directly.
    /// </summary>
    internal class TreeNode<T>
    {
        /// <summary>
        /// Enlarged AABB
        /// </summary>
        internal AABB AABB;

        internal int Child1;
        internal int Child2;

        internal int Height;
        internal int ParentOrNext;
        internal T UserData;

        internal bool IsLeaf()
        {
            return Child1 == DynamicTree<T>.NullNode;
        }
    }
}