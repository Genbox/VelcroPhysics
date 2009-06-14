using System.Windows.Media;

namespace FarseerGames.SimpleSamplesSilverlight.Screens
{
    public delegate void MenuItemSelectedEvent(int index);

    public partial class MenuItem
    {
        private bool _selected;
        public int Index;

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
            get { return _selected; }
            set
            {
                if (value != _selected)
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
                _selected = value;
            }
        }
    }
}