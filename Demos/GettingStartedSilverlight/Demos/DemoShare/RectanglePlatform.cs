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
using FarseerSilverlightDemos.Demos.DemoShare;

namespace FarseerSilverlightDemos.Demos.DemoShare
{
    public class RectanglePlatform
    {
        Body platformBody;
        Geom platformGeom;

        int width;
        int height;
        Vector2 position;
        Media.Color color;
        Media.Color borderColor;
        int collisionGroup;

        Vector2 platformOrigin;

        public RectanglePlatform(int width, int height, Vector2 position, Media.Color color, Media.Color borderColor, int collisionGroup)
        {
            this.width = width;
            this.height = height;
            this.position = position;
            this.color = color;
            this.borderColor = borderColor;
            this.collisionGroup = collisionGroup;
        }

        public void Load(SimulatorView view, PhysicsSimulator physicsSimulator)
        {

            //use the body factory to create the physics body
            platformBody = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, width, height, 1);
            view.AddRectangleToCanvas(platformBody, Media.Colors.White, new Vector2(width, height));
            platformBody.IsStatic = true;
            platformBody.Position = position;

            platformGeom = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, platformBody, width, height);
            platformGeom.CollisionGroup = 100;
            platformGeom.CollisionGroup = collisionGroup;
            platformGeom.FrictionCoefficient = 1;
        }
    }
}
