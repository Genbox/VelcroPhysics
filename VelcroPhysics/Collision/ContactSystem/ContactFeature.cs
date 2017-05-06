namespace VelcroPhysics.Collision.ContactSystem
{
    /// <summary>
    /// The features that intersect to form the contact point
    /// This must be 4 bytes or less.
    /// </summary>
    public struct ContactFeature
    {
        /// <summary>
        /// Feature index on ShapeA
        /// </summary>
        public byte IndexA;

        /// <summary>
        /// Feature index on ShapeB
        /// </summary>
        public byte IndexB;

        /// <summary>
        /// The feature type on ShapeA
        /// </summary>
        public ContactFeatureType TypeA;

        /// <summary>
        /// The feature type on ShapeB
        /// </summary>
        public ContactFeatureType TypeB;
    }
}