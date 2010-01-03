namespace DemoBaseXNA.ScreenSystem
{
    public class MenuItem
    {
        public MenuItem(GameScreen screen, bool isExitItem)
        {
            Screen = screen;
            IsExitItem = isExitItem;
        }

        public GameScreen Screen;
        public bool IsExitItem;
    }
}
