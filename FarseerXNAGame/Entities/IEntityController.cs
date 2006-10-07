using System;
using System.Collections.Generic;
using System.Text;

namespace FarseerGames.FarseerXNAGame.Entities {
    public interface IEntityController<TEntity> {
        TEntity Entity{get; set;}
    }
}
