/*
* Farseer Physics Engine based on Box2D.XNA port:
* Copyright (c) 2010 Ian Qvist
* 
* Box2D.XNA port of Box2D:
* Copyright (c) 2009 Brandon Furtwangler, Nathan Furtwangler
*
* Original source Box2D:
* Copyright (c) 2006-2009 Erin Catto http://www.box2d.org 
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

using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;
using FarseerPhysics.TestBed.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

//TODO: Copy this test to a new test and make this the original revolute test from Box2D

namespace FarseerPhysics.TestBed.Tests
{
    public class RevoluteTest : Test
    {
        private FixedRevoluteJoint _fixedJoint;
        private RevoluteJoint _joint;

        private RevoluteTest()
        {
            //Ground
            var ground = BodyFactory.CreateEdge(World, new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));

			{
				//The big fixed wheel
				CircleShape shape = new CircleShape(5.0f, 5);

				Body body = BodyFactory.CreateBody(World);
				body.Position = new Vector2(-10.0f, 15.0f);
				body.BodyType = BodyType.Dynamic;

				body.CreateFixture(shape);

				_fixedJoint = new FixedRevoluteJoint(body, Vector2.Zero, body.Position);
				_fixedJoint.MotorSpeed = 0.25f * Settings.Pi;
				_fixedJoint.MaxMotorTorque = 5000.0f;
				_fixedJoint.MotorEnabled = true;
				World.AddJoint(_fixedJoint);

				// The small gear attached to the big one
				Body body1 = BodyFactory.CreateGear(World, 1.5f, 10, 0.1f, 1, 1);
				body1.Position = new Vector2(-10.0f, 12.0f);
				body1.BodyType = BodyType.Dynamic;

				_joint = new RevoluteJoint(body, body1, body.GetLocalPoint(body1.Position),
										   Vector2.Zero);
				_joint.MotorSpeed = 1.0f * Settings.Pi;
				_joint.MaxMotorTorque = 5000.0f;
				_joint.MotorEnabled = true;
				_joint.CollideConnected = false;

				World.AddJoint(_joint);

				// Box2d rev 167
				{
					CircleShape circle_shape = new CircleShape(3.0f, 5);
					var circleBody = BodyFactory.CreateBody(World);
					circleBody.Position = new Vector2(5.0f, 30.0f);
					circleBody.BodyType = BodyType.Dynamic;
					circleBody.CreateFixture(circle_shape);
					PolygonShape polygonShape = new PolygonShape(2.0f);
					polygonShape.SetAsBox(10.0f, 0.2f, new Vector2(-10.0f, 0.0f), 0.0f);
					var polygon_body = BodyFactory.CreateBody(World);
					polygon_body.Position = new Vector2(20.0f, 10.0f);
					polygon_body.BodyType = BodyType.Dynamic;
					polygon_body.IsBullet = true;
					polygon_body.CreateFixture(polygonShape);
					RevoluteJoint rjd = new RevoluteJoint(ground, polygon_body, new Vector2(20.0f, 10.0f));
					rjd.LowerLimit = -0.25f * Settings.Pi;
					rjd.UpperLimit = 0.0f;
					rjd.LimitEnabled = true;
					World.AddJoint(rjd);
				}
			}
        }

        public override void Keyboard(KeyboardManager keyboardManager)
        {
            if (keyboardManager.IsNewKeyPress(Keys.L))
            {
                _joint.LimitEnabled = !_joint.LimitEnabled;
                _fixedJoint.LimitEnabled = !_fixedJoint.LimitEnabled;
            }

            if (keyboardManager.IsNewKeyPress(Keys.M))
            {
                _joint.MotorEnabled = !_joint.MotorEnabled;
                _fixedJoint.MotorEnabled = !_fixedJoint.MotorEnabled;
            }

            base.Keyboard(keyboardManager);
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            base.Update(settings, gameTime);
            DebugView.DrawString(50, TextLine, "Keys: (l) limits on/off, (m) motor on/off");
        }

        internal static Test Create()
        {
            return new RevoluteTest();
        }
    }
}