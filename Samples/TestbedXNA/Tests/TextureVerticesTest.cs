/*
* Farseer Physics Engine based on Box2D.XNA port:
* Copyright (c) 2010 Ian Qvist
* 
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

using System.Collections.Generic;
using FarseerPhysics.Common;
using FarseerPhysics.Common.Decomposition;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.TestBed.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerPhysics.TestBed.Tests
{
    public class TextureVerticesTest : Test
    {
        private List<Vertices> _list;
        private Texture2D _polygonTexture;
        private Vertices _verts;

        private TextureVerticesTest()
        {
            //Ground
            FixtureFactory.CreateEdge(World, new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));
        }

        public override void Initialize()
        {
            //load texture that will represent the physics body
            _polygonTexture = GameInstance.Content.Load<Texture2D>("Texture");

            //Create an array to hold the data from the texture
            uint[] data = new uint[_polygonTexture.Width * _polygonTexture.Height];

            //Transfer the texture data to the array
            _polygonTexture.GetData(data);

            //Find the vertices that makes up the outline of the shape in the texture
            _verts = PolygonTools.CreatePolygon(data, _polygonTexture.Width, _polygonTexture.Height, false);

            //For now we need to scale the vertices (result is in pixels, we use meters)
            Vector2 scale = new Vector2(0.07f, 0.07f);
            _verts.Scale(ref scale);

            //Since it is a concave polygon, we need to partition it into several smaller convex polygons
            _list = CDTDecomposer.ConvexPartition(_verts);

            //Create a single body with multiple fixtures
            List<Fixture> compund = FixtureFactory.CreateCompoundPolygon(World, _list, 1);
            compund[0].Body.BodyType = BodyType.Dynamic;

            List<Fixture> fixtures = FixtureFactory.CreateCapsule(World, 3, 1, 1);
            fixtures[0].Body.Position = new Vector2(-10, 15);
            fixtures[0].Body.BodyType = BodyType.Dynamic;

            FixtureFactory.CreateRoundedRectangle(World, 3, 3, 0.25F, 0.25F, 2, 1, new Vector2(-10, 10));

            base.Initialize();
        }

        public static Test Create()
        {
            return new TextureVerticesTest();
        }
    }
}