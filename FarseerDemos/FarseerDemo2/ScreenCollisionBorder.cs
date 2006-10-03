using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;

using FarseerGames.FarseerXNAPhysics;
using FarseerGames.FarseerXNAPhysics.Dynamics;

namespace FarseerDemo2 {
    public class ScreenCollisionBorder {
        private RectangleRigidBody leftBorder;
        private RectangleRigidBody rightBorder;
        private RectangleRigidBody topBorder;
        private RectangleRigidBody bottomBorder;
        private float borderThickness = 40;

        public ScreenCollisionBorder(float screenWidth, float screenHeight, PhysicsSimulator physicsSimulator ) {
            leftBorder = new RectangleRigidBody(borderThickness, screenHeight, 1);
            leftBorder.Position = new Vector2(-borderThickness/2,screenHeight/2);
            leftBorder.FrictionCoefficient = .3f;
            leftBorder.IsStatic = true;

            rightBorder = new RectangleRigidBody(borderThickness, screenHeight, 1);
            rightBorder.Position = new Vector2(screenWidth + borderThickness / 2, screenHeight / 2);
            rightBorder.FrictionCoefficient = .3f;
            rightBorder.IsStatic = true;

            topBorder = new RectangleRigidBody(screenWidth, borderThickness, 1);
            topBorder.Position = new Vector2(screenWidth / 2, -borderThickness / 2);
            topBorder.FrictionCoefficient = .3f;
            topBorder.IsStatic = true;

            bottomBorder = new RectangleRigidBody(screenWidth, borderThickness, 1);
            bottomBorder.Position = new Vector2(screenWidth / 2, screenHeight + borderThickness / 2);
            bottomBorder.FrictionCoefficient = .3f;
            bottomBorder.IsStatic = true;

            physicsSimulator.Add(leftBorder);
            physicsSimulator.Add(rightBorder);
            physicsSimulator.Add(topBorder);
            physicsSimulator.Add(bottomBorder);
        }
    }
}
