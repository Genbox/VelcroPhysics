using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using FarseerGames.FarseerXNAPhysics;

namespace FarseerGames.FarseerXNATestharness.Entities {
    class Pyrmid {
        private BoxEntity[] _boxEntityList = new BoxEntity[78];

        public Pyrmid(Game game, PhysicsSimulator physics, Vector2 bottomLeft, float boxWidth, float padding) {
            BuildPyrmid(game, physics, boxWidth,bottomLeft, padding);
        }

        private void BuildPyrmid(Game game, PhysicsSimulator physics, float boxWidth, Vector2 bottomLeft, float padding) {
            int n = 0;
            Vector2 X = bottomLeft;
            Vector2 Y = X;
            for (int i = 0; i < 12; ++i) {
                Y = X;

                for (int j = i; j < 12; ++j) {
                    _boxEntityList[n] = new BoxEntity(game, boxWidth, boxWidth,1, new Vector2(Y.X, Y.Y),0, false);
                    physics.Add(_boxEntityList[n]);
                    Y += new Vector2(boxWidth * 1.125f, 0.0f);
                    n += 1;
                }

                X += new Vector2(boxWidth * 0.5625f, boxWidth *- 1.1f);
            }

        }

        public void Draw() {
            for (int i = 0; i < _boxEntityList.Length; i++) {
                _boxEntityList[i].Draw();
            }
        }


    }
}
