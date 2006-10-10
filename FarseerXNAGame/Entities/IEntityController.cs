using System;
using System.Collections.Generic;
using System.Text;

namespace FarseerGames.FarseerXNAGame.Entities {
    public interface IEntityController<TEntity> where TEntity : IEntity {
        TEntity Entity{get; set;}
    }
}
