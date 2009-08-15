using System.Collections.Generic;
using System.Text;
using DemoBaseXNA;
using DemoBaseXNA.DrawingSystem;
using DemoBaseXNA.ScreenSystem;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerGames.AdvancedSamplesXNA.Demo8
{
    public class Demo8Screen : GameScreen
    {
        private Body _circleBody;
        private Geom _circleGeom;
        private CircleBrush _circleBrush;

        private Body _rectangleBody;
        private RectangleBrush _rectangleBrush;

        private Vector2 _p1;
        private Vector2 _p2;

        private CircleBrush _marker;

        private LineBrush _lineBrush;
        private List<GeomPointPair> _intersectingGeoms = new List<GeomPointPair>();

        public override void Initialize()
        {
            PhysicsSimulator = new PhysicsSimulator(new Vector2(0, 0));
            PhysicsSimulatorView = new PhysicsSimulatorView(PhysicsSimulator);

            base.Initialize();
        }

        public override void LoadContent()
        {
            _circleBrush = new CircleBrush(64, Color.White, Color.Black);
            _circleBrush.Load(ScreenManager.GraphicsDevice);

            _circleBody = BodyFactory.Instance.CreateCircleBody(PhysicsSimulator, 64, 1);
            _circleBody.Position = new Vector2(725, 384);
            _circleGeom = GeomFactory.Instance.CreateCircleGeom(PhysicsSimulator, _circleBody, 64, 20);

            _rectangleBody = BodyFactory.Instance.CreateRectangleBody(PhysicsSimulator, 128, 128, 1);
            _rectangleBody.Position = new Vector2(256, 384);
            GeomFactory.Instance.CreateRectangleGeom(PhysicsSimulator, _rectangleBody, 128, 128);

            _rectangleBrush = new RectangleBrush(128, 128, Color.Gold, Color.Black);
            _rectangleBrush.Load(ScreenManager.GraphicsDevice);

            _p1 = ScreenManager.ScreenCenter;
            _p2 = _circleGeom.Position;

            _lineBrush = new LineBrush(1, Color.Black);
            _lineBrush.Load(ScreenManager.GraphicsDevice);

            _marker = new CircleBrush(3, Color.Red, Color.Red);
            _marker.Load(ScreenManager.GraphicsDevice);

            base.LoadContent();
        }

        public override void Draw(GameTime gameTime)
        {
            ScreenManager.SpriteBatch.Begin(SpriteBlendMode.AlphaBlend);

            _circleBrush.Draw(ScreenManager.SpriteBatch, _circleBody.Position);
            _rectangleBrush.Draw(ScreenManager.SpriteBatch, _rectangleBody.Position, _rectangleBody.Rotation);
            _lineBrush.Draw(ScreenManager.SpriteBatch, _p1, _p2);

            foreach (GeomPointPair pair in _intersectingGeoms)
            {
                foreach (Vector2 point in pair.Points)
                {
                    _marker.Draw(ScreenManager.SpriteBatch, point);
                }
            }

            ScreenManager.SpriteBatch.DrawString(ScreenManager.SpriteFonts.DetailsFont, string.Format("Colliding with {0} geometries", _intersectingGeoms.Count), new Vector2(50, ScreenManager.ScreenHeight - 70), Color.White);

            ScreenManager.SpriteBatch.End();

            base.Draw(gameTime);
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            _intersectingGeoms.Clear();

            _intersectingGeoms = RayHelper.LineSegmentAllGeomsIntersect(ref _p1, ref _p2, PhysicsSimulator, false);

            if (_intersectingGeoms.Count > 0)
                _lineBrush.Color = Color.Yellow;
            else
                _lineBrush.Color = Color.Black;

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
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

            if (input.CurrentGamePadState.IsConnected)
            {
                HandleGamePadInput(input);
            }
            else
            {
                HandleMouseInput(input);
            }
            base.HandleInput(input);
        }

        private void HandleMouseInput(InputState input)
        {
            _p2 = new Vector2(input.CurrentMouseState.X, input.CurrentMouseState.Y);
        }

        private void HandleGamePadInput(InputState input)
        {
            _p1 = new Vector2(input.CurrentGamePadState.ThumbSticks.Left.X, input.CurrentGamePadState.ThumbSticks.Left.Y);
            _p2 = new Vector2(input.CurrentGamePadState.ThumbSticks.Right.X, input.CurrentGamePadState.ThumbSticks.Right.Y);
        }

        public static string GetTitle()
        {
            return "Demo8: Line intersections";
        }

        private static string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows two bodies and how they");
            sb.AppendLine("intersect with a line");
            sb.AppendLine(string.Empty);
            sb.AppendLine("GamePad:");
            sb.AppendLine("  -Rotate: left and right triggers");
            sb.AppendLine("  -Move point1: left thumbstick");
            sb.AppendLine("  -Move point2: right thumbstick");
            sb.AppendLine(string.Empty);
            sb.AppendLine("Keyboard:");
            sb.AppendLine("  -Rotate: left and right arrows");
            sb.AppendLine("  -Move: A,S,D,W");
            sb.AppendLine(string.Empty);
            sb.AppendLine("Mouse");
            sb.AppendLine("  -Move point: Move mouse");
            sb.AppendLine("  -Move: Hold down left button and drag");
            return sb.ToString();
        }
    }
}