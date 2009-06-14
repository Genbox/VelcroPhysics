using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace SimpleSamplesWPF.Screens
{
    /// <summary>
    /// Interaction logic for MenuItem.xaml
    /// </summary>
    public partial class MenuItem : UserControl
    {
        private bool isSelected;
        public event EventHandler Click;

        public MenuItem(string text)
        {
            InitializeComponent();
            this.textField.Text = text;
        }

        private bool IsSelected
        {
            get { return isSelected; }
            set { 
                if(isSelected == value) return;

                isSelected = value;
                Storyboard storyboard = (Storyboard)FindResource("selectedStoryboard");
                if(isSelected)
                {
                    storyboard.Begin();
                }else
                {
                    storyboard.Stop();
                }
            }
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);

            IsSelected = true;
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);

            IsSelected = false;
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Click != null)
                Click(this, new EventArgs());
        }
    }
}
