using System.Windows.Media;
using System.Windows.Shapes;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Factories;
using FarseerGames.FarseerPhysics.Mathematics;
using SimpleSamplesWPF.Demos;

namespace SimpleSamplesWPF.SharedDemoObjects
{
    public class Border
    {
        private Body borderBody;
        private Geom[] borderGeom;
        private readonly float borderWidth;
        private readonly float height;
        private readonly float width;
        private Vector2 position;

        public Border(float width, float height, float borderWidth, Vector2 position)
        {
            this.width = width;
            this.height = height;
            this.borderWidth = borderWidth;
            this.position = position;
        }

        public void Load(Demo demo, PhysicsSimulator physicsSimulator)
        {
            //use the body factory to create the physics body
            borderBody = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, width, height, 1);
            borderBody.IsStatic = true;
            borderBody.Position = position;
            LoadBorderGeom(physicsSimulator);
            float left = (position.X - width / 2f);
            float top = (position.Y - height / 2f);
            float right = (position.X + width / 2f);
            float bottom = (position.Y + height / 2f);

            Rectangle leftBorder = demo.AddRectangleToCanvas(null, Color.FromArgb(128, 255, 255, 255),
                                                     new Vector2(borderWidth, height));
            Demo.PositionTopLeft(leftBorder, new Vector2(left, top));

            Rectangle rightBorder = demo.AddRectangleToCanvas(null, Color.FromArgb(128, 255, 255, 255),
                                                     new Vector2(borderWidth, height));
            Demo.PositionTopLeft(rightBorder, new Vector2(right-borderWidth, top));

            Rectangle topBorder = demo.AddRectangleToCanvas(null, Color.FromArgb(128, 255, 255, 255),
                                                     new Vector2(width, borderWidth));
            Demo.PositionTopLeft(topBorder, new Vector2(left, top));

            Rectangle bottomBorder = demo.AddRectangleToCanvas(null, Color.FromArgb(128, 255, 255, 255),
                                                     new Vector2(width, borderWidth));
            Demo.PositionTopLeft(bottomBorder, new Vector2(left, bottom - borderWidth));
        }

        public void LoadBorderGeom(PhysicsSimulator physicsSimulator)
        {
            borderGeom = new Geom[4];
            //left border
            Vector2 geometryOffset = new Vector2(-(width * .5f - borderWidth * .5f), 0);
            borderGeom[0] = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, borderBody, borderWidth,
                                                                     height,
                                                                     geometryOffset, 0);
            borderGeom[0].RestitutionCoefficient = .2f;
            borderGeom[0].FrictionCoefficient = .5f;
            borderGeom[0].CollisionGroup = 100;

            //right border (clone left border since geometry is same size)
            geometryOffset = new Vector2(width * .5f - borderWidth * .5f, 0);
            borderGeom[1] = GeomFactory.Instance.CreateGeom(physicsSimulator, borderBody, borderGeom[0],
                                                            geometryOffset,
                                                            0);


            //top border
            geometryOffset = new Vector2(0, -(height * .5f - borderWidth * .5f));
            borderGeom[2] = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, borderBody, width,
                                                                     borderWidth,
                                                                     geometryOffset, 0);
            borderGeom[2].RestitutionCoefficient = .2f;
            borderGeom[2].FrictionCoefficient = .2f;
            borderGeom[2].CollisionGroup = 100;

            //bottom border (clone top border since geometry is same size)
            geometryOffset = new Vector2(0, height * .5f - borderWidth * .5f);
            borderGeom[3] = GeomFactory.Instance.CreateGeom(physicsSimulator, borderBody, borderGeom[2],
                                                            geometryOffset,
                                                            0);
        }
    }
}