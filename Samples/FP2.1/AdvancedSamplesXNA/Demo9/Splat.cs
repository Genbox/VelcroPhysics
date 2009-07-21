using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace FarseerGames.AdvancedSamplesXNA.Demo9
{
    public class Splat
    {
        private Body _splatBody;
        private Texture2D _splatTexture;
        private Vertices _splatTextureVertices;
        private PhysicsSimulator _physicsSimulator;
        private Vector2 _position;

        public Splat(PhysicsSimulator physicsSimulator, Vector2 position)
        {
            _position = position;
            _physicsSimulator = physicsSimulator;
        }

        public void Load(ContentManager content)
        {
            _splatTexture = content.Load<Texture2D>("Content/Terrain");

            //Create an array to hold the data from the texture
            uint[] data = new uint[_splatTexture.Width * _splatTexture.Height];

            //Transfer the texture data to the array
            _splatTexture.GetData(data);

            // get a set of vertices from a texture
            PolygonCreationAssistance pca = new PolygonCreationAssistance(data, _splatTexture.Width, _splatTexture.Height);
            pca.HullTolerance = 6f;
            pca.HoleDetection = false;
            pca.MultipartDetection = false;
            pca.AlphaTolerance = 20;

            // extract those vertices into a Vertices structure
            _splatTextureVertices = Vertices.CreatePolygon(ref pca)[0];

            // create a body 
            _splatBody = BodyFactory.Instance.CreatePolygonBody(_physicsSimulator, _splatTextureVertices, 1);
            _splatBody.Position = _position;

            // use AutoDivide to find up to 25 convex geoms from a set of vertices
            GeomFactory.Instance.CreateSATPolygonGeom(_physicsSimulator, _splatBody, _splatTextureVertices, 25);
        }

        public void Draw(SpriteBatch sb)
        {
            sb.Draw(_splatTexture, _splatBody.Position, null, Color.White, _splatBody.Rotation, _splatTextureVertices.GetCentroid(), 1.0f, SpriteEffects.None, 0.5f);
        }
    }
}