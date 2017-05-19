using System;

namespace VelcroPhysics.Collision.ContactSystem
{
    [Flags]
    internal enum ContactFlags : byte
    {
        Unknown = 0,

        /// <summary>
        /// Used when crawling contact graph when forming islands.
        /// </summary>
        IslandFlag = 1,

        /// <summary>
        /// Set when the shapes are touching.
        /// </summary>
        TouchingFlag = 2,

        /// <summary>
        /// This contact can be disabled (by user)
        /// </summary>
        EnabledFlag = 4,

        /// <summary>
        /// This contact needs filtering because a fixture filter was changed.
        /// </summary>
        FilterFlag = 8,

        /// <summary>
        /// This bullet contact had a TOI event
        /// </summary>
        BulletHitFlag = 16,

        /// <summary>
        /// This contact has a valid TOI in m_toi
        /// </summary>
        TOIFlag = 32
    }
}
