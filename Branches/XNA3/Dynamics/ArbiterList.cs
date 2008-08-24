using System;
using System.Collections.Generic;
using System.Text;

using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Collisions;

namespace FarseerGames.FarseerPhysics.Dynamics {
    public class ArbiterList : List<Arbiter> {
        private List<Arbiter> markedForRemovalList;

        public ArbiterList() {
            markedForRemovalList = new List<Arbiter>();
        }

        public void ForEachSafe(Action<Arbiter> action) {
            for (int i = 0; i < Count; i++) {
                action(this[i]);
            }
        }

        public void RemoveAllSafe(Predicate<Arbiter> match) {
            for (int i = 0; i < Count; i++) {
                if (match(this[i])) {
                    markedForRemovalList.Add(this[i]);
                }
            }
            for (int j = 0; j < markedForRemovalList.Count; j++) {
                this.Remove(markedForRemovalList[j]);
                markedForRemovalList[j].Reset();
            }
            markedForRemovalList.Clear();
        }

        public void RemoveContactCountEqualsZero(Pool<Arbiter> arbiterPool) {
            for (int i = 0; i < Count; i++) {
                if (ContactCountEqualsZero(this[i])) {
                    markedForRemovalList.Add(this[i]);
                }
            }
            for (int j = 0; j < markedForRemovalList.Count; j++) {
                this.Remove(markedForRemovalList[j]);
                arbiterPool.Release (markedForRemovalList[j]);
            }
            markedForRemovalList.Clear();
        }

        public void RemoveContainsDisposedBody(Pool<Arbiter> arbiterPool) {
            for (int i = 0; i < Count; i++) {
                if (ContainsDisposedBody(this[i])) {
                    markedForRemovalList.Add(this[i]);
                }
            }
            for (int j = 0; j < markedForRemovalList.Count; j++) {
                this.Remove(markedForRemovalList[j]);
                arbiterPool.Release(markedForRemovalList[j]);
            }
            markedForRemovalList.Clear();
        }

        internal static bool ContactCountEqualsZero(Arbiter a) {
            return a.ContactCount == 0;
        }

        internal static bool ContainsDisposedBody(Arbiter a) {
            return a.ContainsDisposedGeom();
        }
        
    }
}
