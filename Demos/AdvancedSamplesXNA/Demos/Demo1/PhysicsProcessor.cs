using System;
using System.Threading;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using FarseerGames.FarseerPhysics;

namespace FarseerGames.FarseerPhysicsDemos.Demos.Demo4
{
    // POINT OF INTEREST
    // This is the class that syncronise the threads.
    public class PhysicsProcessor : IDisposable
    {
        // POINT OF INTEREST
        // The simulator what will be used by the processor
        private PhysicsSimulator _physicsSimulator;
        // POINT OF INTEREST
        // When there is only one hardware thread in the system, there is no meaning of multithreading
        private bool _useMultiThreading;
        // POINT OF INTEREST
        // We can use this to diable multithreading for one frame /for the debug view/
        private bool _forceSingleThreaded;
        // POINT OF INTEREST
        // Use this to signal the termination of the physics thread. volatile is a simple syncronisation
        // specifier.
        private volatile bool _doExit;

        // POINT OF INTEREST
        // This is a package holding all the information to update the simulator like time and game related stuff
        struct IterateParam
        {
            GameTime _gameTime;

            public GameTime GameTime { get { return _gameTime; } }

            public IterateParam(GameTime gameTime)
            {
                _gameTime = gameTime;
            }
        };

        // POINT OF INTEREST
        // Our instance of the iteration parameters
        private IterateParam _iterateParam;

        // POINT OF INTEREST
        // This will signal the thread to update the simulator. AutoReset means it is going
        // to unsignal itself after the other thread received the signal.
        private AutoResetEvent _processEvent = new AutoResetEvent(false);
        // POINT OF INTEREST
        // This is signaled when the thread is not updating. the code using this to determine 
        // if the thread is not using the simulator /so it can be disposed/.
        private ManualResetEvent _idleEvent = new ManualResetEvent(true);

        // POINT OF INTEREST
        // The links the physics thread needs to syncronise after the updates
        List<ObjectLinker> _linkList = new List<ObjectLinker>();

        public PhysicsProcessor(PhysicsSimulator physicsSimulator)
        {
            _physicsSimulator = physicsSimulator;
        }

        public void Dispose()
        {
            // POINT OF INTEREST
            // Signal the phyisics thread to exit, reset the iteration params and signal the
            // physics thread so it will exit
            _doExit = true;
            _iterateParam = new IterateParam(new GameTime());
            _processEvent.Set();
        }

        // POINT OF INTEREST
        // This is the entry point for the physics thread, like the Main function for it
        public void StartThinking()
        {
#if XBOX
      // POINT OF INTEREST
      // On the Xbox360, we need to specify the hardware threads /CPUs/ for the physics
      // thread. It is going to run on the fourth HW thread /it is almost fully idle/
      Thread.CurrentThread.SetProcessorAffinity( new int[] { 4 } );
      useMultiThreading = true;
#else
            // POINT OF INTEREST
            // Use multithreading only if there is more than one processor to use
            _useMultiThreading = Environment.ProcessorCount > 1;
#endif
            // POINT OF INTEREST
            // The main loop for the physics thread. Update the simulator until told to exit.
            // Between each update, it is going to wait until the main thread it to update,
            // so the updates are going to happen while the main thread doing something else,
            // like drawing, etc.
            while (!_doExit)
            {
                _processEvent.WaitOne();
                Think();
            }
        }

        // POINT OF INTEREST
        // This is to signal the physics thread to update the simulator. Iterate is
        // going to be called on the main thread. This is not going to update the
        // simulator, just send the signal to the other thread to get the job done.
        public void Iterate(GameTime gameTime, bool forceSingleThreaded)
        {
            // POINT OF INTEREST
            // If the simulation can not be syncronied we force the PhysicsProcessor to do the
            // physics update on this thread /the main thread/.
            _forceSingleThreaded = forceSingleThreaded;
            // POINT OF INTEREST
            // Fill the iterate params with all the needed info and signal the physics thread
            // to iterate the simulator
            _iterateParam = new IterateParam(gameTime);
            _processEvent.Set();

            // POINT OF INTEREST
            // If there is no multithreading enabled /only one HW threads/, or the application
            // disabled it manually /DebugView/, do the update on this thread.
            if (!_useMultiThreading || forceSingleThreaded)
            {
                DoThinkPhysics();
            }
        }

        // POINT OF INTEREST
        // This blocks the caller thread until the physics thread becomes idle /finished the
        //  current frame/
        public void BlockUntilIdle()
        {
            _idleEvent.WaitOne();
        }

        // POINT OF INTEREST
        // Register a link to get it syncronised after each update
        public void AddLink(ObjectLinker link)
        {
            _linkList.Add(link);
        }

        // POINT OF INTEREST
        // This is going to be called by the physics thread. Updates the physics simulator.
        private void Think()
        {
            // POINT OF INTEREST
            // Updates the simulator if neccessary
            if (_useMultiThreading && !_forceSingleThreaded)
            {
                DoThinkPhysics();
            }

            // POINT OF INTEREST
            // After an update turn off the forced single threading. It is turned off every frame.
            _forceSingleThreaded = false;
        }

        // POINT OF INTEREST
        // This is doing the actual physics update. It can be called on both threads. When there are
        // more HW threads present in the system /and the multithreaded processing isn't forced off/
        // it is going to be called by the physics thread, while the main thread is drawing.
        // Otherwise it is going to be called on the main thread, so there is no speed gain.
        private void DoThinkPhysics()
        {
            // POINT OF INTEREST
            // Unsignal the idle signal while working.
            _idleEvent.Reset();
            // POINT OF INTEREST
            // Linit the frame rate to 100ms. This is going to cause a slowdown in the simulator when
            // the framerate drops below 10FPS. But I think the lagg is more frustrating than the
            // slowdown. I prefer a little slowdown, instead of dying between two frames.
            _physicsSimulator.Update(Math.Min(_iterateParam.GameTime.ElapsedGameTime.Milliseconds, 100) * .001f);
            // POINT OF INTEREST
            // Done updating, now syncronise the links
            SyncronizeLinks();
            // POINT OF INTEREST
            // The physics thread is idle again
            _idleEvent.Set();
        }

        // POINT OF INTEREST
        // Syncronise the link. It is going to be called after each physics update.
        private void SyncronizeLinks()
        {
            foreach (ObjectLinker link in _linkList)
            {
                link.Syncronize();
            }
        }
    }
}
