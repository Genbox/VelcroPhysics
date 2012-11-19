using System;
using System.IO;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

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
        _soundVolume = (int)MathHelper.Clamp(_soundVolume, 0f, 100f);
        SoundEffect.MasterVolume = _soundVolume / 100f;
      }
    }
    private static int _soundVolume;

    private MediaManager(Game game)
      : base(game)
    {
      // Add all common graphics
      DirectoryInfo assetFolder = new DirectoryInfo(game.Content.RootDirectory + @"/Common");
      FileInfo[] fileList = assetFolder.GetFiles("*.xnb");

      for (int i = 0; i < fileList.Length; i++)
      {
        string textureName = Path.GetFileNameWithoutExtension(fileList[i].Name);
        _textureList[textureName] = game.Content.Load<Texture2D>(@"Common/" + textureName);
        _textureList[textureName].Name = textureName;
      }

      // Add all demo specific graphics      
      assetFolder = new DirectoryInfo(game.Content.RootDirectory + @"/DemoGFX");
      fileList = assetFolder.GetFiles("*.xnb");

      for (int i = 0; i < fileList.Length; i++)
      {
        string textureName = Path.GetFileNameWithoutExtension(fileList[i].Name);
        _textureList[textureName] = game.Content.Load<Texture2D>(@"DemoGFX/" + textureName);
        _textureList[textureName].Name = textureName;
      }

      // Add samples fonts
      assetFolder = new DirectoryInfo(game.Content.RootDirectory + @"/Fonts");
      fileList = assetFolder.GetFiles("*.xnb");

      for (int i = 0; i < fileList.Length; i++)
      {
        string fontName = Path.GetFileNameWithoutExtension(fileList[i].Name);
        _fontList[fontName] = game.Content.Load<SpriteFont>(@"Fonts/" + fontName);
      }

      // Initialize audio playback
      assetFolder = new DirectoryInfo(game.Content.RootDirectory + @"/DemoSFX");
      fileList = assetFolder.GetFiles("*.xnb");

      for (int i = 0; i < fileList.Length; i++)
      {
        string soundName = Path.GetFileNameWithoutExtension(fileList[i].Name);
        _soundList[soundName] = game.Content.Load<SoundEffect>(@"DemoSFX/" + soundName);
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
