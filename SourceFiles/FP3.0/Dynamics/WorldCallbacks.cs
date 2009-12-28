namespace FarseerPhysics
{
    public interface IDestructionListener
    {
        void SayGoodbye(Joint joint);
        void SayGoodbye(Fixture fixture);
    }

    public interface IContactFilter
    {
        bool ShouldCollide(Fixture fixtureA, Fixture fixtureB);
        bool RayCollide(object userData, Fixture fixture);
    }

    public struct ContactImpulse
    {
        public FixedArray2<float> normalImpulses;
        public FixedArray2<float> tangentImpulses;
    }

    public interface IContactListener
    {
        void BeginContact(Contact contact);
        void EndContact(Contact contact);
        void PreSolve(Contact contact, ref Manifold oldManifold);
        void PostSolve(Contact contact, ref ContactImpulse impulse);
    }

    public class DefaultContactFilter : IContactFilter
    {
        public bool ShouldCollide(Fixture fixtureA, Fixture fixtureB)
        {
            Filter filterA;
            fixtureA.GetFilterData(out filterA);

            Filter filterB;
            fixtureB.GetFilterData(out filterB);

            if (filterA.groupIndex == filterB.groupIndex && filterA.groupIndex != 0)
            {
                return filterA.groupIndex > 0;
            }

            bool collide = (filterA.maskBits & filterB.categoryBits) != 0 && (filterA.categoryBits & filterB.maskBits) != 0;

            return collide;
        }

        public bool RayCollide(object userData, Fixture fixture)
        {
            // By default, cast userData as a fixture, and then collide if the shapes would collide
            if (userData == null)
            {
                return true;
            }

            return ShouldCollide((Fixture)userData, fixture);
        }
    }

    public class DefaultContactListener : IContactListener
    {
        public void BeginContact(Contact contact) { }
        public void EndContact(Contact contact) { }
        public void PreSolve(Contact contact, ref Manifold oldManifold) { }
        public void PostSolve(Contact contact, ref ContactImpulse impulse) { }
    }
}
