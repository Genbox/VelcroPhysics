using System;

namespace VelcroPhysics.Shared.Contracts
{
    public class EnsuresException : Exception
    {
        public EnsuresException(string message) : base(message) { }
    }
}