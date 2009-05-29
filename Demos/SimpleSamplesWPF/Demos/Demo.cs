using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Dynamics.Springs;
using FarseerGames.FarseerPhysics.Factories;
using FarseerGames.FarseerPhysics.Mathematics;
using System.Windows.Shapes;
using System.Windows.Input;
using SimpleSamplesWPF.SharedDemoObjects;
using System.Diagnostics;
using SimpleSamplesWPF.VisualUpdaters;
using Border=SimpleSamplesWPF.SharedDemoObjects.Border;

namespace SimpleSamplesWPF.Demos
{
    public abstract class Demo : Canvas
    {
        public event EventHandler QuitDemo;

        private bool isPaused;
        private GameLoop gameLoop;

        //can be changed by specialization
        protected PhysicsSimulator physicsSimulator;
        protected Body controlledBody;
        protected float forceAmount = 50;
        protected float torqueAmount = 1000;

        private FixedLinearSpring mousePickSpring;
        private FixedLinearSpringVisual mouseSpringVisual;
        private Geom pickedGeom;
        private Border border;

        protected Demo()
        {
            Loaded += OnLoaded;

            IsHitTestVisible = true;
        }

        //make every point succeed in a hit test
        protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
        {
            return new PointHitTestResult(this,hitTestParameters.HitPoint);
        }

        void OnLoaded(object sender, RoutedEventArgs e)
        {
            Initialize();

            //add border
            int borderWidth = (int)(ScreenHeight * .05f);
            border = new Border(ScreenWidth, ScreenHeight, borderWidth,
                                 ScreenCenter);
            border.Load(this, physicsSimulator);
        }

        public void SetGameLoop(GameLoop gameLoop)
        {
            if (this.gameLoop != null)
            {
                this.gameLoop.Update -= GameLoopOnUpdate;
            }

            this.gameLoop = gameLoop;

            if (this.gameLoop != null)
            {
                this.gameLoop.Update += GameLoopOnUpdate;
            }
        }

        public bool IsPaused
        {
            get { return isPaused; }
            set { isPaused = value; }
        }

        public float ScreenWidth
        {
            get
            {
                return (float)ActualWidth;
            }
        }

        public float ScreenHeight
        {
            get
            {
                return (float)ActualHeight;
            }
        }

        public Vector2 ScreenCenter
        {
            get
            {
                return new Vector2(ScreenWidth / 2, ScreenHeight / 2);
            }
        }

        private void GameLoopOnUpdate(TimeSpan span)
        {
            if (!IsVisible) return;
            if (IsPaused) return;

            if (physicsSimulator != null)
            {
                Update(span);
            }
        }

        protected virtual void Update(TimeSpan span)
        {
            HandleKeyboardInput();
            physicsSimulator.Update((float)span.TotalSeconds);
        }

        private void HandleKeyboardInput()
        {
            //handle keys at each update of gameloop

            if (Keyboard.IsKeyDown(Key.Escape))
            {
                if (QuitDemo != null)
                    QuitDemo(this, new EventArgs());
                return;
            }

            if (controlledBody == null) return;
            Vector2 force = Vector2.Zero;
            force.Y = -force.Y;
            if (Keyboard.IsKeyDown(Key.A))
            {
                force += new Vector2(-forceAmount, 0);
            }
            if (Keyboard.IsKeyDown(Key.S))
            {
                force += new Vector2(0, forceAmount);
            }
            if (Keyboard.IsKeyDown(Key.D))
            {
                force += new Vector2(forceAmount, 0);
            }
            if (Keyboard.IsKeyDown(Key.W))
            {
                force += new Vector2(0, -forceAmount);
            }

            controlledBody.ApplyForce(force);

            float torque = 0;
            if (Keyboard.IsKeyDown(Key.K))
            {
                torque -= torqueAmount;
            }
            if (Keyboard.IsKeyDown(Key.L))
            {
                torque += torqueAmount;
            }
            controlledBody.ApplyTorque(torque);
        }

        public abstract string Title { get; }

        public abstract string Details { get; }

        protected abstract void Initialize();

        #region add visuals to canvas

        public Rectangle AddRectangleToCanvas(Body body, Vector2 size)
        {
            return AddRectangleToCanvas(body, Colors.Yellow, size);
        }

        public Rectangle AddRectangleToCanvas(Body body, Color color, Vector2 size)
        {
            Rectangle rectangle = new Rectangle();
            rectangle.StrokeThickness = 2;
            rectangle.Stroke = Brushes.Black;
            rectangle.Fill = new SolidColorBrush(color);
            rectangle.Width = size.X;
            rectangle.Height = size.Y;

            AddVisualToCanvas(rectangle, body);

            return rectangle;
        }



        public Ellipse AddCircleToCanvas(Body body, float radius)
        {
            return AddEllipseToCanvas(body, Colors.White, radius, radius);
        }

        public Ellipse AddCircleToCanvas(Body body, Color color, float radius)
        {
            return AddEllipseToCanvas(body, color, radius, radius);
        }

        public Ellipse AddEllipseToCanvas(Body body, float radiusX, float radiusY)
        {
            return AddEllipseToCanvas(body, Colors.White, radiusX, radiusY);
        }

        public Ellipse AddEllipseToCanvas(Body body, Color color, float radiusX, float radiusY)
        {
            Ellipse ellipse = new Ellipse();
            ellipse.StrokeThickness = 2;
            ellipse.Stroke = Brushes.Black;
            ellipse.Fill = new SolidColorBrush(color);
            ellipse.Width = radiusX * 2;
            ellipse.Height = radiusY * 2;

            AddVisualToCanvas(ellipse, body);

            return ellipse;
        }

        public AgentVisual AddAgentToCanvas(Body body)
        {
            AgentVisual agent = new AgentVisual();

            AddVisualToCanvas(agent, body);

            return agent;
        }

        public FixedLinearSpringVisual AddFixedLinearSpringVisualToCanvas(FixedLinearSpring spring)
        {
            FixedLinearSpringVisual visual = new FixedLinearSpringVisual();

            if(spring != null)
            {
                new FixedLinearSpringVisualHelper(visual, spring);
            }

            AddVisualToCanvas(visual, null);

            return visual;
        }

        private void RemoveVisual(FrameworkElement visual)
        {
            Children.Remove(visual);
        }

        public void AddVisualToCanvas(FrameworkElement visual, Body body)
        {
            if (body != null)
            {
                new BodyVisualHelper(visual, body);
            }

            //hasn't been rendered yet, so needs to work out it's size
            if (visual.ActualWidth == 0 && visual.ActualHeight == 0)
            {
                Debug.Assert(ActualWidth != 0 && ActualHeight != 0);
                visual.Arrange(new Rect(0, 0, ActualWidth, ActualHeight));
            }

            Children.Add(visual);

            visual.IsHitTestVisible = true;
        }

        #endregion

        protected void ClearCanvas()
        {
            Children.Clear();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (mousePickSpring != null)
            {
                Vector2 point = new Vector2((float)(e.GetPosition(this).X), (float)(e.GetPosition(this).Y));
                mousePickSpring.WorldAttachPoint = point;
            }
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);

            if (mousePickSpring != null && mousePickSpring.IsDisposed == false)
            {
                mousePickSpring.Dispose();
                mousePickSpring = null;
                RemoveVisual(mouseSpringVisual);
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            Vector2 point = new Vector2((float)(e.GetPosition(this).X), (float)(e.GetPosition(this).Y));
            pickedGeom = physicsSimulator.Collide(point);
            if (pickedGeom != null)
            {
                mousePickSpring = SpringFactory.Instance.CreateFixedLinearSpring(physicsSimulator, pickedGeom.Body,
                                                                                 pickedGeom.Body.GetLocalPosition(
                                                                                     point), point, 20, 10);
                mouseSpringVisual = AddFixedLinearSpringVisualToCanvas(mousePickSpring);
            }
        }

        public static void CenterAround(FrameworkElement visual, Vector2 position)
        {
            SetLeft(visual, position.X - (visual.ActualWidth / 2));
            SetTop(visual, position.Y - (visual.ActualHeight / 2));
        }

        public static void PositionTopLeft(FrameworkElement visual, Vector2 position)
        {
            SetLeft(visual, position.X);
            SetTop(visual,position.Y);
        }

        public static void SetRotation(FrameworkElement visual, float radians)
        {
            visual.RenderTransform = new RotateTransform((radians * 360) / (2 * Math.PI), visual.ActualWidth / 2, visual.ActualHeight / 2);
        }
    }
}