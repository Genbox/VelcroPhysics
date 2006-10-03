using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using FarseerGames.FarseerXNAGame.Drawing;

namespace FarseerGames.FarseerXNATestharness.EntityViews {
    class BoxShape : PolygonShape, IEntityView {
        float _width;
        float _height;

        public BoxShape(Game game, float width, float height)
            : base(game) {
            World = Matrix.Identity;
            _width = width;
            _height = height;
            Create();
        }

        /// <summary>
        /// Load up the data for the retro ship
        /// </summary>
        /// <param name="data">The vertex buffer</param>
        protected override void FillBuffer(VertexPositionColor[] data) {
            data[0] = new VertexPositionColor(new Vector3(-_width*.5f, -_height*.5f, 0f), Color.White);
            data[1] = new VertexPositionColor(new Vector3(-_width * .5f, _height * .5f, 0f), Color.White);
            data[2] = data[1];
            data[3] = new VertexPositionColor(new Vector3(_width * .5f, _height * .5f, 0f), Color.White);
            data[4] = data[3];
            data[5] = new VertexPositionColor(new Vector3(_width * .5f, -_height * .5f, 0f), Color.White);
            data[6] = data[5];
            data[7] = data[0];
        }

        /// <summary>
        /// Number of vectors in retro ship
        /// </summary>
        protected override int NumberOfVectors {
            get {
                return 4;
            }
        }

        public override void Draw() {
            base.Draw();
        }
    }
}


