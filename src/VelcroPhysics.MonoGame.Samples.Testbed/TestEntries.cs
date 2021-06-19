/*
* Velcro Physics:
* Copyright (c) 2017 Ian Qvist
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

using Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Framework;
using Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Tests;
using Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Tests.Velcro;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Testbed
{
    public static class TestEntries
    {
        public static TestEntry[] TestList =
        {
            //Original tests
            new TestEntry { Name = "Add Pair", CreateTest = AddPairTest.Create },
            new TestEntry { Name = "Apply Force", CreateTest = ApplyForceTest.Create },
            new TestEntry { Name = "Body Types", CreateTest = BodyTypesTest.Create },
            new TestEntry { Name = "Box Stack", CreateTest = BoxStackTest.Create },
            new TestEntry { Name = "Breakable", CreateTest = BreakableTest.Create },
            new TestEntry { Name = "Bridge", CreateTest = BridgeTest.Create },
            new TestEntry { Name = "BulletTest", CreateTest = BulletTest.Create },
            new TestEntry { Name = "CantileverTest", CreateTest = CantileverTest.Create },
            new TestEntry { Name = "CarTest", CreateTest = CarTest.Create },
            new TestEntry { Name = "ChainProblemTest", CreateTest = ChainProblemTest.Create },
            new TestEntry { Name = "ChainTest", CreateTest = ChainTest.Create },
            new TestEntry { Name = "CharacterCollisionTest", CreateTest = CharacterCollisionTest.Create },
            new TestEntry { Name = "CircleStackTest", CreateTest = CircleStackTest.Create },
            //new TestEntry { Name = "CollisionFilteringTest", CreateTest = CollisionFilteringTest.Create },
            new TestEntry { Name = "CollisionProcessingTest", CreateTest = CollisionProcessingTest.Create },
            //new TestEntry { Name = "CompoundShapesTest", CreateTest = CompoundShapesTest.Create },
            new TestEntry { Name = "ConfinedTest", CreateTest = ConfinedTest.Create },
            new TestEntry { Name = "ContinuousTest", CreateTest = ContinuousTest.Create },
            //new TestEntry { Name = "ConvexHullTest", CreateTest = ConvexHullTest.Create },
            new TestEntry { Name = "ConveyorBeltTest", CreateTest = ConveyorBeltTest.Create },
            new TestEntry { Name = "DistanceJointTest", CreateTest = DistanceJointTest.Create },
            //new TestEntry { Name = "DistanceTest", CreateTest = DistanceTest.Create },
            new TestEntry { Name = "DominosTest", CreateTest = DominosTest.Create },
            new TestEntry { Name = "DumpLoaderTest", CreateTest = DumpLoaderTest.Create },
            new TestEntry { Name = "DynamicTreeTest", CreateTest = DynamicTreeTest.Create },
            new TestEntry { Name = "EdgeShapesTest", CreateTest = EdgeShapesTest.Create },
            new TestEntry { Name = "EdgeTest", CreateTest = EdgeTest.Create },
            new TestEntry { Name = "FrictionTest", CreateTest = FrictionTest.Create },
            new TestEntry { Name = "GearJointTest", CreateTest = GearJointTest.Create },
            new TestEntry { Name = "Heavy1Test", CreateTest = Heavy1Test.Create },
            new TestEntry { Name = "heavy2Test", CreateTest = Heavy2Test.Create },
            new TestEntry { Name = "MobileBalanced", CreateTest = MobileBalancedTest.Create },
            new TestEntry { Name = "MobileUnbalanced", CreateTest = MobileUnbalancedTest.Create },
            new TestEntry { Name = "MotorJoint", CreateTest = MotorJointTest.Create },
            new TestEntry { Name = "PinballTest", CreateTest = PinballTest.Create },
            new TestEntry { Name = "PlatformerTest", CreateTest = PlatformerTest.Create },
            new TestEntry { Name = "PolygonCollisionTest", CreateTest = PolygonCollisionTest.Create },
            //new TestEntry { Name = "PolygonShapesTest", CreateTest = PolygonShapesTest.Create },
            new TestEntry { Name = "PrismaticJointTest", CreateTest = PrismaticJointTest.Create },
            new TestEntry { Name = "PulleyJoint", CreateTest = PulleyJointTest.Create },
            new TestEntry { Name = "PyramidTest", CreateTest = PyramidTest.Create },
            new TestEntry { Name = "RayCastTest", CreateTest = RayCastTest.Create },
            new TestEntry { Name = "RevoluteJointTest", CreateTest = RevoluteJointTest.Create },
            //new TestEntry { Name = "SensorTest", CreateTest = SensorsTest.Create },
            new TestEntry { Name = "ShapeCastTest", CreateTest = ShapeCastTest.Create },
            new TestEntry { Name = "ShapeEditingTest", CreateTest = ShapeEditingTest.Create },
            new TestEntry { Name = "SkierTest", CreateTest = SkierTest.Create },
            new TestEntry { Name = "SliderCrank1Test", CreateTest = SliderCrank1Test.Create },
            new TestEntry { Name = "SliderCrank2Test", CreateTest = SliderCrank2Test.Create },
            //new TestEntry { Name = "TheoJansenTest", CreateTest = TheoJansenTest.Create },
            new TestEntry { Name = "TilesTest", CreateTest = TilesTest.Create },
            new TestEntry { Name = "TimeOfImpactTest", CreateTest = TimeOfImpactTest.Create },
            new TestEntry { Name = "TumblerTest", CreateTest = TumblerTest.Create },
            new TestEntry { Name = "WebTest", CreateTest = WebTest.Create },
            new TestEntry { Name = "WheelJointTest", CreateTest = WheelJointTest.Create },
            new TestEntry { Name = "WreckingBallTest", CreateTest = WreckingBallTest.Create },

            //Velcro tests
            new TestEntry { Name = "AngleJointTest", CreateTest = AngleJointTest.Create },
            new TestEntry { Name = "BuoyancyTest", CreateTest = BuoyancyTest.Create },
            new TestEntry { Name = "CheckPolygonTest", CreateTest = CheckPolygonTest.Create },
            new TestEntry { Name = "CircleBenchmarkTest", CreateTest = CircleBenchmarkTest.Create },
            new TestEntry { Name = "CirclePenetrationTest", CreateTest = CirclePenetrationTest.Create },
            //new TestEntry { Name = "CloneTest", CreateTest = CloneTest.Create },
            new TestEntry { Name = "CollisionCallbackTest", CreateTest = CollisionCallbackTest.Create },
            //new TestEntry { Name = "ConvexHull2Test", CreateTest = ConvexHull2Test.Create },
            new TestEntry { Name = "CuttingTest", CreateTest = CuttingTest.Create },
            //new TestEntry { Name = "DeletionTest", CreateTest = DeletionTest.Create },
            //new TestEntry { Name = "DestructibleTerrainTest", CreateTest = DestructibleTerrainTest.Create },
            new TestEntry { Name = "ExplosionTest", CreateTest = ExplosionTest.Create },
            new TestEntry { Name = "GravityControllerTest", CreateTest = GravityControllerTest.Create },
            new TestEntry { Name = "LockTest", CreateTest = LockTest.Create },
            new TestEntry { Name = "PathTest", CreateTest = PathTest.Create },
            new TestEntry { Name = "RockBreakTest", CreateTest = RockBreakTest.Create },
            new TestEntry { Name = "RoundedRectangleTest", CreateTest = RoundedRectangleTest.Create },
            //new TestEntry { Name = "SerializationTest", CreateTest = SerializationTest.Create },
            new TestEntry { Name = "SimpleWindForceTest", CreateTest = SimpleWindForceTest.Create },
            new TestEntry { Name = "SimplificationTest", CreateTest = SimplificationTest.Create },
            new TestEntry { Name = "TextureVerticesTest", CreateTest = TextureVerticesTest.Create },
            //new TestEntry { Name = "TriangulationTest", CreateTest = TriangulationTest.Create },
            new TestEntry { Name = "YuPengPolygonTest", CreateTest = YuPengPolygonTest.Create },
            new TestEntry { Name = null, CreateTest = null }
        };
    }
}