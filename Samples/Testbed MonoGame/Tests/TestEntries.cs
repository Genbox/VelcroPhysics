/*
* Farseer Physics Engine:
* Copyright (c) 2012 Ian Qvist
* 
* Original source Box2D:
* Copyright (c) 2006-2011 Erin Catto http://www.box2d.org 
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

using FarseerPhysics.Testbed.Framework;

namespace FarseerPhysics.Testbed.Tests
{
    public static class TestEntries
    {
        public static TestEntry[] TestList =
        {
            //Original tests
            new TestEntry {Name = "Continuous Test", CreateTest = ContinuousTest.Create},
            new TestEntry {Name = "Time of Impact", CreateTest = TimeOfImpactTest.Create},
            new TestEntry {Name = "Motor joint", CreateTest = MotorJointTest.Create},
            new TestEntry {Name = "One-Sided Platform", CreateTest = OneSidedPlatformTest.Create},
            //new TestEntry {Name = "Dump Shell", CreateTest = DumpShellTest.Create},
            new TestEntry {Name = "Mobile", CreateTest = MobileTest.Create},
            new TestEntry {Name = "MobileBalanced", CreateTest = MobileBalancedTest.Create},
            new TestEntry {Name = "Ray-Cast", CreateTest = RayCastTest.Create},
            new TestEntry {Name = "Conveyor Belt", CreateTest = ConveyorBeltTest.Create},
            new TestEntry {Name = "Gears", CreateTest = GearsTest.Create},
            new TestEntry {Name = "ConvexHull", CreateTest = ConvexHullTest.Create},
            new TestEntry {Name = "Varying Restitution", CreateTest = VaryingRestitutionTest.Create},
            new TestEntry {Name = "Tumbler", CreateTest = TumblerTest.Create},
            new TestEntry {Name = "Tiles", CreateTest = TilesTest.Create},
            new TestEntry {Name = "Cantilever", CreateTest = CantileverTest.Create},
            new TestEntry {Name = "Character collision", CreateTest = CharacterCollisionTest.Create},
            new TestEntry {Name = "Edge Test", CreateTest = EdgeTest.Create},
            new TestEntry {Name = "Body Types", CreateTest = BodyTypesTest.Create},
            new TestEntry {Name = "Shape Editing", CreateTest = ShapeEditingTest.Create},
            new TestEntry {Name = "Car", CreateTest = CarTest.Create},
            new TestEntry {Name = "Apply Force", CreateTest = ApplyForceTest.Create},
            new TestEntry {Name = "Prismatic", CreateTest = PrismaticTest.Create},
            new TestEntry {Name = "Vertical Stack", CreateTest = VerticalStackTest.Create},
            new TestEntry {Name = "SphereStack", CreateTest = SphereStackTest.Create},
            new TestEntry {Name = "Revolute", CreateTest = RevoluteTest.Create},
            new TestEntry {Name = "Pulleys", CreateTest = PulleysTest.Create},
            new TestEntry {Name = "Polygon Shapes", CreateTest = PolyShapesTest.Create},
            new TestEntry {Name = "Web", CreateTest = WebTest.Create},
            new TestEntry {Name = "RopeJoint", CreateTest = RopeTest.Create},
            new TestEntry {Name = "Pinball", CreateTest = PinballTest.Create},
            new TestEntry {Name = "Bullet Test", CreateTest = BulletTest.Create},
            new TestEntry {Name = "Confined", CreateTest = ConfinedTest.Create},
            new TestEntry {Name = "Pyramid", CreateTest = PyramidTest.Create},
            new TestEntry {Name = "Theo Jansen's Walker", CreateTest = TheoJansenTest.Create},
            new TestEntry {Name = "Edge Shapes", CreateTest = EdgeShapesTest.Create},
            new TestEntry {Name = "PolyCollision", CreateTest = PolyCollisionTest.Create},
            new TestEntry {Name = "Bridge", CreateTest = BridgeTest.Create},
            new TestEntry {Name = "Breakable", CreateTest = BreakableTest.Create},
            new TestEntry {Name = "Chain", CreateTest = ChainTest.Create},
            new TestEntry {Name = "Collision Filtering", CreateTest = CollisionFilteringTest.Create},
            new TestEntry {Name = "Collision Processing", CreateTest = CollisionProcessingTest.Create},
            new TestEntry {Name = "Compound Shapes", CreateTest = CompoundShapesTest.Create},
            new TestEntry {Name = "Distance Test", CreateTest = DistanceTest.Create},
            new TestEntry {Name = "Dominos", CreateTest = DominosTest.Create},
            new TestEntry {Name = "Dynamic Tree", CreateTest = DynamicTreeTest.Create},
            new TestEntry {Name = "Sensor Test", CreateTest = SensorTest.Create},
            new TestEntry {Name = "Slider Crank", CreateTest = SliderCrankTest.Create},
            new TestEntry {Name = "Varying Friction", CreateTest = VaryingFrictionTest.Create},
#if WINDOWS
            new TestEntry {Name = "Add Pair Stress Test", CreateTest = AddPairTest.Create},
#endif
            //FPE tests
            new TestEntry {Name = "YuPeng Polygon", CreateTest = YuPengPolygonTest.Create},
            new TestEntry {Name = "Path Test", CreateTest = PathTest.Create},
            new TestEntry {Name = "Cutting of polygons", CreateTest = CuttingTest.Create},
            new TestEntry {Name = "Gravity Controller Test", CreateTest = GravityControllerTest.Create},
            new TestEntry {Name = "Texture to Vertices", CreateTest = TextureVerticesTest.Create},
            new TestEntry {Name = "Rounded rectangle", CreateTest = RoundedRectangle.Create},
            new TestEntry {Name = "Angle Joint", CreateTest = AngleJointTest.Create},
            new TestEntry {Name = "Explosion", CreateTest = ExplosionTest.Create},
            new TestEntry {Name = "Lock Test", CreateTest = LockTest.Create},
            new TestEntry {Name = "Sphere benchmark", CreateTest = CircleBenchmarkTest.Create},
            new TestEntry {Name = "Edgeshape benchmark", CreateTest = EdgeShapeBenchmarkTest.Create},
            new TestEntry {Name = "Circle penetration", CreateTest = CirclePenetrationTest.Create},
            new TestEntry {Name = "Clone Test", CreateTest = CloneTest.Create},
            new TestEntry {Name = "Serialization Test", CreateTest = SerializationTest.Create},
            new TestEntry {Name = "Deletion test", CreateTest = DeletionTest.Create},
            new TestEntry {Name = "Buoyancy test", CreateTest = BuoyancyTest.Create},
            new TestEntry {Name = "Convex hull test", CreateTest = ConvexHullTest2.Create},
            new TestEntry {Name = "Simple Wind Force Test", CreateTest = SimpleWindForceTest.Create},
            new TestEntry {Name = "Simplification", CreateTest = SimplificationTest.Create},
#if WINDOWS
            new TestEntry {Name = "Triangulation", CreateTest = TriangulationTest.Create},
            new TestEntry {Name = "Destructible Terrain Test", CreateTest = DestructibleTerrainTest.Create},
#endif
            new TestEntry {Name = "Check polygon", CreateTest = CheckPolygonTest.Create},
            new TestEntry {Name = null, CreateTest = null}
        };
    }
}