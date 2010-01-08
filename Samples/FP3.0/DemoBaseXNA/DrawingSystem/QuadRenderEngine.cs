/*
 * QuadRenderEngine
 * Copyright (c) 2009-2010 Matthew Bettcher
 * 
 * Based on TriangleRenderHelper from SunBurn Framework.
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
 * 
 * TODO - 
 * 
 *   o Change transformations to use 3x3 Matrix and Vector2
 *   o Use Indexed TriangleList
 *   o Use Indexed TriangleStrip
 *   o Add support for sprite sheet textures
 */


using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DemoBaseXNA.DrawingSystem
{
    /// <summary>
    /// Maintains a list of quadrilaterals and transforms them and renders them as efficently as possible.
    /// </summary>
    public class QuadRenderEngine
    {
        public GraphicsDevice GraphicsDevice;
        public VertexDeclaration VertexDeclaration;

        private List<Quad> _quadList;
        private List<Texture2D> _textureList;
        private VertexPositionColorTexture[] _vertexArray;
        private Vector3[] _quadIdentity;
        private Vector3[] _tempArray;
        private int _vertexCount;
        private int _primitiveCount;
        private int[,] _cache;
        private int[] _cacheCount;
        private BasicEffect _effect;

        public QuadRenderEngine(GraphicsDevice graphicsDevice)
        {
            #warning These need to be adjustable not hard coded.
            _quadList = new List<Quad>(10000);
            _textureList = new List<Texture2D>(500);
            _vertexArray = new VertexPositionColorTexture[60000];
            // cache stores where each quad is stored in the _quadList based on it's texture
            _cache = new int[500, 10000];
            // cacheCount stores how many quads are being rendered per texture
            _cacheCount = new int[500];

            GraphicsDevice = graphicsDevice;
            VertexDeclaration = new VertexDeclaration(GraphicsDevice, VertexPositionColorTexture.VertexElements);
            // for now we are using BasicEffect, but this might change for performance reasons.
            _effect = new BasicEffect(GraphicsDevice, null);
            _effect.TextureEnabled = true;

            // create the _quadIdentity
            _quadIdentity = new Vector3[4];

            _quadIdentity[0].X = -0.5f;
            _quadIdentity[0].Y = -0.5f;
            _quadIdentity[0].Z = 0.0f;

            _quadIdentity[1].X = -0.5f;
            _quadIdentity[1].Y = 0.5f;
            _quadIdentity[1].Z = 0.0f;

            _quadIdentity[2].X = 0.5f;
            _quadIdentity[2].Y = 0.5f;
            _quadIdentity[2].Z = 0.0f;

            _quadIdentity[3].X = 0.5f;
            _quadIdentity[3].Y = -0.5f;
            _quadIdentity[3].Z = 0.0f;

            _tempArray = new Vector3[4];

            // temp for testing only this will become a variable
            _effect.Projection = Matrix.CreateOrthographic(1000, 1000, 0, 1);
        }


        /// <summary>
        /// Adds a texture to the texture list.
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="createMipMaps"></param>
        /// <returns>Index used to reference the texture.</returns>
        public int Submit(Texture2D texture, bool createMipMaps)
        {
            // if the user would like mipmaps generated for the texture
            if (createMipMaps)
                texture.GenerateMipMaps(TextureFilter.Anisotropic);

            // add the texture to the list
            _textureList.Add(texture);

            // return the index
            return _textureList.Count - 1;
        }

        /// <summary>
        /// Adds a quad to render later.
        /// </summary>
        /// <param name="quad"></param>
        public void Submit(Quad quad)
        {
            // based on the texture used...save where the quad was added to the quadList
            _cache[quad.TextureIndex, _cacheCount[quad.TextureIndex]] = _quadList.Count;
            // for each texture we need to know how many quads are being rendered
            _cacheCount[quad.TextureIndex]++;
            // add the quad to the list for processing
            _quadList.Add(quad);
            // be sure to increment the primitive count
            _primitiveCount += 2;
        }

        public void Render()
        {
            Matrix matrix;
            Quad quad;

            // for every texture...used or not?
            for (int i = 0; i < _textureList.Count; i++)
            {
                // for the number of quads rendered by the fisrt texture
                for (int j = 0; j < _cacheCount[i]; j++)
                {
                    // lookup the quad
                    quad = _quadList[_cache[i, j]];

                    // each quad knows everything about itself
                    // all we need to do is create and transform 2 triangles
                    matrix = Matrix.CreateScale(quad.Width, quad.Height, 1) * Matrix.CreateRotationZ(quad.Rotation) * Matrix.CreateTranslation(quad.Position.X, quad.Position.Y, 0);

                    Vector3.Transform(_quadIdentity, ref matrix, _tempArray);

                    _vertexArray[_vertexCount].Position = _tempArray[0];
                    _vertexArray[_vertexCount].TextureCoordinate.X = 0f;
                    _vertexArray[_vertexCount].TextureCoordinate.Y = 0f;
                    _vertexArray[_vertexCount].Color = quad.Tint;
                    _vertexCount++;

                    _vertexArray[_vertexCount].Position = _tempArray[1];
                    _vertexArray[_vertexCount].TextureCoordinate.X = 0f;
                    _vertexArray[_vertexCount].TextureCoordinate.Y = 1f;
                    _vertexArray[_vertexCount].Color = quad.Tint;
                    _vertexCount++;

                    _vertexArray[_vertexCount].Position = _tempArray[2];
                    _vertexArray[_vertexCount].TextureCoordinate.X = 1f;
                    _vertexArray[_vertexCount].TextureCoordinate.Y = 1f;
                    _vertexArray[_vertexCount].Color = quad.Tint;
                    _vertexCount++;

                    _vertexArray[_vertexCount].Position = _tempArray[0];
                    _vertexArray[_vertexCount].TextureCoordinate.X = 0f;
                    _vertexArray[_vertexCount].TextureCoordinate.Y = 0f;
                    _vertexArray[_vertexCount].Color = quad.Tint;
                    _vertexCount++;


                    _vertexArray[_vertexCount].Position = _tempArray[2];
                    _vertexArray[_vertexCount].TextureCoordinate.X = 1f;
                    _vertexArray[_vertexCount].TextureCoordinate.Y = 1f;
                    _vertexArray[_vertexCount].Color = quad.Tint;
                    _vertexCount++;

                    _vertexArray[_vertexCount].Position = _tempArray[3];
                    _vertexArray[_vertexCount].TextureCoordinate.X = 1f;
                    _vertexArray[_vertexCount].TextureCoordinate.Y = 0f;
                    _vertexArray[_vertexCount].Color = quad.Tint;
                    _vertexCount++;

                    
                }

                // this is 6 because we are useing triangle lists
                if (_vertexCount >= 6)
                {
                    _effect.Begin();
                    for (int k = 0; k < _effect.CurrentTechnique.Passes.Count; k++)
                    {
                        EffectPass pass = _effect.CurrentTechnique.Passes[0];
                        pass.Begin();

                        _effect.Texture = _textureList[i];
                        _effect.CommitChanges();

                        //GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionColorTexture>(PrimitiveType.TriangleList, _vertexArray, 0, _vertexCount, _indices, 0, _vertexCount / 3);

                        GraphicsDevice.DrawUserPrimitives<VertexPositionColorTexture>(PrimitiveType.TriangleList, _vertexArray, 0, _vertexCount / 3);

                        pass.End();
                    }
                    _effect.End();
                }

                // reset vertex  count
                _vertexCount = 0;
            }

            _quadList.Clear();

            // clear the cache count
            for (int i = 0; i < 500; i++)
            {
                _cacheCount[i] = 0;
            }
        }

        /*
        public void Render()
        {
            Matrix matrix;

            if (_primitiveCount > 0)
            {

                // loop thru all the quads and create the vertices for them
                foreach (var quad in _quadList)
                {
                    // each quad knows everything about itself
                    // all we need to do is create and transform 2 triangles
                    matrix = Matrix.CreateScale(quad.Width, quad.Height, 1) * Matrix.CreateRotationZ(quad.Rotation) * Matrix.CreateTranslation(quad.Position.X, quad.Position.Y, 0);

                    Vector3.Transform(_quadIdentity, ref matrix, _tempArray);

                    _vertexArray[_vertexCount].Position = _tempArray[0];
                    _vertexArray[_vertexCount].TextureCoordinate.X = 0f;
                    _vertexArray[_vertexCount].TextureCoordinate.Y = 0f;
                    _vertexArray[_vertexCount].Color = quad.Tint;
                    _vertexCount++;

                    _vertexArray[_vertexCount].Position = _tempArray[1];
                    _vertexArray[_vertexCount].TextureCoordinate.X = 0f;
                    _vertexArray[_vertexCount].TextureCoordinate.Y = 1f;
                    _vertexArray[_vertexCount].Color = quad.Tint;
                    _vertexCount++;

                    _vertexArray[_vertexCount].Position = _tempArray[2];
                    _vertexArray[_vertexCount].TextureCoordinate.X = 1f;
                    _vertexArray[_vertexCount].TextureCoordinate.Y = 1f;
                    _vertexArray[_vertexCount].Color = quad.Tint;
                    _vertexCount++;

                    _vertexArray[_vertexCount].Position = _tempArray[0];
                    _vertexArray[_vertexCount].TextureCoordinate.X = 0f;
                    _vertexArray[_vertexCount].TextureCoordinate.Y = 0f;
                    _vertexArray[_vertexCount].Color = quad.Tint;
                    _vertexCount++;

                    _vertexArray[_vertexCount].Position = _tempArray[2];
                    _vertexArray[_vertexCount].TextureCoordinate.X = 1f;
                    _vertexArray[_vertexCount].TextureCoordinate.Y = 1f;
                    _vertexArray[_vertexCount].Color = quad.Tint;
                    _vertexCount++;

                    _vertexArray[_vertexCount].Position = _tempArray[3];
                    _vertexArray[_vertexCount].TextureCoordinate.X = 1f;
                    _vertexArray[_vertexCount].TextureCoordinate.Y = 0f;
                    _vertexArray[_vertexCount].Color = quad.Tint;
                    _vertexCount++;
                }


                // temp for testing only
                _effect.Projection = Matrix.CreateOrthographic(1000, 1000, 0, 1);

                // this is 6 because we are useing triangle lists
                if (_vertexCount >= 6)
                {
                    _effect.Begin();
                    for (int i = 0; i < _effect.CurrentTechnique.Passes.Count; i++)
                    {
                        EffectPass pass = _effect.CurrentTechnique.Passes[0];
                        pass.Begin();

                        _effect.Texture = _textureList[0];
                        _effect.CommitChanges();

                        GraphicsDevice.DrawUserPrimitives<VertexPositionColorTexture>(PrimitiveType.TriangleList, _vertexArray, 0, _primitiveCount);

                        pass.End();
                    }
                    _effect.End();
                }
            }
            _quadList.Clear();
            _vertexCount = 0;
            _primitiveCount = 0;

            // clear the cache count
            for (int i = 0; i < 50; i++)
            {
                _cacheCount[i] = 0;
            }
        }
         * */
    }
}