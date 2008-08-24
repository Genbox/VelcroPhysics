using System;
using System.Collections.Generic;
using System.Text;

namespace FarseerGames.FarseerPhysics.Collisions {
    public class GeomList : List<Geom> {
        private List<Geom> markedForRemovalList = new List<Geom>();

        public delegate void ContentsChangedEventHandler(Geom geom);
        public ContentsChangedEventHandler Added;
        public ContentsChangedEventHandler Removed;

        public new void Add(Geom geom) {
            base.Add(geom);
            if (Added != null) { Added(geom); }
        }

        public new void Remove(Geom geom) {
            base.Remove(geom);
            if (Removed != null) { Removed(geom); }
        }

        private int numberDisposed;
        public int RemoveDisposed() {
            for (int i = 0; i < Count; i++) {
                if (IsDisposed(this[i])) {
                    markedForRemovalList.Add(this[i]);
                }
            }
            for (int j = 0; j < markedForRemovalList.Count; j++) {
                this.Remove(markedForRemovalList[j]);
            }
            numberDisposed = markedForRemovalList.Count;
            markedForRemovalList.Clear();
            return numberDisposed;
        }

        internal static bool IsDisposed(Geom a) {
            return a.IsDisposed;
        }
    }
}
