using System;
using System.Collections.Generic;
using System.Text;

namespace FarseerGames.FarseerXNAGame {
    public class TestEntity : IEntity {
        #region IEntity Members

        public float Orientation() {
            throw new Exception("The method or operation is not implemented.");
        }

        public Microsoft.Xna.Framework.Vector2 Position() {
            throw new Exception("The method or operation is not implemented.");
        }

        public bool Remove {
            get {
                throw new Exception("The method or operation is not implemented.");
            }
            set {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        #endregion
    }
}
