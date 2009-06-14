#if XNA
using FarseerPhysics.Collision;
using FarseerPhysics.Math;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MathHelper=Microsoft.Xna.Framework.MathHelper;

#else
using FarseerPhysics.Collision;
using FarseerPhysics.Math;
#endif

namespace FarseerPhysics.Dynamics.Controllers
{
    /// Calculates buoyancy forces for fluids in the form of a half plane.
    public class BuoyancyController : Controller
    {
        /// Linear drag co-efficient
        public float angularDrag;

        /// The fluid density
        public float density;

        /// Gravity vector, if the world's gravity is not used
        public Vector2 gravity;

        /// Linear drag co-efficient
        public float linearDrag;

        /// The outer surface normal
        public Vector2 normal;

        /// The height of the fluid surface along the normal
        public float offset;

        /// If false, bodies are assumed to be uniformly dense, otherwise use the shapes densities
        public bool useDensity; //False by default to prevent a gotcha

        /// If true, gravity is taken from the world instead of the gravity parameter.
        public bool useWorldGravity;

        /// Fluid velocity, for drag calculations
        public Vector2 velocity;

        public BuoyancyController()
        {
            normal = new Vector2(0, 1);
            offset = 0;
            density = 0;
            velocity = new Vector2(0, 0);
            linearDrag = 0;
            angularDrag = 0;
            useDensity = false;
            useWorldGravity = true;
            gravity = new Vector2(0, 0);
        }

        public override void Step(TimeStep step)
        {
            if (_bodyList == null) return;
            if (useWorldGravity)
            {
                gravity = _world.Gravity;
            }
            for (ControllerEdge i = _bodyList; i != null; i = i.nextBody)
            {
                Body body = i.body;
                if (body.IsSleeping())
                {
                    //Buoyancy force is just a function of position,
                    //so unlike most forces, it is safe to ignore sleeping bodes
                    continue;
                }
                Vector2 areac = new Vector2(0, 0);
                Vector2 massc = new Vector2(0, 0);
                float area = 0;
                float mass = 0;
                for (Shape shape = body.GetShapeList(); shape != null; shape = shape.GetNext())
                {
                    Vector2 sc;
                    float sarea = shape.ComputeSubmergedArea(normal, offset, body.GetXForm(), out sc);
                    area += sarea;
                    areac.X += sarea*sc.X;
                    areac.Y += sarea*sc.Y;
                    float shapeDensity = 0;
                    if (useDensity)
                    {
                        //TODO: Expose density publicly
                        shapeDensity = shape.Density;
                    }
                    else
                    {
                        shapeDensity = 1;
                    }
                    mass += sarea*shapeDensity;
                    massc.X += sarea*sc.X*shapeDensity;
                    massc.Y += sarea*sc.Y*shapeDensity;
                }
                areac.X /= area;
                areac.Y /= area;
                massc.X /= mass;
                massc.Y /= mass;
                if (area < Settings.FLT_EPSILON)
                    continue;
                //Buoyancy
                Vector2 buoyancyForce = -density*area*gravity;
                body.ApplyForce(buoyancyForce, massc);
                //Linear drag
                Vector2 dragForce = body.GetLinearVelocityFromWorldPoint(areac) - velocity;
                dragForce *= -linearDrag*area;
                body.ApplyForce(dragForce, areac);
                //Angular drag
                //TODO: Something that makes more physical sense?
                body.ApplyTorque(-body.GetInertia()/body.GetMass()*area*body.GetAngularVelocity()*angularDrag);
            }
        }

        public override void Draw(DebugDraw debugDraw)
        {
            float r = 1000;
            Vector2 p1 = offset*normal + Math.CommonMath.Cross(normal, r);
            Vector2 p2 = offset*normal - Math.CommonMath.Cross(normal, r);

            Color color = new Color(0, 0, 0.8f);

            debugDraw.DrawSegment(p1, p2, color);
        }
    }
}