using System.Collections.Generic;

namespace VelcroPhysics.Tools.Serialization.XML
{
    internal class XMLFragmentElement
    {
        private List<XMLFragmentAttribute> _attributes = new List<XMLFragmentAttribute>();
        private List<XMLFragmentElement> _elements = new List<XMLFragmentElement>();

        public IList<XMLFragmentElement> Elements
        {
            get { return _elements; }
        }

        public IList<XMLFragmentAttribute> Attributes
        {
            get { return _attributes; }
        }

        public string Name { get; set; }
        public string Value { get; set; }
        public string OuterXml { get; set; }
        public string InnerXml { get; set; }
    }
}