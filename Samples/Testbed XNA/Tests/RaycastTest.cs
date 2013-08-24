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
using System.Collections.Generic;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.Testbed.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FarseerPhysics.Testbed.Tests
{
    public class RayCastTest : Test
    {
        private const int MaxBodies = 256;
        private float _angle;
        private Body[] _bodies = new Body[MaxBodies];
        private int _bodyIndex;
        private CircleShape _circle;

        private RayCastMode _mode;
        private PolygonShape[] _polygons = new PolygonShape[4];


        private RayCastTest()
        {
            {
                Vertices vertices = new Vertices(3);
                vertices.Add(new Vector2(-0.5f, 0.0f));
                vertices.Add(new Vector2(0.5f, 0.0f));
                vertices.Add(new Vector2(0.0f, 1.5f));
                _polygons[0] = new PolygonShape(vertices, 1);
            }

            {
                Vertices vertices2 = new Vertices(3);
                vertices2.Add(new Vector2(-0.1f, 0.0f));
                vertices2.Add(new Vector2(0.1f, 0.0f));
                vertices2.Add(new Vector2(0.0f, 1.5f));
                _polygons[1] = new PolygonShape(vertices2, 1);
            }

            {
                const float w = 1.0f;
                float b = w / (2.0f + (float)Math.Sqrt(2.0));
                float s = (float)Math.Sqrt(2.0) * b;

                Vertices vertices3 = new Vertices(8);
                vertices3.Add(new Vector2(0.5f * s, 0.0f));
                vertices3.Add(new Vector2(0.5f * w, b));
                vertices3.Add(new Vector2(0.5f * w, b + s));
                vertices3.Add(new Vector2(0.5f * s, w));
                vertices3.Add(new Vector2(-0.5f * s, w));
                vertices3.Add(new Vector2(-0.5f * w, b + s));
                vertices3.Add(new Vector2(-0.5f * w, b));
                vertices3.Add(new Vector2(-0.5f * s, 0.0f));
                _polygons[2] = new PolygonShape(vertices3, 1);
            }

            {
                _polygons[3] = new PolygonShape(1);
                _polygons[3].Vertices = PolygonTools.CreateRectangle(0.5f, 0.5f);
            }

            {
                _circle = new CircleShape(0.5f, 1);
            }

            _bodyIndex = 0;

            _angle = 0.0f;
            _mode = RayCastMode.Closest;
        }

        public override void Keyboard(KeyboardManager keyboardManager)
        {
            if (keyboardManager.IsNewKeyPress(Keys.D1))
            {
                Create(0);
            }
            if (keyboardManager.IsNewKeyPress(Keys.D2))
            {
                Create(1);
            }
            if (keyboardManager.IsNewKeyPress(Keys.D3))
            {
                Create(2);
            }
            if (keyboardManager.IsNewKeyPress(Keys.D4))
            {
                Create(3);
            }
            if (keyboardManager.IsNewKeyPress(Keys.D5))
            {
                Create(4);
            }
            if (keyboardManager.IsNewKeyPress(Keys.D))
            {
                DestroyBody();
            }
            if (keyboardManager.IsNewKeyPress(Keys.M))
            {
                switch (_mode)
                {
                    case RayCastMode.Closest:
                        _mode = RayCastMode.Any;
                        break;
                    case RayCastMode.Any:
                        _mode = RayCastMode.Multiple;
                        break;
                    case RayCastMode.Multiple:
                        _mode = RayCastMode.Closest;
                        break;
                    default:
                        break;
                }
            }
        }

        private void DestroyBody()
        {
            for (int i = 0; i < MaxBodies; ++i)
            {
                if (_bodies[i] != null)
                {
                    World.RemoveBody(_bodies[i]);
                    _bodies[i] = null;
                    return;
                }
            }
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            bool advanceRay = settings.Pause == false || settings.SingleStep;

            base.Update(settings, gameTime);

            DrawString("Press 1-5 to drop stuff, m to change the mode");
            DrawString(string.Format("Mode = {0}", _mode));

            const float l = 11.0f;
            Vector2 point1 = new Vector2(0.0f, 10.0f);
            Vector2 d = new Vector2(l * (float)Math.Cos(_angle), l * (float)Math.Sin(_angle));
            Vector2 point2 = point1 + d;

            Vector2 point = Vector2.Zero, normal = Vector2.Zero;

            switch (_mode)
            {
                case RayCastMode.Closest:
                    bool hitClosest = false;
                    World.RayCast((f, p, n, fr) =>
                                      {
                                          Body body = f.Body;
                                          if (body.UserData != null)
                                          {
                                              int index = (int)body.UserData;
                                              if (index == 0)
                                              {
                                                  // filter
                                                  return -1.0f;
                                              }
                                          }

                                          hitClosest = true;
                                          point = p;
                                          normal = n;
                                          return fr;
                                      }, point1, point2);

                    if (hitClosest)
                    {
                        DebugView.BeginCustomDraw(ref GameInstance.Projection, ref GameInstance.View);
                        DebugView.DrawPoint(point, .5f, new Color(0.4f, 0.9f, 0.4f));

                        DebugView.DrawSegment(point1, point, new Color(0.8f, 0.8f, 0.8f));

                        Vector2 head = point + 0.5f * normal;
                        DebugView.DrawSegment(point, head, new Color(0.9f, 0.9f, 0.4f));
                        DebugView.EndCustomDraw();
                    }
                    else
                    {
                        DebugView.BeginCustomDraw(ref GameInstance.Projection, ref GameInstance.View);
                        DebugView.DrawSegment(point1, point2, new Color(0.8f, 0.8f, 0.8f));
                        DebugView.EndCustomDraw();
                    }

                    break;
                case RayCastMode.Any:
                    bool hitAny = false;
                    World.RayCast((f, p, n, fr) =>
                                      {
                                          Body body = f.Body;
                                          if (body.UserData != null)
                                          {
                                              int index = (int)body.UserData;
                                              if (index == 0)
                                              {
                                                  // filter
                                                  return -1.0f;
                                              }
                                          }

                                          hitAny = true;
                                          point = p;
                                          normal = n;
                                          return 0;
                                      }, point1, point2);

                    if (hitAny)
                    {
                        DebugView.BeginCustomDraw(ref GameInstance.Projection, ref GameInstance.View);
                        DebugView.DrawPoint(point, .5f, new Color(0.4f, 0.9f, 0.4f));

                        DebugView.DrawSegment(point1, point, new Color(0.8f, 0.8f, 0.8f));

                        Vector2 head = point + 0.5f * normal;
                        DebugView.DrawSegment(point, head, new Color(0.9f, 0.9f, 0.4f));
                        DebugView.EndCustomDraw();
                    }
                    else
                    {
                        DebugView.BeginCustomDraw(ref GameInstance.Projection, ref GameInstance.View);
                        DebugView.DrawSegment(point1, point2, new Color(0.8f, 0.8f, 0.8f));
                        DebugView.EndCustomDraw();
                    }
                    break;
                case RayCastMode.Multiple:
                    List<Vector2> points = new List<Vector2>();
                    List<Vector2> normals = new List<Vector2>();
                    World.RayCast((f, p, n, fr) =>
                                      {
                                          Body body = f.Body;
                                          if (body.UserData != null)
                                          {
                                              int index = (int)body.UserData;
                                              if (index == 0)
                                              {
                                                  // filter
                                                  return -1.0f;
                                              }
                                          }

                                          points.Add(p);
                                          normals.Add(n);
                                          return 1.0f;
                                      }, point1, point2);

                    DebugView.BeginCustomDraw(ref GameInstance.Projection, ref GameInstance.View);
                    DebugView.DrawSegment(point1, point2, new Color(0.8f, 0.8f, 0.8f));

                    for (int i = 0; i < points.Count; i++)
                    {
                        DebugView.DrawPoint(points[i], .5f, new Color(0.4f, 0.9f, 0.4f));

                        DebugView.DrawSegment(point1, points[i], new Color(0.8f, 0.8f, 0.8f));

                        Vector2 head = points[i] + 0.5f * normals[i];
                        DebugView.DrawSegment(points[i], head, new Color(0.9f, 0.9f, 0.4f));
                    }

                    DebugView.EndCustomDraw();
                    break;
            }

            if (advanceRay)
            {
                _angle += 0.25f * Settings.Pi / 180.0f;
            }
        }

        private void Create(int index)
        {
            if (_bodies[_bodyIndex] != null)
            {
                World.RemoveBody(_bodies[_bodyIndex]);
                _bodies[_bodyIndex] = null;
            }

            float x = Rand.RandomFloat(-10.0f, 10.0f);
            float y = Rand.RandomFloat(0.0f, 20.0f);

            _bodies[_bodyIndex] = BodyFactory.CreateBody(World);

            _bodies[_bodyIndex].Position = new Vector2(x, y);
            _bodies[_bodyIndex].Rotation = Rand.RandomFloat(-Settings.Pi, Settings.Pi);
            _bodies[_bodyIndex].UserData = index;

            if (index == 4)
            {
                _bodies[_bodyIndex].AngularDamping = 0.02f;
            }

            if (index < 4)
            {
                Fixture fixture = _bodies[_bodyIndex].CreateFixture(_polygons[index]);
                fixture.Friction = 0.3f;
            }
            else
            {
                Fixture fixture = _bodies[_bodyIndex].CreateFixture(_circle);
                fixture.Friction = 0.3f;
            }

            _bodyIndex = (_bodyIndex + 1) % MaxBodies;
        }

        internal static Test Create()
        {
            return new RayCastTest();
        }

        #region Nested type: RayCastMode

        private enum RayCastMode
        {
            Closest,
            Any,
            Multiple,
        }

        #endregion
    }
}