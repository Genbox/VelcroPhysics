using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Factories
{
    /// <summary>
    /// An easy to use factory for creating bodies
    /// </summary>
    public static class BodyFactory
    {
        public static Body CreateBody(World world)
        {
            Body body = new Body(world);
            return body;
        }

        public static Body CreateBody(World world, Vector2 position)
        {
            Body body = CreateBody(world);
            body.Position = position;
            return body;
        }
    }
}