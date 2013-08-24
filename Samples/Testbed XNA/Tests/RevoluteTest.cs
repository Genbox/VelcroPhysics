/*
* Farseer Physics Engine:
* Copyright (c) 2012 Ian Qvist
* 
* Original source Box2D:
* Copyright (c) 2006-2011 Erin Catto http://www.box2d.org 
* 
* This software is provided 'as-is', without any express or implied 
* warranty.  In no event will the authors be held liable for any damages 
* arising from the use of this software. 
* Permission is granted to anyone to use this software for any purpose, 
* including commercial applications, and to alter it and redistribute it 
* freely, subject to the following restrictions: 
* 1. The origin of this software must not be misrepresented; you must not 
* claim that you wrote the original software. If you use this software 
* in a product, an acknowledgment in the product documentation would be 
* appreciated but is not required. 
* 2. Altered source versions must be plainly marked as such, and must not be 
* misrepresented as being the original software. 
* 3. This notice may not be removed or altered from any source distribution. 
*/

using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;
using FarseerPhysics.Testbed.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FarseerPhysics.Testbed.Tests
{
    public class RevoluteTest : Test
    {
        private RevoluteJoint _joint;

        private RevoluteTest()
        {
            //Ground
            Body ground = BodyFactory.CreateEdge(World, new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));

            {
                Body bodyB = BodyFactory.CreateCircle(World, 0.5f, 5f, new Vector2(-10.0f, 20.0f));
                bodyB.BodyType = BodyType.Dynamic;

                const float w = 100.0f;
                bodyB.AngularVelocity = w;
                bodyB.LinearVelocity = new Vector2(-8.0f * w, 0.0f);

                _joint = new RevoluteJoint(ground, bodyB, new Vector2(-10.0f, 12.0f), true);
                _joint.MotorSpeed = 1.0f * Settings.Pi;
                _joint.MaxMotorTorque = 10000.0f;
                _joint.MotorEnabled = false;
                _joint.LowerLimit = -0.25f * Settings.Pi;
                _joint.UpperLimit = 0.5f * Settings.Pi;
                _joint.LimitEnabled = true;
                _joint.CollideConnected = true;

                World.AddJoint(_joint);
            }

            {
                Body ball = BodyFactory.CreateCircle(World, 3.0f, 5.0f, new Vector2(5.0f, 30.0f));
                ball.BodyType = BodyType.Dynamic;
                ball.CollisionCategories = Category.Cat1;

                Vertices polygonVertices = PolygonTools.CreateRectangle(10.0f, 0.2f, new Vector2(-10.0f, 0.0f), 0.0f);

                Body polygonBody = BodyFactory.CreatePolygon(World, polygonVertices, 2, new Vector2(20, 10));
                polygonBody.BodyType = BodyType.Dynamic;
                polygonBody.IsBullet = true;

                RevoluteJoint joint = new RevoluteJoint(ground, polygonBody, new Vector2(20, 10), true);
                joint.LowerLimit = -0.25f * Settings.Pi;
                joint.UpperLimit = 0.0f * Settings.Pi;
                joint.LimitEnabled = true;

                World.AddJoint(joint);
            }

            // Tests mass computation of a small object far from the origin
            {
                Vertices verts = new Vertices(3);
                verts.Add(new Vector2(17.63f, 36.31f));
                verts.Add(new Vector2(17.52f, 36.69f));
                verts.Add(new Vector2(17.19f, 36.36f));

                Body polyShape = BodyFactory.CreatePolygon(World, verts, 1);
                polyShape.BodyType = BodyType.Dynamic;
            }
        }

        public override void Keyboard(KeyboardManager keyboardManager)
        {
            if (keyboardManager.IsNewKeyPress(Keys.L))
                _joint.LimitEnabled = !_joint.LimitEnabled;

            if (keyboardManager.IsNewKeyPress(Keys.M))
                _joint.MotorEnabled = !_joint.MotorEnabled;

            base.Keyboard(keyboardManager);
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            base.Update(settings, gameTime);
            DrawString("Keys: (l) limits on/off, (m) motor on/off");
        }

        internal static Test Create()
        {
            return new RevoluteTest();
        }
    }
}