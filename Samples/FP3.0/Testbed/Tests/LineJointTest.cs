/*
* Farseer Physics Engine based on Box2D.XNA port:
* Copyright (c) 2010 Ian Qvist
* 
* Box2D.XNA port of Box2D:
* Copyright (c) 2009 Brandon Furtwangler, Nathan Furtwangler
*
* Original source Box2D:
* Copyright (c) 2006-2009 Erin Catto http://www.gphysics.com 
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
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;
using FarseerPhysics.TestBed.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FarseerPhysics.TestBed.Tests
{
    public class LineJointTest : Test
    {
        private FixedLineJoint _fixedLineJoint;
        private LineJoint _lineJoint;

        private LineJointTest()
        {
            Body ground;
            {
                ground = BodyFactory.CreateBody(World);

                Vertices edge = PolygonTools.CreateEdge(new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));
                PolygonShape shape = new PolygonShape(edge);
                ground.CreateFixture(shape);
            }

            //-------------------------
            // FixedLineJoint example
            //-------------------------
            {
                PolygonShape shape = new PolygonShape(PolygonTools.CreateRectangle(0.5f, 2.0f));
                Body body = BodyFactory.CreateBody(World);
                body.BodyType = BodyType.Dynamic;
                body.Position = new Vector2(0.0f, 7.0f);
                body.CreateFixture(shape, 1);

                Vector2 axis = new Vector2(2.0f, 1.0f);
                axis.Normalize();

                _fixedLineJoint = new FixedLineJoint(body, new Vector2(0.0f, 8.5f), axis);
                _fixedLineJoint.MotorSpeed = 100.0f;
                _fixedLineJoint.MaxMotorForce = 100.0f;
                _fixedLineJoint.MotorEnabled = false;
                _fixedLineJoint.LowerLimit = -4.0f;
                _fixedLineJoint.UpperLimit = 4.0f;
                _fixedLineJoint.EnableLimit = true;
                World.AddJoint(_fixedLineJoint);
            }

            //-------------------------
            // LineJoint example
            //-------------------------
            {
                PolygonShape shape = new PolygonShape(PolygonTools.CreateRectangle(0.5f, 2.0f));
                Body body = BodyFactory.CreateBody(World);
                body.BodyType = BodyType.Dynamic;
                body.Position = new Vector2(10.0f, 7.0f);
                body.CreateFixture(shape, 1);

                Vector2 axis = new Vector2(2.0f, 1.0f);
                axis.Normalize();

                Vector2 anchor = new Vector2(0.0f, 1.5f);
                _lineJoint = new LineJoint(ground, body, ground.GetLocalPoint(body.GetWorldPoint(anchor)), anchor, axis);
                _lineJoint.MotorSpeed = 100.0f;
                _lineJoint.MaxMotorForce = 100.0f;
                _lineJoint.MotorEnabled = false;
                _lineJoint.LowerLimit = -4.0f;
                _lineJoint.UpperLimit = 4.0f;
                _lineJoint.EnableLimit = true;
                World.AddJoint(_lineJoint);
            }
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            base.Update(settings, gameTime);
            DebugView.DrawString(50, TextLine, "Keys: (l) limits on/off, (m) motor on/off");
        }

        public override void Keyboard(KeyboardState state, KeyboardState oldState)
        {
            if (state.IsKeyDown(Keys.L) && oldState.IsKeyUp(Keys.L))
            {
                _lineJoint.EnableLimit = !_lineJoint.EnableLimit;
                _fixedLineJoint.EnableLimit = !_fixedLineJoint.EnableLimit;
            }

            if (state.IsKeyDown(Keys.M) && oldState.IsKeyUp(Keys.M))
            {
                _lineJoint.MotorEnabled = !_lineJoint.MotorEnabled;
                _fixedLineJoint.MotorEnabled = !_fixedLineJoint.MotorEnabled;
            }
        }

        internal static Test Create()
        {
            return new LineJointTest();
        }
    }
}