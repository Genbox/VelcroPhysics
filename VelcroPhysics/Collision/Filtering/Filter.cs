namespace VelcroPhysics.Collision.Filtering
{
    public class Filter
    {
        public Filter(short group, Category category, Category mask)
        {
            Group = group;
            Category = category;
            CategoryMask = mask;
        }

        /// <summary>
        /// Defaults to 0
        /// If Settings.UseFPECollisionCategories is set to false:
        /// Collision groups allow a certain group of objects to never collide (negative)
        /// or always collide (positive). Zero means no collision group. Non-zero group
        /// filtering always wins against the mask bits.
        /// If Settings.UseFPECollisionCategories is set to true:
        /// If 2 fixtures are in the same collision group, they will not collide.
        /// </summary>
        public short Group { get; set; }

        /// <summary>
        /// Defaults to Category.All
        /// The collision mask bits. This states the categories that this
        /// fixture would accept for collision.
        /// Use Settings.UseFPECollisionCategories to change the behavior.
        /// </summary>
        public Category Category { get; set; }

        /// <summary>
        /// The collision categories this fixture is a part of.
        /// If Settings.UseFPECollisionCategories is set to false:
        /// Defaults to Category.Cat1
        /// If Settings.UseFPECollisionCategories is set to true:
        /// Defaults to Category.All
        /// </summary>
        public Category CategoryMask { get; set; }
    }
}