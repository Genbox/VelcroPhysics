using System;
using System.Windows.Controls;
using FarseerPhysics.Components;
using FarseerPhysics.Samples;
using FarseerPhysics.ScreenSystem;

namespace FarseerPhysics
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class FarseerPhysicsGame : Game
    {
        public FarseerPhysicsGame(UserControl userControl, Canvas drawingCanvas, Canvas debugCanvas, TextBlock txtFPS, TextBlock txtDebug)
            : base(userControl, drawingCanvas, debugCanvas, txtDebug)
        {
            TargetElapsedTime = new TimeSpan(0, 0, 0, 0, 16);
            IsFixedTimeStep = true;

            //new-up components and add to Game.Components
            ScreenManager = new ScreenManager(this);
            Components.Add(ScreenManager);

            if (txtFPS != null)
            {
                FrameRateCounter frameRateCounter = new FrameRateCounter(ScreenManager, txtFPS);
                Components.Add(frameRateCounter);
            }

            Demo1Screen demo1 = new Demo1Screen();
            Demo2Screen demo2 = new Demo2Screen();
            Demo3Screen demo3 = new Demo3Screen();
            Demo4Screen demo4 = new Demo4Screen();
            Demo5Screen demo5 = new Demo5Screen();
            Demo6Screen demo6 = new Demo6Screen();
            Demo7Screen demo7 = new Demo7Screen();
            ScreenManager.MainMenuScreen.AddMainMenuItem(demo1.GetTitle(), demo1);
            ScreenManager.MainMenuScreen.AddMainMenuItem(demo2.GetTitle(), demo2);
            ScreenManager.MainMenuScreen.AddMainMenuItem(demo3.GetTitle(), demo3);
            ScreenManager.MainMenuScreen.AddMainMenuItem(demo4.GetTitle(), demo4);
            ScreenManager.MainMenuScreen.AddMainMenuItem(demo5.GetTitle(), demo5);
            ScreenManager.MainMenuScreen.AddMainMenuItem(demo6.GetTitle(), demo6);
            ScreenManager.MainMenuScreen.AddMainMenuItem(demo7.GetTitle(), demo7);

            ScreenManager.GoToMainMenu();
        }

        public ScreenManager ScreenManager { get; set; }
    }
}