using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.Testbed.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FarseerPhysics.Testbed.Tests
{
    public class RoundedRectangle : Test
    {
        private int _segments = 3;

        private RoundedRectangle()
        {
            //Ground
            BodyFactory.CreateEdge(World, new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));

            //Arcs
            BodyFactory.CreateLineArc(World, MathHelper.Pi * 1.5f, 50, 2, false, new Vector2(-15, 10));
            BodyFactory.CreateLineArc(World, MathHelper.Pi, 50, 2, false, new Vector2(-20, 10));
            BodyFactory.CreateLineArc(World, MathHelper.Pi / 1.5f, 50, 2, false, new Vector2(-25, 10));
            BodyFactory.CreateLineArc(World, MathHelper.Pi / 2, 50, 2, false, new Vector2(-30, 10));

            BodyFactory.CreateLineArc(World, MathHelper.Pi * 1.5f, 50, 2, true, new Vector2(-15, 25));
            BodyFactory.CreateLineArc(World, MathHelper.Pi, 50, 2, true, new Vector2(-20, 25));
            BodyFactory.CreateLineArc(World, MathHelper.Pi / 1.5f, 50, 2, true, new Vector2(-25, 25));
            BodyFactory.CreateLineArc(World, MathHelper.Pi / 2, 50, 2, true, new Vector2(-30, 25));

            BodyFactory.CreateSolidArc(World, 1, MathHelper.Pi * 1.5f, 50, 2, new Vector2(-15, 40));
            BodyFactory.CreateSolidArc(World, 1, MathHelper.Pi, 50, 2, new Vector2(-20, 40));
            BodyFactory.CreateSolidArc(World, 1, MathHelper.Pi / 1.5f, 50, 2, new Vector2(-25, 40));
            BodyFactory.CreateSolidArc(World, 1, MathHelper.Pi / 2, 50, 2, new Vector2(-30, 40));

            Create(0);
        }

        public override void Keyboard(KeyboardManager keyboardManager)
        {
            if (keyboardManager.IsNewKeyPress(Keys.A))
                _segments++;

            if (keyboardManager.IsNewKeyPress(Keys.S) && _segments > 0)
                _segments--;

            if (keyboardManager.IsNewKeyPress(Keys.D))
                Create(0);

            if (keyboardManager.IsNewKeyPress(Keys.F))
                Create(1);

            base.Keyboard(keyboardManager);
        }

        private void Create(int type)
        {
            Vector2 position = new Vector2(0, 30);

            switch (type)
            {
                default:
                    Body rounded = BodyFactory.CreateRoundedRectangle(World, 10, 10, 2.5F, 2.5F, _segments, 10, position);
                    rounded.BodyType = BodyType.Dynamic;
                    break;
                case 1:
                    Body capsule = BodyFactory.CreateCapsule(World, 10, 2, (int)MathHelper.Max(_segments, 1), 3, (int)MathHelper.Max(_segments, 1), 10, position);
                    capsule.BodyType = BodyType.Dynamic;
                    break;
            }
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            base.Update(settings, gameTime);
            DrawString("Segments: " + _segments + "\nPress: 'A' to increase segments, 'S' decrease segments\n'D' to create rectangle. 'F' to create capsule.");
        }

        internal static Test Create()
        {
            return new RoundedRectangle();
        }
    }
}