using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using FarseerGames.FarseerPhysics.Dynamics.Springs;
using FarseerGames.FarseerPhysics.Factories;
using SWM = System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Drawing;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Mathematics;
using FarseerGames.FarseerPhysics.Collisions;
using System.Collections.Generic;
using FarseerSilverlightDemos.Drawing;

namespace FarseerSilverlightDemos
{
    public class SimulatorView : Canvas
    {
        protected List<IDrawingBrush> drawingList = new List<IDrawingBrush>();
        protected DemoMenu menu;
        protected PhysicsSimulator physicsSimulator;
        protected float forceAmount = 50;
        protected float torqueAmount = 1000;
        protected Body controlledBody = null;
        Canvas simulatorCanvas;
        public delegate void QuitEvent();
        public event QuitEvent Quit;
        double leftoverUpdateTime = 0;
        FixedLinearSpringBrush mouseSpringBrush;

        FixedLinearSpring mousePickSpring;
        Geom pickedGeom;

        public void ClearCanvas()
        {
            simulatorCanvas.Children.Clear();
        }

        public SimulatorView()
            : base()
        {
            simulatorCanvas = new Canvas();
            simulatorCanvas.Width = 1024;
            simulatorCanvas.Height = 768;
            this.Children.Add(simulatorCanvas);
            menu = new DemoMenu();
            menu.Title = Title;
            menu.Details = Details;
            this.Children.Add(menu);
            menu.SetValue(Canvas.ZIndexProperty, 1000);
            Page.gameLoop.Update += new GameLoop.UpdateDelegate(gameLoop_Update);
            simulatorCanvas.MouseLeftButtonDown += new MouseButtonEventHandler(SimulatorView_MouseLeftButtonDown);
            simulatorCanvas.MouseLeftButtonUp += new MouseButtonEventHandler(SimulatorView_MouseLeftButtonUp);
            simulatorCanvas.MouseMove += new MouseEventHandler(SimulatorView_MouseMove);
            simulatorCanvas.IsHitTestVisible = true;
            simulatorCanvas.Background = new SWM.SolidColorBrush(SWM.Color.FromArgb(255, 100, 149, 237));
        }

        void SimulatorView_MouseMove(object sender, MouseEventArgs e)
        {
            if (mousePickSpring != null)
            {
                Vector2 point = new Vector2((float)(e.GetPosition(this).X), (float)(e.GetPosition(this).Y));
                mousePickSpring.WorldAttachPoint = point;
            }
        }

        void SimulatorView_MouseLeftButtonUp(object sender, MouseEventArgs e)
        {
            if (mousePickSpring != null && mousePickSpring.IsDisposed == false)
            {
                mousePickSpring.Dispose();
                mousePickSpring = null;
                RemoveFixedLinearSpringBrush(mouseSpringBrush);
            }
        }

        void SimulatorView_MouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            Vector2 point = new Vector2((float)(e.GetPosition(this).X), (float)(e.GetPosition(this).Y));
            pickedGeom = physicsSimulator.Collide(point);
            if (pickedGeom != null)
            {
                mousePickSpring = ControllerFactory.Instance.CreateFixedLinearSpring(physicsSimulator, pickedGeom.Body, pickedGeom.Body.GetLocalPosition(point), point, 20, 10);
                mouseSpringBrush = AddFixedLinearSpringBrushToCanvas(mousePickSpring);
            }
        }

        public bool Visible
        {
            get
            {
                return Visibility == Visibility.Visible;
            }
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
            get
            {
                return menu.Visible;
            }
            set
            {
                menu.Visible = value;
            }
        }

        public CircleBrush AddCircleToCanvas(Body body, float radius)
        {
            CircleBrush circle = new CircleBrush();
            circle.Radius = radius;
            circle.Extender.Body = body;
            simulatorCanvas.Children.Add(circle);
            drawingList.Add(circle);
            return circle;
        }

        public CircleBrush AddCircleToCanvas(Body body, SWM.Color color, float radius)
        {
            CircleBrush circle = new CircleBrush();
            circle.Radius = radius;
            circle.Extender.Body = body;
            circle.Extender.Color = color;
            simulatorCanvas.Children.Add(circle);
            drawingList.Add(circle);
            return circle;
        }

        public RectangleBrush AddRectangleToCanvas(Body body, Vector2 size)
        {
            RectangleBrush rect = new RectangleBrush();
            rect.Extender.Body = body;
            rect.Size = size;
            simulatorCanvas.Children.Add(rect);
            drawingList.Add(rect);
            return rect;
        }

        public RectangleBrush AddRectangleToCanvas(Body body, SWM.Color color, Vector2 size)
        {
            RectangleBrush rect = new RectangleBrush();
            rect.Extender.Body = body;
            rect.Size = size;
            rect.Extender.Color = color;
            simulatorCanvas.Children.Add(rect);
            drawingList.Add(rect);
            return rect;
        }

        public AgentBrush AddAgentToCanvas(Body body)
        {
            AgentBrush agent = new AgentBrush();
            agent.Extender.Body = body;
            simulatorCanvas.Children.Add(agent);
            drawingList.Add(agent);
            return agent;
        }

        public FixedLinearSpringBrush AddFixedLinearSpringBrushToCanvas(FixedLinearSpring spring)
        {
            FixedLinearSpringBrush fls = new FixedLinearSpringBrush();
            fls.FixedLinearSpring = spring;
            simulatorCanvas.Children.Add(fls);
            drawingList.Add(fls);
            return fls;
        }

        public void RemoveFixedLinearSpringBrush(FixedLinearSpringBrush fls)
        {
            simulatorCanvas.Children.Remove(fls);
            drawingList.Remove(fls);
        }

        public virtual void Update(TimeSpan ElapsedTime)
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

        void gameLoop_Update(TimeSpan ElapsedTime)
        {
            if (!Visible) return;
            double secs = ElapsedTime.TotalSeconds + leftoverUpdateTime;
            while (secs > .01)
            {
                Update(ElapsedTime);
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
            leftoverUpdateTime = secs;
        }

        void HandleKeyboard()
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
                if (Page.KeyHandler.IsKeyPressed(Key.A)) { force += new Vector2(-forceAmount, 0); }
                if (Page.KeyHandler.IsKeyPressed(Key.S)) { force += new Vector2(0, forceAmount); }
                if (Page.KeyHandler.IsKeyPressed(Key.D)) { force += new Vector2(forceAmount, 0); }
                if (Page.KeyHandler.IsKeyPressed(Key.W)) { force += new Vector2(0, -forceAmount); }

                controlledBody.ApplyForce(force);

                float torque = 0;
                if (Page.KeyHandler.IsKeyPressed(Key.K)) { torque -= torqueAmount; }
                if (Page.KeyHandler.IsKeyPressed(Key.L)) { torque += torqueAmount; }
                controlledBody.ApplyTorque(torque);
            }
        }

        public void Dispose()
        {
            menu.Dispose();
            this.Visible = false;
        }

        public virtual string Title
        {
            get
            {
                return "Title";
            }
        }

        public virtual string Details
        {
            get
            {
                return "Details";
            }
        }
    }
}
