using System;

namespace VelcroPhysics.Tools.Serialization.XML
{
    internal class XMLFragmentException : Exception
    {
        public XMLFragmentException(string message)
            : base(message) { }
    }
}