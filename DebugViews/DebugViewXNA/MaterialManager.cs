using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerPhysics.DebugViews
{
    public enum MaterialType
    {
        Blank, Circles, Dots, Face, Squares, Stars, Tiles, Waves, Pavement
    }

    public class MaterialManager
    {
        private Texture2D[] _faceAnim = new Texture2D[2];
        private double _timer;
        private Dictionary<MaterialType, Texture2D> _materials = new Dictionary<MaterialType, Texture2D>();

        public void LoadContent(ContentManager contentManager)
        {
            _faceAnim[0] = contentManager.Load<Texture2D>("Materials/face1");
            _faceAnim[1] = contentManager.Load<Texture2D>("Materials/face2");
            _timer = 1000.0;

            _materials[MaterialType.Blank] = contentManager.Load<Texture2D>("Materials/blank");
            _materials[MaterialType.Circles] = contentManager.Load<Texture2D>("Materials/circles");
            _materials[MaterialType.Dots] = contentManager.Load<Texture2D>("Materials/dots");
            _materials[MaterialType.Squares] = contentManager.Load<Texture2D>("Materials/squares");
            _materials[MaterialType.Stars] = contentManager.Load<Texture2D>("Materials/stars");
            _materials[MaterialType.Tiles] = contentManager.Load<Texture2D>("Materials/tiles");
            _materials[MaterialType.Waves] = contentManager.Load<Texture2D>("Materials/waves");
            _materials[MaterialType.Pavement] = contentManager.Load<Texture2D>("Materials/pavement");
            _materials[MaterialType.Face] = _faceAnim[0];
        }

        public void Update(GameTime gameTime)
        {
            _timer -= gameTime.ElapsedGameTime.TotalMilliseconds;
            if (_timer <= 0.0)
            {
                if (_materials[MaterialType.Face] == _faceAnim[0])
                {
                    _materials[MaterialType.Face] = _faceAnim[1];
                    _timer = 200.0;
                }
                else
                {
                    _materials[MaterialType.Face] = _faceAnim[0];
                    _timer = 5000.0;
                }
            }
        }

        public Texture2D GetMaterialTexture(MaterialType type)
        {
            return _materials[type];
        }

        public bool GetMaterialWrap(MaterialType type)
        {
            if (type == MaterialType.Face)
            {
                return false;
            }
            return true;
        }
    }

    public class DebugMaterial
    {
        public DebugMaterial(MaterialType type)
        {
            Color = Color.White;
            Depth = 0f;
            Type = type;
            Scale = 1f;
            switch (type)
            {
                case MaterialType.Face:
                    CenterOnBody = false;
                    break;
                default:
                    CenterOnBody = true;
                    break;
            }
        }

        public Color Color { get; set; }
        public float Depth { get; set; }
        public MaterialType Type { get; private set; }
        public bool CenterOnBody { get; set; }
        public float Scale { get; set; }
    }
}
