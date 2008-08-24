using System;
using System.Collections.Generic;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Mathematics;

namespace FarseerGames.FarseerPhysics.Controllers
{
    public class FluidDragController : Controller
    {
        #region Delegates

        public delegate void OnEntryHandler(Geom geom);

        #endregion

        private readonly Dictionary<Geom, bool> geomInFluidList;

        private readonly List<Geom> geomList;
        private float area;
        private Vector2 axis = Vector2.Zero;
        private Vector2 buoyancyForce = Vector2.Zero;
        private Vector2 centroid = Vector2.Zero;

        //Note: Cleanup, variable never used
        private float centroidSpeed;
        private Vector2 centroidVelocity;
        private float density;
        private float dragArea;
        private IFluidContainer fluidContainer;
        private Vector2 gravity = Vector2.Zero;
        private float linearDragCoefficient;
        private Vector2 linearDragForce = Vector2.Zero;
        private Vector2 localCentroid = Vector2.Zero;
        private float max;
        private float min;
        public OnEntryHandler OnEntry;
        private float partialMass;
        private float rotationalDragCoeficient;
        private float rotationalDragTorque;
        private float totalArea;
        private Vector2 totalForce;
        private Vector2 vert;
        private Vertices vertices;

        public FluidDragController()
        {
            geomList = new List<Geom>();
            geomInFluidList = new Dictionary<Geom, bool>();
        }

        public void Initialize(IFluidContainer fluidContainer, float density, float linearDragCoeficient,
                               float rotationalDragCoeficient, Vector2 gravity)
        {
            this.fluidContainer = fluidContainer;
            this.density = density;
            linearDragCoefficient = linearDragCoeficient;
            this.rotationalDragCoeficient = rotationalDragCoeficient;
            this.gravity = gravity;
            vertices = new Vertices();
        }

        public void AddGeom(Geom geom)
        {
            geomList.Add(geom);
            geomInFluidList.Add(geom, false);
        }

        public override void Validate()
        {
            //do nothing
        }

        public void Reset()
        {
            geomInFluidList.Clear();
            foreach (Geom geom in geomList)
            {
                geomInFluidList.Add(geom, false);
            }
        }

        public override void Update(float dt)
        {
            for (int i = 0; i < geomList.Count; i++)
            {
                totalArea = geomList[i].localVertices.GetArea();
                if (!fluidContainer.Intersect(geomList[i].aabb)) continue;
                FindVerticesInFluid(geomList[i]);
                if (vertices.Count < 3) continue;

                area = vertices.GetArea();
                if (area < .000001) continue;

                centroid = vertices.GetCentroid(area);

                CalculateBuoyancy();

                CalculateDrag(geomList[i]);

                Vector2.Add(ref buoyancyForce, ref linearDragForce, out totalForce);
                geomList[i].body.ApplyForceAtWorldPoint(ref totalForce, ref centroid);

                geomList[i].body.ApplyTorque(rotationalDragTorque);

                if (geomInFluidList[geomList[i]] == false)
                {
                    geomInFluidList[geomList[i]] = true;
                    if (OnEntry != null)
                    {
                        OnEntry(geomList[i]);
                    }
                }
            }
        }

        private void FindVerticesInFluid(Geom geom)
        {
            vertices.Clear();
            for (int i = 0; i < geom.worldVertices.Count; i++)
            {
                vert = geom.worldVertices[i];
                if (fluidContainer.Contains(ref vert))
                {
                    vertices.Add(vert);
                }
            }
        }

        private void CalculateAreaAndCentroid()
        {
            area = vertices.GetArea();

            centroid = vertices.GetCentroid(area);
        }

        private void CalculateBuoyancy()
        {
            buoyancyForce = -gravity*area*density;
        }


        private void CalculateDrag(Geom geom)
        {
            //localCentroid = geom.body.GetLocalPosition(centroid);
            geom.body.GetVelocityAtWorldPoint(ref centroid, out centroidVelocity);

            axis.X = -centroidVelocity.Y;
            axis.Y = centroidVelocity.X;
            //can't normalize a zero length vector
            if (axis.X != 0 || axis.Y != 0)
            {
                axis.Normalize();
            }

            vertices.ProjectToAxis(ref axis, out min, out max);

            dragArea = Math.Abs(max - min);

            partialMass = geom.body.mass*(area/totalArea);

            linearDragForce = -.5f*density*dragArea*linearDragCoefficient*partialMass*centroidVelocity;

            rotationalDragTorque = -geom.body.angularVelocity*rotationalDragCoeficient*partialMass;
        }
    }
}