/*
* Velcro Physics
* Copyright (c) 2021 Ian Qvist
* 
* Original source Box2D:
* Copyright (c) 2019 Erin Catto
*
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included in all
* copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*/

#define CaseOne

using Genbox.VelcroPhysics.Collision.Distance;
using Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Framework;
using Genbox.VelcroPhysics.Shared;
using Genbox.VelcroPhysics.Utilities;
using Microsoft.Xna.Framework;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Tests
{
    public class ShapeCastTest : Test
    {
        private readonly float _radiusA;
        private readonly float _radiusB;

        private Transform _transformA;
        private Transform _transformB;

        private readonly Vector2 _translationB;

        private readonly Vector2[] _vA;
        private readonly Vector2[] _vB;

        public ShapeCastTest()
        {
#if CaseOne
            _vA = new Vector2[3];
            _vA[0] = new Vector2(-0.5f, 1.0f);
            _vA[1] = new Vector2(0.5f, 1.0f);
            _vA[2] = new Vector2(0.0f, 0.0f);
            _radiusA = Settings.PolygonRadius;

            _vB = new Vector2[4];
            _vB[0] = new Vector2(-0.5f, -0.5f);
            _vB[1] = new Vector2(0.5f, -0.5f);
            _vB[2] = new Vector2(0.5f, 0.5f);
            _vB[3] = new Vector2(-0.5f, 0.5f);
            _radiusB = Settings.PolygonRadius;

            _transformA.p = new Vector2(0.0f, 0.25f);
            _transformA.q.SetIdentity();
            _transformB.p = new Vector2(-4.0f, 0.0f);
            _transformB.q.SetIdentity();
            _translationB = new Vector2(8.0f, 0.0f);
#elif CaseTwo

            _vA = new Vector2[1];
            _vA[0] = new Vector2(0.0f, 0.0f);
            _radiusA = 0.5f;

            _vB = new Vector2[1];
            _vB[0] = new Vector2(0.0f, 0.0f);
            _radiusB = 0.5f;

            _transformA.p = new Vector2(0.0f, 0.25f);
            _transformA.q.SetIdentity();
            _transformB.p = new Vector2(-4.0f, 0.0f);
            _transformB.q.SetIdentity();
            _translationB = new Vector2(8.0f, 0.0f);
#elif CaseTree
            _vA = new Vector2[2];
            _vA[0] = new Vector2(0.0f, 0.0f);
            _vA[1] = new Vector2(2.0f, 0.0f);
            _radiusA = Settings.PolygonRadius;

            _vB = new Vector2[1];
            _vB[0] = new Vector2(0.0f, 0.0f);
            _radiusB = 0.25f;

            // Initial overlap
            _transformA.p = new Vector2(0.0f, 0.0f);
            _transformA.q.SetIdentity();
            _transformB.p = new Vector2(-0.244360745f, 0.05999358f);
            _transformB.q.SetIdentity();
            _translationB = new Vector2(0.0f, 0.0399999991f);
#endif
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            base.Update(settings, gameTime);

            ShapeCastInput input = new ShapeCastInput();
            input.ProxyA = new DistanceProxy(_vA, _radiusA);
            input.ProxyB = new DistanceProxy(_vB, _radiusB);
            input.TransformA = _transformA;
            input.TransformB = _transformB;
            input.TranslationB = _translationB;

            bool hit = DistanceGJK.ShapeCast(ref input, out ShapeCastOutput output);

            Transform transformB2;
            transformB2.q = _transformB.q;
            transformB2.p = _transformB.p + output.Lambda * input.TranslationB;

            DistanceInput distanceInput = new DistanceInput();
            distanceInput.ProxyA = new DistanceProxy(_vA, _radiusA);
            distanceInput.ProxyB = new DistanceProxy(_vB, _radiusB);
            distanceInput.TransformA = _transformA;
            distanceInput.TransformB = transformB2;
            distanceInput.UseRadii = false;

            DistanceGJK.ComputeDistance(ref distanceInput, out DistanceOutput distanceOutput, out _);

            DebugView.BeginCustomDraw(ref GameInstance.Projection, ref GameInstance.View);
            DebugView.DrawString(5, TextLine, $"hit = {hit}, iters = {output.Iterations}, lambda = {output.Lambda}, distance = {distanceOutput.Distance}");
            TextLine += 30;

            Vector2[] vertices = new Vector2[Settings.MaxPolygonVertices];

            for (int i = 0; i < _vA.Length; ++i)
            {
                vertices[i] = MathUtils.Mul(ref _transformA, _vA[i]);
            }

            if (_vA.Length == 1)
                DebugView.DrawCircle(vertices[0], _radiusA, new Color(0.9f, 0.9f, 0.9f));
            else
                DebugView.DrawPolygon(vertices, _vA.Length, new Color(0.9f, 0.9f, 0.9f));

            for (int i = 0; i < _vB.Length; ++i)
            {
                vertices[i] = MathUtils.Mul(ref _transformB, _vB[i]);
            }

            if (_vB.Length == 1)
                DebugView.DrawCircle(vertices[0], _radiusB, new Color(0.5f, 0.9f, 0.5f));
            else
                DebugView.DrawPolygon(vertices, _vB.Length, new Color(0.5f, 0.9f, 0.5f));

            for (int i = 0; i < _vB.Length; ++i)
            {
                vertices[i] = MathUtils.Mul(ref transformB2, _vB[i]);
            }

            if (_vB.Length == 1)
                DebugView.DrawCircle(vertices[0], _radiusB, new Color(0.5f, 0.7f, 0.9f));
            else
                DebugView.DrawPolygon(vertices, _vB.Length, new Color(0.5f, 0.7f, 0.9f));

            if (hit)
            {
                Vector2 p1 = output.Point;
                DebugView.DrawPoint(p1, 0.05f, new Color(0.9f, 0.3f, 0.3f));
                Vector2 p2 = p1 + output.Normal;
                DebugView.DrawSegment(p1, p2, new Color(0.9f, 0.3f, 0.3f));
            }

            DebugView.EndCustomDraw();
        }

        public static Test Create()
        {
            return new ShapeCastTest();
        }
    }
}