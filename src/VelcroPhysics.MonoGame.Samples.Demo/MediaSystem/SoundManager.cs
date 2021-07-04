using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Demo.MediaSystem
{
    public class SoundManager
    {
        private int _soundVolume;
        private readonly Dictionary<string, SoundEffect> _soundList = new Dictionary<string, SoundEffect>();

        public SoundManager(ContentManager content)
        {
            // Initialize audio playback
            DirectoryInfo currentAssetFolder = new DirectoryInfo(content.RootDirectory + "/DemoSFX");
            FileInfo[] currentFileList = currentAssetFolder.GetFiles("*.xnb");

            for (int i = 0; i < currentFileList.Length; i++)
            {
                string soundName = Path.GetFileNameWithoutExtension(currentFileList[i].Name);
                _soundList[soundName] = content.Load<SoundEffect>("DemoSFX/" + soundName);
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

        public int SoundVolume
        {
            get => _soundVolume;
            set
            {
                _soundVolume = (int)MathHelper.Clamp(value, 0f, 100f);
                SoundEffect.MasterVolume = _soundVolume / 100f;
            }
        }

        /// <summary>Plays a fire-and-forget sound effect by name.</summary>
        /// <param name="soundName">The name of the sound to play.</param>
        public void PlaySoundEffect(string soundName)
        {
            if (!_soundList.TryGetValue(soundName, out SoundEffect soundEffect))
                throw new FileNotFoundException($"Unable to find \"{soundName}\"");

            soundEffect.Play();
        }

        /// <summary>Plays a sound effect by name and returns an instance of that sound.</summary>
        /// <param name="soundName">The name of the sound to play.</param>
        /// <param name="looped">True if sound effect should loop.</param>
        public SoundEffectInstance PlaySoundEffect(string soundName, bool looped)
        {
            if (!_soundList.TryGetValue(soundName, out SoundEffect soundEffect))
                throw new FileNotFoundException($"Unable to find \"{soundName}\"");

            SoundEffectInstance instance = null;

            try
            {
                instance = soundEffect.CreateInstance();
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

            return instance;
        }
    }
}