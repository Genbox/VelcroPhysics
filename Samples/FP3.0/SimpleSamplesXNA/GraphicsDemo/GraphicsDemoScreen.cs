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

namespace FarseerGames.SimpleSamplesXNA.GraphicsDemo
{
    /// <summary>
    /// Designed to show the new rendering engine.
    /// </summary>
    public class GraphicsDemoScreen : GameScreen
    {
        List<Body> _globeBodies = new List<Body>();
        
        QuadRenderEngine renderEngine;
        float radius = 0.1f;
        Stopwatch watch;
        Random rand = new Random();
        
        public override void Initialize()
        {
            PhysicsSimulator.Gravity = new Vector2(0, -10f);

            for (int i = 0; i < 1000; i++)
            {
                //use the body factory to create the physics body
                _globeBodies.Add(PhysicsSimulator.CreateBody());
                _globeBodies[_globeBodies.Count - 1].Position = new Vector2(((float)rand.NextDouble() * 9f) - 4.5f, ((float)rand.NextDouble() * 9f) - 4.5f);
                _globeBodies[_globeBodies.Count - 1].BodyType = BodyType.Dynamic;
                Vertices box = PolygonTools.CreateBox(radius, radius);
                PolygonShape shape = new PolygonShape(box, 1);
                Fixture fix = _globeBodies[_globeBodies.Count - 1].CreateFixture(shape);
                fix.Friction = 0.5f;
                fix.Restitution = 0f;
            }
           

            // init the new render engine
            renderEngine = new QuadRenderEngine(ScreenManager.GraphicsDevice);

            renderEngine.Submit(ScreenManager.ContentManager.Load<Texture2D>("Content/Crate"), true);

            watch = new Stopwatch();
            
            base.Initialize();
        }

        public override void LoadContent()
        {
            base.LoadContent();
        }

        public override void UnloadContent()
        {
            base.UnloadContent();
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        public override void Draw(GameTime gameTime)
        {
            
            watch.Start();
            // add a quad
            foreach (var globe in _globeBodies)
            {
                Color tint;

                if (globe.Awake == true)
                    tint = Color.White;
                else
                    tint = Color.LightBlue;
                
                renderEngine.Submit(new Quad(globe.Position, globe.Rotation,
                    (radius + 0.0055f) * 2, (radius + 0.0055f) * 2, 0, tint));
            }

            //renderEngine.Render();
            watch.Stop();

            ScreenManager.SpriteBatch.Begin();

            ScreenManager.SpriteBatch.DrawString(ScreenManager.SpriteFonts.DiagnosticSpriteFont, watch.ElapsedMilliseconds.ToString(), new Vector2(5, 0), Color.Black);
            ScreenManager.SpriteBatch.DrawString(ScreenManager.SpriteFonts.DiagnosticSpriteFont, Stopwatch.Frequency.ToString(), new Vector2(50,0), Color.Black);

            ScreenManager.SpriteBatch.End();

            watch.Reset();
            
            
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

            if (input.CurrentKeyboardState.IsKeyDown(Keys.Space))
            {
                ScreenManager.Game.IsFixedTimeStep = true;
            }
            else
            {
                //ScreenManager.Game.IsFixedTimeStep = false;
            }
            
            base.HandleInput(input);
        }

        public static string GetTitle()
        {
            return "Graphics Demo: Cool new interface";
        }

        public static string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows a single body with geometry");
            sb.AppendLine("attached.");
            sb.AppendLine(string.Empty);
            sb.AppendLine("GamePad:");
            sb.AppendLine("  -Rotate: left and right triggers");
            sb.AppendLine("  -Move: left thumbstick");
            sb.AppendLine(string.Empty);
            sb.AppendLine("Keyboard:");
            sb.AppendLine("  -Rotate: left and right arrows");
            sb.AppendLine("  -Move: A,S,D,W");
            return sb.ToString();
        }
    }
}