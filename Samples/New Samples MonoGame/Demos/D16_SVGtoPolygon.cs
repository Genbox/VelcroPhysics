using System.Text;
using FarseerPhysics.Content;
using FarseerPhysics.Samples.ScreenSystem;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Samples.Demos
{
    internal class D16_SVGtoPolygon : PhysicsDemoScreen
    {
        #region Demo description
        public override string GetTitle()
        {
            return "SVG Importer to polygons";
        }

        public override string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows how to load vertices from a SVG.");
            sb.AppendLine();
            sb.AppendLine("GamePad:");
            sb.Append("  - Exit to demo selection: Back button");
#if WINDOWS
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine("Keyboard:");
            sb.AppendLine("  - Exit to demo selection: Escape");
#endif
            return sb.ToString();
        }
        #endregion

        private PolygonContainer _farseerPoly;

        public override void LoadContent()
        {
            base.LoadContent();

            World.Gravity = Vector2.Zero;

            _farseerPoly = Framework.Content.Load<PolygonContainer>("Pipeline/FarseerPolygon");
        }

        public override void Draw(GameTime gameTime)
        {
            DebugView.BeginCustomDraw(Camera.SimProjection, Camera.SimView);
            foreach (Polygon p in _farseerPoly.Values)
            {
                DebugView.DrawPolygon(p.Vertices.ToArray(), p.Vertices.Count, Color.Black, p.Closed);
            }
            DebugView.EndCustomDraw();

            base.Draw(gameTime);
        }
    }
}