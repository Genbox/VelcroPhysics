using System.Collections.Generic;
using System.Text;
using FarseerPhysics.Common;
using FarseerPhysics.Common.Decomposition;
using FarseerPhysics.DemoBaseXNA;
using FarseerPhysics.DemoBaseXNA.ScreenSystem;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerPhysics.AdvancedSamplesXNA
{
    internal class Demo1Screen : GameScreen, IDemoScreen
    {
        private List<Vertices> _list;
        private Texture2D _polygonTexture;
        private Vertices _verts;

        #region IDemoScreen Members

        public string GetTitle()
        {
            return "Demo1: Map vertices from textures";
        }

        public string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows how to map vertices from a texture");
            sb.AppendLine(string.Empty);
            sb.AppendLine("Keyboard:");
            sb.AppendLine("  -Rotate : left and right arrows");
            sb.AppendLine("  -Move : A,S,D,W");
            return sb.ToString();
        }

        #endregion

        public override void Initialize()
        {
            World = new World(new Vector2(0, -50));

            base.Initialize();
        }

        public override void LoadContent()
        {
            //load texture that will represent the physics body
            _polygonTexture = ScreenManager.ContentManager.Load<Texture2D>("Texture");

            //Create an array to hold the data from the texture
            uint[] data = new uint[_polygonTexture.Width * _polygonTexture.Height];

            //Transfer the texture data to the array
            _polygonTexture.GetData(data);

            //Find the vertices that makes up the outline of the shape in the texture
            _verts = PolygonTools.CreatePolygon(data, _polygonTexture.Width, _polygonTexture.Height, true);

            //For now we need to scale the vertices (result is in pixels, we use meters)
            Vector2 scale = new Vector2(0.07f, 0.07f);
            _verts.Scale(ref scale);

            //Since it is a concave polygon, we need to partition it into several smaller convex polygons
            _list = BayazitDecomposer.ConvexPartition(_verts);

            //Create a single body with multiple fixtures
            List<Fixture> compund = FixtureFactory.CreateCompoundPolygon(World, _list, 1);
            compund[0].Body.BodyType = BodyType.Dynamic;
            compund[0].Body.Position = new Vector2(-10, 0);

            base.LoadContent();
        }

        public override void HandleInput(InputState input)
        {
            if (input.CurrentGamePadState.IsConnected)
            {
                //HandleGamePadInput(input);
            }
            else
            {
                //HandleKeyboardInput(input);
            }

            base.HandleInput(input);
        }
    }
}