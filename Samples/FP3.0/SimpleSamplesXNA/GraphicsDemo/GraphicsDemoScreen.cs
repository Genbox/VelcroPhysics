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
        QuadRenderEngine renderEngine;
        Texture2D texture;
        float angle = 0;
        Stopwatch watch;
        Color tempColor;
        Random rand = new Random();
        
        public override void Initialize()
        {
            // init the new render engine
            renderEngine = new QuadRenderEngine(ScreenManager.GraphicsDevice);

            tempColor = Color.White;

            
            for (int i = 0; i < 49; i++)
            {
                tempColor.R -= (byte)((float)rand.NextDouble() * 255f);
                tempColor.G -= (byte)((float)rand.NextDouble() * 255f);
                tempColor.B -= (byte)((float)rand.NextDouble() * 255f);
                texture = DrawingHelper.CreateRectangleTexture(ScreenManager.GraphicsDevice, 100, 50, 2, tempColor, Color.Black);
                renderEngine.Submit(texture, true);
            }
            

            renderEngine.Submit(ScreenManager.ContentManager.Load<Texture2D>("Content/Star"), true);

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
            // add a quad
            for (int i = 0; i < 1000; i++)
            {
                angle = ((float)rand.NextDouble() * 6.14f) - 3.14f;

                renderEngine.Submit(new Quad(new Vector2(((float)rand.NextDouble() * 900f) - 450f, ((float)rand.NextDouble() * 900f) - 450f), angle,
                    ((float)rand.NextDouble() * 100f) + 10f, ((float)rand.NextDouble() * 50f) + 5f, (int)((float)rand.NextDouble() * 50)));
            }

            //angle = 1f;
            
            
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        public override void Draw(GameTime gameTime)
        {
            //ScreenManager.GraphicsDevice.RenderState.FillMode = FillMode.WireFrame;
            //ScreenManager.GraphicsDevice.RenderState.CullMode = CullMode.None;
            watch.Start();
            renderEngine.Render();
            watch.Stop();
            //ScreenManager.GraphicsDevice.RenderState.FillMode = FillMode.Solid;
            //ScreenManager.GraphicsDevice.RenderState.CullMode = CullMode.CullCounterClockwiseFace;

            ScreenManager.SpriteBatch.Begin();

            ScreenManager.SpriteBatch.DrawString(ScreenManager.SpriteFonts.DiagnosticSpriteFont, watch.ElapsedTicks.ToString(), new Vector2(5, 0), Color.Black);
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