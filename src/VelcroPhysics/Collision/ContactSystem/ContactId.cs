using System.Runtime.InteropServices;

namespace Genbox.VelcroPhysics.Collision.ContactSystem
{
    /// <summary>Contact ids to facilitate warm starting.</summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct ContactId
    {
        /// <summary>The features that intersect to form the contact point</summary>
        [FieldOffset(0)]
        public ContactFeature ContactFeature;

        /// <summary>Used to quickly compare contact ids.</summary>
        [FieldOffset(0)]
        public uint Key;
    }
}