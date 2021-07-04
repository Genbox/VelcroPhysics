using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Demo.MediaSystem
{
    public class FontManager
    {
        private readonly Dictionary<string, SpriteFont> _fontList = new Dictionary<string, SpriteFont>();

        public FontManager(ContentManager content)
        {
            // Add samples fonts
            DirectoryInfo currentAssetFolder = new DirectoryInfo(content.RootDirectory + "/Fonts");
            FileInfo[] currentFileList = currentAssetFolder.GetFiles("*.xnb");

            for (int i = 0; i < currentFileList.Length; i++)
            {
                string fontName = Path.GetFileNameWithoutExtension(currentFileList[i].Name);
                _fontList[fontName] = content.Load<SpriteFont>("Fonts/" + fontName);
            }
        }

        public SpriteFont GetFont(string fontName)
        {
            if (_fontList.TryGetValue(fontName, out SpriteFont font))
                return font;

            throw new FileNotFoundException($"The font \"{fontName}\" was not found");
        }
    }
}