using System;
using FarseerPhysics.Collision;
using FarseerPhysics.Components;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Farseer3TestBed
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Game
    {
        private Body body;
        private int count = 0;
        private Texture2D cursorTex;
        private float cyclesLeftOver = 0.0f;
        private XNADebugRenderer debugView;
        private bool everyOther = true;
        private SpriteFont font;
        private GraphicsDeviceManager graphics;
        private Body groundBody;
        private float lastFrameTime = 0.0f;
        private SpriteBatch spriteBatch;
        private PhysicsSimulator world;
        private Vector2 xboxMouse;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 600;
            graphics.IsFullScreen = false;
            graphics.PreferMultiSampling = true;
            graphics.SynchronizeWithVerticalRetrace = false;
            TargetElapsedTime = new TimeSpan(0, 0, 0, 0, 16);
            IsFixedTimeStep = false;
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
            // TODO: Add your initialization logic here
            // Define the size of the world. Simulation will still work
            // if bodies reach the end of the world, but it will be slower.
            AABB worldAABB = new AABB();
            worldAABB.LowerBound = new Vector2(-100.0f, -100.0f);
            worldAABB.UpperBound = new Vector2(100.0f, 100.0f);

            // Construct a world object, which will hold and simulate the rigid bodies.
            world = new PhysicsSimulator(worldAABB, new Vector2(0, -10.0f), true);

            world.SetWarmStarting(true);
            world.SetContinuousPhysics(true);

            debugView = new XNADebugRenderer(GraphicsDevice);
            uint flags = 0;
            flags += (uint) 1*(uint) DebugDraw.DrawFlags.Shape; // works
            //flags += (uint)1 * (uint)DebugDraw.DrawFlags.Joint;           
            //flags += (uint)1 * (uint)DebugDraw.DrawFlags.CoreShape;
            //flags += (uint)1 * (uint)DebugDraw.DrawFlags.Aabb;            // works
            //flags += (uint)1 * (uint)DebugDraw.DrawFlags.Obb;             // works
            //flags += (uint)1 * (uint)DebugDraw.DrawFlags.Pair;            // works
            //flags += (uint)1 * (uint)DebugDraw.DrawFlags.CenterOfMass;    // works
            //flags += (uint)1 * (uint)DebugDraw.DrawFlags.Controller;
            debugView.Flags = (DebugDraw.DrawFlags) flags;

            world.SetDebugDraw(debugView);

            BodyDef groundBodyDef = new BodyDef();
            groundBodyDef.Position = new Vector2(0f, -25f);
            //groundBodyDef.Angle = MathHelper.ToRadians(10);

            // Call the body factory which creates the ground box shape.
            // The body is also added to the world.
            groundBody = world.CreateBody(groundBodyDef);

            // Define the ground box shape.
            PolygonDef groundShapeDef = new PolygonDef();

            // The extents are the half-widths of the box.
            groundShapeDef.SetAsBox(50.0f, 1.0f);
            groundShapeDef.Friction = 0.0f;

            // Add the ground shape to the ground body.
            groundBody.CreateShape(groundShapeDef);

            // Define the ground box shape.
            PolygonDef groundShapeDef1 = new PolygonDef();

            // The extents are the half-widths of the box.
            groundShapeDef1.SetAsBox(1.0f, 50.0f);
            groundShapeDef1.Friction = 0.0f;
            for (int i = 0; i < groundShapeDef1.VertexCount; i++)
                groundShapeDef1.Vertices[i] += new Vector2(-40, 0);

            // Add the ground shape to the ground body.
            groundBody.CreateShape(groundShapeDef1);

            // Define the ground box shape.
            PolygonDef groundShapeDef2 = new PolygonDef();

            // The extents are the half-widths of the box.
            groundShapeDef2.SetAsBox(1.0f, 50.0f);
            groundShapeDef2.Friction = 0.0f;
            for (int i = 0; i < groundShapeDef2.VertexCount; i++)
                groundShapeDef2.Vertices[i] += new Vector2(40, 0);

            // Add the ground shape to the ground body.
            groundBody.CreateShape(groundShapeDef2);

            Components.Add(new FrameRateCounter(this, GraphicsDevice, Content));
            xboxMouse = new Vector2(400, 400);

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            font = Content.Load<SpriteFont>("Monaco");
            cursorTex = Content.Load<Texture2D>("cursor");
        }


        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            KeyboardState keys = Keyboard.GetState();

            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed
                || keys.IsKeyDown(Keys.Escape))
                Exit();

            Random rand = new Random();
#if !XBOX
            MouseState m = Mouse.GetState();

            if (m.LeftButton == ButtonState.Pressed)
            {
                for (int j = 0; j < 1; j++)
                {
                    // Define the dynamic body. We set its position and call the body factory.
                    BodyDef bodyDef = new BodyDef();
                    bodyDef.Position = debugView.ConvertScreenToWorld(m.X, m.Y);
                    //bodyDef.Position = new Vector2((m.X - 400) / 4, -(m.Y - 300) / 4);
                    body = world.CreateBody(bodyDef);
                    //body.AllowSleeping(true);

                    // Define another box shape for our dynamic body.
                    if (everyOther)
                    {
                        CircleDef shapeDef = new CircleDef();
                        shapeDef.Radius = (float) rand.NextDouble() + 0.5f;
                        // Set the box density to be non-zero, so it will be dynamic.
                        shapeDef.Density = 8.0f;

                        // Override the default friction.
                        shapeDef.Friction = 1.0f;

                        // Override the default restitution.
                        shapeDef.Restitution = 0.0f;

                        // Add the shape to the body.
                        body.CreateShape(shapeDef);

                        // Now tell the dynamic body to compute it's mass properties base
                        // on its shape.
                        body.SetMassFromShapes();
                        everyOther = !everyOther;
                    }
                    else
                    {
                        PolygonDef shapeDef = new PolygonDef();

                        shapeDef.SetAsBox(0.5f, 1.0f);

                        // Set the box density to be non-zero, so it will be dynamic.
                        shapeDef.Density = 10f;

                        // Override the default friction.
                        shapeDef.Friction = 1.0f;

                        // Override the default restitution.
                        shapeDef.Restitution = 0.0f;

                        // Add the shape to the body.
                        body.CreateShape(shapeDef);

                        // Now tell the dynamic body to compute it's mass properties base
                        // on its shape.
                        body.SetMassFromShapes();
                        everyOther = !everyOther;
                    }
                }

                count = 0;
            }
            count++;
#endif
#if XBOX
            GamePadState g = GamePad.GetState(PlayerIndex.One);

            // update xbox mouse position
            xboxMouse += new Vector2(g.ThumbSticks.Left.X * gameTime.ElapsedGameTime.Milliseconds * 0.5f,
                -g.ThumbSticks.Left.Y * gameTime.ElapsedGameTime.Milliseconds * 0.5f);

            if (g.Buttons.A == ButtonState.Pressed && count > 10)
            {
                // Define the dynamic body. We set its position and call the body factory.
                BodyDef bodyDef = new BodyDef();
                bodyDef.Position = debugView.ConvertScreenToWorld(xboxMouse.X, xboxMouse.Y);
                body = world.CreateBody(bodyDef);

                // Define another box shape for our dynamic body.
                    if (everyOther)
                    {
                        CircleDef shapeDef = new CircleDef();
                        shapeDef.Radius = (float)rand.NextDouble() + 0.5f;
                        // Set the box density to be non-zero, so it will be dynamic.
                        shapeDef.Density = 8.0f;

                        // Override the default friction.
                        shapeDef.Friction = 1.0f;

                        // Override the default restitution.
                        shapeDef.Restitution = 0.0f;

                        // Add the shape to the body.
                        body.CreateShape(shapeDef);

                        // Now tell the dynamic body to compute it's mass properties base
                        // on its shape.
                        body.SetMassFromShapes();
                        everyOther = !everyOther;
                    }
                    else
                    {
                        PolygonDef shapeDef = new PolygonDef();

                        shapeDef.SetAsBox(0.5f, 1.0f);
                        
                        // Set the box density to be non-zero, so it will be dynamic.
                        shapeDef.Density = 10f;

                        // Override the default friction.
                        shapeDef.Friction = 1.0f;

                        // Override the default restitution.
                        shapeDef.Restitution = 0.0f;

                        // Add the shape to the body.
                        body.CreateShape(shapeDef);

                        // Now tell the dynamic body to compute it's mass properties base
                        // on its shape.
                        body.SetMassFromShapes();
                        everyOther = !everyOther;
                    }
                count = 0;
            }
            count++;
#endif
            world.Step(gameTime.ElapsedGameTime.Ticks*0.0000001f, 8, 3);

            base.Update(gameTime);
        }


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Gray);

            debugView.Render();

            spriteBatch.Begin();

            spriteBatch.DrawString(font, world.GetBodyCount().ToString(), new Vector2(50, 50), Color.Black);

#if XBOX
            spriteBatch.Draw(cursorTex, xboxMouse, Color.White);
#endif

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}