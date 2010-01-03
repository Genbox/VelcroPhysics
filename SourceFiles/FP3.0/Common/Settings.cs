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

namespace FarseerPhysics
{
    public static class Settings
    {
        public const float MaxFloat = 3.402823466e+38f;
        public const float Epsilon = 1.192092896e-07f;
        public const float Pi = 3.14159265359f;

        /// The maximum number of vertices on a convex polygon.
        public const int MaxPolygonVertices = 8;

        /// This is used to fatten AABBs in the dynamic tree. This allows proxies
        /// to move by a small amount without triggering a tree adjustment.
        /// This is in meters.
        public const float AabbExtension = 0.1f;

        /// This is used to fatten AABBs in the dynamic tree. This is used to predict
        /// the future position based on the current displacement.
        /// This is a dimensionless multiplier.
        public const float AabbMultiplier = 2.0f;

        /// A small length used as a collision and constraint tolerance. Usually it is
        /// chosen to be numerically significant, but visually insignificant.
        public const float LinearSlop = 0.005f;

        /// A small angle used as a collision and constraint tolerance. Usually it is
        /// chosen to be numerically significant, but visually insignificant.
        public const float AngularSlop = (2.0f / 180.0f * Pi);

        /// The radius of the polygon/edge shape skin. This should not be modified. Making
        /// this smaller means polygons will have and insufficient for continuous collision.
        /// Making it larger may create artifacts for vertex collision.
        public const float PolygonRadius = (2.0f * LinearSlop);

        // Dynamics

        /// Maximum number of contacts to be handled to solve a TOI island.
        public const int MaxTOIContactsPerIsland = 32;

        /// Maximum number of joints to be handled to solve a TOI island.
        public const int MaxTOIJointsPerIsland = 32;

        /// A velocity threshold for elastic collisions. Any collision with a relative linear
        /// velocity below this threshold will be treated as inelastic.
        public const float VelocityThreshold = 1.0f;

        /// The maximum linear position correction used when solving constraints. This helps to
        /// prevent overshoot.
        public const float MaxLinearCorrection = 0.2f;

        /// The maximum angular position correction used when solving constraints. This helps to
        /// prevent overshoot.
        public const float MaxAngularCorrection = (8.0f / 180.0f * Pi);

        /// The maximum linear velocity of a body. This limit is very large and is used
        /// to prevent numerical problems. You shouldn't need to adjust this.
        public const float MaxTranslation = 2.0f;
        public const float MaxTranslationSquared = (MaxTranslation * MaxTranslation);

        /// The maximum angular velocity of a body. This limit is very large and is used
        /// to prevent numerical problems. You shouldn't need to adjust this.
        public const float MaxRotation = (0.5f * Pi);
        public const float MaxRotationSquared = (MaxRotation * MaxRotation);

        /// This scale factor controls how fast overlap is resolved. Ideally this would be 1 so
        /// that overlap is removed in one time step. However using values close to 1 often lead
        /// to overshoot.
        public const float ContactBaumgarte = 0.2f;

        // Sleep

        /// The time that a body must be still before it will go to sleep.
        public const float TimeToSleep = 0.5f;

        /// A body cannot sleep if its linear velocity is above this tolerance.
        public const float LinearSleepTolerance = 0.01f;

        /// A body cannot sleep if its angular velocity is above this tolerance.
        public const float AngularSleepTolerance = (2.0f / 180.0f * Pi);

        /// Friction mixing law. Feel free to customize this.
        public static float MixFriction(float friction1, float friction2)
        {
	        return (float)Math.Sqrt((friction1 * friction2));
        }

        /// Restitution mixing law. Feel free to customize this.
        public static float MixRestitution(float restitution1, float restitution2)
        {
	        return restitution1 > restitution2 ? restitution1 : restitution2;
        }
    }
}
