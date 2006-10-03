using Microsoft.Xna.Framework;
using System;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerGames.FarseerXNAGame.Drawing {
    /// <summary>
    /// Shape is the base class of any object that is renderable
    /// </summary>
    public abstract class Shape  {
        /// <summary>
        /// The vertex buffer used by this shape
        /// </summary>
        protected VertexBuffer buffer;
        protected VertexDeclaration vertexDecl;

        /// <summary>
        /// The current world matrix used to render this shape
        /// </summary>
        public Matrix World;

        private Game game = null;

        protected Game GameInstance {
            get {
                return game;
            }
        }

        /// <summary>
        /// Creates a new shape. Calls the virtual Create method to generate any vertex buffers etc
        /// </summary>
        public Shape(Game game) {
            this.game = game;
        }

        /// <summary>
        /// Creates the vertex buffers etc. This routine is called on object creation and on device reset etc
        /// </summary>
        abstract public void Create();

        /// <summary>
        /// Renders the shape. Base class does nothing
        /// </summary>
        public virtual void Draw() {
            //TODO: Needs to be put into a default sahder somehow
            //Game.GraphicsDevice.Transform.World = World;
        }

        /// <summary>
        /// Updates the shape. Base class does nothing
        /// </summary>
        /// <param name="time">Game Time</param>
        /// <param name="elapsedTime">Elapsed game time since last call</param>
        public virtual void Update(TimeSpan time, TimeSpan elapsedTime) {
            //Nothing for now
        }

        abstract public void Reset();

        /// <summary>
        /// Called when a device is created
        /// </summary>
        public virtual void Reset(Game game) {
            this.game = game;
            Reset();
            Create();
        }
    }
}
