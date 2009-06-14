using System.Windows;
using System.Windows.Controls;
using FarseerGames.WaterSampleSilverlight.Controllers;

namespace FarseerGames.WaterSampleSilverlight
{
    public partial class Page
    {
        #region properties

        #endregion

        #region public methods

        public Page()
        {
            InitializeComponent();
            Loaded += Page_Loaded;
        }

        #endregion

        #region private methods

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //create the game controller.
            _gameController = new GameController(this);
            _gameController.Initialize();
        }

        #endregion

        public void Navigate(UserControl screen)
        {
            Content = screen;
        }

        #region events

        #endregion

        #region private variables

        private GameController _gameController;

        #endregion
    }
}