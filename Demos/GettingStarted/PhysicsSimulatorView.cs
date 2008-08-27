using System;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Dynamics.Joints;
using FarseerGames.FarseerPhysics.Dynamics.Springs;
using FarseerGames.FarseerPhysicsDemos.DrawingSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerGames.FarseerPhysicsDemos
{
    public class PhysicsSimulatorView
    {
        private readonly Vector2 performancePanelPosition = new Vector2(100, 110);
        private readonly PhysicsSimulator physicsSimulator;

        //aabb
        private Color aabbColor = new Color(0, 0, 0, 150); // Color.Gainsboro;
        private LineBrush aabbLineBrush;
        private int aabbLineThickness = 1;
        private string applyForces = "Apply Forces: {0}";
        private string applyImpulses = "Apply Impulses: {0}";
        private Vector2 attachPoint1;
        private Vector2 attachPoint2;
        private Vector2 body1AttachPointInWorldCoordinates;
        private Vector2 body2AttachPointInWorldCoordinates;
        private string broadPhaseCollision = "Broad Phase Collsion: {0}";
        private string cleanUp = "Clean Up: {0}";
        private CircleBrush contactCircleBrush;
        private Color contactColor = new Color(255, 0, 0, 150);
        private int contactRadius = 4;
        private Color coordinateAxisColor = new Color(0, 0, 0, 150);
        private LineBrush coordinateAxisLineBrush;
        private int coordinateAxisLineLength = 20;
        private int coordinateAxisLineThickness = 1;
        private Color edgeColor = new Color(0, 0, 0, 150);
        private LineBrush edgeLineBrush;
        private int edgeLineThickness = 1;
        private bool enableAABBView = true;
        private bool enableContactView = true;
        private bool enableCoordinateAxisView = true;
        private bool enableEdgeView;

        //grid
        private bool enableGridView;
        private bool enablePerformancePanelView = true;
        private bool enablePinJointView = true;

        //coordinate axis
        private bool enableRevoluteJointView = true;
        private bool enableSliderJointView = true;
        private bool enableSpringView = true;
        private bool enableVerticeView = true;
        private CircleBrush gridCircleBrush;
        private Color gridColor = new Color(0, 0, 0, 150);
        private int gridRadius = 1;
        private string narrowPhaseCollision = "Narrow Phase Collsion: {0}";
        private Color performancePanelColor = new Color(0, 0, 0, 150);
        private int performancePanelHeight = 150;
        private Color performancePanelTextColor = new Color(0, 0, 0, 255);
        private Texture2D performancePanelTexture;
        private int performancePanelWidth = 220;

        //pin joint
        private Color pinJointColor = new Color(0, 0, 0, 200);
        private LineBrush pinJointLineBrush;
        private int pinJointLineThickness = 1;
        private RectangleBrush pinJointRectangleBrush;
        private Color revoluteJointColor = new Color(0, 0, 0, 200);
        private LineBrush revoluteJointLineBrush;
        private int revoluteJointLineThickness = 1;
        private RectangleBrush revoluteJointRectangleBrush;

        //slider joint
        private Color sliderJointColor = new Color(0, 0, 0, 200);
        private LineBrush sliderJointLineBrush;
        private int sliderJointLineThickness = 1;
        private RectangleBrush sliderJointRectangleBrush;
        private CircleBrush springCircleBrush;
        private LineBrush springLineBrush;
        private Color springLineColor = new Color(0, 0, 0, 150);
        private int springLineThickness = 1;
        private SpriteFont spriteFont;
        private string updatePosition = "Update Positions: {0}";
        private string updateTotal = "Update Total: {0}";
        private Vector2 vectorTemp1;
        private CircleBrush verticeCircleBrush;
        private Color verticeColor = new Color(0, 50, 0, 150);
        private int verticeRadius = 3;
        private Vector2 worldAttachPoint;

        public PhysicsSimulatorView(PhysicsSimulator physicsSimulator)
        {
            this.physicsSimulator = physicsSimulator;
        }

        //aabb
        public Color AABBColor
        {
            get { return aabbColor; }
            set { aabbColor = value; }
        }

        public int AABBLineThickness
        {
            get { return aabbLineThickness; }
            set { aabbLineThickness = value; }
        }

        public bool EnableAABBView
        {
            get { return enableAABBView; }
            set { enableAABBView = value; }
        }

        //vertices
        public int VerticeRadius
        {
            get { return verticeRadius; }
            set { verticeRadius = value; }
        }

        public Color VerticeColor
        {
            get { return verticeColor; }
            set { verticeColor = value; }
        }

        public bool EnableVerticeView
        {
            get { return enableVerticeView; }
            set { enableVerticeView = value; }
        }

        //edges
        public int EdgeLineThickness
        {
            get { return edgeLineThickness; }
            set { edgeLineThickness = value; }
        }

        public Color EdgeColor
        {
            get { return edgeColor; }
            set { edgeColor = value; }
        }

        public bool EnableEdgeView
        {
            get { return enableEdgeView; }
            set { enableEdgeView = value; }
        }

        //grid
        public int GridRadius
        {
            get { return gridRadius; }
            set { gridRadius = value; }
        }

        public Color GridColor
        {
            get { return gridColor; }
            set { gridColor = value; }
        }

        public bool EnableGridView
        {
            get { return enableGridView; }
            set { enableGridView = value; }
        }

        //coordinate axis
        public int CoordinateAxisLineThickness
        {
            get { return coordinateAxisLineThickness; }
            set { coordinateAxisLineThickness = value; }
        }

        public Color CoordinateAxisColor
        {
            get { return coordinateAxisColor; }
            set { coordinateAxisColor = value; }
        }

        public int CoordinateAxisLineLength
        {
            get { return coordinateAxisLineLength; }
            set { coordinateAxisLineLength = value; }
        }

        public bool EnableCoordinateAxisView
        {
            get { return enableCoordinateAxisView; }
            set { enableCoordinateAxisView = value; }
        }

        //contacts
        public int ContactRadius
        {
            get { return contactRadius; }
            set { contactRadius = value; }
        }

        public Color ContactColor
        {
            get { return contactColor; }
            set { contactColor = value; }
        }

        public bool EnableContactView
        {
            get { return enableContactView; }
            set { enableContactView = value; }
        }

        //springs
        public Color SpringLineColor
        {
            get { return springLineColor; }
            set { springLineColor = value; }
        }

        public int SpringLineThickness
        {
            get { return springLineThickness; }
            set { springLineThickness = value; }
        }

        public bool EnableSpingView
        {
            get { return enableSpringView; }
            set { enableSpringView = value; }
        }

        //revolute joint
        public Color RevoluteJointLineColor
        {
            get { return revoluteJointColor; }
            set { revoluteJointColor = value; }
        }

        public int RevoluteJointLineThickness
        {
            get { return revoluteJointLineThickness; }
            set { revoluteJointLineThickness = value; }
        }

        public bool EnableRevoluteJointView
        {
            get { return enableRevoluteJointView; }
            set { enableRevoluteJointView = value; }
        }

        //pin joint
        public Color PinJointLineColor
        {
            get { return pinJointColor; }
            set { pinJointColor = value; }
        }

        public int PinJointLineThickness
        {
            get { return pinJointLineThickness; }
            set { pinJointLineThickness = value; }
        }

        public bool EnablePinJointView
        {
            get { return enablePinJointView; }
            set { enablePinJointView = value; }
        }

        //slider joint
        public Color SliderJointLineColor
        {
            get { return sliderJointColor; }
            set { sliderJointColor = value; }
        }

        public int SliderJointLineThickness
        {
            get { return sliderJointLineThickness; }
            set { sliderJointLineThickness = value; }
        }

        public bool EnableSliderJointView
        {
            get { return enableSliderJointView; }
            set { enableSliderJointView = value; }
        }

        //performance panel
        public Color PerformancePanelColor
        {
            get { return performancePanelColor; }
            set { performancePanelColor = value; }
        }

        public Color PerformancePanelTextColor
        {
            get { return performancePanelTextColor; }
            set { performancePanelTextColor = value; }
        }

        public bool EnablePerformancePanelView
        {
            get { return enablePerformancePanelView; }
            set { enablePerformancePanelView = value; }
        }

        public virtual void LoadContent(GraphicsDevice graphicsDevice, ContentManager content)
        {
            LoadVerticeContent(graphicsDevice);
            LoadEdgeContent(graphicsDevice);
            LoadGridContent(graphicsDevice);
            LoadAABBContent(graphicsDevice);
            LoadCoordinateAxisContent(graphicsDevice);
            LoadContactContent(graphicsDevice);
            LoadPerformancePanelContent(graphicsDevice, content);
            LoadSpringContent(graphicsDevice);
            LoadRevoluteJointContent(graphicsDevice);
            LoadPinJointContent(graphicsDevice);
            LoadSliderJointContent(graphicsDevice);
        }

        public virtual void UnloadContent(GraphicsDevice graphicsDevice, ContentManager content)
        {
            LoadVerticeContent(graphicsDevice);
            LoadEdgeContent(graphicsDevice);
            LoadGridContent(graphicsDevice);
            LoadAABBContent(graphicsDevice);
            LoadCoordinateAxisContent(graphicsDevice);
            LoadContactContent(graphicsDevice);
            LoadPerformancePanelContent(graphicsDevice, content);
            LoadSpringContent(graphicsDevice);
            LoadRevoluteJointContent(graphicsDevice);
            LoadPinJointContent(graphicsDevice);
            LoadSliderJointContent(graphicsDevice);
        }

        private void LoadPerformancePanelContent(GraphicsDevice graphicsDevice, ContentManager content)
        {
            performancePanelTexture = DrawingHelper.CreateRectangleTexture(graphicsDevice, performancePanelWidth,
                                                                           performancePanelHeight,
                                                                           new Color(0, 0, 0, 155));
            spriteFont = content.Load<SpriteFont>(@"Content\Fonts\diagnosticFont");
        }

        private void LoadContactContent(GraphicsDevice graphicsDevice)
        {
            contactCircleBrush = new CircleBrush(contactRadius, contactColor, contactColor);
            contactCircleBrush.Load(graphicsDevice);
        }

        private void LoadVerticeContent(GraphicsDevice graphicsDevice)
        {
            verticeCircleBrush = new CircleBrush(verticeRadius, verticeColor, verticeColor);
            verticeCircleBrush.Load(graphicsDevice);
        }

        private void LoadEdgeContent(GraphicsDevice graphicsDevice)
        {
            edgeLineBrush = new LineBrush(edgeLineThickness, edgeColor);
            edgeLineBrush.Load(graphicsDevice);
        }

        private void LoadGridContent(GraphicsDevice graphicsDevice)
        {
            gridCircleBrush = new CircleBrush(gridRadius, gridColor, gridColor);
            gridCircleBrush.Load(graphicsDevice);
        }

        private void LoadAABBContent(GraphicsDevice graphicsDevice)
        {
            //load aabb texture
            aabbLineBrush = new LineBrush(aabbLineThickness, aabbColor);
            aabbLineBrush.Load(graphicsDevice);
        }

        private void LoadCoordinateAxisContent(GraphicsDevice graphicsDevice)
        {
            coordinateAxisLineBrush = new LineBrush(coordinateAxisLineThickness, coordinateAxisColor);
            coordinateAxisLineBrush.Load(graphicsDevice);
        }

        private void LoadSpringContent(GraphicsDevice graphicsDevice)
        {
            springLineBrush = new LineBrush(springLineThickness, springLineColor);
            springCircleBrush = new CircleBrush(2, springLineColor, springLineColor);

            springLineBrush.Load(graphicsDevice);
            springCircleBrush.Load(graphicsDevice);
        }

        private void LoadRevoluteJointContent(GraphicsDevice graphicsDevice)
        {
            revoluteJointLineBrush = new LineBrush(revoluteJointLineThickness, revoluteJointColor);
            revoluteJointRectangleBrush = new RectangleBrush(10, 10, revoluteJointColor, revoluteJointColor);

            revoluteJointLineBrush.Load(graphicsDevice);
            revoluteJointRectangleBrush.Load(graphicsDevice);
        }

        private void LoadPinJointContent(GraphicsDevice graphicsDevice)
        {
            pinJointLineBrush = new LineBrush(pinJointLineThickness, pinJointColor);
            pinJointRectangleBrush = new RectangleBrush(10, 10, pinJointColor, pinJointColor);

            pinJointLineBrush.Load(graphicsDevice);
            pinJointRectangleBrush.Load(graphicsDevice);
        }

        private void LoadSliderJointContent(GraphicsDevice graphicsDevice)
        {
            sliderJointLineBrush = new LineBrush(sliderJointLineThickness, sliderJointColor);
            sliderJointRectangleBrush = new RectangleBrush(10, 10, sliderJointColor, sliderJointColor);

            sliderJointLineBrush.Load(graphicsDevice);
            sliderJointRectangleBrush.Load(graphicsDevice);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (enableVerticeView || enableEdgeView)
            {
                DrawVerticesAndEdges(spriteBatch);
            }
            if (enableGridView)
            {
                DrawGrid(spriteBatch);
            }
            if (enableAABBView)
            {
                DrawAABB(spriteBatch);
            }
            if (enableCoordinateAxisView)
            {
                DrawCoordinateAxis(spriteBatch);
            }
            if (enableContactView)
            {
                DrawContacts(spriteBatch);
            }
            if (enablePerformancePanelView)
            {
                DrawPerformancePanel(spriteBatch);
            }
            if (EnableSpingView)
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
            spriteBatch.Draw(performancePanelTexture, performancePanelPosition, performancePanelColor);

            spriteBatch.DrawString(spriteFont, String.Format(updateTotal, physicsSimulator.UpdateTime.ToString("0.00")),
                                   new Vector2(110, 110), Color.White);
            spriteBatch.DrawString(spriteFont, String.Format(cleanUp, physicsSimulator.CleanUpTime.ToString("0.00")),
                                   new Vector2(120, 125), Color.White);
            spriteBatch.DrawString(spriteFont,
                                   String.Format(broadPhaseCollision,
                                                 physicsSimulator.BroadPhaseCollisionTime.ToString("0.00")),
                                   new Vector2(120, 140), Color.White);
            spriteBatch.DrawString(spriteFont,
                                   String.Format(narrowPhaseCollision,
                                                 physicsSimulator.NarrowPhaseCollisionTime.ToString("0.00")),
                                   new Vector2(120, 155), Color.White);
            spriteBatch.DrawString(spriteFont,
                                   String.Format(applyForces, physicsSimulator.ApplyForcesTime.ToString("0.00")),
                                   new Vector2(120, 170), Color.White);
            spriteBatch.DrawString(spriteFont,
                                   String.Format(applyImpulses, physicsSimulator.ApplyImpulsesTime.ToString("0.00")),
                                   new Vector2(120, 185), Color.White);
            spriteBatch.DrawString(spriteFont,
                                   String.Format(updatePosition, physicsSimulator.UpdatePositionsTime.ToString("0.00")),
                                   new Vector2(120, 200), Color.White);
            //spriteBatch.DrawString(spriteFont, String.Format("Broadphase Pairs: {0}",this.physicsSimulator.sweepAndPrune.collisionPairs.Keys.Count), new Vector2(120, 215), Color.White);
        }

        private void DrawContacts(SpriteBatch spriteBatch)
        {
            //draw contact textures
            for (int i = 0; i < physicsSimulator.ArbiterList.Count; i++)
            {
                for (int j = 0; j < physicsSimulator.ArbiterList[i].ContactList.Count; j++)
                {
                    contactCircleBrush.Draw(spriteBatch, physicsSimulator.ArbiterList[i].ContactList[j].Position);
                }
            }
        }

        private void DrawVerticesAndEdges(SpriteBatch spriteBatch)
        {
            //draw vertice texture
            int verticeCount;
            for (int i = 0; i < physicsSimulator.GeomList.Count; i++)
            {
                verticeCount = physicsSimulator.GeomList[i].LocalVertices.Count;
                for (int j = 0; j < verticeCount; j++)
                {
                    if (enableEdgeView)
                    {
                        if (j < verticeCount - 1)
                        {
                            edgeLineBrush.Draw(spriteBatch, physicsSimulator.GeomList[i].WorldVertices[j],
                                               physicsSimulator.GeomList[i].WorldVertices[j + 1]);
                        }
                        else
                        {
                            edgeLineBrush.Draw(spriteBatch, physicsSimulator.GeomList[i].WorldVertices[j],
                                               physicsSimulator.GeomList[i].WorldVertices[0]);
                        }
                    }
                    if (enableVerticeView)
                    {
                        verticeCircleBrush.Draw(spriteBatch, physicsSimulator.GeomList[i].WorldVertices[j]);
                    }
                }
            }
        }

        private void DrawAABB(SpriteBatch spriteBatch)
        {
            //draw aabb
            Vector2 min;
            Vector2 max;

            Vector2 topRight;
            Vector2 bottomLeft;

            for (int i = 0; i < physicsSimulator.GeomList.Count; i++)
            {
                min = physicsSimulator.GeomList[i].AABB.Min;
                max = physicsSimulator.GeomList[i].AABB.Max;

                topRight = new Vector2(max.X, min.Y);
                bottomLeft = new Vector2(min.X, max.Y);
                aabbLineBrush.Draw(spriteBatch, min, topRight);
                aabbLineBrush.Draw(spriteBatch, topRight, max);
                aabbLineBrush.Draw(spriteBatch, max, bottomLeft);
                aabbLineBrush.Draw(spriteBatch, bottomLeft, min);
            }
        }

        private void DrawGrid(SpriteBatch spriteBatch)
        {
            //draw grid
            Vector2 point;
            int count;
            for (int i = 0; i < physicsSimulator.GeomList.Count; i++)
            {
                if (physicsSimulator.GeomList[i].Grid == null)
                {
                    continue;
                }
                count = physicsSimulator.GeomList[i].Grid.Points.Length;
                for (int j = 0; j < count; j++)
                {
                    point = physicsSimulator.GeomList[i].GetWorldPosition(physicsSimulator.GeomList[i].Grid.Points[j]);
                    gridCircleBrush.Draw(spriteBatch, point);
                }
            }
        }

        private void DrawCoordinateAxis(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < physicsSimulator.BodyList.Count; i++)
            {
                Vector2 startX =
                    physicsSimulator.BodyList[i].GetWorldPosition(new Vector2(-coordinateAxisLineLength/2f, 0));
                Vector2 endX = physicsSimulator.BodyList[i].GetWorldPosition(new Vector2(coordinateAxisLineLength/2f, 0));
                Vector2 startY =
                    physicsSimulator.BodyList[i].GetWorldPosition(new Vector2(0, -coordinateAxisLineLength/2f));
                Vector2 endY = physicsSimulator.BodyList[i].GetWorldPosition(new Vector2(0, coordinateAxisLineLength/2f));

                coordinateAxisLineBrush.Draw(spriteBatch, startX, endX);
                coordinateAxisLineBrush.Draw(spriteBatch, startY, endY);
            }
        }

        private void DrawSprings(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < physicsSimulator.ControllerList.Count; i++)
            {
                if (physicsSimulator.ControllerList[i] is FixedLinearSpring)
                {
                    FixedLinearSpring fixedLinearSpring = (FixedLinearSpring) physicsSimulator.ControllerList[i];
                    worldAttachPoint = fixedLinearSpring.WorldAttachPoint;
                    body1AttachPointInWorldCoordinates =
                        fixedLinearSpring.Body.GetWorldPosition(fixedLinearSpring.BodyAttachPoint);
                    springCircleBrush.Draw(spriteBatch, body1AttachPointInWorldCoordinates);
                    springCircleBrush.Draw(spriteBatch, worldAttachPoint);

                    Vector2.Lerp(ref worldAttachPoint, ref body1AttachPointInWorldCoordinates, .25f, out vectorTemp1);
                    springCircleBrush.Draw(spriteBatch, vectorTemp1);

                    Vector2.Lerp(ref worldAttachPoint, ref body1AttachPointInWorldCoordinates, .50f, out vectorTemp1);
                    springCircleBrush.Draw(spriteBatch, vectorTemp1);

                    Vector2.Lerp(ref worldAttachPoint, ref body1AttachPointInWorldCoordinates, .75f, out vectorTemp1);
                    springCircleBrush.Draw(spriteBatch, vectorTemp1);

                    springLineBrush.Draw(spriteBatch, body1AttachPointInWorldCoordinates,
                                         fixedLinearSpring.WorldAttachPoint);
                }
            }

            for (int i = 0; i < physicsSimulator.ControllerList.Count; i++)
            {
                if (physicsSimulator.ControllerList[i] is LinearSpring)
                {
                    LinearSpring linearSpring = (LinearSpring) physicsSimulator.ControllerList[i];
                    attachPoint1 = linearSpring.AttachPoint1;
                    attachPoint2 = linearSpring.AttachPoint2;
                    linearSpring.Body1.GetWorldPosition(ref attachPoint1, out body1AttachPointInWorldCoordinates);
                    linearSpring.Body2.GetWorldPosition(ref attachPoint2, out body2AttachPointInWorldCoordinates);
                    springCircleBrush.Draw(spriteBatch, body1AttachPointInWorldCoordinates);
                    springCircleBrush.Draw(spriteBatch, body2AttachPointInWorldCoordinates);

                    Vector2.Lerp(ref body1AttachPointInWorldCoordinates, ref body2AttachPointInWorldCoordinates, .25f,
                                 out vectorTemp1);
                    springCircleBrush.Draw(spriteBatch, vectorTemp1);

                    Vector2.Lerp(ref body1AttachPointInWorldCoordinates, ref body2AttachPointInWorldCoordinates, .50f,
                                 out vectorTemp1);
                    springCircleBrush.Draw(spriteBatch, vectorTemp1);

                    Vector2.Lerp(ref body1AttachPointInWorldCoordinates, ref body2AttachPointInWorldCoordinates, .75f,
                                 out vectorTemp1);
                    springCircleBrush.Draw(spriteBatch, vectorTemp1);

                    springLineBrush.Draw(spriteBatch, body1AttachPointInWorldCoordinates,
                                         body2AttachPointInWorldCoordinates);
                }
            }
        }

        private void DrawRevoluteJoints(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < physicsSimulator.JointList.Count; i++)
            {
                if (physicsSimulator.JointList[i] is FixedRevoluteJoint)
                {
                    FixedRevoluteJoint fixedRevoluteJoint = (FixedRevoluteJoint) physicsSimulator.JointList[i];
                    revoluteJointRectangleBrush.Draw(spriteBatch, fixedRevoluteJoint.Anchor);
                }

                if (physicsSimulator.JointList[i] is RevoluteJoint)
                {
                    RevoluteJoint revoluteJoint = (RevoluteJoint) physicsSimulator.JointList[i];
                    revoluteJointRectangleBrush.Draw(spriteBatch, revoluteJoint.CurrentAnchor);
                    revoluteJointLineBrush.Draw(spriteBatch, revoluteJoint.CurrentAnchor, revoluteJoint.Body1.Position);
                    revoluteJointLineBrush.Draw(spriteBatch, revoluteJoint.CurrentAnchor, revoluteJoint.Body2.Position);
                }
            }
        }

        private void DrawPinJoints(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < physicsSimulator.JointList.Count; i++)
            {
                if (physicsSimulator.JointList[i] is PinJoint)
                {
                    PinJoint pinJoint = (PinJoint) physicsSimulator.JointList[i];
                    pinJointRectangleBrush.Draw(spriteBatch, pinJoint.WorldAnchor1);
                    pinJointRectangleBrush.Draw(spriteBatch, pinJoint.WorldAnchor2);
                    pinJointLineBrush.Draw(spriteBatch, pinJoint.WorldAnchor1, pinJoint.WorldAnchor2);
                }
            }
        }


        private void DrawSliderJoints(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < physicsSimulator.JointList.Count; i++)
            {
                if (physicsSimulator.JointList[i] is SliderJoint)
                {
                    SliderJoint sliderJoint = (SliderJoint) physicsSimulator.JointList[i];
                    sliderJointRectangleBrush.Draw(spriteBatch, sliderJoint.WorldAnchor1);
                    sliderJointRectangleBrush.Draw(spriteBatch, sliderJoint.WorldAnchor2);
                    sliderJointLineBrush.Draw(spriteBatch, sliderJoint.WorldAnchor1, sliderJoint.WorldAnchor2);
                }
            }
        }
    }
}