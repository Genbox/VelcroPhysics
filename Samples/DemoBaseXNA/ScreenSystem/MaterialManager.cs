using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace FarseerPhysics.DemoBaseXNA.ScreenSystem
{
    public enum MaterialType
    {
        Blank, Circles, Dots, Face, Squares, Stars, Tiles, Waves
    }

    public class MaterialManager
    {
        private Texture2D[] faceAnim = new Texture2D[2];
        private double timer;
        private Dictionary<MaterialType, Texture2D> materials = new Dictionary<MaterialType, Texture2D>();

        public void LoadContent(ContentManager contentManager)
        {
            faceAnim[0] = contentManager.Load<Texture2D>("Materials/face1");
            faceAnim[1] = contentManager.Load<Texture2D>("Materials/face2");
            timer = 1000.0;

            materials[MaterialType.Blank] = contentManager.Load<Texture2D>("Common/blank");
            materials[MaterialType.Circles] = contentManager.Load<Texture2D>("Materials/circles");
            materials[MaterialType.Dots] = contentManager.Load<Texture2D>("Materials/dots");
            materials[MaterialType.Squares] = contentManager.Load<Texture2D>("Materials/squares");
            materials[MaterialType.Stars] = contentManager.Load<Texture2D>("Materials/stars");
            materials[MaterialType.Tiles] = contentManager.Load<Texture2D>("Materials/tiles");
            materials[MaterialType.Waves] = contentManager.Load<Texture2D>("Materials/waves");
            materials[MaterialType.Face] = faceAnim[0];
        }

        public void Update(GameTime gameTime)
        {
            timer -= gameTime.ElapsedGameTime.TotalMilliseconds;
            if (timer <= 0.0)
            {
                if (materials[MaterialType.Face] == faceAnim[0])
                {
                    materials[MaterialType.Face] = faceAnim[1];
                    timer = 200.0;
                }
                else
                {
                    materials[MaterialType.Face] = faceAnim[0];
                    timer = 5000.0;
                }
            }
        }

        public Texture2D GetMaterialTexture(MaterialType type)
        {
            return materials[type];
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

    public class DemoMaterial
    {
        public DemoMaterial(MaterialType type)
        {
            Color1 = Color.White;
            Color2 = Color.White;
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

        public Color Color1 { get; set; }
        public Color Color2 { get; set; }
        public Color Color
        {
            set
            {
                Color1 = value;
                Color2 = value;
            }
        }
        public float Depth { get; set; }
        public MaterialType Type { get; private set; }
        public bool CenterOnBody { get; set; }
        public float Scale { get; set; }
    }
}
