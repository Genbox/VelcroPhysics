using System;
using System.Collections.Generic;
using System.Text;

namespace FarseerGames.FarseerXNAGame.Entities {
    public interface IEntityView<TEntity> : IDisposable  {
        TEntity Entity {get;set;}
        void UpdateView();
    }
}
