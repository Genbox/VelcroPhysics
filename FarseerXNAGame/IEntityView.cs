using System;
using System.Collections.Generic;
using System.Text;

namespace FarseerGames.FarseerXNAGame {
    interface IEntityView<T> {
         T t {get;set;}

        void Draw();
    }
}
