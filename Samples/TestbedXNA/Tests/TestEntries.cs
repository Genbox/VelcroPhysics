/*
* Farseer Physics Engine based on Box2D.XNA port:
* Copyright (c) 2010 Ian Qvist
* 
* Box2D.XNA port of Box2D:
* Copyright (c) 2009 Brandon Furtwangler, Nathan Furtwangler
*
* Original source Box2D:
* Copyright (c) 2006-2009 Erin Catto http://www.box2d.org 
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
        public static TestEntry[] TestList =
            {
                //Original tests
                new TestEntry {Name = "Car test", CreateFcn = CarTest.Create},
                new TestEntry {Name = "Rope Test", CreateFcn = RopeTest.Create},
                new TestEntry {Name = "Character collision", CreateFcn = CharacterCollisionTest.Create},
                new TestEntry {Name = "Edge Test", CreateFcn = EdgeTest.Create},
                new TestEntry {Name = "One-Sided Platform", CreateFcn = OneSidedPlatformTest.Create},
                new TestEntry {Name = "Pinball", CreateFcn = PinballTest.Create},
                new TestEntry {Name = "Bullet Test", CreateFcn = BulletTest.Create},
                new TestEntry {Name = "Continuous Test", CreateFcn = ContinuousTest.Create},
                //Can crash the application on Xbox360
#if (!XBOX360)
                new TestEntry {Name = "Tiles", CreateFcn = TilesTest.Create},
#endif
                new TestEntry {Name = "Web", CreateFcn = WebTest.Create},
                new TestEntry {Name = "Vertical Stack", CreateFcn = VerticalStackTest.Create},
                new TestEntry {Name = "Time of Impact", CreateFcn = TimeOfImpactTest.Create},
                new TestEntry {Name = "Ray-Cast", CreateFcn = RayCastTest.Create},
                new TestEntry {Name = "Confined", CreateFcn = ConfinedTest.Create},
                new TestEntry {Name = "Pyramid", CreateFcn = PyramidTest.Create},
                new TestEntry {Name = "Varying Restitution", CreateFcn = VaryingRestitutionTest.Create},
                new TestEntry {Name = "Theo Jansen's Walker", CreateFcn = TheoJansenTest.Create},
                new TestEntry {Name = "Body Types", CreateFcn = BodyTypesTest.Create},
                new TestEntry {Name = "Prismatic", CreateFcn = PrismaticTest.Create},
                new TestEntry {Name = "Edge Shapes", CreateFcn = EdgeShapes.Create},
                new TestEntry {Name = "PolyCollision", CreateFcn = PolyCollisionTest.Create},
                new TestEntry {Name = "Polygon Shapes", CreateFcn = PolyShapesTest.Create},
                new TestEntry {Name = "Apply Force", CreateFcn = ApplyForceTest.Create},
                new TestEntry {Name = "Cantilever", CreateFcn = CantileverTest.Create},
                new TestEntry {Name = "SphereStack", CreateFcn = SphereStackTest.Create},
                new TestEntry {Name = "Bridge", CreateFcn = BridgeTest.Create},
                new TestEntry {Name = "Chain", CreateFcn = ChainTest.Create},
                new TestEntry {Name = "Collision Filtering", CreateFcn = CollisionFilteringTest.Create},
                new TestEntry {Name = "Collision Processing", CreateFcn = CollisionProcessingTest.Create},
                new TestEntry {Name = "Compound Shapes", CreateFcn = CompoundShapes.Create},
                new TestEntry {Name = "Distance Test", CreateFcn = DistanceTest.Create},
                new TestEntry {Name = "Dominos", CreateFcn = DominosTest.Create},
                new TestEntry {Name = "Dynamic Tree", CreateFcn = DynamicTreeTest.Create},
                new TestEntry {Name = "Gears", CreateFcn = GearsTest.Create},
                new TestEntry {Name = "Wheel Joint", CreateFcn = WheelJointTest.Create},
                new TestEntry {Name = "Pulleys", CreateFcn = PulleysTest.Create},
                new TestEntry {Name = "Revolute", CreateFcn = RevoluteTest.Create},
                new TestEntry {Name = "Sensor Test", CreateFcn = SensorTest.Create},
                new TestEntry {Name = "Shape Editing", CreateFcn = ShapeEditingTest.Create},
                new TestEntry {Name = "Slider Crank", CreateFcn = SliderCrankTest.Create},
                new TestEntry {Name = "Varying Friction", CreateFcn = VaryingFrictionTest.Create},
                //FPE tests
                new TestEntry {Name = "YuPeng Polygon", CreateFcn = YuPengPolygonTest.Create},
                new TestEntry {Name = "Path Test", CreateFcn = PathTest.Create},
                new TestEntry {Name = "Cutting of polygons", CreateFcn = CuttingTest.Create},
                new TestEntry {Name = "Controllers", CreateFcn = ControllerTest.Create},
                new TestEntry {Name = "Texture to Vertices", CreateFcn = TextureVerticesTest.Create},
                new TestEntry {Name = "Rounded rectangle", CreateFcn = RoundedRectangle.Create},
                new TestEntry {Name = "Angle Joint", CreateFcn = AngleJointTest.Create},
                new TestEntry {Name = "Slider Joint", CreateFcn = SliderJointTest.Create},
                new TestEntry {Name = "Breakable", CreateFcn = BreakableTest.Create},
                new TestEntry {Name = "Explosion", CreateFcn = ExplosionTest.Create},
                new TestEntry {Name = "Lock Test", CreateFcn = LockTest.Create},
                new TestEntry {Name = "Sphere benchmark", CreateFcn = CircleBenchmarkTest.Create},
                new TestEntry {Name = "Edgeshape benchmark", CreateFcn = EdgeShapeBenchmark.Create},
                new TestEntry {Name = "Circle penetration", CreateFcn = CirclePenetrationTest.Create},
                new TestEntry {Name = "Clone Test", CreateFcn = CloneTest.Create},
                //Can crash the application on Xbox360
#if (!XBOX360)
                new TestEntry {Name = "Serialization Test", CreateFcn = SerializationTest.Create},
#endif
                new TestEntry {Name = "Destructible Terrain YuPeng Test", CreateFcn = DestructibleTerrainYuPengTest.Create},
                new TestEntry {Name = "Destructible Terrain MS Test", CreateFcn = DestructibleTerrainMSTest.Create},
                new TestEntry {Name = "Deletion test", CreateFcn = DeletionTest.Create},
                new TestEntry {Name = "Buoyancy test", CreateFcn = BuoyancyTest.Create},
                new TestEntry {Name = "Convex hull test", CreateFcn = ConvexHullTest.Create},
                new TestEntry {Name = "Simple Wind Force Test", CreateFcn = SimpleWindForceTest.Create},
                new TestEntry {Name = "Quad Tree BroadPhase test", CreateFcn = QuadTreeTest.Create},
                new TestEntry {Name = null, CreateFcn = null}
            };
    }
}