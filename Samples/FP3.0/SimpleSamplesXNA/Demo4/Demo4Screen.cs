using System;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using DemoBaseXNA;
using DemoBaseXNA.DrawingSystem;
using DemoBaseXNA.ScreenSystem;
using FarseerPhysics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FarseerGames.SimpleSamplesXNA.Demo4
{
    /// <summary>
    /// Designed to show the new rendering engine.
    /// </summary>
    public class Demo4Screen : GameScreen
    {
        List<Body> _crateBodies;

        float crateSize = 0.5f;
        int texture;
        int Count = 15;

        public override void Initialize()
        {
            PhysicsSimulator = new World(new Vector2(0, -9.8f), true);
            PhysicsSimulatorView = new PhysicsSimulatorView(PhysicsSimulator, ScreenManager.Camera);

            _crateBodies = new List<Body>();

            Vertices box = PolygonTools.CreateBox(crateSize, crateSize);
            PolygonShape shape = new PolygonShape(box, 10f);

            Vector2 x = new Vector2(-15f, -17.5f);
            Vector2 deltaX = new Vector2(crateSize * 1.01f, crateSize * 2f);
            Vector2 deltaY = new Vector2(crateSize * 2.01f, 0.0f);

            for (int i = 0; i < Count; ++i)
            {
                Vector2 y = x;

                for (int j = i; j < Count; ++j)
                {
                    Body body = PhysicsSimulator.CreateBody();
                    _crateBodies.Add(body);
                    body.BodyType = BodyType.Dynamic;
                    body.Position = y;
                    Fixture fix = body.CreateFixture(shape);
                    fix.Friction = 1.0f;

                    y += deltaY;
                }

                x += deltaX;
            }

            base.Initialize();
        }

        public override void LoadContent()
        {
           
            // this is the crate image and yes you probably should load that content inside LoadContent...
            //ScreenManager.QuadRenderEngine.Submit(ScreenManager.ContentManager.Load<Texture2D>("Content/crate2"), true);

            // this is the default image and looks just like the old farseer
            texture = ScreenManager.QuadRenderEngine.Submit(DrawingHelper.CreateRectangleTexture(ScreenManager.GraphicsDevice, 50, 50, 1, Color.White, Color.Black), false);

            base.LoadContent();
        }

        public override void Draw(GameTime gameTime)
        {
            // add a quad for each body
            foreach (var crate in _crateBodies)
            {
                Color tint;

                // if the crate is awake no tint
                if (crate.Awake == true)
                    tint = Color.White;

                // otherwise tint it light blue
                else
                    tint = Color.LightBlue;

                // submit the quad to the QuadRenderEngine
                // we add 0.0055 to the crates size to help cover cracks when stacking crates
                //ScreenManager.QuadRenderEngine.Submit(new Quad(crate.Position, crate.Rotation,
                //    (crateSize + 0.0055f) * 2, (crateSize + 0.0055f) * 2, texture, tint));
            }

            // Call Render and relax.
           // ScreenManager.QuadRenderEngine.Render(ScreenManager.Camera.Projection, ScreenManager.Camera.View);

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
            return "Demo4: Stacked Objects";
        }

        public static string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows the stacking stability of the engine.");
            sb.AppendLine("It shows a stack of rectangular bodies stacked in");
            sb.AppendLine("the shape of a pyramid.");
            sb.AppendLine(string.Empty);
            sb.AppendLine("GamePad:");
            sb.AppendLine("  -Rotate: left and right triggers");
            sb.AppendLine("  -Move: left thumbstick");
            sb.AppendLine(string.Empty);
            sb.AppendLine("Keyboard:");
            sb.AppendLine("  -Rotate: left and right arrows");
            sb.AppendLine("  -Move: A,S,D,W");
            sb.AppendLine(string.Empty);
            sb.AppendLine("Mouse");
            sb.AppendLine("  -Hold down left button and drag");
            return sb.ToString();
        }
    }
}