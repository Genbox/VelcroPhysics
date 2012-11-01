namespace FarseerPhysics
{
    /// <summary>
    /// This is the base class for all game objects
    /// </summary>
    public class DrawableGameComponent
    {
        public DrawableGameComponent(Game game)
        {
            Game = game;
        }

        public Game Game { get; private set; }

        protected virtual void Initialize() { }

        protected virtual void LoadContent() { }

        protected virtual void UnloadContent() { }

        public virtual void Update(GameTime gameTime) { }

        public virtual void Draw(GameTime gameTime) { }
    }
}
