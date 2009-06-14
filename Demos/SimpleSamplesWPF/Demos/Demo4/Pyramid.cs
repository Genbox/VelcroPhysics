using System.Windows.Media;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Factories;
using FarseerGames.FarseerPhysics.Mathematics;

namespace SimpleSamplesWPF.Demos.Demo4
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

        public void Load(Demo demo, PhysicsSimulator physicsSimulator)
        {
            int count = _bottomRowBlockCount * (1 + _bottomRowBlockCount) / 2;
            _blockBody = new Body[count];
            _blockGeom = new Geom[count];

            for (int i = 0; i < _blockBody.Length; i++)
            {
                _blockBody[i] = BodyFactory.Instance.CreateBody(physicsSimulator, _referenceBody);
                _blockGeom[i] = GeomFactory.Instance.CreateGeom(physicsSimulator, _blockBody[i], _referenceGeom);
                demo.AddRectangleToCanvas(_blockBody[i], Colors.White, new Vector2(32, 32));
            }

            CreatePyramid();
        }

        private void CreatePyramid()
        {
            Vector2 rowOffset = new Vector2((_blockWidth / 2) + (_horizontalSpacing / 2), -(_blockHeight + _verticleSpacing));
            Vector2 colOffset = new Vector2(_horizontalSpacing + _blockWidth, 0);
            int blockCounter = 0;

            for (int i = 0; i < _bottomRowBlockCount; i++)
            {
                Vector2 position = _bottomRightBlockPosition + rowOffset * i;
                for (int j = 0; j < _bottomRowBlockCount - i; j++)
                {
                    Vector2 rowPosition = position + colOffset * j;
                    _blockBody[blockCounter].Position = rowPosition;
                    blockCounter += 1;
                }
            }
        }
    }
}
