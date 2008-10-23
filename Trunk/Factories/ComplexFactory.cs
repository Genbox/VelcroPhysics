using System;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Dynamics.Joints;
#if (XNA)
using Microsoft.Xna.Framework;
using FarseerGames.FarseerPhysics.Dynamics.PathGenerator;      // TODO need to write Curve class for silverlight so this is XNA only right now
#else
using FarseerGames.FarseerPhysics.Mathematics;
#endif

namespace FarseerGames.FarseerPhysics.Factories
{
    /// <summary>
    /// An easy to use factory for creating complex structures
    /// </summary>
    public class ComplexFactory
    {
        //TODO: list
        // 1. Create chain object so that people can change the properties of the chain.
        // 2. Remove ball at end of chain

        private static ComplexFactory _instance;

        private ComplexFactory()
        {
        }

        public static ComplexFactory Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ComplexFactory();
                }
                return _instance;
            }
        }
#if (XNA)
        public Path CreateChain(Vector2 start, Vector2 end, float width, float height, float mass, bool pinStart, bool pinEnd)
        {
            bool flip = true;
            Path p = new Path(width, height, mass, false);  // create the path
            p.Add(start);                                   // add starting point
            p.Add(Path.FindMidpoint(start, end));           // add midpoint of line (must have this because my code needs at least 3 control points)
            p.Add(end);                                     // add end point

            p.Update();                                     // call update to create all the bodies

            Geom g;
            for (int i = 0; i < (p.Bodies.Count - 1); i++)
            {
                if (flip)
                {
                    g = GeomFactory.Instance.CreateRectangleGeom(p.Bodies[i], width, height);
                    flip = !flip;
                }
                else
                {
                    g = GeomFactory.Instance.CreateRectangleGeom(p.Bodies[i], width, height * (1.0f / 3.0f));
                    flip = !flip;
                }
                g.CollisionCategories = CollisionCategory.Cat2;     // currently seting up the collision so chain will not collide with itself
                g.CollidesWith = CollisionCategory.Cat1;
                p.Add(g);                                           // add a geom to the chain
            }
            p.LinkBodies();         // link bodies together with revolute joints

            if (pinStart)
                p.Add(JointFactory.Instance.CreateFixedRevoluteJoint(p.Bodies[0], start));
            if (pinEnd)
                p.Add(JointFactory.Instance.CreateFixedRevoluteJoint(p.Bodies[p.Bodies.Count - 1], p.ControlPoints[2]));

            return (p);
        }

        /// <summary>
        /// Creates a chain with the specified number of links.
        /// </summary>
        /// <param name="physicsSimulator">The physics simulator.</param>
        /// <param name="startPosition">The start position.</param>
        /// <param name="endPosition">The end position.</param>
        /// <param name="links">The number of links.</param>
        public void CreateChain(PhysicsSimulator physicsSimulator, Vector2 startPosition, Vector2 endPosition, int links)
        {
            Vector2 delta = endPosition - startPosition;
            float length = delta.Length(); // length of chain
            delta.Normalize();
            float angle = (float)Math.Cos(delta.X / length);
            delta *= (length / links);
            Body a = null, b;
            Geom g;
            PinJoint r;

            for (int i = 0; i < links; i++)
            {
                b = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, length / links, 10, 1);
                b.Position = startPosition + (delta * i);
                b.Rotation = angle;
                g = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, b, length / links, 10);
                g.CollisionCategories = CollisionCategory.Cat2;
                g.CollisionGroup = 2;

                if (i >= 1)
                {
                    r = JointFactory.Instance.CreatePinJoint(physicsSimulator, a, new Vector2((length / links) / 2, 0), b,
                                                             new Vector2(-(length / links) / 2, 0));
                    r.TargetDistance = 0;
                }
                a = b;
            }
        }

        /// <summary>
        /// Creates a chain with the specified width and length
        /// </summary>
        /// <param name="physicsSimulator">The physics simulator.</param>
        /// <param name="startPosition">The start position.</param>
        /// <param name="endPosition">The end position.</param>
        /// <param name="linkLength">Length of the link.</param>
        /// <param name="linkWidth">Width of the link.</param>
        /// <param name="createGeometry">if set to <c>true</c> [create geometry].</param>
        /// <param name="chainCollidesWithSelf">if set to <c>true</c> [chain collides with self].</param>
        public void CreateChain(PhysicsSimulator physicsSimulator, Vector2 startPosition, Vector2 endPosition, float linkLength, float linkWidth,
                                 bool createGeometry, bool chainCollidesWithSelf)
        {
            //Get the target length of the chain
            float length = (startPosition - endPosition).Length() * 1.4f;
            int numOfLinks = (int)((length / linkLength) * 1.0f);

            //Find how many links we will make based on the target length and desired link length
            float sideLinkWidth = linkWidth * 0.33333f;

            //Side link is 1/3 of flat link width NOTE: we may make this a static coefficient
            Vector2 deltaVec;

            //Here we must get a delta vector 
            if (startPosition.X < endPosition.X)
                deltaVec.X = endPosition.X - startPosition.X;
            else
                deltaVec.X = startPosition.X - endPosition.X;

            if (startPosition.Y < endPosition.Y)
                deltaVec.Y = endPosition.Y - startPosition.Y;
            else
                deltaVec.Y = startPosition.Y - endPosition.Y;

            //Find the angle of the chain in radians
            float angle = (float)Math.Cos(deltaVec.X / length);

            //Normalize our delta vector
            deltaVec.Normalize();

            //Scale delta vector
            deltaVec *= (length / numOfLinks) * 0.75f;

            Body a = null, b, c; // temp bodies
            Geom g; // temp geom
            PinJoint r; // temp pin joint

            for (int i = 0; i < numOfLinks; i++)
            {
                if (i % 2 == 0)
                {
                    b = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, linkLength, linkWidth, 0.5f);
                    b.Position = startPosition + (deltaVec * i);
                    b.Rotation = angle;

                    if (createGeometry)
                    {
                        g = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, b, linkLength, linkWidth);

                        if (chainCollidesWithSelf)
                        {
                            g.CollisionCategories = CollisionCategory.Cat10;
                        }
                        else
                        {
                            g.CollisionCategories = CollisionCategory.Cat10;
                            g.CollisionGroup = 1;
                        }
                    }
                }
                else
                {
                    b = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, linkLength, sideLinkWidth, 0.5f);
                    b.Position = startPosition + (deltaVec * i);
                    b.Rotation = angle;

                    if (createGeometry)
                    {
                        g = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, b, linkLength, sideLinkWidth);

                        if (chainCollidesWithSelf)
                        {
                            g.CollisionEnabled = false;
                        }
                        else
                        {
                            g.CollisionCategories = CollisionCategory.Cat10;
                            g.CollisionGroup = 1;
                        }
                    }
                }

                //For testing only
                if (i == 0)
                    JointFactory.Instance.CreateFixedRevoluteJoint(physicsSimulator, b, startPosition);
                if (i == (numOfLinks - 1))
                {
                    c = BodyFactory.Instance.CreateCircleBody(physicsSimulator, 50.0f, 1.0f);
                    c.Position = endPosition;
                    g = GeomFactory.Instance.CreateCircleGeom(physicsSimulator, c, 50, 36);
                    g.CollisionEnabled = true;
                    g.CollisionCategories = CollisionCategory.Cat10;
                    g.CollisionGroup = 1;
                    JointFactory.Instance.CreateRevoluteJoint(physicsSimulator, c, b, startPosition + (deltaVec * i));
                }

                //We have passed the first link
                if (i >= 1 && i < (numOfLinks))
                {
                    r = JointFactory.Instance.CreatePinJoint(physicsSimulator, a, new Vector2((linkLength / 2.0f) * 0.75f, 0), b,
                                                             new Vector2(-(linkLength / 2.0f) * 0.75f, 0));
                    r.TargetDistance = 0;
                    r.BiasFactor = 0.1f;
                    r.Softness = 0.1f;
                    r.Breakpoint = linkLength;
                }
                a = b;
            }
        }
#endif
    }
}