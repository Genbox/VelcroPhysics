using System;
using System.Collections.Generic;
using System.Text;

using FarseerGames.FarseerXNAGame.Entities;

namespace FarseerGames.FarseerXNAGame.Input {
    public class KeyboardEntityController<TEntity> : IEntityController<TEntity> where TEntity : IEntity {
        protected TEntity entity;
        protected IKeyboardInputService keyboardInputService;

        public KeyboardEntityController(IKeyboardInputService keyboardInputService) {
            this.keyboardInputService = keyboardInputService;
        }

        public KeyboardEntityController(TEntity entity, IKeyboardInputService keyboardInputService) {
            this.keyboardInputService = keyboardInputService;
            this.entity = entity;
        }

        public TEntity Entity {
            get { return entity; }
            set { entity = value; }
        }

    }
}
