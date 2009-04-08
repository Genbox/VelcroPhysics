namespace FarseerGames.AdvancedSamplesXNA.Demos.Demo8
{
    public class TextMessage
    {
        public float ElapsedTime;
        public string Text;

        public TextMessage(string text)
        {
            Text = text;
            ElapsedTime = 0;
        }

        public TextMessage(string text, float time)
        {
            Text = text;
            ElapsedTime = time;
        }
    }

}
