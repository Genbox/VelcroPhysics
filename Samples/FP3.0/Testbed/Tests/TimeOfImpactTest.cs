/*
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

using FarseerPhysics.TestBed.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerPhysics.TestBed.Tests
{
    public class TimeOfImpactTest : Test
    {
        private TimeOfImpactTest()
        {
            {
                _shapeA.SetAsBox(10.0f, 0.2f);
            }

            {
                _shapeB.SetAsBox(2.0f, 0.1f);
            }
        }

        internal static Test Create()
        {
            return new TimeOfImpactTest();
        }

        public override void Step(Framework.Settings settings)
        {
            base.Step(settings);

            Sweep sweepA = new Sweep();
            sweepA.c0 = new Vector2(0.0f, -0.2f);
            sweepA.a0 = 0.0f;
            sweepA.c = sweepA.c0;
            sweepA.a = sweepA.a0;
            sweepA.t0 = 0.0f;
            sweepA.localCenter = Vector2.Zero;

            Sweep sweepB = new Sweep();
            sweepB.c0 = new Vector2(-0.076157160f, 0.16447277f);
            sweepB.a0 = -9.4497271f;
            sweepB.c = new Vector2(-0.25650328f, -0.63657403f);
            sweepB.a = -9.0383911f;
            sweepB.t0 = 0.0f;
            sweepB.localCenter = Vector2.Zero;

            TOIInput input = new TOIInput();
            input.proxyA.Set(_shapeA);
            input.proxyB.Set(_shapeB);
            input.sweepA = sweepA;
            input.sweepB = sweepB;
            input.tolerance = Settings.LinearSlop;

            float toi = TimeOfImpact.CalculateTimeOfImpact(ref input);

            _debugView.DrawString(50, _textLine, "toi = {0:n}", toi);
            _textLine += 15;

            _debugView.DrawString(50, _textLine, "max toi iters = {0:n}, max root iters = {1:n}", TimeOfImpact.ToiMaxIters,
                                  TimeOfImpact.ToiMaxRootIters);
            _textLine += 15;

            FixedArray8<Vector2> vertices = new FixedArray8<Vector2>();

            Transform transformA;
            sweepA.GetTransform(out transformA, 0.0f);
            for (int i = 0; i < _shapeA.Vertices.Count; ++i)
            {
                vertices[i] = MathUtils.Multiply(ref transformA, _shapeA.Vertices[i]);
            }
            _debugView.DrawPolygon(ref vertices, _shapeA.Vertices.Count, new Color(0.9f, 0.9f, 0.9f));

            Transform transformB;
            sweepB.GetTransform(out transformB, 0.0f);
            for (int i = 0; i < _shapeB.Vertices.Count; ++i)
            {
                vertices[i] = MathUtils.Multiply(ref transformB, _shapeB.Vertices[i]);
            }
            _debugView.DrawPolygon(ref vertices, _shapeB.Vertices.Count, new Color(0.5f, 0.9f, 0.5f));

            sweepB.GetTransform(out transformB, toi);
            for (int i = 0; i < _shapeB.Vertices.Count; ++i)
            {
                vertices[i] = MathUtils.Multiply(ref transformB, _shapeB.Vertices[i]);
            }
            _debugView.DrawPolygon(ref vertices, _shapeB.Vertices.Count, new Color(0.5f, 0.7f, 0.9f));

            sweepB.GetTransform(out transformB, 1.0f);
            for (int i = 0; i < _shapeB.Vertices.Count; ++i)
            {
                vertices[i] = MathUtils.Multiply(ref transformB, _shapeB.Vertices[i]);
            }
            _debugView.DrawPolygon(ref vertices, _shapeB.Vertices.Count, new Color(0.9f, 0.5f, 0.5f));
        }

        private PolygonShape _shapeA = new PolygonShape(0);
        private PolygonShape _shapeB = new PolygonShape(0);
    }
}