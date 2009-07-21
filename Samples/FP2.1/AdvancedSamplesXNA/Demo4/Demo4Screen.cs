using System.Text;
using DemoBaseXNA;
using DemoBaseXNA.DrawingSystem;
using DemoBaseXNA.ScreenSystem;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerGames.AdvancedSamplesXNA.Demo4
{
    public class Demo4Screen : GameScreen
    {
        private Texture2D _chainTexture;
        private Vector2 _chainOrigin;
        private Path _chain;
        private Path _chainPin;
        private Path _chainSpring;
        private Path _chainSpring2;
        private Path _chainSlide;

        public override void Initialize()
        {
            PhysicsSimulator = new PhysicsSimulator(new Vector2(0, 150));
            PhysicsSimulatorView = new PhysicsSimulatorView(PhysicsSimulator);

            base.Initialize();
        }

        public override void LoadContent()
        {
            _chainTexture = DrawingHelper.CreateRectangleTexture(ScreenManager.GraphicsDevice, 20, 20, Color.White,
                                                                 Color.Black);
            _chainOrigin = new Vector2(_chainTexture.Width / 2f, _chainTexture.Height / 2f);


            _chain = ComplexFactory.Instance.CreateChain(PhysicsSimulator, new Vector2(150, 100), new Vector2(200, 300), 20.0f, 10.0f, 1, true, false, LinkType.RevoluteJoint);
            _chain.CreateGeoms(PhysicsSimulator, 1);

            _chainPin = ComplexFactory.Instance.CreateChain(PhysicsSimulator, new Vector2(250, 100), new Vector2(300, 300), 20.0f, 10.0f, 1, true, false, LinkType.PinJoint);
            _chainPin.CreateGeoms(PhysicsSimulator, 2);

            ComplexFactory.Instance.SpringConstant = 150;        // values inside let us setup additional parameters
            ComplexFactory.Instance.DampingConstant = 10;
            ComplexFactory.Instance.SpringRestLengthFactor = 1f;
            _chainSpring = ComplexFactory.Instance.CreateChain(PhysicsSimulator, new Vector2(350, 100), new Vector2(400, 300), 20.0f, 10.0f, 1, true, false, LinkType.LinearSpring);
            _chainSpring.CreateGeoms(PhysicsSimulator, 3);

            ComplexFactory.Instance.Min = 0;
            ComplexFactory.Instance.Max = 15;
            _chainSlide = ComplexFactory.Instance.CreateChain(PhysicsSimulator, new Vector2(450, 100), new Vector2(500, 300), 20.0f, 10.0f, 1, true, false, LinkType.SliderJoint);
            _chainSlide.CreateGeoms(PhysicsSimulator, 4);


            ComplexFactory.Instance.SpringConstant = 300;        // values inside let us setup additional parameters
            ComplexFactory.Instance.DampingConstant = 10;
            ComplexFactory.Instance.SpringRestLengthFactor = 0.1f;
            _chainSpring2 = ComplexFactory.Instance.CreateChain(PhysicsSimulator, new Vector2(650, 100), new Vector2(600, 600), 20.0f, 10.0f, 40.0f, 1, true, false, LinkType.LinearSpring);
            _chainSpring2.CreateGeoms(PhysicsSimulator, 5);

            base.LoadContent();
        }

        public override void Draw(GameTime gameTime)
        {
            ScreenManager.SpriteBatch.Begin(SpriteBlendMode.AlphaBlend);

            foreach (Body body in _chain.Bodies)
            {
                ScreenManager.SpriteBatch.Draw(_chainTexture, body.Position, null, Color.White, body.Rotation, _chainOrigin, 1, SpriteEffects.None, 1);
            }

            foreach (Body body in _chainPin.Bodies)
            {
                ScreenManager.SpriteBatch.Draw(_chainTexture, body.Position, null, Color.White, body.Rotation, _chainOrigin, 1, SpriteEffects.None, 1);
            }

            foreach (Body body in _chainSpring.Bodies)
            {
                ScreenManager.SpriteBatch.Draw(_chainTexture, body.Position, null, Color.White, body.Rotation, _chainOrigin, 1, SpriteEffects.None, 1);
            }

            foreach (Body body in _chainSpring2.Bodies)
            {
                ScreenManager.SpriteBatch.Draw(_chainTexture, body.Position, null, Color.White, body.Rotation, _chainOrigin, 1, SpriteEffects.None, 1);
            }

            foreach (Body body in _chainSlide.Bodies)
            {
                ScreenManager.SpriteBatch.Draw(_chainTexture, body.Position, null, Color.White, body.Rotation, _chainOrigin, 1, SpriteEffects.None, 1);
            }

            ScreenManager.SpriteBatch.End();

            base.Draw(gameTime);
        }

        public override void HandleInput(InputState input)
        {
            if (firstRun)
            {
                ScreenManager.AddScreen(new PauseScreen(GetTitle(), GetDetails()));
                firstRun = false;
            }
            if (input.PauseGame)
            {
                ScreenManager.AddScreen(new PauseScreen(GetTitle(), GetDetails()));
            }

            base.HandleInput(input);
        }

        public static string GetTitle()
        {
            return "Demo4: Chains factory";
        }

        private static string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows how to use the chain");
            sb.AppendLine("factory with the path generator.");
            sb.AppendLine(string.Empty);
            sb.AppendLine("Keyboard:");
            sb.AppendLine("  -Rotate : left and right arrows");
            sb.AppendLine("  -Move : A,S,D,W");
            return sb.ToString();
        }
    }
}