/*
  Box2DX Copyright (c) 2008 Ihar Kalasouski http://code.google.Com/p/box2dx
  Box2D original C++ version Copyright (c) 2006-2008 Erin Catto http://www.gphysics.Com

  This software is provided 'as-is', without any express or implied
  warranty.  In no event will the authors be held liable for any damages
  arising from the use of this software.

  Permission is granted to anyone to use this software for any purpose,
  including commercial applications, and to alter it and redistribute it
  freely, subject to the following restrictions:

  1. The origin of this software must not be misrepresented; you must not
     claim that you wrote the original software. If you use this software
     in a product, an acknowledgment in the product documentation would be
     appreciated but is not required.
  2. Altered source versions must be plainly marked as such, and must not be
     misrepresented as being the original software.
  3. This notice may not be removed or altered from any source distribution.
*/

using Box2DX.Common;
using Box2DX.Collision;
using Box2DX.Dynamics;

namespace TestBed
{
    public class TimeOfImpact : Test
    {
        PolygonShape _shapeA;
        PolygonShape _shapeB;

        public TimeOfImpact()
        {
            {
                _shapeA = new PolygonShape();
                _shapeA.SetAsBox(10.0f, 0.2f);
            }

            {
                _shapeB = new PolygonShape();
                _shapeB.SetAsBox(2.0f, 0.1f);
            }
        }

        public static Test Create()
        {
            return new TimeOfImpact();
        }

        public override void Step(Settings settings)
        {
            base.Step(settings);

            Sweep sweepA = new Sweep();
            sweepA.C0.Set(0.0f, -0.2f);
            sweepA.A0 = 0.0f;
            sweepA.C = sweepA.C0;
            sweepA.A = sweepA.A0;
            sweepA.T0 = 0.0f;
            sweepA.LocalCenter.SetZero();

            Sweep sweepB = new Sweep();
            sweepB.C0.Set(-0.076157160f, 0.16447277f);
            sweepB.A0 = -9.4497271f;
            sweepB.C.Set(-0.25650328f, -0.63657403f);
            sweepB.A = -9.0383911f;
            sweepB.T0 = 0.0f;
            sweepB.LocalCenter.SetZero();

            Collision.TOIInput input = new Collision.TOIInput();
            input.ProxyA.Set(_shapeA);
            input.ProxyB.Set(_shapeB);
            input.SweepA = sweepA;
            input.SweepB = sweepB;
            input.Tolerance = Box2DX.Common.Settings.LinearSlop;

            float toi = Collision.TimeOfImpact(input);

            OpenGLDebugDraw.DrawString(5, _textLine, string.Format("toi = {0}", toi));
            _textLine += 15;

#warning "get variables from TOI implementation"
            int _maxToiIters = 0, _maxToiRootIters = 0;
            OpenGLDebugDraw.DrawString(5, _textLine, string.Format("max toi iters = {1}, max root iters = {1}", _maxToiIters, _maxToiRootIters));
            _textLine += 15;

            Vec2[] vertices = new Vec2[Box2DX.Common.Settings.MaxPolygonVertices];

            Transform transformA;
            sweepA.GetTransform(out transformA, 0.0f);
            for (int i = 0; i < _shapeA.VertexCount; ++i)
            {
                vertices[i] = Math.Mul(transformA, _shapeA.Vertices[i]);
            }
            _debugDraw.DrawPolygon(vertices, _shapeA.VertexCount, new Color(0.9f, 0.9f, 0.9f));

            Transform transformB;
            sweepB.GetTransform(out transformB, 0.0f);
            for (int i = 0; i < _shapeB.VertexCount; ++i)
            {
                vertices[i] = Math.Mul(transformB, _shapeB.Vertices[i]);
            }
            _debugDraw.DrawPolygon(vertices, _shapeB.VertexCount, new Color(0.5f, 0.9f, 0.5f));

            sweepB.GetTransform(out transformB, toi);
            for (int i = 0; i < _shapeB.VertexCount; ++i)
            {
                vertices[i] = Math.Mul(transformB, _shapeB.Vertices[i]);
            }
            _debugDraw.DrawPolygon(vertices, _shapeB.VertexCount, new Color(0.5f, 0.7f, 0.9f));

            sweepB.GetTransform(out transformB, 1.0f);
            for (int i = 0; i < _shapeB.VertexCount; ++i)
            {
                vertices[i] = Math.Mul(transformB, _shapeB.Vertices[i]);
            }
            _debugDraw.DrawPolygon(vertices, _shapeB.VertexCount, new Color(0.9f, 0.5f, 0.5f));
        }
    }
}