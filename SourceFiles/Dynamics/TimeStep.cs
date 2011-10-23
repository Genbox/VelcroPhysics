﻿/*
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

using Microsoft.Xna.Framework;

namespace FarseerPhysics.Dynamics
{
    /// <summary>
    /// This is an internal structure.
    /// </summary>
    public struct TimeStep
    {
        /// <summary>
        /// Time step (Delta time)
        /// </summary>
        public float dt;

        /// <summary>
        /// dt * inv_dt0
        /// </summary>
        public float dtRatio;

        /// <summary>
        /// Inverse time step (0 if dt == 0).
        /// </summary>
        public float inv_dt;
    }

    /// This is an internal structure.
    public struct b2Position
    {
        public Vector2 c;
        public float a;
    }

    /// This is an internal structure.
    public struct b2Velocity
    {
        public Vector2 v;
        public float w;
    }

    /// Solver Data
    public struct b2SolverData
    {
        public TimeStep step;
        public b2Position positions;
        public b2Velocity velocities;
    }
}