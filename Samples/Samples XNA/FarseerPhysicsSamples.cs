using System;
using System.Reflection;
using Microsoft.Xna.Framework;
using FarseerPhysics.Samples.Demos;
using FarseerPhysics.Samples.ScreenSystem;

namespace FarseerPhysics.Samples
{
  internal static class Program
  {
    /// <summary>
    /// The main entry point for the samples
    /// </summary>
    private static void Main(string[] args)
    {
      using (FarseerPhysicsSamples game = new FarseerPhysicsSamples())
      {
        game.Run();
      }
    }
  }

  /// <summary>
  /// This is the main type for the samples
  /// </summary>
  public class FarseerPhysicsSamples : Game
  {
    private GraphicsDeviceManager _graphics;

    public FarseerPhysicsSamples()
    {
      Window.Title = "Farseer Physics Samples";
      _graphics = new GraphicsDeviceManager(this);
      _graphics.PreferMultiSampling = true;
#if WINDOWS || XBOX
      _graphics.PreferredBackBufferWidth = 1280;
      _graphics.PreferredBackBufferHeight = 720;
      ConvertUnits.SetDisplayUnitToSimUnitRatio(24f);
      IsFixedTimeStep = true;
#elif WINDOWS_PHONE
      _graphics.PreferredBackBufferWidth = 800;
      _graphics.PreferredBackBufferHeight = 480;
      ConvertUnits.SetDisplayUnitToSimUnitRatio(16f);
      IsFixedTimeStep = false;
#endif
#if WINDOWS
      _graphics.IsFullScreen = false;
#elif XBOX || WINDOWS_PHONE
      _graphics.IsFullScreen = true;
#endif

      Content.RootDirectory = "Content";

      //new-up components and add to Game.Components
      ScreenManager = new ScreenManager(this);
      Components.Add(ScreenManager);

      FrameRateCounter frameRateCounter = new FrameRateCounter(ScreenManager);
      frameRateCounter.DrawOrder = 101;
      Components.Add(frameRateCounter);
    }

    public ScreenManager ScreenManager { get; set; }

    /// <summary>
    /// Allows the game to perform any initialization it needs to before starting to run.
    /// This is where it can query for any required services and load any non-graphic
    /// related content.  Calling base.Initialize will enumerate through any components
    /// and initialize them as well.
    /// </summary>
    protected override void Initialize()
    {
      base.Initialize();

      MenuScreen menuScreen = new MenuScreen("Farseer Physics Samples");
      menuScreen.AddMenuItem("Demos", EntryType.Separator, null);

      Assembly SamplesFramework = Assembly.GetExecutingAssembly();

      foreach (Type SampleType in SamplesFramework.GetTypes())
      {
        if (SampleType.IsSubclassOf(typeof(PhysicsGameScreen)))
        {
          PhysicsGameScreen DemoScreen = SamplesFramework.CreateInstance(SampleType.ToString()) as PhysicsGameScreen;
          menuScreen.AddMenuItem(DemoScreen.GetTitle(), EntryType.Screen, DemoScreen);
        }
      }

      menuScreen.AddMenuItem("", EntryType.Separator, null);
      menuScreen.AddMenuItem("Exit", EntryType.ExitItem, null);

      ScreenManager.AddScreen(new BackgroundScreen());
      ScreenManager.AddScreen(menuScreen);
      ScreenManager.AddScreen(new LogoScreen(TimeSpan.FromSeconds(3.0)));
    }
  }
}