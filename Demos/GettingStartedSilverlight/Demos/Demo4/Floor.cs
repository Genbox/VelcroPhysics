using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using Media = System.Windows.Media;
using System.Windows.Media.Animation;
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
    public class Floor
    {
        Body floorBody;
        Geom floorGeom;

        int width;
        int height;
        Vector2 position;

        public Floor(int width, int height, Vector2 position)
        {
            this.width = width;
            this.height = height;
            this.position = position;
        }

        public void Load(SimulatorView view, PhysicsSimulator physicsSimulator)
        {
            //use the body factory to create the physics body
            floorBody = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, width, height, 1);
            view.AddRectangleToCanvas(floorBody, Media.Colors.White, new Vector2(width, height));
            floorBody.IsStatic = true;
            floorGeom = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, floorBody, width, height);
            floorGeom.RestitutionCoefficient = .4f;
            floorGeom.FrictionCoefficient = .4f;
            floorBody.Position = position;
        }
    }
}
