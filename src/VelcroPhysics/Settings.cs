/*
* Velcro Physics:
* Copyright (c) 2017 Ian Qvist
* 
* MIT License
*
* Copyright (c) 2019 Erin Catto
*
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included in all
* copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*/

using System;
using Genbox.VelcroPhysics.Collision.Filtering;
using Genbox.VelcroPhysics.Utilities;

namespace Genbox.VelcroPhysics
{
    public static class Settings
    {
        /* Velcro */

        /// <summary>Set this to true to skip sanity checks in the engine. This will speed up the tools by removing the overhead of
        /// the checks, but you will need to handle checks yourself where it is needed.</summary>
        public static bool SkipSanityChecks = false;

        /// <summary>If true, it will run a GiftWrap convex hull on all polygon inputs. This makes for a more stable engine when
        /// given random input, but if speed of the creation of polygons are more important, you might want to set this to false.</summary>
        public static bool UseConvexHullPolygons = true;

        /// <summary>Velcro Physics has a different way of filtering fixtures than Box2d. We have both FPE and Box2D filtering in
        /// the engine. If you are upgrading from earlier versions of FPE, set this to true and DefaultFixtureCollisionCategories
        /// to Category.All.</summary>
        public static bool UseFPECollisionCategories;

        /// <summary>This is used by the Fixture constructor as the default value for Fixture.CollisionCategories member. Note that
        /// you may need to change this depending on the setting of UseFPECollisionCategories, above.</summary>
        public static Category DefaultFixtureCollisionCategories = Category.Cat1;

        /// <summary>This is used by the Fixture constructor as the default value for Fixture.CollidesWith member.</summary>
        public static Category DefaultFixtureCollidesWith = Category.All;

        /// <summary>This is used by the Fixture constructor as the default value for Fixture.IgnoreCCDWith member.</summary>
        public static Category DefaultFixtureIgnoreCCDWith = Category.None;

        /// <summary>
        /// Set the default collision group
        /// </summary>
        public static short DefaultCollisionGroup = 0;

        //Velcro: Moved the maximum number of iterations to Settings
        /// <summary>Defines the maximum number of iterations made by the GJK algorithm.</summary>
        public static int MaxGJKIterations = 20;

        /* Common */

        /// <summary>This is used to fatten AABBs in the dynamic tree. This allows proxies to move by a small amount without
        /// triggering a tree adjustment. This is in meters.</summary>
        public static float AABBExtension = 0.1f;

        /// <summary>This is used to fatten AABBs in the dynamic tree. This is used to predict the future position based on the
        /// current displacement. This is a dimensionless multiplier.</summary>
        public static float AABBMultiplier = 4.0f;

        /// <summary>A small length used as a collision and constraint tolerance. Usually it is chosen to be numerically
        /// significant, but visually insignificant.</summary>
        public static float LinearSlop = 0.005f;

        /// <summary>A small angle used as a collision and constraint tolerance. Usually it is chosen to be numerically
        /// significant, but visually insignificant.</summary>
        public static float AngularSlop = 2.0f / 180.0f * MathConstants.Pi;

        /// <summary>The radius of the polygon/edge shape skin. This should not be modified. Making this smaller means polygons
        /// will have an insufficient buffer for continuous collision. Making it larger may create artifacts for vertex collision.</summary>
        public static float PolygonRadius = 2.0f * LinearSlop;

        /// <summary>Maximum number of sub-steps per contact in continuous physics simulation.</summary>
        public static int MaxSubSteps = 8;

        // Dynamics

        /// <summary>Maximum number of contacts to be handled to solve a TOI impact.</summary>
        public static int MaxTOIContacts = 32;

        /// <summary>The maximum linear position correction used when solving constraints. This helps to prevent overshoot.</summary>
        public static float MaxLinearCorrection = 0.2f;

        /// <summary>The maximum angular position correction used when solving constraints. This helps to prevent overshoot.</summary>
        public static float MaxAngularCorrection = 8.0f / 180.0f * MathConstants.Pi;

        /// <summary>The maximum linear velocity of a body. This limit is very large and is used to prevent numerical problems. You
        /// shouldn't need to adjust this.</summary>
        public static float MaxTranslation = 2.0f;

        /// <summary>The maximum angular velocity of a body. This limit is very large and is used to prevent numerical problems.
        /// You shouldn't need to adjust this.</summary>
        public static float MaxRotation = 0.5f * MathConstants.Pi;

        /// <summary>This scale factor controls how fast overlap is resolved. Ideally this would be 1 so that overlap is removed in
        /// one time step. However using values close to 1 often lead to overshoot.</summary>
        public static float Baumgarte = 0.2f;

        public static float TOIBaumgarte = 0.75f;

        /* Sleep */

        /// <summary>The time that a body must be still before it will go to sleep.</summary>
        public static float TimeToSleep = 0.5f;

        /// <summary>A body cannot sleep if its linear velocity is above this tolerance.</summary>
        public static float LinearSleepTolerance = 0.01f;

        /// <summary>A body cannot sleep if its angular velocity is above this tolerance.</summary>
        public static float AngularSleepTolerance = 2.0f / 180.0f * MathConstants.Pi;

        /// <summary>By default, forces are cleared automatically after each call to Step. The default behavior is modified with
        /// this setting. The purpose of this setting is to support sub-stepping. Sub-stepping is often used to maintain a fixed
        /// sized time step under a variable frame-rate. When you perform sub-stepping you should disable auto clearing of forces
        /// and instead call ClearForces after all sub-steps are complete in one pass of your game loop.</summary>
        public static bool AutoClearForces = true;

        /// <summary>The maximum number of vertices on a convex polygon.</summary>
        public static int MaxPolygonVertices = 8;

        /* Internals */

        /// <summary>The maximum number of contact points between two convex shapes. DO NOT CHANGE THIS VALUE!</summary>
        internal const int MaxManifoldPoints = 2;

        //Velcro: Moved EnableSubStepping from TimeStep to Settings
        /// <summary>Enable or disable sub stepping. Used for debugging.</summary>
        internal const bool EnableSubStepping = false;

        //Velcro: Moved this value out of the contact solver and into Settings
        /// <summary>Enable or disable the block contact solver. Used for debugging.</summary>
        internal const bool BlockSolve = true;

        /// <summary>Gets called when there is a collision between 2 fixtures. The first two parameters are the friction values of
        /// either fixture. The return value should be the desired friction value of the collection.</summary>
        public static Func<float, float, float> MixFriction = DefaultMixFriction;

        /// <summary>Gets called when there is a collision between 2 fixtures. The first two parameters are the restitution values
        /// of either fixture. The return value should be the desired restitution value of the collection.</summary>
        public static Func<float, float, float> MixRestitution = DefaultMixRestitution;

        public static Func<float, float, float> MixRestitutionThreshold = DefaultMixRestitutionThreshold;

        /// <summary>Friction mixing law. The idea is to allow either fixture to drive the friction to zero. For example, anything
        /// slides on ice.</summary>
        private static float DefaultMixFriction(float friction1, float friction2)
        {
            return (float)Math.Sqrt(friction1 * friction2);
        }

        /// <summary>Restitution mixing law. The idea is allow for anything to bounce off an inelastic surface. For example, a
        /// superball bounces on anything.</summary>
        private static float DefaultMixRestitution(float restitution1, float restitution2)
        {
            return restitution1 > restitution2 ? restitution1 : restitution2;
        }

        /// <summary>Restitution mixing law. This picks the lowest value.</summary>
        private static float DefaultMixRestitutionThreshold(float threshold1, float threshold2)
        {
            return threshold1 < threshold2 ? threshold1 : threshold2;
        }
    }
}