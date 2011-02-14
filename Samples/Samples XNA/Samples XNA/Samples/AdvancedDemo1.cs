using System.Collections.Generic;
using System.Text;
using FarseerPhysics.Common;
using FarseerPhysics.Common.Decomposition;
using FarseerPhysics.Common.PolygonManipulation;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerPhysics.SamplesFramework
{
    internal class AdvancedDemo1 : PhysicsGameScreen, IDemoScreen
    {
        #region IDemoScreen Members

        public string GetTitle()
        {
            return "Texture to vertices";
        }

        public string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("TODO: Add sample description!");
            sb.AppendLine(string.Empty);
            sb.AppendLine("GamePad:");
            sb.AppendLine("  - Move cursor: left thumbstick");
            sb.AppendLine("  - Grab object (beneath cursor): A button");
            sb.AppendLine("  - Drag grabbed object: left thumbstick");
            sb.AppendLine("  - Exit to menu: Back button");
            sb.AppendLine(string.Empty);
            sb.AppendLine("Keyboard:");
            sb.AppendLine("  - Exit to menu: Escape");
            sb.AppendLine(string.Empty);
            sb.AppendLine("Mouse / Touchscreen");
            sb.AppendLine("  - Grab object (beneath cursor): Left click");
            sb.AppendLine("  - Drag grabbed object: move mouse / finger");
            return sb.ToString();
        }

        #endregion

        private BasicEffect _effect;
        private Body _compund;
        private Vector2 _origin;
        private Texture2D _polygonTexture;
        private Vector2 _scale;

        public override void LoadContent()
        {
            base.LoadContent();

            World.Gravity = Vector2.Zero;

            new Border(World, ScreenManager.GraphicsDevice.Viewport);

            _effect = new BasicEffect(ScreenManager.GraphicsDevice);
            _effect.TextureEnabled = true;
            _effect.VertexColorEnabled = true;

            //load texture that will represent the physics body
            _polygonTexture = ScreenManager.Content.Load<Texture2D>("Samples/object");

            //Create an array to hold the data from the texture
            uint[] data = new uint[_polygonTexture.Width * _polygonTexture.Height];

            //Transfer the texture data to the array
            _polygonTexture.GetData(data);

            //Find the vertices that makes up the outline of the shape in the texture
            Vertices textureVertices = PolygonTools.CreatePolygon(data, _polygonTexture.Width, false);

            //The tool return vertices as they were found in the texture.
            //We need to find the real center (centroid) of the vertices for 2 reasons:

            //1. To translate the vertices so the polygon is centered around the centroid.
            Vector2 centroid = -textureVertices.GetCentroid();
            textureVertices.Translate(ref centroid);

            //2. To draw the texture the correct place.
            _origin = -centroid;

            //We simplify the vertices found in the texture.
            textureVertices = SimplifyTools.ReduceByDistance(textureVertices, 4f);

            //Since it is a concave polygon, we need to partition it into several smaller convex polygons
            List<Vertices> list = BayazitDecomposer.ConvexPartition(textureVertices);

            //Now we need to scale the vertices (result is in pixels, we use meters)
            //At the same time we flip the y-axis.
            _scale = new Vector2(0.05f, -0.05f);

            foreach (Vertices vertices in list)
            {
                vertices.Scale(ref _scale);

                //When we flip the y-axis, the orientation can change.
                //We need to remember that FPE works with CCW polygons only.
                vertices.ForceCounterClockWise();
            }

            //Create a single body with multiple fixtures
            _compund = BodyFactory.CreateCompoundPolygon(World, list, 1f, BodyType.Dynamic);
            _compund.BodyType = BodyType.Dynamic;
        }

        public override void Draw(GameTime gameTime)
        {
            _effect.Projection = Camera.Projection;
            _effect.View = Camera.View;
            ScreenManager.SpriteBatch.Begin(0, null, null, null, null, _effect);
            ScreenManager.SpriteBatch.Draw(_polygonTexture, _compund.Position, null, Color.Tomato,
                                           _compund.Rotation, _origin, _scale, SpriteEffects.None, 0f);
            ScreenManager.SpriteBatch.End();

            base.Draw(gameTime);
        }
    }
}