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

using System;
using Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Framework;
using Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Tests;
using Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Tests.Velcro;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Testbed
{
    public static class TestEntries
    {
        public static Func<Test>[] TestList =
        {
            //Original tests
            AddPairTest.Create,
            ApplyForceTest.Create,
            BodyTypesTest.Create,
            BoxStackTest.Create,
            BreakableTest.Create,
            BridgeTest.Create,
            BulletTest.Create,
            CantileverTest.Create,
            CarTest.Create,
            ChainProblemTest.Create,
            ChainTest.Create,
            CharacterCollisionTest.Create,
            CircleStackTest.Create,
            CollisionFilteringTest.Create,
            CollisionProcessingTest.Create,
            CompoundShapesTest.Create,
            ConfinedTest.Create,
            ContinuousTest.Create,
            ConvexHullTest.Create,
            ConveyorBeltTest.Create,
            DistanceJointTest.Create,
            DistanceTest.Create,
            DominosTest.Create,
            DumpLoaderTest.Create,
            DynamicTreeTest.Create,
            EdgeShapesTest.Create,
            EdgeTest.Create,
            FrictionTest.Create,
            GearJointTest.Create,
            Heavy1Test.Create,
            Heavy2Test.Create,
            MobileBalancedTest.Create,
            MobileUnbalancedTest.Create,
            MotorJointTest.Create,
            PinballTest.Create,
            PlatformerTest.Create,
            PolygonCollisionTest.Create,
            PolygonShapesTest.Create,
            PrismaticJointTest.Create,
            PulleyJointTest.Create,
            PyramidTest.Create,
            RayCastTest.Create,
            RestitutionTest.Create,
            RevoluteJointTest.Create,
            SensorsTest.Create,
            ShapeCastTest.Create,
            ShapeEditingTest.Create,
            SkierTest.Create,
            SliderCrank1Test.Create,
            SliderCrank2Test.Create,
            TheoJansenTest.Create,
            TilesTest.Create,
            TimeOfImpactTest.Create,
            TumblerTest.Create,
            WebTest.Create,
            WheelJointTest.Create,
            WreckingBallTest.Create,

            //
            //  Velcro tests
            //
            AngleJointTest.Create,
            BuoyancyTest.Create,
            CheckPolygonTest.Create,
            CircleBenchmarkTest.Create,
            CirclePenetrationTest.Create,
            CollisionCallbackTest.Create,
            ConvexHull2Test.Create,
            CuttingTest.Create,
            DeletionTest.Create,
            DestructibleTerrainTest.Create,
            ExplosionTest.Create,
            GravityControllerTest.Create,
            LockTest.Create,
            PathTest.Create,
            RockBreakTest.Create,
            RoundedRectangleTest.Create,
            SimpleWindForceTest.Create,
            SimplificationTest.Create,
            TextureVerticesTest.Create,
            TriangulationTest.Create,
            YuPengPolygonTest.Create,

            //
            // Disabled for now
            //
            //CloneTest.Create,
            //SerializationTest.Create,

            null
        };
    }
}