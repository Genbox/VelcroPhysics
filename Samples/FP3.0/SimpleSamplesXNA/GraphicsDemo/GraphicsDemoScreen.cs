using System.Text;
using System.Collections.Generic;
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
        PolygonRenderHelper _polyRenderer;
        BasicEffect _effect;
        
        public override void Initialize()
        {
            _effect = new BasicEffect(ScreenManager.GraphicsDevice, null);
            _effect.VertexColorEnabled = true;
            _effect.Projection = Matrix.CreateOrthographic(10, 10, 0, 1);
            
            _polyRenderer = new PolygonRenderHelper(100);

            Vertices verts = new Vertices();

            verts.Add(new Vector2(-1,-1));
            verts.Add(new Vector2(-0.5f, 0));
            verts.Add(new Vector2(-1,1));
            verts.Add(new Vector2(0, 0.5f));
            verts.Add(new Vector2(1,1));
            verts.Add(new Vector2(0.5f, 0));
            verts.Add(new Vector2(1,-1));
            verts.Add(new Vector2(0, -0.5f));

            _polyRenderer.Submit(verts, Color.Green);

            //Vertices convexHull = new Vertices(verts.GetConvexHull());

            //Vector2 t = new Vector2(3, 0);

            //convexHull.Translate(ref t);

            //_polyRenderer.Submit(convexHull, Color.Yellow);
            
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
            ScreenManager.GraphicsDevice.RenderState.FillMode = FillMode.WireFrame;
            ScreenManager.GraphicsDevice.RenderState.CullMode = CullMode.None;

            _polyRenderer.Render(ScreenManager.GraphicsDevice, _effect);

            ScreenManager.GraphicsDevice.RenderState.FillMode = FillMode.Solid;
            ScreenManager.GraphicsDevice.RenderState.CullMode = CullMode.CullCounterClockwiseFace;
            
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