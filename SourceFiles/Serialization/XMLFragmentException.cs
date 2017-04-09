using System;

namespace VelcroPhysics.Serialization
{
    internal class XMLFragmentException : Exception
    {
        public XMLFragmentException(string message)
            : base(message) { }
    }
}