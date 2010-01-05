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

using System;
using FarseerPhysics.TestBed.Framework;
using FarseerPhysics.TestBed.Tests;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FarseerPhysics.TestBed
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Game
    {
        private const float settingsHz = 60.0f;
        private IEventTrace et;
        private TestEntry entry;
        private GraphicsDeviceManager graphics;
        private int height = 480;
        private GamePadState oldGamePad;
        private KeyboardState oldKeyboardState;
        private MouseState oldMouseState;
        private Framework.Settings settings = new Framework.Settings();
        private BasicEffect simpleColorEffect;
        private SpriteBatch spriteBatch;
        private SpriteFont spriteFont;
        private Test test;
        private int testCount;
        private int testIndex;
        private int testSelection;
        int th;
        int tw;
        private VertexDeclaration vertexDecl;
        private Vector2 viewCenter = new Vector2(0.0f, 20.0f);
        private float viewZoom = 1.0f;
        private int width = 640;
        private const bool _traceEnabled = true;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferMultiSampling = true;
            IsMouseVisible = true;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            et = new EventTrace(this);
            TraceEvents.Register(et);

            testCount = 0;
            while (TestEntries.g_testEntries[testCount].CreateFcn != null)
            {
                ++testCount;
            }

            testIndex = MathUtils.Clamp(testIndex, 0, testCount - 1);
            testSelection = testIndex;
            StartTest(testIndex);

            //settings.drawAABBs = 1;
        }

        private void StartTest(int index)
        {
            entry = TestEntries.g_testEntries[index];
            test = entry.CreateFcn();
            test.GameInstance = this;
            test.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteFont = Content.Load<SpriteFont>("font");
            simpleColorEffect = new BasicEffect(GraphicsDevice, null);
            simpleColorEffect.VertexColorEnabled = true;

            vertexDecl = new VertexDeclaration(GraphicsDevice, VertexPositionColor.VertexElements);
            DebugViewXNA.DebugViewXNA._device = GraphicsDevice;
            DebugViewXNA.DebugViewXNA._batch = spriteBatch;
            DebugViewXNA.DebugViewXNA._font = spriteFont;

            oldKeyboardState = Keyboard.GetState();
            oldMouseState = Mouse.GetState();
            oldGamePad = GamePad.GetState(PlayerIndex.One);

            Resize(GraphicsDevice.PresentationParameters.BackBufferWidth,
                   GraphicsDevice.PresentationParameters.BackBufferHeight);
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (_traceEnabled)
                et.BeginTrace(TraceEvents.UpdateEventId);

            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                Exit();

            KeyboardState newKeyboardState = Keyboard.GetState();
            GamePadState newGamePad = GamePad.GetState(PlayerIndex.One);

            // Press 'z' to zoom out.
            if (newKeyboardState.IsKeyDown(Keys.Z) && oldKeyboardState.IsKeyUp(Keys.Z))
            {
                viewZoom = Math.Min(1.1f * viewZoom, 20.0f);
                Resize(width, height);
            }
            // Press 'x' to zoom in.
            else if (newKeyboardState.IsKeyDown(Keys.X) && oldKeyboardState.IsKeyUp(Keys.X))
            {
                viewZoom = Math.Max(0.9f * viewZoom, 0.02f);
                Resize(width, height);
            }
            // Press 'r' to reset.
            else if (newKeyboardState.IsKeyDown(Keys.R) && oldKeyboardState.IsKeyUp(Keys.R))
            {
                Restart();
            }
            // Press space to launch a bomb.
            else if ((newKeyboardState.IsKeyDown(Keys.Space) && oldKeyboardState.IsKeyUp(Keys.Space)) ||
                     newGamePad.IsButtonDown(Buttons.B) && oldGamePad.IsButtonUp(Buttons.B))
            {
                if (test != null)
                {
                    test.LaunchBomb();
                }
            }
            else if ((newKeyboardState.IsKeyDown(Keys.P) && oldKeyboardState.IsKeyUp(Keys.P)) ||
                     newGamePad.IsButtonDown(Buttons.Start) && oldGamePad.IsButtonUp(Buttons.Start))
            {
                settings.Pause = settings.Pause > 0 ? 1 : (uint)0;
            }
            // Press I to prev test.
            else if ((newKeyboardState.IsKeyDown(Keys.I) && oldKeyboardState.IsKeyUp(Keys.I)) ||
                     newGamePad.IsButtonDown(Buttons.LeftShoulder) && oldGamePad.IsButtonUp(Buttons.LeftShoulder))
            {
                --testSelection;
                if (testSelection < 0)
                {
                    testSelection = testCount - 1;
                }
            }
            // Press O to next test.
            else if ((newKeyboardState.IsKeyDown(Keys.O) && oldKeyboardState.IsKeyUp(Keys.O)) ||
                     newGamePad.IsButtonDown(Buttons.RightShoulder) && oldGamePad.IsButtonUp(Buttons.RightShoulder))
            {
                ++testSelection;
                if (testSelection == testCount)
                {
                    testSelection = 0;
                }
            }
            // Press left to pan left.
            else if (newKeyboardState.IsKeyDown(Keys.Left) && oldKeyboardState.IsKeyUp(Keys.Left))
            {
                viewCenter.X -= 0.5f;
                Resize(width, height);
            }
            // Press right to pan right.
            else if (newKeyboardState.IsKeyDown(Keys.Right) && oldKeyboardState.IsKeyUp(Keys.Right))
            {
                viewCenter.X += 0.5f;
                Resize(width, height);
            }
            // Press down to pan down.
            else if (newKeyboardState.IsKeyDown(Keys.Down) && oldKeyboardState.IsKeyUp(Keys.Down))
            {
                viewCenter.Y -= 0.5f;
                Resize(width, height);
            }
            // Press up to pan up.
            else if (newKeyboardState.IsKeyDown(Keys.Up) && oldKeyboardState.IsKeyUp(Keys.Up))
            {
                viewCenter.Y += 0.5f;
                Resize(width, height);
            }
            // Press home to reset the view.
            else if (newKeyboardState.IsKeyDown(Keys.Home) && oldKeyboardState.IsKeyUp(Keys.Home))
            {
                viewZoom = 1.0f;
                viewCenter = new Vector2(0.0f, 20.0f);
                Resize(width, height);
            }
            else
            {
                if (test != null)
                {
                    test.Keyboard(newKeyboardState, oldKeyboardState);
                }
            }

            MouseState newMouseState = Mouse.GetState();

            if (test != null)
                test.Mouse(newMouseState, oldMouseState);

            base.Update(gameTime);

            oldKeyboardState = newKeyboardState;
            oldMouseState = newMouseState;
            oldGamePad = newGamePad;

            if (_traceEnabled)
                et.EndTrace(TraceEvents.UpdateEventId);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            if (_traceEnabled)
                et.BeginTrace(TraceEvents.DrawEventId);

            GraphicsDevice.Clear(Color.Black);

            GraphicsDevice.VertexDeclaration = vertexDecl;
            simpleColorEffect.Begin();
            simpleColorEffect.Techniques[0].Passes[0].Begin();

            test.SetTextLine(30);
            settings.Hz = settingsHz;

            if (_traceEnabled)
                et.BeginTrace(TraceEvents.PhysicsEventId);

            test.Update(settings);

            if (_traceEnabled)
                et.EndTrace(TraceEvents.PhysicsEventId);

            test.DrawTitle(50, 15, entry.Name);

            if (testSelection != testIndex)
            {
                testIndex = testSelection;
                StartTest(testIndex);
                viewZoom = 1.0f;
                viewCenter = new Vector2(0.0f, 20.0f);
                Resize(width, height);
            }

            test.DebugView.FinishDrawShapes();

            simpleColorEffect.Techniques[0].Passes[0].End();
            simpleColorEffect.End();

            if (test != null)
            {
                spriteBatch.Begin();
                test.DebugView.FinishDrawString();
                spriteBatch.End();
            }
            base.Draw(gameTime);

            if (_traceEnabled)
            {
                et.EndTrace(TraceEvents.DrawEventId);
                et.EndTrace(TraceEvents.FrameEventId);
                et.ResetFrame();
                et.BeginTrace(TraceEvents.FrameEventId);
            }
        }

        private void Resize(int w, int h)
        {
            width = w;
            height = h;

            tw = GraphicsDevice.Viewport.Width;
            th = GraphicsDevice.Viewport.Height;

            float ratio = (float)tw / (float)th;

            Vector2 extents = new Vector2(ratio * 25.0f, 25.0f);
            extents *= viewZoom;

            Vector2 lower = viewCenter - extents;
            Vector2 upper = viewCenter + extents;

            // L/R/B/T
            simpleColorEffect.Parameters["Projection"].SetValue(Matrix.CreateOrthographicOffCenter(lower.X, upper.X,
                                                                                                   lower.Y, upper.Y, -1,
                                                                                                   1));
        }

        public Vector2 ConvertScreenToWorld(int x, int y)
        {
            float u = x / (float)tw;
            float v = (th - y) / (float)th;

            float ratio = (float)tw / (float)th;
            Vector2 extents = new Vector2(ratio * 25.0f, 25.0f);
            extents *= viewZoom;

            Vector2 lower = viewCenter - extents;
            Vector2 upper = viewCenter + extents;

            Vector2 p = new Vector2();
            p.X = (1.0f - u) * lower.X + u * upper.X;
            p.Y = (1.0f - v) * lower.Y + v * upper.Y;
            return p;
        }

        private void Restart()
        {
            StartTest(testIndex);
            Resize(width, height);
        }

        private void Pause()
        {
            settings.Pause = (uint)(settings.Pause > 0 ? 0 : 1);
        }

        private void SingleStep()
        {
            settings.Pause = 1;
            settings.SingleStep = 1;
        }
    }
}