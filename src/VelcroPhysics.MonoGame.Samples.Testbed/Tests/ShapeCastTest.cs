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
using Genbox.VelcroPhysics.Collision.Narrowphase;
using Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Framework;
using Genbox.VelcroPhysics.Shared;
using Genbox.VelcroPhysics.Utilities;
using Microsoft.Xna.Framework;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Tests
{
    internal class ShapeCastTest : Test
    {
        private readonly Vector2[] _vAs = new Vector2[Settings.MaxPolygonVertices];
        private readonly int _countA;
        private readonly float _radiusA;

        private readonly Vector2[] _vBs = new Vector2[Settings.MaxPolygonVertices];
        private readonly int _countB;
        private readonly float _radiusB;

        private Transform _transformA;
        private Transform _transformB;
        private readonly Vector2 _translationB;

        private ShapeCastTest()
        {
#if true
            _vAs[0] = new Vector2(-0.5f, 1.0f);
            _vAs[1] = new Vector2(0.5f, 1.0f);
            _vAs[2] = new Vector2(0.0f, 0.0f);
            _countA = 3;
            _radiusA = Settings.PolygonRadius;

            _vBs[0] = new Vector2(-0.5f, -0.5f);
            _vBs[1] = new Vector2(0.5f, -0.5f);
            _vBs[2] = new Vector2(0.5f, 0.5f);
            _vBs[3] = new Vector2(-0.5f, 0.5f);
            _countB = 4;
            _radiusB = Settings.PolygonRadius;

            _transformA.p = new Vector2(0.0f, 0.25f);
            _transformA.q.SetIdentity();
            _transformB.p = new Vector2(-4.0f, 0.0f);
            _transformB.q.SetIdentity();
            _translationB = new Vector2(8.0f, 0.0f);
#elif false
		_vAs[0].Set(0.0f, 0.0f);
		_countA = 1;
		_radiusA = 0.5f;

		_vBs[0].Set(0.0f, 0.0f);
		_countB = 1;
		_radiusB = 0.5f;

		_transformA.p = new Vector2(0.0f, 0.25f);
		_transformA.q.SetIdentity();
		_transformB.p = new Vector2(-4.0f, 0.0f);
		_transformB.q.SetIdentity();
		_translationB.Set(8.0f, 0.0f);
#else
		_vAs[0].Set(0.0f, 0.0f);
		_vAs[1].Set(2.0f, 0.0f);
		_countA = 2;
		_radiusA = b2_polygonRadius;

		_vBs[0].Set(0.0f, 0.0f);
		_countB = 1;
		_radiusB = 0.25f;

		// Initial overlap
		_transformA.p = new Vector2(0.0f, 0.0f);
		_transformA.q.SetIdentity();
		_transformB.p = new Vector2(-0.244360745f, 0.05999358f);
		_transformB.q.SetIdentity();
		_translationB.Set(0.0f, 0.0399999991f);
#endif
        }

        internal static Test Create()
        {
            return new ShapeCastTest();
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            base.Update(settings, gameTime);

            ShapeCastInput input = new ShapeCastInput();
            input.ProxyA = new DistanceProxy(_vAs, _radiusA);
            input.ProxyB = new DistanceProxy(_vBs, _radiusB);
            input.TransformA = _transformA;
            input.TransformB = _transformB;
            input.TranslationB = _translationB;

            ShapeCastOutput output;
            bool hit = DistanceGJK.ShapeCast(ref input, out output);

            Transform transformB2;
            transformB2.q = _transformB.q;
            transformB2.p = _transformB.p + output.Lambda * input.TranslationB;

            DistanceInput distanceInput = new DistanceInput();
            distanceInput.ProxyA = new DistanceProxy(_vAs, _radiusA);
            distanceInput.ProxyB = new DistanceProxy(_vBs, _radiusB);
            distanceInput.TransformA = _transformA;
            distanceInput.TransformB = transformB2;
            distanceInput.UseRadii = false;
            SimplexCache simplexCache;
            DistanceOutput distanceOutput;

            DistanceGJK.ComputeDistance(ref distanceInput, out distanceOutput, out simplexCache);

            DrawString($"hit = {(hit ? "true" : "false")}, iters = {output.Iterations}, lambda = {output.Lambda}, distance = {distanceOutput.Distance}");

            Vector2[] vertices = new Vector2[Settings.MaxPolygonVertices];

            for (int i = 0; i < _countA; ++i)
                vertices[i] = MathUtils.Mul(ref _transformA, _vAs[i]);

            DebugView.BeginCustomDraw(ref GameInstance.Projection, ref GameInstance.View);

            if (_countA == 1)
                DebugView.DrawCircle(vertices[0], _radiusA, new Color(0.9f, 0.9f, 0.9f));
            else
                DebugView.DrawPolygon(vertices, _countA, new Color(0.9f, 0.9f, 0.9f));

            for (int i = 0; i < _countB; ++i)
                vertices[i] = MathUtils.Mul(ref _transformB, _vBs[i]);

            if (_countB == 1)
                DebugView.DrawCircle(vertices[0], _radiusB, new Color(0.5f, 0.9f, 0.5f));
            else
                DebugView.DrawPolygon(vertices, _countB, new Color(0.5f, 0.9f, 0.5f));

            for (int i = 0; i < _countB; ++i)
                vertices[i] = MathUtils.Mul(ref transformB2, _vBs[i]);

            if (_countB == 1)
                DebugView.DrawCircle(vertices[0], _radiusB, new Color(0.5f, 0.7f, 0.9f));
            else
                DebugView.DrawPolygon(vertices, _countB, new Color(0.5f, 0.7f, 0.9f));

            if (hit)
            {
                Vector2 p1 = output.Point;
                DebugView.DrawPoint(p1, 10.0f, new Color(0.9f, 0.3f, 0.3f));
                Vector2 p2 = p1 + output.Normal;
                DebugView.DrawSegment(p1, p2, new Color(0.9f, 0.3f, 0.3f));
            }

            DebugView.EndCustomDraw();
        }
    }
}