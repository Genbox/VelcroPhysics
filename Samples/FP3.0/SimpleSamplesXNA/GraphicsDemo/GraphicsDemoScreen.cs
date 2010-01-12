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
        const string _graphDemoText = "Graph Rendering Demo";
        
        Dictionary<string, GraphRenderHelper> _graphs;
        Rectangle _graphRect;
        Texture2D _texture;
        
        public override void Initialize()
        {
            _graphs = new Dictionary<string, GraphRenderHelper>();

            _graphs.Add("First Graph", new GraphRenderHelper(ScreenManager.GraphicsDevice, 151, 0, 100, Color.LightGreen));
            _graphs.Add("Second Graph", new GraphRenderHelper(ScreenManager.GraphicsDevice, 50, 10, 50, Color.Red));
            _graphs.Add("Third Graph", new GraphRenderHelper(ScreenManager.GraphicsDevice, 100, 0.2f, 1, Color.Blue));

            base.Initialize();
        }

        public override void LoadContent()
        {
            _texture = ScreenManager.ContentManager.Load<Texture2D>("Content/Common/gradient");
            
            base.LoadContent();
        }

        public override void UnloadContent()
        {
            base.UnloadContent();
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            _graphs["First Graph"].UpdateGraph((float)Math.Sin(gameTime.TotalGameTime.TotalSeconds) * 50f + 50f);
            _graphs["Second Graph"].UpdateGraph((float)Math.Sin(gameTime.TotalGameTime.TotalSeconds) * 50f);
            _graphs["Third Graph"].UpdateGraph((float)Math.Sin(gameTime.TotalGameTime.TotalMilliseconds) * 1f);
            
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        public override void Draw(GameTime gameTime)
        {
            _graphRect = new Rectangle((int)(ScreenManager.GraphicsDevice.Viewport.Width * 0.02f), (int)(ScreenManager.GraphicsDevice.Viewport.Height * 0.04f),
                (int)(ScreenManager.GraphicsDevice.Viewport.Width * 0.5f - ScreenManager.GraphicsDevice.Viewport.Width * 0.04f), (int)(ScreenManager.GraphicsDevice.Viewport.Height * 0.25f));
            
            float x = _graphRect.Width - ScreenManager.SpriteFonts.DiagnosticSpriteFont.MeasureString(_graphDemoText).X;
            Vector2 stringPosition = new Vector2((_graphRect.X + (x / 2f)),
                _graphRect.Y - 25);
            
            ScreenManager.SpriteBatch.Begin();

            ScreenManager.SpriteBatch.DrawString(ScreenManager.SpriteFonts.DiagnosticSpriteFont, _graphDemoText, stringPosition, Color.White);

            ScreenManager.SpriteBatch.Draw(_texture, _graphRect, Color.White);

            ScreenManager.SpriteBatch.End();

            foreach (GraphRenderHelper graph in _graphs.Values)
            {
                graph.Render(_graphRect);
            }
            
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
            return "Graphics Demo: QuadRenderEngine is easy and fast.";
        }

        public static string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows several of QuadRenderEngines");
            sb.AppendLine("features.");
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