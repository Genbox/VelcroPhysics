/*
* Box2D.XNA port of Box2D:
* Copyright (c) 2009 Brandon Furtwangler, Nathan Furtwangler
*
* Original source Box2D:
* Copyright (c) 2006-2009 Erin Catto http://www.gphysics.com 
* 
* This software is provided 'as-is', without any express or implied 
* warranty.  In no event will the authors be held liable for any damages 
* arising from the use of this software. 
* Permission is granted to anyone to use this software for any purpose, 
* including commercial applications, and to alter it and redistribute it 
* freely, subject to the following restrictions: 
* 1. The origin of this software must not be misrepresented; you must not 
* claim that you wrote the original software. If you use this software 
* in a product, an acknowledgment in the product documentation would be 
* appreciated but is not required. 
* 2. Altered source versions must be plainly marked as such, and must not be 
* misrepresented as being the original software. 
* 3. This notice may not be removed or altered from any source distribution. 
*/

using System;
using Box2D.XNA.TestBed.Framework;

namespace Box2D.XNA.TestBed.Tests
{
    public static class TestEntries
    {
        public static TestEntry[] g_testEntries =
        {
            new TestEntry() { name="Polygon Shapes", createFcn=PolyShapes.Create},
            new TestEntry() { name="Apply Force", createFcn=ApplyForce.Create},
            new TestEntry() { name="Cantilever", createFcn=Cantilever.Create},
            new TestEntry() { name="Body Types", createFcn=BodyTypes.Create},
            new TestEntry() { name="Pyramid", createFcn=Pyramid.Create},
	        new TestEntry() { name="Bridge", createFcn=Bridge.Create},
	        new TestEntry() { name="CCD Test", createFcn=CCDTest.Create},
	        new TestEntry() { name="Chain", createFcn=Chain.Create},
	        new TestEntry() { name="Collision Filtering", createFcn=CollisionFiltering.Create},
	        new TestEntry() { name="Collision Processing", createFcn=CollisionProcessing.Create},
	        new TestEntry() { name="Compound Shapes", createFcn=CompoundShapes.Create},
	        new TestEntry() { name="Distance Test", createFcn=DistanceTest.Create},
	        new TestEntry() { name="Dominos", createFcn=Dominos.Create},
	        new TestEntry() { name="Dynamic Tree", createFcn=DynamicTreeTest.Create},
	        new TestEntry() { name="Gears", createFcn=Gears.Create},
	        new TestEntry() { name="Line Joint", createFcn=LineJoint.Create},
	        new TestEntry() { name="PolyCollision", createFcn=PolyCollision.Create},
	        new TestEntry() { name="Prismatic", createFcn=Prismatic.Create},
	        new TestEntry() { name="Pulleys", createFcn=Pulleys.Create},
	        new TestEntry() { name="Revolute", createFcn=Revolute.Create},
	        new TestEntry() { name="Sensor Test", createFcn=SensorTest.Create},
	        new TestEntry() { name="Shape Editing", createFcn=ShapeEditing.Create},
            new TestEntry() { name="SphereStack", createFcn=SphereStack.Create},
	        new TestEntry() { name="Slider Crank", createFcn=SliderCrank.Create},
	        new TestEntry() { name="Theo Jansen's Walker", createFcn=TheoJansen.Create},
	        new TestEntry() { name="Time of Impact", createFcn=TimeOfImpact.Create},
	        new TestEntry() { name="Varying Friction", createFcn=VaryingFriction.Create},
	        new TestEntry() { name="Varying Restitution", createFcn=VaryingRestitution.Create},
	        new TestEntry() { name="Vertical Stack", createFcn=VerticalStack.Create},
	        new TestEntry() { name="Web", createFcn=Web.Create},
            new TestEntry() { name="Confined", createFcn=Confined.Create},
            new TestEntry() { name="Breakable", createFcn=Breakable.Create},
            new TestEntry() { name="Ray-Cast", createFcn=RayCast.Create},
            new TestEntry() { name="One-Sided Platform", createFcn=OneSidedPlatform.Create},
	        new TestEntry() { name=null, createFcn=null}
        };
    }
}
