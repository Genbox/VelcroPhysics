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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerPhysics.TestBed.Tests
{
    public class TextureVerticesTest : Test
    {
        private Texture2D _polygonTexture;
        private Body _polygonBody;

        private TextureVerticesTest()
        {
            {
                Body ground = World.CreateBody();

                Vertices edge = PolygonTools.CreateEdge(new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));
                PolygonShape shape = new PolygonShape(edge, 0);
                ground.CreateFixture(shape);
            }
        }

        public override void Initialize()
        {
            //load texture that will represent the physics body
            _polygonTexture = GameInstance.Content.Load<Texture2D>("Texture");

            //Create an array to hold the data from the texture
            uint[] data = new uint[_polygonTexture.Width * _polygonTexture.Height];

            //Transfer the texture data to the array
            _polygonTexture.GetData(data);

            Vertices verts = PolygonTools.CreatePolygon(data, _polygonTexture.Width, _polygonTexture.Height);
            Vector2 scale = new Vector2(0.1f, 0.1f);
            PolygonTools.Scale(ref verts, ref scale);

            vertices = verts.ToArray();

            //PolygonShape diamondShape = new PolygonShape(verts, 100);

            //Use the body factory to create the physics body
            //_polygonBody = World.CreateBody();
            //_polygonBody.Position = new Vector2(0, 0);
            //_polygonBody.CreateFixture(diamondShape);

            base.Initialize();
        }

        private Vector2[] vertices;
        public override void Step(FarseerPhysics.TestBed.Framework.Settings settings)
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                _debugView.DrawCircle(vertices[i],0.1f,Color.White);
                
            }

            base.Step(settings);
        }

        public static Test Create()
        {
            return new TextureVerticesTest();
        }
    }
}