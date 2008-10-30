using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Dynamics.Springs;
using FarseerGames.FarseerPhysics.Factories;
using FarseerGames.FarseerPhysics.Mathematics;
using FarseerSilverlightManual.Drawing;
using FarseerSilverlightManual.Objects;
using Border = FarseerSilverlightManual.Demos.DemoShare.Border;
using SWM = System.Windows.Media;

namespace FarseerSilverlightManual.Screens
{
    public class SimulatorView : Canvas
    {
        private Border _border;
        private double _leftoverUpdateTime;
        private FixedLinearSpring _mousePickSpring;
        private FixedLinearSpringBrush _mouseSpringBrush;
        private Geom _pickedGeom;
        protected List<IDrawingBrush> drawingList = new List<IDrawingBrush>();
        protected PhysicsSimulator physicsSimulator;

        public SimulatorView()
        {
            physicsSimulator = new PhysicsSimulator(new Vector2(0, 150));
            Width = 1024;
            Height = 768;
            Page.gameLoop.Update += GameLoop_Update;
            MouseLeftButtonDown += SimulatorView_MouseLeftButtonDown;
            MouseLeftButtonUp += SimulatorView_MouseLeftButtonUp;
            MouseMove += SimulatorView_MouseMove;
            IsHitTestVisible = true;
            Background = new SolidColorBrush(Color.FromArgb(255, 100, 149, 237));
            int borderWidth = (int)(ScreenManager.ScreenHeight * .05f);
            _border = new Border(ScreenManager.ScreenWidth + borderWidth * 2, ScreenManager.ScreenHeight + borderWidth * 2,
                                 borderWidth, ScreenManager.ScreenCenter);
            _border.Load(this, physicsSimulator);
        }

        private void SimulatorView_MouseMove(object sender, MouseEventArgs e)
        {
            if (_mousePickSpring == null)
                return;

            Vector2 point = new Vector2((float)(e.GetPosition(this).X), (float)(e.GetPosition(this).Y));
            _mousePickSpring.WorldAttachPoint = point;
        }

        private void SimulatorView_MouseLeftButtonUp(object sender, MouseEventArgs e)
        {
            if (_mousePickSpring == null || _mousePickSpring.IsDisposed)
                return;

            _mousePickSpring.Dispose();
            _mousePickSpring = null;
            RemoveFixedLinearSpringBrush(_mouseSpringBrush);
        }

        private void SimulatorView_MouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            Vector2 point = new Vector2((float)(e.GetPosition(this).X), (float)(e.GetPosition(this).Y));
            _pickedGeom = physicsSimulator.Collide(point);

            if (_pickedGeom == null)
                return;

            //Remove it from canvas if it already exist. This is to prevent a bork with mouse events.
            RemoveFixedLinearSpringBrush(_mouseSpringBrush);

            if (_mousePickSpring != null && !_mousePickSpring.IsDisposed)
            {
                _mousePickSpring.Dispose();
                _mousePickSpring = null;
            }

            _mousePickSpring = SpringFactory.Instance.CreateFixedLinearSpring(physicsSimulator, _pickedGeom.Body,
                                                                              _pickedGeom.Body.GetLocalPosition(
                                                                                  point), point, 20, 10);
            _mouseSpringBrush = AddFixedLinearSpringBrushToCanvas(_mousePickSpring);
        }

        public CircleBrush AddCircleToCanvas(Body body, float radius)
        {
            CircleBrush circle = new CircleBrush();
            circle.Radius = radius;
            circle.Extender.Body = body;
            Children.Add(circle);
            drawingList.Add(circle);
            return circle;
        }

        public CircleBrush AddCircleToCanvas(Body body, Color color, float radius)
        {
            CircleBrush circle = new CircleBrush();
            circle.Radius = radius;
            circle.Extender.Body = body;
            circle.Extender.Color = color;
            Children.Add(circle);
            drawingList.Add(circle);
            return circle;
        }

        public CircleBrush AddCircleToCanvas(Color color, float radius)
        {
            CircleBrush circle = new CircleBrush();
            circle.Radius = radius;
            circle.Extender.Color = color;
            Children.Add(circle);
            return circle;
        }

        public RectangleBrush AddRectangleToCanvas(Body body, Vector2 size)
        {
            RectangleBrush rect = new RectangleBrush();
            rect.Extender.Body = body;
            rect.Size = size;
            Children.Add(rect);
            drawingList.Add(rect);
            return rect;
        }

        public RectangleBrush AddRectangleToCanvas(Body body, Color color, Vector2 size)
        {
            RectangleBrush rect = new RectangleBrush();
            rect.Extender.Body = body;
            rect.Size = size;
            rect.Extender.Color = color;
            Children.Add(rect);
            drawingList.Add(rect);
            return rect;
        }

        public AgentBrush AddAgentToCanvas(Body body)
        {
            AgentBrush agent = new AgentBrush();
            agent.Extender.Body = body;
            Children.Add(agent);
            drawingList.Add(agent);
            return agent;
        }

        public FixedLinearSpringBrush AddFixedLinearSpringBrushToCanvas(FixedLinearSpring spring)
        {
            FixedLinearSpringBrush fls = new FixedLinearSpringBrush();
            fls.FixedLinearSpring = spring;
            Children.Add(fls);
            drawingList.Add(fls);
            return fls;
        }

        public LinearSpringBrush AddLinearSpringBrushToCanvas(LinearSpring spring)
        {
            LinearSpringBrush fls = new LinearSpringBrush();
            fls.LinearSpring = spring;
            Children.Add(fls);
            drawingList.Add(fls);
            return fls;
        }

        public void RemoveFixedLinearSpringBrush(FixedLinearSpringBrush fls)
        {
            Children.Remove(fls);
            drawingList.Remove(fls);
        }

        public virtual void Update(TimeSpan elapsedTime)
        {
        }

        public virtual void Initialize()
        {
            foreach (IDrawingBrush b in drawingList)
            {
                b.Update();
            }
        }

        private void GameLoop_Update(TimeSpan elapsedTime)
        {
            double secs = elapsedTime.TotalSeconds + _leftoverUpdateTime;
            while (secs > .01)
            {
                Update(elapsedTime);
                physicsSimulator.Update(.01f);
                foreach (IDrawingBrush b in drawingList)
                {
                    b.Update();
                }

                secs -= .01;
            }
            _leftoverUpdateTime = secs;
        }
    }
}