// MIT License

// Copyright (c) 2019 Erin Catto

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using Genbox.VelcroPhysics.Collision.Distance;
using Genbox.VelcroPhysics.Collision.Shapes;
using Genbox.VelcroPhysics.Collision.TOI;
using Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Framework;
using Genbox.VelcroPhysics.Shared;
using Genbox.VelcroPhysics.Utilities;
using Microsoft.Xna.Framework;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Tests
{
    internal class TimeOfImpactTest : Test
    {
        private PolygonShape _shapeA = new PolygonShape(1.0f);
        private PolygonShape _shapeB = new PolygonShape(1.0f);

        private TimeOfImpactTest()
        {
            _shapeA.SetAsBox(25.0f, 5.0f);
            _shapeB.SetAsBox(2.5f, 2.5f);
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
            sweepB.A0 = 513.36676f; // - 162.0f * MathConstants.Pi;
            sweepB.C = new Vector2(54.595478f, -51.083473f);
            sweepB.A = 513.62781f; //  - 162.0f * MathConstants.Pi;
            sweepB.LocalCenter = Vector2.Zero;

            //sweepB.a0 -= 300.0f * MathConstants.Pi;
            //sweepB.a -= 300.0f * MathConstants.Pi;

            TOIInput input = new TOIInput();
            input.ProxyA = new DistanceProxy(_shapeA, 0);
            input.ProxyB = new DistanceProxy(_shapeB, 0);
            input.SweepA = sweepA;
            input.SweepB = sweepB;
            input.TMax = 1.0f;

            TOIOutput output;

            TimeOfImpact.CalculateTimeOfImpact(ref input, out output);

            DrawString("toi = " + output.T);
            DrawString($"max toi iters = {TimeOfImpact.TOIMaxIters}, max root iters = {TimeOfImpact.TOIMaxRootIters}");

            Vector2[] vertices = new Vector2[Settings.MaxPolygonVertices];

            DebugView.BeginCustomDraw(ref GameInstance.Projection, ref GameInstance.View);

            Transform transformA;
            sweepA.GetTransform(out transformA, 0.0f);
            for (int i = 0; i < _shapeA.Vertices.Count; ++i)
                vertices[i] = MathUtils.Mul(ref transformA, _shapeA.Vertices[i]);
            DebugView.DrawPolygon(vertices, _shapeA.Vertices.Count, new Color(0.9f, 0.9f, 0.9f));

            Transform transformB;
            sweepB.GetTransform(out transformB, 0.0f);

            //b2Vec2 localPoint(2.0f, -0.1f);

            for (int i = 0; i < _shapeB.Vertices.Count; ++i)
                vertices[i] = MathUtils.Mul(ref transformB, _shapeB.Vertices[i]);
            DebugView.DrawPolygon(vertices, _shapeB.Vertices.Count, new Color(0.5f, 0.9f, 0.5f));

            sweepB.GetTransform(out transformB, output.T);
            for (int i = 0; i < _shapeB.Vertices.Count; ++i)
                vertices[i] = MathUtils.Mul(ref transformB, _shapeB.Vertices[i]);
            DebugView.DrawPolygon(vertices, _shapeB.Vertices.Count, new Color(0.5f, 0.7f, 0.9f));

            sweepB.GetTransform(out transformB, 1.0f);
            for (int i = 0; i < _shapeB.Vertices.Count; ++i)
                vertices[i] = MathUtils.Mul(ref transformB, _shapeB.Vertices[i]);
            DebugView.DrawPolygon(vertices, _shapeB.Vertices.Count, new Color(0.9f, 0.5f, 0.5f));

            DebugView.EndCustomDraw();

#if false
		for (float t = 0.0f; t < 1.0f; t += 0.1f)
		{
			sweepB.GetTransform(&transformB, t);
			for (int i = 0; i < _shapeB.m_count; ++i)
			{
				vertices[i] = MathUtils.Mul(transformB, _shapeB.m_vertices[i]);
			}
			DebugView.DrawPolygon(vertices, _shapeB.m_count, b2Color(0.9f, 0.5f, 0.5f));
		}
#endif
        }
    }
}