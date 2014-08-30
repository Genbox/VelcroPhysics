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

using FarseerPhysics.Collision;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Testbed.Framework;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Testbed.Tests
{
    public class TimeOfImpactTest : Test
    {
        private PolygonShape _shapeA = new PolygonShape(1);
        private PolygonShape _shapeB = new PolygonShape(1);

        private TimeOfImpactTest()
        {
            _shapeA.Vertices = PolygonTools.CreateRectangle(25.0f, 5.0f);
            _shapeB.Vertices = PolygonTools.CreateRectangle(2.5f, 2.5f);
        }

        internal static Test Create()
        {
            return new TimeOfImpactTest();
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            base.Update(settings, gameTime);

            Sweep sweepA = new Sweep();
            sweepA.C0 = new Vector2(24.0f, -60.0f);
            sweepA.A0 = 2.95f;
            sweepA.C = sweepA.C0;
            sweepA.A = sweepA.A0;
            sweepA.LocalCenter = Vector2.Zero;

            Sweep sweepB = new Sweep();
            sweepB.C0 = new Vector2(53.474274f, -50.252514f);
            sweepB.A0 = 513.36676f; // - 162.0f * b2_pi;
            sweepB.C = new Vector2(54.595478f, -51.083473f);
            sweepB.A = 513.62781f; //  - 162.0f * b2_pi;
            sweepB.LocalCenter = Vector2.Zero;

            //sweepB.a0 -= 300.0f * b2_pi;
            //sweepB.a -= 300.0f * b2_pi;

            TOIInput input = new TOIInput();
            input.ProxyA.Set(_shapeA, 0);
            input.ProxyB.Set(_shapeB, 0);
            input.SweepA = sweepA;
            input.SweepB = sweepB;
            input.TMax = 1.0f;

            TOIOutput output;
            TimeOfImpact.CalculateTimeOfImpact(out output, input);

            DrawString("TOI = " + output.T);
            DrawString(string.Format("Max TOI iters = {0:n}, Max root iters = {1:n}", TimeOfImpact.TOIMaxIters, TimeOfImpact.TOIMaxRootIters));

            Vector2[] vertices = new Vector2[Settings.MaxPolygonVertices];

            DebugView.BeginCustomDraw(ref GameInstance.Projection, ref GameInstance.View);
            Transform transformA;
            sweepA.GetTransform(out transformA, 0.0f);
            for (int i = 0; i < _shapeA.Vertices.Count; ++i)
            {
                vertices[i] = MathUtils.Mul(ref transformA, _shapeA.Vertices[i]);
            }
            DebugView.DrawPolygon(vertices, _shapeA.Vertices.Count, new Color(0.9f, 0.9f, 0.9f));

            Transform transformB;
            sweepB.GetTransform(out transformB, 0.0f);

            for (int i = 0; i < _shapeB.Vertices.Count; ++i)
            {
                vertices[i] = MathUtils.Mul(ref transformB, _shapeB.Vertices[i]);
            }
            DebugView.DrawPolygon(vertices, _shapeB.Vertices.Count, new Color(0.5f, 0.9f, 0.5f));

            sweepB.GetTransform(out transformB, output.T);
            for (int i = 0; i < _shapeB.Vertices.Count; ++i)
            {
                vertices[i] = MathUtils.Mul(ref transformB, _shapeB.Vertices[i]);
            }
            DebugView.DrawPolygon(vertices, _shapeB.Vertices.Count, new Color(0.5f, 0.7f, 0.9f));

            sweepB.GetTransform(out transformB, 1.0f);
            for (int i = 0; i < _shapeB.Vertices.Count; ++i)
            {
                vertices[i] = MathUtils.Mul(ref transformB, _shapeB.Vertices[i]);
            }
            DebugView.DrawPolygon(vertices, _shapeB.Vertices.Count, new Color(0.9f, 0.5f, 0.5f));
            DebugView.EndCustomDraw();
        }
    }
}