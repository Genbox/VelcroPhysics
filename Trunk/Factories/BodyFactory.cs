using System;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;

namespace FarseerGames.FarseerPhysics.Factories
{
    /// <summary>
    /// An easy to use factory for creating bodies
    /// </summary>
    public class BodyFactory
    {
        private static BodyFactory _instance;

        private BodyFactory()
        {
        }

        public static BodyFactory Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new BodyFactory();
                }
                return _instance;
            }
        }

        //rectangles
        public Body CreateRectangleBody(PhysicsSimulator physicsSimulator, float width, float height, float mass)
        {
            Body body = CreateRectangleBody(width, height, mass);
            physicsSimulator.Add(body);
            return body;
        }

        public Body CreateRectangleBody(float width, float height, float mass)
        {
            Body body = new Body();
            body.Mass = mass;

            //MOI for rectangles.
            body.MomentOfInertia = mass * (width * width + height * height) / 12;
            return body;
        }

        //circles
        public Body CreateCircleBody(PhysicsSimulator physicsSimulator, float radius, float mass)
        {
            Body body = CreateCircleBody(radius, mass);
            physicsSimulator.Add(body);
            return body;
        }

        public Body CreateCircleBody(float radius, float mass)
        {
            Body body = new Body();
            body.Mass = mass;

            //MOI for circles
            body.MomentOfInertia = .5f * mass * (float)Math.Pow(radius, 2f);
            return body;
        }

        /// <summary>
        /// Creates a Body.  The moment of inertia of the body is calculated from the
        /// set of vertices passed in to this method. The vertices should represent a polygon
        /// </summary>
        /// <param name="physicsSimulator"><see cref="PhysicsSimulator"/> to add this body to.</param>
        /// <param name="vertices">Vertices representing some polygon</param>
        /// <param name="mass">Mass of the Body</param>
        /// <returns></returns>
        public Body CreatePolygonBody(PhysicsSimulator physicsSimulator, Vertices vertices, float mass)
        {
            Body body = CreatePolygonBody(vertices, mass);
            physicsSimulator.Add(body);
            return body;
        }

        /// <summary>
        /// Creates a Body.  The moment of inertia of the body is calculated from the
        /// set of vertices passed in to this method. The vertices should represent a polygon
        /// </summary>
        /// <param name="vertices">Vertices representing some polygon</param>
        /// <param name="mass">Mass of the Body</param>
        /// <returns></returns>
        public Body CreatePolygonBody(Vertices vertices, float mass)
        {
            Body body = new Body();
            body.Mass = mass;
            body.MomentOfInertia = mass * vertices.GetMomentOfInertia();
            body.position = vertices.GetCentroid();
            return body;
        }

        //misc
        public Body CreateBody(PhysicsSimulator physicsSimulator, float mass, float momentOfInertia)
        {
            Body body = CreateBody(mass, momentOfInertia);
            physicsSimulator.Add(body);
            return body;
        }

        public Body CreateBody(float mass, float momentOfInertia)
        {
            Body body = new Body();
            body.Mass = mass;
            body.MomentOfInertia = momentOfInertia;
            return body;
        }

        public Body CreateBody(PhysicsSimulator physicsSimulator, Body body)
        {
            Body bodyClone = CreateBody(body);
            physicsSimulator.Add(bodyClone);
            return bodyClone;
        }

        public Body CreateBody(Body body)
        {
            Body bodyClone = new Body(body);
            return bodyClone;
        }

        //ellipses
        /// <summary>
        /// Creates a ellipse body.
        /// </summary>
        /// <param name="physicsSimulator">The physics simulator.</param>
        /// <param name="xRadius">The width.</param>
        /// <param name="yRadius">The height.</param>
        /// <param name="mass">The mass.</param>
        /// <returns></returns>
        public Body CreateEllipseBody(PhysicsSimulator physicsSimulator, float xRadius, float yRadius, float mass)
        {
            Body body = CreateEllipseBody(xRadius, yRadius, mass);
            physicsSimulator.Add(body);
            return body;
        }

        /// <summary>
        /// Creates a ellipse body.
        /// </summary>
        /// <param name="xRadius">The width.</param>
        /// <param name="yRadius">The height.</param>
        /// <param name="mass">The mass.</param>
        /// <returns></returns>
        public Body CreateEllipseBody(float xRadius, float yRadius, float mass)
        {
            Body body = new Body();
            body.Mass = mass;

            if (xRadius == yRadius)
                body.MomentOfInertia = .5f * mass * (float)Math.Pow(xRadius, 2f);
            else
                //Note: This formular is for rectangles and not ellipses.
                body.MomentOfInertia = mass * (xRadius * xRadius + yRadius * yRadius) / 12;

            return body;
        }
    }
}