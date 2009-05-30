using System.Collections.Generic;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Factories;
using FarseerGames.AdvancedSamplesXNA.DrawingSystem;
using FarseerGames.AdvancedSamplesXNA.ScreenSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace FarseerGames.AdvancedSamplesXNA.Demos.Demo10
{
    internal class Splat
    {
        private List<Geom> _splatGeomList;
        private Body _splatBody;

        private static Texture2D _splatTexture;
        private static Vertices _splatTextureVertices;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="content"></param>
        public static void Load(ContentManager content)
        {
            _splatTexture = content.Load<Texture2D>("Content/Terrain");

            //Create an array to hold the data from the texture
            uint[] data = new uint[_splatTexture.Width * _splatTexture.Height];

            //Transfer the texture data to the array
            _splatTexture.GetData(data);

            _splatTextureVertices = Vertices.CreatePolygon(data, _splatTexture.Width, _splatTexture.Height, new Vector2(), 128, 2.0f);
        }

        public Splat(PhysicsSimulator ps, Vector2 position)
        {
            _splatGeomList = new List<Geom>();
            
            _splatBody = BodyFactory.Instance.CreatePolygonBody(_splatTextureVertices, 1);
            _splatBody.Position = position;

            _splatGeomList = AutoDivide.DivideGeom(_splatTextureVertices, _splatBody);

            ps.Add(_splatBody);

            foreach (Geom g in _splatGeomList)
            {
                ps.Add(g);
            }
        }

        public void Draw(SpriteBatch sb)
        {
            sb.Draw(_splatTexture, _splatBody.Position, null, Color.White, _splatBody.Rotation, _splatTextureVertices.GetCentroid(), 1.0f, SpriteEffects.None, 0.5f);
        }
    }
}