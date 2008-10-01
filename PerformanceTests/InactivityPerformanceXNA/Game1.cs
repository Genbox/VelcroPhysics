using System;
using System.Collections.Generic;
using FarseerGames.FarseerPhysics;
using FarseerPerformanceTest;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace InactivityPerformanceXNA
{
    public class Game1 : Game
    {
        private List<Box> _boxes;
        private GraphicsDeviceManager _graphics;
        private Ground _ground;
        private KeyboardState _oldKs;
        private MouseState _oldMs;
        private SpriteBatch _spriteBatch;
        private SpriteFont _spriteFont;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            IsMouseVisible = true;
            // we are using a fixed timestep. this normally causes problems when using farseer physics
            IsFixedTimeStep = true;
            TargetElapsedTime = new TimeSpan(0, 0, 0, 0, 1); //1 ms --> 1000 fps for physics update  

            _graphics.SynchronizeWithVerticalRetrace = false;
            _graphics.ApplyChanges();

            Globals.Physics = new PhysicsSimulator(new Vector2(0, 800));

            #region additional physics simulator setup

            // we prefer an update interval of 0.001 seconds
            // but allow the engine to use a maximum of 0.015 seconds if it needs more time
            Globals.Physics.Scaling = new Scaling(0.001f, 0.045f);
            Globals.Physics.Scaling.Enabled = true;

            // this distance has to be big enough to make sure all objects are reactivated 
            // before they could collide with an currently active object
            // our biggest object is 25*25 -> 120 would be big enough
            Globals.Physics.InactivityController.ActivationDistance = 120;
            // deactivate the object after 2 seconds of idle time
            Globals.Physics.InactivityController.MaxIdleTime = 2000;
            Globals.Physics.InactivityController.Enabled = false;
            // take a look at the box class for additional settings

            #endregion

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _spriteFont = Content.Load<SpriteFont>("arial");

            _boxes = new List<Box>();

            // add some boxes...
            for (int u = 1; u < 6; u++)
            {
                for (int i = 1; i < 14; i++)
                {
                    _boxes.Add(new Box(this, new Vector2(80 + i*28, 00 + u*28)));
                }
            }

            _ground = new Ground(new Vector2(400, 600));
        }

        protected override void Update(GameTime gameTime)
        {
            MouseState ms = Mouse.GetState();
            KeyboardState ks = Keyboard.GetState();

            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                Exit();
            }

            if (ms.LeftButton == ButtonState.Pressed && _oldMs.LeftButton == ButtonState.Released)
            {
                _boxes.Add(new Box(this, new Vector2(ms.X, ms.Y)));
                Console.WriteLine(_boxes.Count.ToString());
            }

            if (ks.IsKeyDown(Keys.F1) && _oldKs.IsKeyUp(Keys.F1))
            {
                Globals.Physics.Scaling.Enabled = !Globals.Physics.Scaling.Enabled;
            }

            if (ks.IsKeyDown(Keys.F2) && _oldKs.IsKeyUp(Keys.F2))
            {
                Globals.Physics.InactivityController.Enabled = !Globals.Physics.InactivityController.Enabled;
            }

            if (Globals.Physics.Scaling.Enabled)
            {
                Globals.Physics.Update(gameTime.ElapsedGameTime.Milliseconds*0.001f,
                                       gameTime.ElapsedRealTime.Milliseconds*0.001f);
            }
            else
            {
                Globals.Physics.Update(gameTime.ElapsedGameTime.Milliseconds*0.001f);
            }


            _oldMs = ms;
            _oldKs = ks;

            base.Update(gameTime);
        }


        protected override void Draw(GameTime gameTime)
        {
            _graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None);
            RenderBoxes();
            RenderInfo();
            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private void RenderBoxes()
        {
            Color color;

            foreach (Box b in _boxes)
            {
                // visualize the current box state
                if (b.Body.Enabled == false)
                {
                    color = Color.Green;
                }
                else
                {
                    if (b.Body.Moves == false)
                    {
                        color = Color.Yellow;
                    }
                    else
                    {
                        color = Color.Red;
                    }
                }
                // draw the box
                _spriteBatch.Draw(b.Texture, b.Position, null, color, b.Body.Rotation, b.Center, 1, SpriteEffects.None, 0);
            }
        }

        private void RenderInfo()
        {
            string info;

            info = "Physics update interval: " + Math.Round(Globals.Physics.Scaling.UpdateInterval, 4) +
                   Environment.NewLine;
            info += "Scaling: " + ((Globals.Physics.Scaling.Enabled) ? "enabled" : "disabled") +
                    " (press <F1> to toggle)" + Environment.NewLine;
            info += "InactivityController: " + ((Globals.Physics.InactivityController.Enabled) ? "enabled" : "disabled") +
                    " (press <F2> to toggle)" + Environment.NewLine;
            info += "Box count: " + _boxes.Count;


            // render the info string
            _spriteBatch.DrawString(_spriteFont, info, Vector2.Zero, Color.White);
        }
    }
}