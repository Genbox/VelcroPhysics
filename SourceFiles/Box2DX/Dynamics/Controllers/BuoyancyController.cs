using System;
using System.Collections.Generic;
using System.Text;

using Box2DX.Common;
using Box2DX.Collision;

namespace Box2DX.Dynamics.Controllers
{
	/// Calculates buoyancy forces for fluids in the form of a half plane.
	public class BuoyancyController : Controller
	{
		/// The outer surface normal
		public Vec2 normal;
		/// The height of the fluid surface along the normal
		public float offset;
		/// The fluid density
		public float density;
		/// Fluid velocity, for drag calculations
		public Vec2 velocity;
		/// Linear drag co-efficient
		public float linearDrag;
		/// Linear drag co-efficient
		public float angularDrag;
		/// If false, bodies are assumed to be uniformly dense, otherwise use the shapes densities
		public bool useDensity; //False by default to prevent a gotcha
		/// If true, gravity is taken from the world instead of the gravity parameter.
		public bool useWorldGravity;
		/// Gravity vector, if the world's gravity is not used
		public Vec2 gravity;

		public BuoyancyController(){
			normal = new Vec2(0,1);
			offset = 0;
			density =0;
			velocity = new Vec2(0,0);
			linearDrag=0;
			angularDrag=0;
			useDensity = false;
			useWorldGravity = true;
			gravity = new Vec2(0,0);
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
				Vec2 areac = new Vec2(0, 0);
				Vec2 massc = new Vec2(0, 0);
				float area = 0;
				float mass = 0;
				for (Shape shape = body.GetShapeList(); shape != null; shape = shape.GetNext())
				{
					Vec2 sc;
					float sarea = shape.ComputeSubmergedArea(normal, offset, body.GetXForm(), out sc);
					area += sarea;
					areac.X += sarea * sc.X;
					areac.Y += sarea * sc.Y;
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
					mass += sarea * shapeDensity;
					massc.X += sarea * sc.X * shapeDensity;
					massc.Y += sarea * sc.Y * shapeDensity;
				}
				areac.X /= area;
				areac.Y /= area;
				massc.X /= mass;
				massc.Y /= mass;
				if (area < Box2DX.Common.Settings.FLT_EPSILON)
					continue;
				//Buoyancy
				Vec2 buoyancyForce = -density * area * gravity;
				body.ApplyForce(buoyancyForce, massc);
				//Linear drag
				Vec2 dragForce = body.GetLinearVelocityFromWorldPoint(areac) - velocity;
				dragForce *= -linearDrag * area;
				body.ApplyForce(dragForce, areac);
				//Angular drag
				//TODO: Something that makes more physical sense?
				body.ApplyTorque(-body.GetInertia() / body.GetMass() * area * body.GetAngularVelocity() * angularDrag);
			}
		}

		public override void Draw(DebugDraw debugDraw)
		{
			float r = 1000;
			Vec2 p1 = offset * normal + Vec2.Cross(normal, r);
			Vec2 p2 = offset * normal - Vec2.Cross(normal, r);

			Color color = new Color(0,0,0.8f);

			debugDraw.DrawSegment(p1, p2, color);
		}
	}
}