using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Dynamics.Springs;
using FarseerGames.FarseerPhysics.Factories;
using FarseerGames.FarseerPhysics.Mathematics;
using FarseerGames.SimpleSamplesSilverlight.Objects;
using FarseerGames.SimpleSamplesSilverlight.Screens;
using FarseerGames.SimpleSamplesSilverlight.Drawing;
using SWM = System.Windows.Media;

namespace FarseerGames.SimpleSamplesSilverlight
{
    public class SimulatorView : Canvas
    {
        #region Delegates

        public delegate void QuitEvent();

        #endregion

        private double _leftoverUpdateTime;

        private FixedLinearSpring _mousePickSpring;
        private FixedLinearSpringBrush _mouseSpringBrush;
        private Geom _pickedGeom;
        private Canvas _simulatorCanvas;
        protected Body controlledBody;

        protected List<IDrawingBrush> drawingList = new List<IDrawingBrush>();
        protected float forceAmount = 50;
        protected DemoMenu menu;
        protected PhysicsSimulator physicsSimulator;
        protected float torqueAmount = 1000;

        public SimulatorView()
        {
            _simulatorCanvas = new Canvas();
            _simulatorCanvas.Width = 1024;
            _simulatorCanvas.Height = 768;
            Children.Add(_simulatorCanvas);
            menu = new DemoMenu();
            menu.Title = Title;
            menu.Details = Details;
            Children.Add(menu);
            menu.SetValue(ZIndexProperty, 1000);
            Page.gameLoop.Update += gameLoop_Update;
            _simulatorCanvas.MouseLeftButtonDown += SimulatorView_MouseLeftButtonDown;
            _simulatorCanvas.MouseLeftButtonUp += SimulatorView_MouseLeftButtonUp;
            _simulatorCanvas.MouseMove += SimulatorView_MouseMove;
            _simulatorCanvas.IsHitTestVisible = true;
            _simulatorCanvas.Background = new SolidColorBrush(Color.FromArgb(255, 100, 149, 237));
        }

        public bool Visible
        {
            get { return Visibility == Visibility.Visible; }
            set
            {
                if (value)
                {
                    Visibility = Visibility.Visible;
                    menu.lastKey = Key.ENTER;
                }
                else
                {
                    Visibility = Visibility.Collapsed;
                }
            }
        }

        public bool MenuActive
        {
            get { return menu.Visible; }
            set { menu.Visible = value; }
        }

        public virtual string Title
        {
            get { return "Title"; }
        }

        public virtual string Details
        {
            get { return "Details"; }
        }

        public event QuitEvent Quit;

        public void ClearCanvas()
        {
            _simulatorCanvas.Children.Clear();
        }

        private void SimulatorView_MouseMove(object sender, MouseEventArgs e)
        {
            if (_mousePickSpring != null)
            {
                Vector2 point = new Vector2((float) (e.GetPosition(this).X), (float) (e.GetPosition(this).Y));
                _mousePickSpring.WorldAttachPoint = point;
            }
        }

        private void SimulatorView_MouseLeftButtonUp(object sender, MouseEventArgs e)
        {
            if (_mousePickSpring != null && _mousePickSpring.IsDisposed == false)
            {
                _mousePickSpring.Dispose();
                _mousePickSpring = null;
                RemoveFixedLinearSpringBrush(_mouseSpringBrush);
            }
        }

        private void SimulatorView_MouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            Vector2 point = new Vector2((float) (e.GetPosition(this).X), (float) (e.GetPosition(this).Y));
            _pickedGeom = physicsSimulator.Collide(point);
            if (_pickedGeom != null)
            {
                _mousePickSpring = SpringFactory.Instance.CreateFixedLinearSpring(physicsSimulator, _pickedGeom.Body,
                                                                                  _pickedGeom.Body.GetLocalPosition(
                                                                                      point), point, 20, 10);
                _mouseSpringBrush = AddFixedLinearSpringBrushToCanvas(_mousePickSpring);
            }
        }

        public CircleBrush AddCircleToCanvas(Body body, float radius)
        {
            CircleBrush circle = new CircleBrush();
            circle.Radius = radius;
            circle.Extender.Body = body;
            _simulatorCanvas.Children.Add(circle);
            drawingList.Add(circle);
            return circle;
        }

        public CircleBrush AddCircleToCanvas(Body body, Color color, float radius)
        {
            CircleBrush circle = new CircleBrush();
            circle.Radius = radius;
            circle.Extender.Body = body;
            circle.Extender.Color = color;
            _simulatorCanvas.Children.Add(circle);
            drawingList.Add(circle);
            return circle;
        }

        public RectangleBrush AddRectangleToCanvas(Body body, Vector2 size)
        {
            RectangleBrush rect = new RectangleBrush();
            rect.Extender.Body = body;
            rect.Size = size;
            _simulatorCanvas.Children.Add(rect);
            drawingList.Add(rect);
            return rect;
        }

        public RectangleBrush AddRectangleToCanvas(Body body, Color color, Vector2 size)
        {
            RectangleBrush rect = new RectangleBrush();
            rect.Extender.Body = body;
            rect.Size = size;
            rect.Extender.Color = color;
            _simulatorCanvas.Children.Add(rect);
            drawingList.Add(rect);
            return rect;
        }

        public AgentBrush AddAgentToCanvas(Body body)
        {
            AgentBrush agent = new AgentBrush();
            agent.Extender.Body = body;
            _simulatorCanvas.Children.Add(agent);
            drawingList.Add(agent);
            return agent;
        }

        public FixedLinearSpringBrush AddFixedLinearSpringBrushToCanvas(FixedLinearSpring spring)
        {
            FixedLinearSpringBrush fls = new FixedLinearSpringBrush();
            fls.FixedLinearSpring = spring;
            _simulatorCanvas.Children.Add(fls);
            drawingList.Add(fls);
            return fls;
        }

        public void RemoveFixedLinearSpringBrush(FixedLinearSpringBrush fls)
        {
            _simulatorCanvas.Children.Remove(fls);
            drawingList.Remove(fls);
        }

        public virtual void Update(TimeSpan elapsedTime)
        {
            HandleKeyboard();
        }

        public virtual void Initialize()
        {
            foreach (IDrawingBrush b in drawingList)
            {
                b.Update();
            }
        }

        private void gameLoop_Update(TimeSpan elapsedTime)
        {
            if (!Visible) return;
            double secs = elapsedTime.TotalSeconds + _leftoverUpdateTime;
            while (secs > .01)
            {
                Update(elapsedTime);
                if (MenuActive == false)
                {
                    physicsSimulator.Update(.01f);
                    foreach (IDrawingBrush b in drawingList)
                    {
                        b.Update();
                    }
                }
                secs -= .01;
            }
            _leftoverUpdateTime = secs;
        }

        private void HandleKeyboard()
        {
            if (!Visible) return;
            if (MenuActive)
            {
                menu.HandleKeyboard();
                if (menu.QuitSelected)
                {
                    if (Quit != null) Quit();
                }
            }
            else
            {
                if (Page.KeyHandler.IsKeyPressed(Key.ESCAPE))
                {
                    menu.lastKey = Key.ESCAPE;
                    MenuActive = true;
                    return;
                }
                if (controlledBody == null) return;
                Vector2 force = Vector2.Zero;
                force.Y = -force.Y;
                if (Page.KeyHandler.IsKeyPressed(Key.A))
                {
                    force += new Vector2(-forceAmount, 0);
                }
                if (Page.KeyHandler.IsKeyPressed(Key.S))
                {
                    force += new Vector2(0, forceAmount);
                }
                if (Page.KeyHandler.IsKeyPressed(Key.D))
                {
                    force += new Vector2(forceAmount, 0);
                }
                if (Page.KeyHandler.IsKeyPressed(Key.W))
                {
                    force += new Vector2(0, -forceAmount);
                }

                controlledBody.ApplyForce(force);

                float torque = 0;
                if (Page.KeyHandler.IsKeyPressed(Key.K))
                {
                    torque -= torqueAmount;
                }
                if (Page.KeyHandler.IsKeyPressed(Key.L))
                {
                    torque += torqueAmount;
                }
                controlledBody.ApplyTorque(torque);
            }
        }

        public void Dispose()
        {
            menu.Dispose();
            Visible = false;
        }
    }
}