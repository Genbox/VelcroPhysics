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
        const string _animationDemoText = "Animated Sprite Sheet Rendering Demo";

        #region Graph Rendering Demo

        Dictionary<string, GraphRenderHelper> _graphs;
        Rectangle _graphRect;
        Texture2D _texture;

        #endregion

        #region Animated Sprite Sheet Demo
        int _explostionTextureIndex;
        List<Quad> _explosionQuads;
        List<RectF> _explosionFrames;
        RectF _explosionRect;
        #endregion

        public override void Initialize()
        {
            #region Graph Rendering Demo

            _graphs = new Dictionary<string, GraphRenderHelper>();

            _graphs.Add("First Graph", new GraphRenderHelper(ScreenManager.GraphicsDevice, 151, 0, 150, Color.LightGreen));
            _graphs.Add("Second Graph", new GraphRenderHelper(ScreenManager.GraphicsDevice, 50, 10, 50, Color.Red));
            _graphs.Add("Third Graph", new GraphRenderHelper(ScreenManager.GraphicsDevice, 100, 0.2f, 1, Color.Blue));
            _graphs.Add("Mouse X Graph", new GraphRenderHelper(ScreenManager.GraphicsDevice, 50, 0, ScreenManager.GraphicsDevice.Viewport.Width, Color.Orange));
            _graphs.Add("Mouse Y Graph", new GraphRenderHelper(ScreenManager.GraphicsDevice, 50, 0, ScreenManager.GraphicsDevice.Viewport.Height, Color.Yellow));
            
            #endregion

            #region Animated Sprite Sheet Demo

            _explosionQuads = new List<Quad>();
            _explosionFrames = new List<RectF>();

            #endregion

            base.Initialize();
        }

        public override void LoadContent()
        {
            #region Graph Rendering Demo
            _texture = ScreenManager.ContentManager.Load<Texture2D>("Content/Common/gradient");
            #endregion

            #region Animated Sprite Sheet Demo

            _explostionTextureIndex = ScreenManager.QuadRenderEngine.Submit(ScreenManager.ContentManager.Load<Texture2D>("Content/Explosions"), true);

            for (int i = 0; i < 16; i++)
            {
                _explosionFrames.Add(new RectF(((float)i * 64f) / 1024f, 65f / 512f, 62f / 1024f, 62f / 512f));
            }

            #endregion
            base.LoadContent();
        }

        public override void UnloadContent()
        {
            base.UnloadContent();
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            MouseState mouseState = Mouse.GetState();

            #region Graph Rendering Demo

            _graphs["First Graph"].UpdateGraph((float)Math.Sin(gameTime.TotalGameTime.TotalSeconds) * 50f + 50f);
            _graphs["Second Graph"].UpdateGraph((float)Math.Sin(gameTime.TotalGameTime.TotalSeconds) * 50f);
            _graphs["Third Graph"].UpdateGraph((float)Math.Sin(gameTime.TotalGameTime.TotalMilliseconds) * 1f);
            _graphs["Mouse X Graph"].UpdateGraph((float)mouseState.X);
            _graphs["Mouse Y Graph"].UpdateGraph((float)mouseState.Y);

            #endregion

            #region Animated Sprite Sheet Demo

            Random rand = new Random();

            _explosionRect = new RectF(ScreenManager.GraphicsDevice.Viewport.Width * 0.5f + ScreenManager.GraphicsDevice.Viewport.Width * 0.02f, 
                ScreenManager.GraphicsDevice.Viewport.Height * 0.04f,
                ScreenManager.GraphicsDevice.Viewport.Width * 0.5f - ScreenManager.GraphicsDevice.Viewport.Width * 0.04f, 
                ScreenManager.GraphicsDevice.Viewport.Height * 0.25f);

            float size = (float)rand.NextDouble() * 64f + 32f;

            if (_explosionRect.Contains(new Vector2(mouseState.X, mouseState.Y)))
            {
                Quad quad = new Quad(new Vector2(mouseState.X + (float)rand.NextDouble() * 50f - 50f, mouseState.Y + (float)rand.NextDouble() * 50f - 50f),
                    0, size, size, (float)rand.NextDouble(), 0.5f, _explostionTextureIndex, Color.White, true);
                quad.Frames = _explosionFrames;
                _explosionQuads.Add(quad);
            }

            for (int i = 0; i < _explosionQuads.Count; i++)
			{
                _explosionQuads[i].CurrentFrame++;
                
                if (_explosionQuads[i].CurrentFrame >= 15)
                    _explosionQuads.RemoveAt(i);

                if (_explosionQuads.Count > 0)
                    ScreenManager.QuadRenderEngine.Submit(_explosionQuads[i]);
			}

            #endregion

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        public override void Draw(GameTime gameTime)
        {
            #region Graph Rendering Demo

            _graphRect = new Rectangle((int)(ScreenManager.GraphicsDevice.Viewport.Width * 0.02f), (int)(ScreenManager.GraphicsDevice.Viewport.Height * 0.04f),
                (int)(ScreenManager.GraphicsDevice.Viewport.Width * 0.5f - ScreenManager.GraphicsDevice.Viewport.Width * 0.02f), (int)(ScreenManager.GraphicsDevice.Viewport.Height * 0.25f));
            
            ScreenManager.SpriteBatch.Begin();

            float x = _graphRect.Width - ScreenManager.SpriteFonts.DiagnosticSpriteFont.MeasureString(_graphDemoText).X;
            Vector2 stringPosition = new Vector2((_graphRect.X + (x / 2f)),
                _graphRect.Y - 25);

            ScreenManager.SpriteBatch.DrawString(ScreenManager.SpriteFonts.DiagnosticSpriteFont, _graphDemoText, stringPosition, Color.White);

            x = _explosionRect.Width - ScreenManager.SpriteFonts.DiagnosticSpriteFont.MeasureString(_animationDemoText).X;
            stringPosition = new Vector2((_explosionRect.X + (x / 2f)),
                _explosionRect.Y - 25);

            ScreenManager.SpriteBatch.DrawString(ScreenManager.SpriteFonts.DiagnosticSpriteFont, _animationDemoText, stringPosition, Color.White);

            ScreenManager.SpriteBatch.Draw(_texture, _graphRect, Color.White);

            ScreenManager.SpriteBatch.Draw(_texture, new Rectangle((int)_explosionRect.X, (int)_explosionRect.Y, (int)_explosionRect.Width, (int)_explosionRect.Height), Color.White);

            ScreenManager.SpriteBatch.End();

            foreach (GraphRenderHelper graph in _graphs.Values)
            {
                graph.Render(_graphRect);
            }

            #endregion

            // Call Render and relax.
            ScreenManager.QuadRenderEngine.Render(Matrix.CreateOrthographicOffCenter(0, ScreenManager.GraphicsDevice.Viewport.Width, ScreenManager.GraphicsDevice.Viewport.Height, 0, 0, 1), Matrix.Identity);

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
            return "Graphics Demo: Rendering only.";
        }

        public static string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows several of QuadRenderEngines");
            sb.AppendLine("features. As well as several helper classes");
            sb.AppendLine("and how to use them. Nothing is this demo uses");
            sb.AppendLine("any physics at all.");
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