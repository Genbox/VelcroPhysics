using System;
using System.Collections.Generic;
using System.Text;

namespace FarseerGames.FarseerXNAGame {
    class TestEntityView : IEntityView<TestEntity> {
        #region IEntityView<TestEntity> Members

        public TestEntity t {
            get {
                throw new Exception("The method or operation is not implemented.");
            }
            set {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        #endregion

        #region IEntityView<TestEntity> Members


        public void Draw() {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion
    }
}
