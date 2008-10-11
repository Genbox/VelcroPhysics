using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerGames.AdvancedSamples.Demos.Demo1
{
    internal class Pyramid
    {
        private Body[] _blockBody;
        private Geom[] _blockGeom;
        private float _blockHeight;
        private ObjectLinker[] _blockLink;
        private float _blockWidth;
        private Vector2 _bottomRightBlockPosition;
        private int _bottomRowBlockCount;
        private float _horizontalSpacing;
        private Body _referenceBody;
        private Geom _referenceGeom;
        private float _verticleSpacing;

        public Pyramid(Body referenceBody, Geom referenceGeom, float horizontalSpacing, float verticleSpacing,
                       float blockWidth, float blockHeight, int bottomRowBlockCount, Vector2 bottomRightBlockPosition)
        {
            _referenceBody = referenceBody;
            _referenceGeom = referenceGeom;
            _horizontalSpacing = horizontalSpacing;
            _verticleSpacing = verticleSpacing;
            _blockWidth = blockWidth;
            _blockHeight = blockHeight;
            _bottomRowBlockCount = bottomRowBlockCount;
            _bottomRightBlockPosition = bottomRightBlockPosition;
        }

        public void Load(PhysicsSimulator physicsSimulator, PhysicsProcessor physicsProcessor)
        {
            int count = _bottomRowBlockCount*(1 + _bottomRowBlockCount)/2;
            _blockBody = new Body[count];
            _blockGeom = new Geom[count];
            _blockLink = new ObjectLinker[count];

            for (int i = 0; i < _blockBody.Length; i++)
            {
                _blockBody[i] = BodyFactory.Instance.CreateBody(physicsSimulator, _referenceBody);
                _blockGeom[i] = GeomFactory.Instance.CreateGeom(physicsSimulator, _blockBody[i], _referenceGeom);
                // POINT OF INTEREST
                // Create the link for each body, and register it into the phyisics processor
                _blockLink[i] = new ObjectLinker(_blockBody[i]);
                physicsProcessor.AddLink(_blockLink[i]);
            }

            CreatePyramid();
        }

        private void CreatePyramid()
        {
            Vector2 rowOffset = new Vector2((_blockWidth/2) + (_horizontalSpacing/2), -(_blockHeight + _verticleSpacing));
            Vector2 colOffset = new Vector2(_horizontalSpacing + _blockWidth, 0);
            int blockCounter = 0;
            for (int i = 0; i < _bottomRowBlockCount; i++)
            {
                Vector2 position = _bottomRightBlockPosition + rowOffset*i;
                for (int j = 0; j < _bottomRowBlockCount - i; j++)
                {
                    Vector2 rowPosition = position + colOffset*j;
                    _blockBody[blockCounter].Position = rowPosition;
                    // POINT OF INTEREST
                    // Resync after the position has been set
                    _blockLink[blockCounter].Syncronize();
                    blockCounter += 1;
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D texture)
        {
            // POINT OF INTEREST
            // Now use the link's values for drawing
            for (int i = 0; i < _blockLink.Length; i++)
            {
                spriteBatch.Draw(texture, _blockLink[i].Position, null, Color.White, _blockLink[i].Rotation,
                                 new Vector2(texture.Width/2f, texture.Height/2f), 1, SpriteEffects.None, 0f);
            }
        }
    }
}