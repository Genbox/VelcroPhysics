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
                _shapeA.SetAsBox(0.2f, 1.0f, new Vector2(0.5f, 1.0f), 0.0f);
            }

            {
                _shapeB.SetAsBox(2.0f, 0.1f);
            }
        }

        internal static Test Create()
        {
            return new TimeOfImpactTest();
        }

        public override void Update(Framework.Settings settings)
        {
            base.Update(settings);

            Sweep sweepA = new Sweep();
            sweepA.Center0 = Vector2.Zero;
            sweepA.Angle0 = 0.0f;
            sweepA.Center = sweepA.Center0;
            sweepA.Angle = sweepA.Angle0;
            sweepA.LocalCenter = Vector2.Zero;

            Sweep sweepB = new Sweep();
            sweepB.Center0 = new Vector2(-0.20382018f, 2.1368704f);
            sweepB.Angle0 = -3.1664171f;
            sweepB.Center = new Vector2(-0.26699525f, 2.3552670f);
            sweepB.LocalCenter = Vector2.Zero;

            TOIInput input = new TOIInput();
            input.ProxyA.Set(_shapeA);
            input.ProxyB.Set(_shapeB);
            input.SweepA = sweepA;
            input.SweepB = sweepB;
            input.TMax = 1.0f;

            TOIOutput output;
            TimeOfImpact.CalculateTimeOfImpact(out output, ref input);

            DebugView.DrawString(50, TextLine, "toi = {0:n}", output.t);
            TextLine += 15;

            DebugView.DrawString(50, TextLine, "max toi iters = {0:n}, max root iters = {1:n}", TimeOfImpact.ToiMaxIters,
                                  TimeOfImpact.ToiMaxRootIters);
            TextLine += 15;

            Vector2[] vertices = new Vector2[Settings.MaxPolygonVertices];

            Transform transformA;
            sweepA.GetTransform(out transformA, 0.0f);
            for (int i = 0; i < _shapeA.Vertices.Count; ++i)
            {
                vertices[i] = MathUtils.Multiply(ref transformA, _shapeA.Vertices[i]);
            }
            DebugView.DrawPolygon(ref vertices, _shapeA.Vertices.Count, new Color(0.9f, 0.9f, 0.9f));

            Transform transformB;
            sweepB.GetTransform(out transformB, 0.0f);

            Vector2 localPoint = new Vector2(2.0f, -0.1f);
            Vector2 rB = MathUtils.Multiply(ref transformB, localPoint) - sweepB.Center0;
            float wB = sweepB.Angle - sweepB.Angle0;
            Vector2 vB = sweepB.Center - sweepB.Center0;
            Vector2 v = vB + MathUtils.Cross(wB, rB); 

            for (int i = 0; i < _shapeB.Vertices.Count; ++i)
            {
                vertices[i] = MathUtils.Multiply(ref transformB, _shapeB.Vertices[i]);
            }
            DebugView.DrawPolygon(ref vertices, _shapeB.Vertices.Count, new Color(0.5f, 0.9f, 0.5f));

            sweepB.GetTransform(out transformB, output.t);
            for (int i = 0; i < _shapeB.Vertices.Count; ++i)
            {
                vertices[i] = MathUtils.Multiply(ref transformB, _shapeB.Vertices[i]);
            }
            DebugView.DrawPolygon(ref vertices, _shapeB.Vertices.Count, new Color(0.5f, 0.7f, 0.9f));

            sweepB.GetTransform(out transformB, 1.0f);
            for (int i = 0; i < _shapeB.Vertices.Count; ++i)
            {
                vertices[i] = MathUtils.Multiply(ref transformB, _shapeB.Vertices[i]);
            }
            DebugView.DrawPolygon(ref vertices, _shapeB.Vertices.Count, new Color(0.9f, 0.5f, 0.5f));
        }

        private PolygonShape _shapeA = new PolygonShape(0);
        private PolygonShape _shapeB = new PolygonShape(0);
    }
}