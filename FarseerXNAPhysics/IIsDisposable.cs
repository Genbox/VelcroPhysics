using System;
using System.Collections.Generic;
using System.Text;

namespace FarseerGames.FarseerXNAPhysics {
    public interface IIsDisposable : IDisposable {
       bool IsDisposed {get;}
    }
}
