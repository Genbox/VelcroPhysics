using System;
using System.Collections.Generic;
using System.Text;

namespace TestBed
{
    public static class TestEntries
    {
        public static TestEntry[] _testEntries = new TestEntry[]
		{		
	        new TestEntry("Polygon Shapes", PolyShapes.Create),
	        new TestEntry("Apply Force", ApplyForce.Create),
	        new TestEntry("Cantilever", Cantilever.Create),
	        new TestEntry("Body Types", BodyTypes.Create),
            new TestEntry("CCD Test", CCDTest.Create),
	        new TestEntry("SphereStack", SphereStack.Create),
            new TestEntry("Vertical Stack", VerticalStack.Create),
            new TestEntry("Confined", Confined.Create),
	        new TestEntry("Bridge", Bridge.Create),
	        new TestEntry("Breakable", Breakable.Create),
	        new TestEntry("Varying Restitution", VaryingRestitution.Create),
            new TestEntry("Ray-Cast", RayCast.Create),
	        new TestEntry("Pyramid", Pyramid.Create),     
            new TestEntry("PolyCollision", PolyCollision.Create),
        	new TestEntry("One-Sided Platform", OneSidedPlatform.Create), 
            new TestEntry("Chain", Chain.Create),
	        new TestEntry("Collision Filtering", CollisionFiltering.Create),
	        new TestEntry("Collision Processing", CollisionProcessing.Create),
	        new TestEntry("Compound Shapes", CompoundShapes.Create),
	        new TestEntry("Distance Test", DistanceTest.Create),
	        new TestEntry("Dominos", Dominos.Create),
	        new TestEntry("Dynamic Tree", DynamicTreeTest.Create),
	        new TestEntry("Gears", Gears.Create),
	        new TestEntry("Line Joint", LineJoint.Create),
	        new TestEntry("Prismatic", Prismatic.Create),
	        new TestEntry("Pulleys", Pulleys.Create),
	        new TestEntry("Revolute", Revolute.Create),
		    new TestEntry("Sensor Test", SensorTest.Create),
	        new TestEntry("Shape Editing", ShapeEditing.Create),
	        new TestEntry("Slider Crank", SliderCrank.Create),
	        new TestEntry("Theo Jansen's Walker", TheoJansen.Create),
	        new TestEntry("Time of Impact", TimeOfImpact.Create),
	        new TestEntry("Varying Friction", VaryingFriction.Create),
	        new TestEntry("Web", Web.Create),
		};
    }
}
