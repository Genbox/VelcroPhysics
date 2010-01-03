using System;
using DemoBaseXNA.DrawingSystem;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Dynamics.Joints;
using FarseerGames.FarseerPhysics.Dynamics.Springs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DemoBaseXNA
{
    /// <summary>
    /// Draws the elements inside a <see cref="PhysicsSimulator"/>. Great for debugging physics related problems.
    /// </summary>
    public sealed class PhysicsSimulatorView
    {
        private PhysicsSimulator _physicsSimulator;

        //Performance panel
        private bool _enablePerformancePanelView = true;
        private const string _applyForces = "Apply Forces: {0}";
        private const string _applyImpulses = "Apply Impulses: {0}";
        private const string _arbiterCount = "Arbiters: {0}";
        private const string _bodyCount = "Bodies: {0}";
        private const string _broadPhaseCollision = "Broad Phase Collision: {0}";
        private const string _cleanUp = "Clean Up: {0}";
        private const string _controllerCount = "Controllers: {0}";
        private const string _geomCount = "Geoms: {0}";
        private const string _jointCount = "Joints: {0}";
        private const string _narrowPhaseCollision = "Narrow Phase Collision: {0}";
        private const string _springCount = "Springs: {0}";
        private const string _updatePosition = "Update Positions: {0}";
        private const string _updateTotal = "Update Total: {0}";
        private Color _performancePanelColor = new Color(0, 0, 0, 150);
        private bool _performancePanelCount = true;
        private Vector2 _performancePanelPosition = new Vector2(100, 110);
        private Color _performancePanelTextColor = new Color(0, 0, 0, 255);
        private Texture2D _performancePanelTexture;
        private int _performancePanelWidth = 220;
        private const int _performancePanelHeight = 130;
        private SpriteFont _spriteFont;

        //AABB
        private Color _aabbColor = new Color(0, 0, 0, 150); // Color.Gainsboro;
        private LineBrush _aabbLineBrush;
        private int _aabbLineThickness = 1;

        //Contacts
        private bool _enableContactView = true;
        private CircleBrush _contactCircleBrush;
        private Color _contactColor = new Color(255, 0, 0, 150);
        private int _contactRadius = 4;

        //Coordinate axis
        private bool _enableCoordinateAxisView = true;
        private Color _coordinateAxisColor = new Color(0, 0, 0, 150);
        private LineBrush _coordinateAxisLineBrush;
        private int _coordinateAxisLineLength = 20;
        private int _coordinateAxisLineThickness = 1;

        //Vertice
        private bool _enableVerticeView = true;
        private CircleBrush _verticeCircleBrush;
        private Color _verticeColor = new Color(0, 50, 0, 150);
        private int _verticeRadius = 3;

        //Edge
        private bool _enableEdgeView;
        private Color _edgeColor = new Color(0, 0, 0, 150);
        private LineBrush _edgeLineBrush;
        private int _edgeLineThickness = 1;
        private bool _enableAABBView = true;

        //Revolute joint
        private bool _enableRevoluteJointView = true;
        private Color _revoluteJointColor = new Color(0, 0, 0, 200);
        private LineBrush _revoluteJointLineBrush;
        private int _revoluteJointLineThickness = 1;
        private RectangleBrush _revoluteJointRectangleBrush;

        //Pin joint
        private bool _enablePinJointView = true;
        private Color _pinJointColor = new Color(0, 0, 0, 200);
        private LineBrush _pinJointLineBrush;
        private int _pinJointLineThickness = 1;
        private RectangleBrush _pinJointRectangleBrush;

        //Slider joint
        private bool _enableSliderJointView = true;
        private Color _sliderJointColor = new Color(0, 0, 0, 200);
        private LineBrush _sliderJointLineBrush;
        private int _sliderJointLineThickness = 1;
        private RectangleBrush _sliderJointRectangleBrush;

        //Spring 
        private bool _enableSpringView = true;
        private Vector2 _attachPoint1;
        private Vector2 _attachPoint2;
        private Vector2 _body1AttachPointInWorldCoordinates;
        private Vector2 _body2AttachPointInWorldCoordinates;
        private CircleBrush _springCircleBrush;
        private LineBrush _springLineBrush;
        private Color _springLineColor = new Color(0, 0, 0, 150);
        private int _springLineThickness = 1;
        private Vector2 _worldAttachPoint;
        private Vector2 _vectorTemp1;

        public PhysicsSimulatorView(PhysicsSimulator physicsSimulator)
        {
            _physicsSimulator = physicsSimulator;

            if (_performancePanelCount)
                _performancePanelWidth = 360;
        }

        //aabb
        public Color AABBColor
        {
            get { return _aabbColor; }
            set { _aabbColor = value; }
        }

        public int AABBLineThickness
        {
            get { return _aabbLineThickness; }
            set { _aabbLineThickness = value; }
        }

        public bool EnableAABBView
        {
            get { return _enableAABBView; }
            set { _enableAABBView = value; }
        }

        //vertices
        public int VerticeRadius
        {
            get { return _verticeRadius; }
            set { _verticeRadius = value; }
        }

        public Color VerticeColor
        {
            get { return _verticeColor; }
            set { _verticeColor = value; }
        }

        public bool EnableVerticeView
        {
            get { return _enableVerticeView; }
            set { _enableVerticeView = value; }
        }

        //edges
        public int EdgeLineThickness
        {
            get { return _edgeLineThickness; }
            set { _edgeLineThickness = value; }
        }

        public Color EdgeColor
        {
            get { return _edgeColor; }
            set { _edgeColor = value; }
        }

        public bool EnableEdgeView
        {
            get { return _enableEdgeView; }
            set { _enableEdgeView = value; }
        }

        //coordinate axis
        public int CoordinateAxisLineThickness
        {
            get { return _coordinateAxisLineThickness; }
            set { _coordinateAxisLineThickness = value; }
        }

        public Color CoordinateAxisColor
        {
            get { return _coordinateAxisColor; }
            set { _coordinateAxisColor = value; }
        }

        public int CoordinateAxisLineLength
        {
            get { return _coordinateAxisLineLength; }
            set { _coordinateAxisLineLength = value; }
        }

        public bool EnableCoordinateAxisView
        {
            get { return _enableCoordinateAxisView; }
            set { _enableCoordinateAxisView = value; }
        }

        //contacts
        public int ContactRadius
        {
            get { return _contactRadius; }
            set { _contactRadius = value; }
        }

        public Color ContactColor
        {
            get { return _contactColor; }
            set { _contactColor = value; }
        }

        public bool EnableContactView
        {
            get { return _enableContactView; }
            set { _enableContactView = value; }
        }

        //springs
        public Color SpringLineColor
        {
            get { return _springLineColor; }
            set { _springLineColor = value; }
        }

        public int SpringLineThickness
        {
            get { return _springLineThickness; }
            set { _springLineThickness = value; }
        }

        public bool EnableSpringView
        {
            get { return _enableSpringView; }
            set { _enableSpringView = value; }
        }

        //revolute joint
        public Color RevoluteJointLineColor
        {
            get { return _revoluteJointColor; }
            set { _revoluteJointColor = value; }
        }

        public int RevoluteJointLineThickness
        {
            get { return _revoluteJointLineThickness; }
            set { _revoluteJointLineThickness = value; }
        }

        public bool EnableRevoluteJointView
        {
            get { return _enableRevoluteJointView; }
            set { _enableRevoluteJointView = value; }
        }

        //pin joint
        public Color PinJointLineColor
        {
            get { return _pinJointColor; }
            set { _pinJointColor = value; }
        }

        public int PinJointLineThickness
        {
            get { return _pinJointLineThickness; }
            set { _pinJointLineThickness = value; }
        }

        public bool EnablePinJointView
        {
            get { return _enablePinJointView; }
            set { _enablePinJointView = value; }
        }

        //slider joint
        public Color SliderJointLineColor
        {
            get { return _sliderJointColor; }
            set { _sliderJointColor = value; }
        }

        public int SliderJointLineThickness
        {
            get { return _sliderJointLineThickness; }
            set { _sliderJointLineThickness = value; }
        }

        public bool EnableSliderJointView
        {
            get { return _enableSliderJointView; }
            set { _enableSliderJointView = value; }
        }

        //performance panel
        public Color PerformancePanelColor
        {
            get { return _performancePanelColor; }
            set { _performancePanelColor = value; }
        }

        public Color PerformancePanelTextColor
        {
            get { return _performancePanelTextColor; }
            set { _performancePanelTextColor = value; }
        }

        public bool EnablePerformancePanelView
        {
            get { return _enablePerformancePanelView; }
            set { _enablePerformancePanelView = value; }
        }

        public bool EnablePerformancePanelBodyCount
        {
            get { return _performancePanelCount; }
            set
            {
                _performancePanelWidth = value ? 360 : 220;
                _performancePanelCount = value;
            }
        }

        public void LoadContent(GraphicsDevice graphicsDevice, ContentManager content)
        {
            LoadVerticeContent(graphicsDevice);
            LoadEdgeContent(graphicsDevice);
            LoadAABBContent(graphicsDevice);
            LoadCoordinateAxisContent(graphicsDevice);
            LoadContactContent(graphicsDevice);
            LoadPerformancePanelContent(graphicsDevice, content);
            LoadSpringContent(graphicsDevice);
            LoadRevoluteJointContent(graphicsDevice);
            LoadPinJointContent(graphicsDevice);
            LoadSliderJointContent(graphicsDevice);
        }

        public void UnloadContent(GraphicsDevice graphicsDevice, ContentManager content)
        {
            //UnloadVerticeContent();
            //UnloadEdgeContent();
            //UnloadAABBContent();
            //UnloadCoordinateAxisContent();
            //UnloadContactContent();
            //UnloadPerformancePanelContent();
            //UnloadSpringContent();
            //UnloadRevoluteJointContent();
            //UnloadPinJointContent();
            //UnloadSliderJointContent();
        }

        private void LoadPerformancePanelContent(GraphicsDevice graphicsDevice, ContentManager content)
        {
            _performancePanelTexture = DrawingHelper.CreateRectangleTexture(graphicsDevice, _performancePanelWidth,
                                                                            _performancePanelHeight,
                                                                            new Color(0, 0, 0, 155));
            _spriteFont = content.Load<SpriteFont>(@"Content\Fonts\diagnosticFont");
        }

        private void LoadContactContent(GraphicsDevice graphicsDevice)
        {
            _contactCircleBrush = new CircleBrush(_contactRadius, _contactColor, _contactColor);
            _contactCircleBrush.Load(graphicsDevice);
        }

        private void LoadVerticeContent(GraphicsDevice graphicsDevice)
        {
            _verticeCircleBrush = new CircleBrush(_verticeRadius, _verticeColor, _verticeColor);
            _verticeCircleBrush.Load(graphicsDevice);
        }

        private void LoadEdgeContent(GraphicsDevice graphicsDevice)
        {
            _edgeLineBrush = new LineBrush(_edgeLineThickness, _edgeColor);
            _edgeLineBrush.Load(graphicsDevice);
        }

        private void LoadAABBContent(GraphicsDevice graphicsDevice)
        {
            //load aabb texture
            _aabbLineBrush = new LineBrush(_aabbLineThickness, _aabbColor);
            _aabbLineBrush.Load(graphicsDevice);
        }

        private void LoadCoordinateAxisContent(GraphicsDevice graphicsDevice)
        {
            _coordinateAxisLineBrush = new LineBrush(_coordinateAxisLineThickness, _coordinateAxisColor);
            _coordinateAxisLineBrush.Load(graphicsDevice);
        }

        private void LoadSpringContent(GraphicsDevice graphicsDevice)
        {
            _springLineBrush = new LineBrush(_springLineThickness, _springLineColor);
            _springCircleBrush = new CircleBrush(2, _springLineColor, _springLineColor);

            _springLineBrush.Load(graphicsDevice);
            _springCircleBrush.Load(graphicsDevice);
        }

        private void LoadRevoluteJointContent(GraphicsDevice graphicsDevice)
        {
            _revoluteJointLineBrush = new LineBrush(_revoluteJointLineThickness, _revoluteJointColor);
            _revoluteJointRectangleBrush = new RectangleBrush(10, 10, _revoluteJointColor, _revoluteJointColor);

            _revoluteJointLineBrush.Load(graphicsDevice);
            _revoluteJointRectangleBrush.Load(graphicsDevice);
        }

        private void LoadPinJointContent(GraphicsDevice graphicsDevice)
        {
            _pinJointLineBrush = new LineBrush(_pinJointLineThickness, _pinJointColor);
            _pinJointRectangleBrush = new RectangleBrush(10, 10, _pinJointColor, _pinJointColor);

            _pinJointLineBrush.Load(graphicsDevice);
            _pinJointRectangleBrush.Load(graphicsDevice);
        }

        private void LoadSliderJointContent(GraphicsDevice graphicsDevice)
        {
            _sliderJointLineBrush = new LineBrush(_sliderJointLineThickness, _sliderJointColor);
            _sliderJointRectangleBrush = new RectangleBrush(10, 10, _sliderJointColor, _sliderJointColor);

            _sliderJointLineBrush.Load(graphicsDevice);
            _sliderJointRectangleBrush.Load(graphicsDevice);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (_enableVerticeView || _enableEdgeView)
            {
                DrawVerticesAndEdges(spriteBatch);
            }
            if (_enableAABBView)
            {
                DrawAABB(spriteBatch);
            }
            if (_enableCoordinateAxisView)
            {
                DrawCoordinateAxis(spriteBatch);
            }
            if (_enableContactView)
            {
                DrawContacts(spriteBatch);
            }
            if (_enablePerformancePanelView)
            {
                DrawPerformancePanel(spriteBatch);
            }
            if (EnableSpringView)
            {
                DrawSprings(spriteBatch);
            }
            if (EnableRevoluteJointView)
            {
                DrawRevoluteJoints(spriteBatch);
            }
            if (EnablePinJointView)
            {
                DrawPinJoints(spriteBatch);
            }
            if (EnableSliderJointView)
            {
                DrawSliderJoints(spriteBatch);
            }
        }

        private void DrawPerformancePanel(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_performancePanelTexture, _performancePanelPosition, _performancePanelColor);

            spriteBatch.DrawString(_spriteFont,
                                   String.Format(_updateTotal, _physicsSimulator.UpdateTime.ToString("0.00")),
                                   new Vector2(110, 110), Color.White);
            spriteBatch.DrawString(_spriteFont, String.Format(_cleanUp, _physicsSimulator.CleanUpTime.ToString("0.00")),
                                   new Vector2(120, 125), Color.White);
            spriteBatch.DrawString(_spriteFont,
                                   String.Format(_broadPhaseCollision,
                                                 _physicsSimulator.BroadPhaseCollisionTime.ToString("0.00")),
                                   new Vector2(120, 140), Color.White);
            spriteBatch.DrawString(_spriteFont,
                                   String.Format(_narrowPhaseCollision,
                                                 _physicsSimulator.NarrowPhaseCollisionTime.ToString("0.00")),
                                   new Vector2(120, 155), Color.White);
            spriteBatch.DrawString(_spriteFont,
                                   String.Format(_applyForces, _physicsSimulator.ApplyForcesTime.ToString("0.00")),
                                   new Vector2(120, 170), Color.White);
            spriteBatch.DrawString(_spriteFont,
                                   String.Format(_applyImpulses, _physicsSimulator.ApplyImpulsesTime.ToString("0.00")),
                                   new Vector2(120, 185), Color.White);
            spriteBatch.DrawString(_spriteFont,
                                   String.Format(_updatePosition, _physicsSimulator.UpdatePositionsTime.ToString("0.00")),
                                   new Vector2(120, 200), Color.White);

            spriteBatch.DrawString(_spriteFont,
                                   String.Format(_bodyCount, _physicsSimulator.BodyList.Count),
                                   new Vector2(340, 125), Color.White);

            spriteBatch.DrawString(_spriteFont,
                                   String.Format(_geomCount, _physicsSimulator.GeomList.Count),
                                   new Vector2(340, 140), Color.White);

            spriteBatch.DrawString(_spriteFont,
                                   String.Format(_jointCount, _physicsSimulator.JointList.Count),
                                   new Vector2(340, 155), Color.White);

            spriteBatch.DrawString(_spriteFont,
                                   String.Format(_springCount, _physicsSimulator.SpringList.Count),
                                   new Vector2(340, 170), Color.White);

            spriteBatch.DrawString(_spriteFont,
                                   String.Format(_controllerCount, _physicsSimulator.ControllerList.Count),
                                   new Vector2(340, 185), Color.White);

            spriteBatch.DrawString(_spriteFont,
                                   String.Format(_arbiterCount, _physicsSimulator.ArbiterList.Count),
                                   new Vector2(340, 200), Color.White);


            //spriteBatch.DrawString(_spriteFont, String.Format("Broadphase Pairs: {0}",this._physicsSimulator.sweepAndPrune.collisionPairs.Keys.Count), new Vector2(120, 215), Color.White);
        }

        private void DrawContacts(SpriteBatch spriteBatch)
        {
            //draw contact textures
            for (int i = 0; i < _physicsSimulator.ArbiterList.Count; i++)
            {
                for (int j = 0; j < _physicsSimulator.ArbiterList[i].ContactList.Count; j++)
                {
                    _contactCircleBrush.Draw(spriteBatch, _physicsSimulator.ArbiterList[i].ContactList[j].Position);
                }
            }
        }

        private void DrawVerticesAndEdges(SpriteBatch spriteBatch)
        {
            //draw vertice texture
            for (int i = 0; i < _physicsSimulator.GeomList.Count; i++)
            {
                int verticeCount = _physicsSimulator.GeomList[i].LocalVertices.Count;
                for (int j = 0; j < verticeCount; j++)
                {
                    if (_enableEdgeView)
                    {
                        if (j < verticeCount - 1)
                        {
                            _edgeLineBrush.Draw(spriteBatch, _physicsSimulator.GeomList[i].WorldVertices[j],
                                                _physicsSimulator.GeomList[i].WorldVertices[j + 1]);
                        }
                        else
                        {
                            _edgeLineBrush.Draw(spriteBatch, _physicsSimulator.GeomList[i].WorldVertices[j],
                                                _physicsSimulator.GeomList[i].WorldVertices[0]);
                        }
                    }
                    if (_enableVerticeView)
                    {
                        _verticeCircleBrush.Draw(spriteBatch, _physicsSimulator.GeomList[i].WorldVertices[j]);
                    }
                }
            }
        }

        private void DrawAABB(SpriteBatch spriteBatch)
        {
            //draw aabb
            for (int i = 0; i < _physicsSimulator.GeomList.Count; i++)
            {
                Vector2 min = _physicsSimulator.GeomList[i].AABB.Min;
                Vector2 max = _physicsSimulator.GeomList[i].AABB.Max;

                Vector2 topRight = new Vector2(max.X, min.Y);
                Vector2 bottomLeft = new Vector2(min.X, max.Y);
                _aabbLineBrush.Draw(spriteBatch, min, topRight);
                _aabbLineBrush.Draw(spriteBatch, topRight, max);
                _aabbLineBrush.Draw(spriteBatch, max, bottomLeft);
                _aabbLineBrush.Draw(spriteBatch, bottomLeft, min);
            }
        }

        private void DrawCoordinateAxis(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < _physicsSimulator.BodyList.Count; i++)
            {
                Vector2 startX =
                    _physicsSimulator.BodyList[i].GetWorldPosition(new Vector2(-_coordinateAxisLineLength / 2f, 0));
                Vector2 endX =
                    _physicsSimulator.BodyList[i].GetWorldPosition(new Vector2(_coordinateAxisLineLength / 2f, 0));
                Vector2 startY =
                    _physicsSimulator.BodyList[i].GetWorldPosition(new Vector2(0, -_coordinateAxisLineLength / 2f));
                Vector2 endY =
                    _physicsSimulator.BodyList[i].GetWorldPosition(new Vector2(0, _coordinateAxisLineLength / 2f));

                _coordinateAxisLineBrush.Draw(spriteBatch, startX, endX);
                _coordinateAxisLineBrush.Draw(spriteBatch, startY, endY);
            }
        }

        private void DrawSprings(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < _physicsSimulator.SpringList.Count; i++)
            {
                if (!(_physicsSimulator.SpringList[i] is FixedLinearSpring))
                    continue;

                FixedLinearSpring fixedLinearSpring = (FixedLinearSpring)_physicsSimulator.SpringList[i];
                _worldAttachPoint = fixedLinearSpring.WorldAttachPoint;
                _body1AttachPointInWorldCoordinates =
                    fixedLinearSpring.Body.GetWorldPosition(fixedLinearSpring.BodyAttachPoint);
                _springCircleBrush.Draw(spriteBatch, _body1AttachPointInWorldCoordinates);
                _springCircleBrush.Draw(spriteBatch, _worldAttachPoint);

                Vector2.Lerp(ref _worldAttachPoint, ref _body1AttachPointInWorldCoordinates, .25f, out _vectorTemp1);
                _springCircleBrush.Draw(spriteBatch, _vectorTemp1);

                Vector2.Lerp(ref _worldAttachPoint, ref _body1AttachPointInWorldCoordinates, .50f, out _vectorTemp1);
                _springCircleBrush.Draw(spriteBatch, _vectorTemp1);

                Vector2.Lerp(ref _worldAttachPoint, ref _body1AttachPointInWorldCoordinates, .75f, out _vectorTemp1);
                _springCircleBrush.Draw(spriteBatch, _vectorTemp1);

                _springLineBrush.Draw(spriteBatch, _body1AttachPointInWorldCoordinates,
                                      fixedLinearSpring.WorldAttachPoint);
            }

            for (int i = 0; i < _physicsSimulator.SpringList.Count; i++)
            {
                if (!(_physicsSimulator.SpringList[i] is LinearSpring)) continue;

                LinearSpring linearSpring = (LinearSpring)_physicsSimulator.SpringList[i];
                _attachPoint1 = linearSpring.AttachPoint1;
                _attachPoint2 = linearSpring.AttachPoint2;
                linearSpring.Body1.GetWorldPosition(ref _attachPoint1, out _body1AttachPointInWorldCoordinates);
                linearSpring.Body2.GetWorldPosition(ref _attachPoint2, out _body2AttachPointInWorldCoordinates);
                _springCircleBrush.Draw(spriteBatch, _body1AttachPointInWorldCoordinates);
                _springCircleBrush.Draw(spriteBatch, _body2AttachPointInWorldCoordinates);

                Vector2.Lerp(ref _body1AttachPointInWorldCoordinates, ref _body2AttachPointInWorldCoordinates, .25f,
                             out _vectorTemp1);
                _springCircleBrush.Draw(spriteBatch, _vectorTemp1);

                Vector2.Lerp(ref _body1AttachPointInWorldCoordinates, ref _body2AttachPointInWorldCoordinates, .50f,
                             out _vectorTemp1);
                _springCircleBrush.Draw(spriteBatch, _vectorTemp1);

                Vector2.Lerp(ref _body1AttachPointInWorldCoordinates, ref _body2AttachPointInWorldCoordinates, .75f,
                             out _vectorTemp1);
                _springCircleBrush.Draw(spriteBatch, _vectorTemp1);

                _springLineBrush.Draw(spriteBatch, _body1AttachPointInWorldCoordinates,
                                      _body2AttachPointInWorldCoordinates);
            }
        }

        private void DrawRevoluteJoints(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < _physicsSimulator.JointList.Count; i++)
            {
                if (_physicsSimulator.JointList[i] is FixedRevoluteJoint)
                {
                    FixedRevoluteJoint fixedRevoluteJoint = (FixedRevoluteJoint)_physicsSimulator.JointList[i];
                    _revoluteJointRectangleBrush.Draw(spriteBatch, fixedRevoluteJoint.Anchor, 0);
                }

                if (!(_physicsSimulator.JointList[i] is RevoluteJoint)) continue;

                RevoluteJoint revoluteJoint = (RevoluteJoint)_physicsSimulator.JointList[i];
                _revoluteJointRectangleBrush.Draw(spriteBatch, revoluteJoint.CurrentAnchor, 0);
                _revoluteJointLineBrush.Draw(spriteBatch, revoluteJoint.CurrentAnchor, revoluteJoint.Body1.Position);
                _revoluteJointLineBrush.Draw(spriteBatch, revoluteJoint.CurrentAnchor, revoluteJoint.Body2.Position);
            }
        }

        private void DrawPinJoints(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < _physicsSimulator.JointList.Count; i++)
            {
                if (!(_physicsSimulator.JointList[i] is PinJoint))
                    continue;

                PinJoint pinJoint = (PinJoint)_physicsSimulator.JointList[i];
                _pinJointRectangleBrush.Draw(spriteBatch, pinJoint.WorldAnchor1, 0);
                _pinJointRectangleBrush.Draw(spriteBatch, pinJoint.WorldAnchor2, 0);
                _pinJointLineBrush.Draw(spriteBatch, pinJoint.WorldAnchor1, pinJoint.WorldAnchor2);
            }
        }

        private void DrawSliderJoints(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < _physicsSimulator.JointList.Count; i++)
            {
                if (!(_physicsSimulator.JointList[i] is SliderJoint))
                    continue;

                SliderJoint sliderJoint = (SliderJoint)_physicsSimulator.JointList[i];
                _sliderJointRectangleBrush.Draw(spriteBatch, sliderJoint.WorldAnchor1, 0);
                _sliderJointRectangleBrush.Draw(spriteBatch, sliderJoint.WorldAnchor2, 0);
                _sliderJointLineBrush.Draw(spriteBatch, sliderJoint.WorldAnchor1, sliderJoint.WorldAnchor2);
            }
        }
    }
}