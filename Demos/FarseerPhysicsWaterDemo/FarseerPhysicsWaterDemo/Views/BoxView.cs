using System.Windows.Controls;
using FarseerPhysicsWaterDemo.Models;
using FarseerPhysicsWaterDemo.RenderSystem;
using FarseerGames.FarseerPhysics.Mathematics;
using FarseerGames.FarseerPhysics;

namespace FarseerPhysicsWaterDemo.Views
{
    public class BoxView
    {
        #region properties
        public BoxModel BoxModel { get; private set; }
        #endregion

        #region public methods
        public BoxView(BoxModel boxModel, Sprite sprite)
        {
            BoxModel = boxModel;
            _sprite = sprite;
        }

        public void Initialize(PhysicsSimulator physicsSimulator, Canvas canvas)
        {
            canvas.Children.Add(_sprite);
        }

        public void Draw()
        {
            Vector2 position = ConvertUnits.ToDisplayUnits(BoxModel.Body.Position);
            _sprite.Position = position;
            _sprite.Rotation = MathHelper.DegreesToRadiansRatio * BoxModel.Body.Rotation;
        }
        #endregion

        #region private methods
        #endregion

        #region events
        #endregion

        #region private variables
        Sprite _sprite;        
        #endregion
    }
}
