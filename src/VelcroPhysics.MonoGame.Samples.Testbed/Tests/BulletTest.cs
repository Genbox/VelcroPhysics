/*
* Velcro Physics:
* Copyright (c) 2017 Ian Qvist
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

using Microsoft.Xna.Framework;
using VelcroPhysics.Collision.Distance;
using VelcroPhysics.Collision.TOI;
using VelcroPhysics.Dynamics;
using VelcroPhysics.Factories;
using VelcroPhysics.MonoGame.Samples.Testbed.Framework;

namespace VelcroPhysics.MonoGame.Samples.Testbed.Tests
{
    public class BulletTest : Test
    {
        private readonly Body _body;
        private readonly Body _bullet;
        private float _x;

        private BulletTest()
        {
            BodyFactory.CreateEdge(World, new Vector2(-10, 0), new Vector2(10, 0));
            BodyFactory.CreateRectangle(World, 0.4f, 2f, 0, new Vector2(0.5f, 1.0f));

            //Bar
            _body = BodyFactory.CreateRectangle(World, 4f, 0.2f, 1, new Vector2(0.5f, 1.0f));
            _body.Position = new Vector2(0, 4);
            _body.BodyType = BodyType.Dynamic;

            //Bullet
            _bullet = BodyFactory.CreateRectangle(World, 0.5f, 0.5f, 100);
            _bullet.IsBullet = true;
            _bullet.BodyType = BodyType.Dynamic;
            _x = 0.20352793f;
            _bullet.Position = new Vector2(_x, 10);

            _bullet.LinearVelocity = new Vector2(0, -50);
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            base.Update(settings, gameTime);

            if (DistanceGJK.GJKCalls > 0)
                DrawString($"GJK calls = {DistanceGJK.GJKCalls:n}, Ave GJK iters = {DistanceGJK.GJKIters / (float)DistanceGJK.GJKCalls:n}, Max GJK iters = {DistanceGJK.GJKMaxIters:n}");

            if (TimeOfImpact.TOICalls > 0)
            {
                DrawString($"TOI calls = {TimeOfImpact.TOICalls:n}, Ave TOI iters = {TimeOfImpact.TOIIters / (float)TimeOfImpact.TOICalls:n}, Max TOI iters = {TimeOfImpact.TOIMaxRootIters:n}");

                DrawString($"Ave TOI root iters = {TimeOfImpact.TOIRootIters / (float)TimeOfImpact.TOICalls:n}, Max TOI root iters = {TimeOfImpact.TOIMaxRootIters:n}");
            }

            if (StepCount % 60 == 0)
                Launch();
        }

        private void Launch()
        {
            _body.SetTransform(new Vector2(0.0f, 4.0f), 0.0f);
            _body.LinearVelocity = Vector2.Zero;
            _body.AngularVelocity = 0;

            _x = Rand.RandomFloat(-1.0f, 1.0f);
            _bullet.SetTransform(new Vector2(_x, 10.0f), 0.0f);
            _bullet.LinearVelocity = new Vector2(0.0f, -50.0f);
            _bullet.AngularVelocity = 0;

            DistanceGJK.GJKCalls = 0;
            DistanceGJK.GJKIters = 0;
            DistanceGJK.GJKMaxIters = 0;

            TimeOfImpact.TOICalls = 0;
            TimeOfImpact.TOIIters = 0;
            TimeOfImpact.TOIMaxIters = 0;
            TimeOfImpact.TOIRootIters = 0;
            TimeOfImpact.TOIMaxRootIters = 0;
        }

        internal static Test Create()
        {
            return new BulletTest();
        }
    }
}