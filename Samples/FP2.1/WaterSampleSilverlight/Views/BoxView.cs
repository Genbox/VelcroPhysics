using System.Windows.Controls;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Mathematics;
using FarseerGames.WaterSampleSilverlight.Models;
using FarseerGames.WaterSampleSilverlight.RenderSystem;

namespace FarseerGames.WaterSampleSilverlight.Views
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
            _sprite.Rotation = MathHelper.ToDegrees(BoxModel.Body.Rotation);
        }

        #endregion

        #region private methods

        #endregion

        #region events

        #endregion

        #region private variables

        private Sprite _sprite;

        #endregion
    }
}