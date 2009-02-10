using System;

namespace FarseerGames.FarseerPhysics.Interfaces
{
    /// <summary>
    /// Interface used by the generic list
    /// </summary>
    public interface IIsDisposable : IDisposable
    {
        bool IsDisposed { get; set; }
    }
}
