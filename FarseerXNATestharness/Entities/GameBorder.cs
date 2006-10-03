using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using FarseerGames.FarseerXNAPhysics;

namespace FarseerGames.FarseerXNATestharness.Entities {
    class GameBorder {
        BoxEntity _leftBorder;
        BoxEntity _bottomBorder;
        BoxEntity _rightBorder;
        BoxEntity _topBorder;

        float borderThickness = 1;

        public GameBorder(Game game, PhysicsSimulator physics,  float width, float height) {
            _leftBorder = new BoxEntity(game, borderThickness, height + 2*borderThickness,1,new Vector2(-width * .5f + borderThickness * .5f,0f),0, true);
            _bottomBorder = new BoxEntity(game, width, borderThickness,1,new Vector2(0,height*.5f + borderThickness *.5f), 0,true);
            _rightBorder = new BoxEntity(game, borderThickness, height + 2 * borderThickness,1, new Vector2(width * .5f - borderThickness * .5f, 0f), 0,true);
            _topBorder = new BoxEntity(game, width, borderThickness,1, new Vector2(0, -height * .5f - borderThickness * .5f),0, true);

            physics.Add(_leftBorder);
            physics.Add(_bottomBorder);
            physics.Add(_rightBorder);
            physics.Add(_topBorder);
        }

        public void Draw() {
            _leftBorder.Draw();
            _rightBorder.Draw();
            _bottomBorder.Draw();
            _topBorder.Draw();
        }

    }
}
