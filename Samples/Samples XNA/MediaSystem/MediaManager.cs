#region Using System
using System;
using System.IO;
using System.Collections.Generic;
#endregion
#region Using XNA
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
#endregion

namespace FarseerPhysics.Samples.MediaSystem
{
  public class MediaManager : GameComponent
  {
    private static MediaManager _mediaManager = null;

    private static Dictionary<string, Texture2D> _textureList = new Dictionary<string, Texture2D>();
    private static Dictionary<string, SpriteFont> _fontList = new Dictionary<string, SpriteFont>();

    private static Dictionary<string, SoundEffect> _soundList = new Dictionary<string, SoundEffect>();

    public static int SoundVolume
    {
      get { return _soundVolume; }
      set
      {
        _soundVolume = (int)MathHelper.Clamp(value, 0f, 100f);
        SoundEffect.MasterVolume = _soundVolume / 100f;
      }
    }
    private static int _soundVolume;

    private MediaManager(Game game)
      : base(game)
    {
      DirectoryInfo currentAssetFolder;
      FileInfo[] currentFileList;

      // Load all graphics
      string[] gfxFolders = { "Common", "DemoGFX", "Materials" };
      foreach (string folder in gfxFolders)
      {
        currentAssetFolder = new DirectoryInfo(game.Content.RootDirectory + "/" + folder);
        currentFileList = currentAssetFolder.GetFiles("*.xnb");
        for (int i = 0; i < currentFileList.Length; i++)
        {
          string textureName = Path.GetFileNameWithoutExtension(currentFileList[i].Name);
          _textureList[textureName] = game.Content.Load<Texture2D>(folder + "/" + textureName);
          _textureList[textureName].Name = textureName;
        }
      }

      // Add samples fonts
      currentAssetFolder = new DirectoryInfo(game.Content.RootDirectory + "/Fonts");
      currentFileList = currentAssetFolder.GetFiles("*.xnb");

      for (int i = 0; i < currentFileList.Length; i++)
      {
        string fontName = Path.GetFileNameWithoutExtension(currentFileList[i].Name);
        _fontList[fontName] = game.Content.Load<SpriteFont>("Fonts/" + fontName);
      }

      // Initialize audio playback
      currentAssetFolder = new DirectoryInfo(game.Content.RootDirectory + "/DemoSFX");
      currentFileList = currentAssetFolder.GetFiles("*.xnb");

      for (int i = 0; i < currentFileList.Length; i++)
      {
        string soundName = Path.GetFileNameWithoutExtension(currentFileList[i].Name);
        _soundList[soundName] = game.Content.Load<SoundEffect>("DemoSFX/" + soundName);
        _soundList[soundName].Name = soundName;
      }

      try
      {
        SoundVolume = 100;
      }
      catch (NoAudioHardwareException)
      {
        // silently fall back to silence
      }
    }

    public static void Initialize(Game game)
    {
      if (_mediaManager == null && game != null)
      {
        _mediaManager = new MediaManager(game);
        game.Components.Add(_mediaManager);
      }
    }

    public static Texture2D GetTexture(string textureName)
    {
      Texture2D texture;
      GetTexture(textureName, out texture);
      return texture;
    }

    public static void GetTexture(string textureName, out Texture2D texture)
    {
      if (_mediaManager != null && _textureList.ContainsKey(textureName))
      {
        texture = _textureList[textureName];
      }
      else
      {
        texture = null;
        throw new FileNotFoundException();
      }
    }

    public static SpriteFont GetFont(string fontName)
    {
      SpriteFont font;
      GetFont(fontName, out font);
      return font;
    }

    public static void GetFont(string fontName, out SpriteFont font)
    {
      if (_mediaManager != null && _fontList.ContainsKey(fontName))
      {
        font = _fontList[fontName];
      }
      else
      {
        font = null;
        throw new FileNotFoundException();
      }
    }

    /// <summary>
    /// Plays a fire-and-forget sound effect by name.
    /// </summary>
    /// <param name="soundName">The name of the sound to play.</param>
    public static void PlaySoundEffect(string soundName)
    {
      if (_mediaManager != null && _soundList.ContainsKey(soundName))
      {
        _soundList[soundName].Play();
      }
      else
      {
        throw new FileNotFoundException();
      }
    }

    /// <summary>
    /// Plays a sound effect by name and returns an instance of that sound.
    /// </summary>
    /// <param name="soundName">The name of the sound to play.</param>
    /// <param name="looped">True if sound effect should loop.</param>
    /// <param name="instance">The SoundEffectInstance created for this sound effect.</param>
    public static void PlaySoundEffect(string soundName, bool looped, out SoundEffectInstance instance)
    {
      instance = null;
      if (_mediaManager != null && _soundList != null && _soundList.ContainsKey(soundName))
      {
        try
        {
          instance = _soundList[soundName].CreateInstance();
          if (instance != null)
          {
            instance.IsLooped = looped;
            instance.Play();
          }
        }
        catch (InstancePlayLimitException)
        {
          // silently fail (returns null instance) if instance limit reached
        }
      }
      else
      {
        throw new FileNotFoundException();
      }
    }
  }
}