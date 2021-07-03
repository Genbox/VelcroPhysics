namespace Genbox.VelcroPhysics.Collision.Filtering
{
    public class Filter
    {
        public Filter()
        {
            Group = Settings.DefaultCollisionGroup;
            Category = Settings.DefaultFixtureCollisionCategories;
            CategoryMask = Settings.DefaultFixtureCollidesWith;
        }

        public Filter(short group, Category category, Category mask)
        {
            Group = group;
            Category = category;
            CategoryMask = mask;
        }

        /// <summary>Collision groups allow a certain group of objects to never collide(negative) or always collide (positive).
        /// Zero means no collision group. Non-zero group filtering always wins against the mask bits.</summary>
        public short Group { get; set; }

        /// <summary>The collision category bits. Normally you would just set one bit.</summary>
        public Category Category { get; set; }

        /// <summary>The collision mask bits. This states the categories that this shape would accept for collision.</summary>
        public Category CategoryMask { get; set; }
    }
}