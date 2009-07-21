using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DemoBaseXNA.DemoShare
{
    public class Pyramid
    {
        private Body[] _blockBody;
        private Geom[] _blockGeom;
        private float _blockHeight;
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

        public Geom[] Geoms
        {
            get { return _blockGeom; }
        }

        public void Load(PhysicsSimulator physicsSimulator)
        {
            int count = _bottomRowBlockCount*(1 + _bottomRowBlockCount)/2;
            _blockBody = new Body[count];
            _blockGeom = new Geom[count];

            for (int i = 0; i < _blockBody.Length; i++)
            {
                _blockBody[i] = BodyFactory.Instance.CreateBody(physicsSimulator, _referenceBody);
                _blockGeom[i] = GeomFactory.Instance.CreateGeom(physicsSimulator, _blockBody[i], _referenceGeom);
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
                    blockCounter += 1;
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D texture)
        {
            for (int i = 0; i < _blockBody.Length; i++)
            {
                spriteBatch.Draw(texture, _blockBody[i].Position, null, Color.White, _blockBody[i].Rotation,
                                 new Vector2(texture.Width/2f, texture.Height/2f), 1, SpriteEffects.None, 0f);
            }
        }
    }
}