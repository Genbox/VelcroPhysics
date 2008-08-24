using System;
using System.Collections.Generic;
using System.Text;

#if (XNA)
using Microsoft.Xna.Framework; 
#endif

using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Mathematics;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Collisions;

namespace FarseerGames.FarseerPhysics.Dynamics {
    public class BodyFactory {
        private static BodyFactory instance;

        private BodyFactory() { }

        public static BodyFactory Instance {
            get {
                if (instance == null) { instance = new BodyFactory(); }
                return instance;
            }
        }

        //rectangles
        public Body CreateRectangleBody(PhysicsSimulator physicsSimulator, float width, float height, float mass) {
            Body body = CreateRectangleBody(width, height, mass);
            physicsSimulator.Add(body);
            return body;
        }

        public Body CreateRectangleBody(float width, float height, float mass) {
            Body body = new Body();
            body.Mass = mass;
            body.MomentOfInertia = mass * (width * width + height * height) / 12;
            return body;
        }

        //circles
        public Body CreateCircleBody(PhysicsSimulator physicsSimulator, float radius, float mass) {
            Body body = CreateCircleBody(radius, mass);
            physicsSimulator.Add(body);
            return body;
        }

        public Body CreateCircleBody(float radius, float mass) {
            Body body = new Body();
            body.Mass = mass;
            body.MomentOfInertia = .5f * mass * (float)Math.Pow((double)radius, 2f);
            return body;
        }

        /// <summary>
        /// Creates a Body.  The moment of inertia of the body is calculated from the
        /// set of vertices passed in to this method. The vertices should represent a polygon
        /// </summary>
        /// <param name="physicsSimulator">PhysicsSimulator to add this body to.</param>
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
        public Body CreateBody(PhysicsSimulator physicsSimulator,float mass, float momentOfInertia) {
            Body body = CreateBody(mass, momentOfInertia);
            physicsSimulator.Add(body);
            return body;
        }

        public Body CreateBody(float mass, float momentOfInertia) {
            Body body = new Body();
            body.Mass = mass;
            body.MomentOfInertia = momentOfInertia;
            return body;
        }
        
        public Body CreateBody(PhysicsSimulator physicsSimulator, Body body) {
            Body bodyClone = CreateBody(body);
            physicsSimulator.Add(bodyClone);
            return bodyClone;
        }

        public Body CreateBody(Body body) {
            Body bodyClone = new Body(body);
            return bodyClone;
        }

        public static float MOIForRectangle(float width, float height, float mass){
            return mass * (width * width + height * height) / 12;
        }
    }
}
