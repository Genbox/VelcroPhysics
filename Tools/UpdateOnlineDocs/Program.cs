using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace UpdateOnlineDocs
{
    class Program
    {
        static void Main(string[] args)
        {
            string docPath = args[0];
            string replacementDocPath = args[1];

            //string docPath = "Farseer Physics Engine 2.0 Manual.htm";
            //string replacementDocPath = "ReplacementText.txt";

            string docContent = File.ReadAllText(docPath, Encoding.UTF8);
            string replacementContent = File.ReadAllText(replacementDocPath, Encoding.UTF8);

            UpdateDocumentation(docPath, docContent, replacementContent);
        }

        private static string UpdateReplacementText(string replacementContent, int number)
        {
            Regex regex = new Regex("value=\\\"demo\\=[0-9]\\\"", RegexOptions.CultureInvariant | RegexOptions.Compiled);
            string regexReplace = string.Format("value=\"demo={0}\"", number);

            return regex.Replace(replacementContent, regexReplace);
        }

        private static void UpdateDocumentation(string docPath, string docContent, string replacementContent)
        {
            Regex regex = new Regex("\\|\\|Demo(?<Number>[0-9])\\|\\|", RegexOptions.CultureInvariant | RegexOptions.Compiled);

            foreach (Match match in regex.Matches(docContent))
            {
                int number = int.Parse(match.Groups["Number"].Value);
                string updateText = UpdateReplacementText(replacementContent, number);

                docContent = docContent.Replace(string.Format("||Demo{0}||", number), updateText);
            }

            File.WriteAllText(docPath, docContent, Encoding.UTF8);
        }
    }
}