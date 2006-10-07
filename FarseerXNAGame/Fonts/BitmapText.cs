using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework.Graphics;

namespace FarseerGames.FarseerXNAGame.Fonts {
    internal class BitmapText {
        private int _x;
        private int _y;
        private Color _color = Color.Black;
        private string _text;

        internal BitmapText(int x, int y, Color color, string text) {
            _x = x;
            _y = y;
            _color = color;
            _text = text;
        }

        public int X {
            get { return _x; }
            set { _x = value; }
        }

        public int Y {
            get { return _y; }
            set { _y = value; }
        }

        public Color Color {
            get { return _color; }
            set { _color = value; }
        }

        public string Text {
            get { return _text; }
            set { _text = value; }
        }
    }
}
