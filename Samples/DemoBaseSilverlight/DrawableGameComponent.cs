using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace FarseerPhysics.DemoBaseSilverlight
{
    /// <summary>
    /// This is the base class for all game objects
    /// </summary>
    public class DrawableGameComponent
    {
        public Game Game { get; private set; }

        public DrawableGameComponent(Game game)
        {
            Game = game;
        }

        protected virtual void Initialize() { }

        protected virtual void LoadContent() { }

        protected virtual void UnloadContent() { }

        public virtual void Update(GameTime gameTime) { }

        public virtual void Draw(GameTime gameTime) { }
    }
}
