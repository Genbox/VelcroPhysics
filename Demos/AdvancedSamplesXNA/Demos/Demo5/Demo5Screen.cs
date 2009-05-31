using System.Text;
using FarseerGames.AdvancedSamplesXNA.DrawingSystem;
using FarseerGames.AdvancedSamplesXNA.ScreenSystem;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerGames.AdvancedSamplesXNA.Demos.Demo5
{
    public class Demo5Screen : GameScreen
    {
        private Texture2D _chainTexture;
        private Vector2 _chainOrigin;
        private Path _chain;
        private Path _chainPin;
        private Path _chainSpring;
        private Path _chainSilde;

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

            _chainPin = ComplexFactory.Instance.CreateChain(PhysicsSimulator, new Vector2(250, 100), new Vector2(400, 300), 20.0f, 10.0f, 1, true, false, LinkType.PinJoint);
            _chainPin.CreateGeoms(PhysicsSimulator, 2);

            ComplexFactory.Instance.SpringConstant = 150;        // values inside let us setup additional parameters
            ComplexFactory.Instance.DampingConstant = 10;
            _chainSpring = ComplexFactory.Instance.CreateChain(PhysicsSimulator, new Vector2(350, 100), new Vector2(500, 300), 20.0f, 10.0f, 1, true, false, LinkType.LinearSpring);
            _chainSpring.CreateGeoms(PhysicsSimulator, 3);

            ComplexFactory.Instance.Min = 0;
            ComplexFactory.Instance.Max = 15;
            _chainSilde = ComplexFactory.Instance.CreateChain(PhysicsSimulator, new Vector2(450, 100), new Vector2(600, 300), 20.0f, 10.0f, 1, true, false, LinkType.SliderJoint);
            _chainSilde.CreateGeoms(PhysicsSimulator, 4);


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

            foreach (Body body in _chainSilde.Bodies)
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
                ScreenManager.AddScreen(new PauseScreen(GetTitle(), GetDetails(), this));
                firstRun = false;
            }
            if (input.PauseGame)
            {
                ScreenManager.AddScreen(new PauseScreen(GetTitle(), GetDetails(), this));
            }

            base.HandleInput(input);
        }

        public static string GetTitle()
        {
            return "Demo5: Chains factory";
        }

        public static string GetDetails()
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
