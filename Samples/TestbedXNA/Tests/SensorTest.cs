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

using System;
using FarseerPhysics.Collision;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.TestBed.Framework;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.TestBed.Tests
{
    public class SensorTest : Test
    {
        private SensorTest()
        {
            Body body = BodyFactory.CreateRectangle(World, 1, 1, 1);
            body.BodyType = BodyType.Dynamic;
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            Vector2 min = -new Vector2(10);
            Vector2 max = new Vector2(10);

            AABB affected = new AABB(ref min, ref max);
            Fixture fix = null;
            World.QueryAABB(fixture =>
                                {
                                    fix = fixture;
                                    return true;
                                }, ref affected);


            //DebugView.BeginCustomDraw(ref GameInstance.Projection, ref GameInstance.View);
            //if (fix != null)
            //    DebugView.DrawPoint(fix.Body.Position, 1, Color.Red);

            //DebugView.DrawAABB(ref affected, Color.AliceBlue);
            //DebugView.EndCustomDraw();

            base.Update(settings, gameTime);
        }

        internal static Test Create()
        {
            return new SensorTest();
        }
    }
}