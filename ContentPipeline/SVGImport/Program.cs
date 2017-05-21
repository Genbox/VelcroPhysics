using System.Collections.Generic;
using VelcroPhysics.ContentPipelines.SVGImport.Objects;

namespace VelcroPhysics.ContentPipelines.SVGImport
{
    class Program
    {
        public static void Main()
        {
            SVGImporter i = new SVGImporter();
            List<PathDefinition> res = i.Import(@"E:\Source control\VelcroPhysics\Samples\Demo\Content\Pipeline\Body.svg", null);


        }
    }
}
