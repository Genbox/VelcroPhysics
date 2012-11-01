namespace FarseerPhysics.ScreenSystem
{
    public class MenuItem
    {
        public bool IsExitItem;
        public GameScreen Screen;

        public MenuItem(GameScreen screen, bool isExitItem)
        {
            Screen = screen;
            IsExitItem = isExitItem;
        }
    }
}