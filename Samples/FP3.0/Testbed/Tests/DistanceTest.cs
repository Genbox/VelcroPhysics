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

using System;
using Box2D.XNA.TestBed.Framework;
using Box2D.XNA;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Box2D.XNA.TestBed.Tests
{
    public class DistanceTest : Test
    {
        DistanceTest()
	    {
		    {
			    _transformA.SetIdentity();
			    _transformA.Position = new Vector2(0.0f, -0.2f);
			    _polygonA.SetAsBox(10.0f, 0.2f);
		    }

		    {
			    _positionB = new Vector2(12.017401f, 0.13678508f);
			    _angleB = -0.0109265f;
			    _transformB.Set(_positionB, _angleB);

			    _polygonB.SetAsBox(2.0f, 0.1f);
		    }
	    }

	    internal static Test Create()
	    {
		    return new DistanceTest();
	    }

	    public override void Step(Framework.Settings settings)
	    {
		    base.Step(settings);

		    DistanceInput input = new DistanceInput();
            input.proxyA.Set(_polygonA);
            input.proxyB.Set(_polygonB);
		    input.transformA = _transformA;
		    input.transformB = _transformB;
		    input.useRadii = true;
		    SimplexCache cache = new SimplexCache();
		    cache.count = 0;
		    DistanceOutput output = new DistanceOutput();
		    Distance.ComputeDistance( out output, out cache, ref input);

            _debugDraw.DrawString(50, _textLine, "distance = {0:n}", output.distance);
		    _textLine += 15;

            _debugDraw.DrawString(50, _textLine, "iterations = {0:n}", output.iterations);
		    _textLine += 15;

		    {
			    Color color = new Color(0.9f, 0.9f, 0.9f);
                FixedArray8<Vector2> v = new FixedArray8<Vector2>();
			    for (int i = 0; i < _polygonA._vertexCount; ++i)
			    {
				    v[i] = MathUtils.Multiply(ref _transformA, _polygonA._vertices[i]);
			    }
			    _debugDraw.DrawPolygon(ref v, _polygonA._vertexCount, color);

			    for (int i = 0; i < _polygonB._vertexCount; ++i)
			    {
				    v[i] = MathUtils.Multiply(ref _transformB, _polygonB._vertices[i]);
			    }
			    _debugDraw.DrawPolygon(ref v, _polygonB._vertexCount, color);
		    }

		    Vector2 x1 = output.pointA;
		    Vector2 x2 = output.pointB;


            _debugDraw.DrawPoint(x1, 0.5f, new Color(1.0f, 0.0f, 0.0f));
            _debugDraw.DrawPoint(x2, 0.5f, new Color(1.0f, 0.0f, 0.0f));

            _debugDraw.DrawSegment(x1, x2, new Color(1.0f, 1.0f, 0.0f));
	    }

	    public override void Keyboard(KeyboardState state, KeyboardState oldState)
	    {
            if (state.IsKeyDown(Keys.A))
            {
                _positionB.X -= 0.1f;
            }
            if (state.IsKeyDown(Keys.D))
            {
                _positionB.X += 0.1f;
            }
            if (state.IsKeyDown(Keys.S))
            {
                _positionB.Y -= 0.1f;
            }
            if (state.IsKeyDown(Keys.W))
            {
                _positionB.Y += 0.1f;
            }
            if (state.IsKeyDown(Keys.Q))
            {
                _angleB += 0.1f * (float)Box2D.XNA.Settings.b2_pi;
            }
            if (state.IsKeyDown(Keys.E))
            {
                _angleB -= 0.1f * (float)Box2D.XNA.Settings.b2_pi;
            }

		    _transformB.Set(_positionB, _angleB);
	    }

        Vector2 _positionB = Vector2.Zero;
	    float _angleB;

        Transform _transformA = new Transform();
        Transform _transformB = new Transform();
	    PolygonShape _polygonA = new PolygonShape();
	    PolygonShape _polygonB = new PolygonShape();
    }
}
