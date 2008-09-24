using System.Windows.Controls;
using System.Windows.Media;

namespace FarseerSilverlightDemos
{
    public delegate void MenuItemSelectedEvent(int index);

    public partial class MenuItem : UserControl
    {
        public int Index;
        private bool selected;

        public MenuItem()
        {
            InitializeComponent();
        }

        public string Text
        {
            set { text.Text = value; }
            get { return text.Text; }
        }

        public bool Selected
        {
            get { return selected; }
            set
            {
                if (value != selected)
                {
                    if (value)
                    {
                        text.Foreground = new SolidColorBrush(Colors.White);
                        selectedStoryboard.Begin();
                    }
                    else
                    {
                        selectedStoryboard.Stop();
                        text.Foreground = new SolidColorBrush(Colors.Black);
                    }
                }
                selected = value;
            }
        }
    }
}