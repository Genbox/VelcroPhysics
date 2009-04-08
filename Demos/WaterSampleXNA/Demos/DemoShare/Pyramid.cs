using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerGames.WaterSampleXNA.Demos.DemoShare
{
    internal class Pyramid
    {
        public Body[] Bodies;
        public Geom[] Geoms;
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

        public void Load(PhysicsSimulator physicsSimulator)
        {
            int count = _bottomRowBlockCount*(1 + _bottomRowBlockCount)/2;
            Bodies = new Body[count];
            Geoms = new Geom[count];

            for (int i = 0; i < Bodies.Length; i++)
            {
                Bodies[i] = BodyFactory.Instance.CreateBody(physicsSimulator, _referenceBody);
                Geoms[i] = GeomFactory.Instance.CreateGeom(physicsSimulator, Bodies[i], _referenceGeom);
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
                    Bodies[blockCounter].Position = rowPosition;
                    blockCounter += 1;
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D texture)
        {
            for (int i = 0; i < Bodies.Length; i++)
            {
                spriteBatch.Draw(texture, Bodies[i].Position, null, Color.White, Bodies[i].Rotation,
                                 new Vector2(texture.Width/2f, texture.Height/2f), 1, SpriteEffects.None, 0f);
            }
        }
    }
}