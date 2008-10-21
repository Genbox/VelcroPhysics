using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

using FarseerPhysicsWaterDemo.Controllers;

namespace FarseerPhysicsWaterDemo
{
    public partial class Page : UserControl
    {
        #region properties
        #endregion

        #region public methods
        public Page()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(Page_Loaded);
        }
        #endregion

        #region private methods
        void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //create the game controller.
            _gameController = new GameController(this);
            _gameController.Initialize();
        }
        #endregion

        public void Navigate(UserControl screen)
        {
            this.Content = screen;
        }

        #region events
        #endregion

        #region private variables
        private GameController _gameController;
        #endregion
    }
}
