using Microsoft.Xna.Framework;

namespace VelcroPhysics.Shared
{
    /// <summary>
    /// A transform contains translation and rotation. It is used to represent
    /// the position and orientation of rigid frames.
    /// </summary>
    public struct Transform
    {
        public Vector2 p;
        public Rot q;

        /// <summary>
        /// Initialize using a position vector and a rotation matrix.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="rotation">The r.</param>
        public Transform(ref Vector2 position, ref Rot rotation)
        {
            p = position;
            q = rotation;
        }

        /// <summary>
        /// Set this to the identity transform.
        /// </summary>
        public void SetIdentity()
        {
            p = Vector2.Zero;
            q.SetIdentity();
        }

        /// <summary>
        /// Set this based on the position and angle.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="angle">The angle.</param>
        public void Set(Vector2 position, float angle)
        {
            p = position;
            q.Set(angle);
        }
    }
}