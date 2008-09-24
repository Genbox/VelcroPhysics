using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Media = System.Windows.Media;
using System.Windows.Shapes;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Drawing;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Mathematics;
using System.Collections.Generic;
using FarseerSilverlightDemos.Drawing;

namespace FarseerSilverlightDemos.Demos.Demo4
{
    public class Pyramid
    {
        Body referenceBody;
        Geom referenceGeom;

        Body[] blockBody;
        Geom[] blockGeom;

        private float horizontalSpacing;
        private float verticleSpacing;
        private float blockWidth;
        private float blockHeight;
        private int bottomRowBlockCount;
        private Vector2 bottomRightBlockPosition;


        public Pyramid(Body referenceBody, Geom referenceGeom, float horizontalSpacing, float verticleSpacing, float blockWidth, float blockHeight, int bottomRowBlockCount, Vector2 bottomRightBlockPosition)
        {
            this.referenceBody = referenceBody;
            this.referenceGeom = referenceGeom;
            this.horizontalSpacing = horizontalSpacing;
            this.verticleSpacing = verticleSpacing;
            this.blockWidth = blockWidth;
            this.blockHeight = blockHeight;
            this.bottomRowBlockCount = bottomRowBlockCount;
            this.bottomRightBlockPosition = bottomRightBlockPosition;
        }

        public void Load(SimulatorView view, PhysicsSimulator physicsSimulator)
        {
            int count = bottomRowBlockCount * (1 + bottomRowBlockCount) / 2;
            blockBody = new Body[count];
            blockGeom = new Geom[count];

            for (int i = 0; i < blockBody.Length; i++)
            {
                blockBody[i] = BodyFactory.Instance.CreateBody(physicsSimulator, referenceBody);
                blockGeom[i] = GeomFactory.Instance.CreateGeom(physicsSimulator, blockBody[i], referenceGeom);
                view.AddRectangleToCanvas(blockBody[i], Media.Colors.White, new Vector2(32, 32));
            }

            CreatePyramid();
        }

        private void CreatePyramid()
        {
            Vector2 rowOffset = new Vector2((blockWidth / 2) + (horizontalSpacing / 2), -(blockHeight + verticleSpacing));
            Vector2 colOffset = new Vector2(horizontalSpacing + blockWidth, 0);
            Vector2 position = Vector2.Zero;
            int blockCounter = 0;
            for (int i = 0; i < bottomRowBlockCount; i++)
            {
                position = bottomRightBlockPosition + rowOffset * i;
                for (int j = 0; j < bottomRowBlockCount - i; j++)
                {
                    Vector2 rowPosition = position + colOffset * j;
                    blockBody[blockCounter].Position = rowPosition;
                    blockCounter += 1;
                }
            }
        }

    }
}
