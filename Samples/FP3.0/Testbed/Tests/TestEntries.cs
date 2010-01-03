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

using FarseerPhysics.TestBed.Framework;

namespace FarseerPhysics.TestBed.Tests
{
    public static class TestEntries
    {
        public static TestEntry[] g_testEntries =
            {
                new TestEntry {name = "Collision Filtering", createFcn = CollisionFilteringTest.Create},
                new TestEntry {name = "Web", createFcn = WebTest.Create},
                new TestEntry {name = "Body Types", createFcn = BodyTypesTest.Create},
                new TestEntry {name = "Pyramid", createFcn = PyramidTest.Create},
                new TestEntry {name = "Theo Jansen's Walker", createFcn = TheoJansenTest.Create},
                new TestEntry {name = "Shape Editing", createFcn = ShapeEditingTest.Create},
                new TestEntry {name = "Sensor Test", createFcn = SensorTest.Create},
                new TestEntry {name = "Prismatic", createFcn = PrismaticTest.Create},
                new TestEntry {name = "Compound Shapes", createFcn = CompoundShapes.Create},
                new TestEntry {name = "Cantilever", createFcn = CantileverTest.Create},
                new TestEntry {name = "SphereStack", createFcn = SphereStackTest.Create},
                new TestEntry {name = "Collision Processing", createFcn = CollisionProcessingTest.Create},
                new TestEntry {name = "Polygon Shapes", createFcn = PolyShapesTest.Create},
                new TestEntry {name = "Apply Force", createFcn = ApplyForceTest.Create},
                new TestEntry {name = "Bridge", createFcn = BridgeTest.Create},
                new TestEntry {name = "CCD Test", createFcn = CCDTest.Create},
                new TestEntry {name = "Chain", createFcn = ChainTest.Create},
                new TestEntry {name = "Distance Test", createFcn = DistanceTest.Create},
                new TestEntry {name = "Dominos", createFcn = DominosTest.Create},
                new TestEntry {name = "Dynamic Tree", createFcn = DynamicTreeTest.Create},
                new TestEntry {name = "Gears", createFcn = GearsTest.Create},
                new TestEntry {name = "Line Joint", createFcn = LineJointTest.Create},
                new TestEntry {name = "PolyCollision", createFcn = PolyCollisionTest.Create},
                new TestEntry {name = "Pulleys", createFcn = PulleysTest.Create},
                new TestEntry {name = "Revolute", createFcn = RevoluteTest.Create},
                new TestEntry {name = "Slider Crank", createFcn = SliderCrankTest.Create},
                new TestEntry {name = "Time of Impact", createFcn = TimeOfImpactTest.Create},
                new TestEntry {name = "Varying Friction", createFcn = VaryingFrictionTest.Create},
                new TestEntry {name = "Varying Restitution", createFcn = VaryingRestitutionTest.Create},
                new TestEntry {name = "Vertical Stack", createFcn = VerticalStackTest.Create},
                new TestEntry {name = "Confined", createFcn = ConfinedTest.Create},
                new TestEntry {name = "Breakable", createFcn = BreakableTest.Create},
                new TestEntry {name = "Ray-Cast", createFcn = RayCastTest.Create},
                new TestEntry {name = "One-Sided Platform", createFcn = OneSidedPlatformTest.Create},
                new TestEntry {name = null, createFcn = null}
            };
    }
}