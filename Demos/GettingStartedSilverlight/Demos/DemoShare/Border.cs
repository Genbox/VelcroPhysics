using System.Windows.Media;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Factories;
using FarseerGames.FarseerPhysics.Mathematics;
using FarseerSilverlightDemos.Drawing;

namespace FarseerSilverlightDemos.Demos.DemoShare
{
    public class Border
    {
        private Body borderBody;
        private Geom[] borderGeom;

        private int borderWidth;
        private int height;
        private Vector2 position;
        private int width;

        public Border(int width, int height, int borderWidth, Vector2 position)
        {
            this.width = width;
            this.height = height;
            this.borderWidth = borderWidth;
            this.position = position;
        }

        public void Load(SimulatorView view, PhysicsSimulator physicsSimulator)
        {
            //use the body factory to create the physics body
            borderBody = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, width, height, 1);
            borderBody.IsStatic = true;
            borderBody.Position = position;
            LoadBorderGeom(physicsSimulator);
            float left = (position.X - width/2);
            float top = (position.Y - height/2);
            float right = (position.X + width/2);
            float bottom = (position.Y + height/2);
            RectangleBrush r1 = view.AddRectangleToCanvas(null, Color.FromArgb(128, 255, 255, 255),
                                                          new Vector2(borderWidth*2, ScreenManager.ScreenHeight));
            r1.Extender.Position = new Vector2(left, position.Y);
            RectangleBrush r2 = view.AddRectangleToCanvas(null, Color.FromArgb(128, 255, 255, 255),
                                                          new Vector2(borderWidth*2, ScreenManager.ScreenHeight));
            r2.Extender.Position = new Vector2(right, position.Y);
            RectangleBrush r3 = view.AddRectangleToCanvas(null, Color.FromArgb(128, 255, 255, 255),
                                                          new Vector2(ScreenManager.ScreenWidth, borderWidth*2));
            r3.Extender.Position = new Vector2(position.X, top);
            RectangleBrush r4 = view.AddRectangleToCanvas(null, Color.FromArgb(128, 255, 255, 255),
                                                          new Vector2(ScreenManager.ScreenWidth, borderWidth*2));
            r4.Extender.Position = new Vector2(position.X, bottom);
        }

        public void LoadBorderGeom(PhysicsSimulator physicsSimulator)
        {
            Vector2 geometryOffset = Vector2.Zero;

            borderGeom = new Geom[4];
            //left border
            geometryOffset = new Vector2(-(width*.5f - borderWidth*.5f), 0);
            borderGeom[0] = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, borderBody, borderWidth, height,
                                                                     geometryOffset, 0);
            borderGeom[0].RestitutionCoefficient = .2f;
            borderGeom[0].FrictionCoefficient = .5f;
            borderGeom[0].CollisionGroup = 100;

            //right border (clone left border since geometry is same size)
            geometryOffset = new Vector2(width*.5f - borderWidth*.5f, 0);
            borderGeom[1] = GeomFactory.Instance.CreateGeom(physicsSimulator, borderBody, borderGeom[0], geometryOffset,
                                                            0);


            //top border
            geometryOffset = new Vector2(0, -(height*.5f - borderWidth*.5f));
            borderGeom[2] = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, borderBody, width, borderWidth,
                                                                     geometryOffset, 0);
            borderGeom[2].RestitutionCoefficient = .2f;
            borderGeom[2].FrictionCoefficient = .2f;
            borderGeom[2].CollisionGroup = 100;
            borderGeom[2].CollisionGridCellSize = 20;
            borderGeom[2].ComputeCollisionGrid();

            //bottom border (clone top border since geometry is same size)
            geometryOffset = new Vector2(0, height*.5f - borderWidth*.5f);
            borderGeom[3] = GeomFactory.Instance.CreateGeom(physicsSimulator, borderBody, borderGeom[2], geometryOffset,
                                                            0);
        }
    }
}