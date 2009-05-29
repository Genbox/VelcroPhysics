using System;
using System.Windows;
using SimpleSamplesWPF.Demos;
using SimpleSamplesWPF.Screens;

namespace SimpleSamplesWPF
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        private MainMenu mainMenu;
        private DemoMenu demoMenu;
        private Demo currentDemo;

        private readonly GameLoop gameLoop = new GameLoop();

        public Window1()
        {
            InitializeComponent();

            fps.SetGameLoop(gameLoop);

            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs args)
        {
            gameLoop.Start();

            ShowMainMenu();
        }

        void MainMenuItemClicked(Type chosenDemoType)
        {
            HideMainMenu();

            //shows demo
            Demo demo = (Demo) Activator.CreateInstance(chosenDemoType);
            demo.IsPaused = true; //pause it for now (while demo menu is shown)

            ShowDemo(demo);
            ShowDemoMenu(demo);
        }

        private void QuitToMainMenu(object sender, EventArgs e)
        {
            HideDemo();

            HideDemoMenu();
            ShowMainMenu();
        }

        private void ResumeToDemo(object sender, EventArgs e)
        {
            HideDemoMenu();

            //start demo running
            currentDemo.IsPaused = false;
        }

        private void QuitDemo(object sender, EventArgs e)
        {
            currentDemo.IsPaused = true;

            ShowDemoMenu(currentDemo);
        }

        #region show/hide views

        private void ShowMainMenu()
        {
            if (mainMenu == null)
            {
                this.mainMenu = new MainMenu();
                mainMenu.MenuItemClicked += MainMenuItemClicked;
                foregroundViewContainer.Content = mainMenu;

                foregroundViewContainer.Visibility = Visibility.Visible;
            }
        }

        private void HideMainMenu()
        {
            if (mainMenu != null)
            {
                mainMenu.MenuItemClicked -= MainMenuItemClicked;
                foregroundViewContainer.Content = null;
                mainMenu = null;

                foregroundViewContainer.Visibility = Visibility.Hidden;
            }
        }

        private void ShowDemoMenu(Demo demo)
        {
            if (demoMenu == null)
            {
                demoMenu = new DemoMenu(demo);
                demoMenu.ResumeToDemo += ResumeToDemo;
                demoMenu.Quit += QuitToMainMenu;
                foregroundViewContainer.Content = demoMenu;

                foregroundViewContainer.Visibility = Visibility.Visible;
            }
        }

        private void HideDemoMenu()
        {
            if (demoMenu != null)
            {
                demoMenu.ResumeToDemo -= ResumeToDemo;
                demoMenu.Quit -= QuitToMainMenu;
                foregroundViewContainer.Content = null;
                demoMenu = null;

                foregroundViewContainer.Visibility = Visibility.Hidden;
            }
        }

        private void ShowDemo(Demo demo)
        {
            if (currentDemo == null)
            {
                currentDemo = demo;
                currentDemo.QuitDemo += (QuitDemo);
                demoContainer.Content = demo;
                currentDemo.SetGameLoop(gameLoop);
            }
        }

        private void HideDemo()
        {
            if (currentDemo != null)
            {
                currentDemo.QuitDemo -= (QuitDemo);
                demoContainer.Content = null;
                currentDemo = null;
            }
        }

        #endregion

    }
}
