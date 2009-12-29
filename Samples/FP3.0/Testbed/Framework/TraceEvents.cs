﻿/*
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.TestBed.Framework
{
    public static class TraceEvents
    {
        public static void Register(IEventTrace et)
        {
            FrameEventId = et.Register("Frame", Color.Orange);
            UpdateEventId = et.Register("Update", Color.Blue);
            PhysicsEventId = et.Register("Physics", Color.Cyan);
            DrawEventId = et.Register("Draw", Color.Red);
        }

        public static int FrameEventId { get; private set; }
        public static int UpdateEventId { get; private set; }
        public static int PhysicsEventId { get; private set; }
        public static int DrawEventId { get; private set; }
    }
}