// Contributor: Andrew D. Jones

using System.Collections.Generic;
using System.Diagnostics;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Interfaces;

namespace FarseerGames.FarseerPhysics.Collisions
{
    /// <summary>
    /// This class is used to isolate the AABB pairs that are currently in a collision
    /// state without having to check all pair combinations. It relies heavily on frame
    /// coherence or the idea that objects will typically be near their last position
    /// from frame to frame. The class caches the various state information and doesn't
    /// update it unless an extent on an axis "swaps" positions with its neighbor.
    /// Note: If your application has "teleporting" objects or objects that are 
    /// extremely high-speed in relation to other objects, then this Sweep and Prune
    /// method may breakdown. 
    /// </summary>
    public class SweepAndPruneCollider : IBroadPhaseCollider
    {
        private const float _floatTolerance = 0.01f; //1.5f; //.01f;
        private PhysicsSimulator _physicsSimulator;
        private ExtentList _xExtentList;
        private ExtentInfoList _xInfoList;
        private ExtentList _yExtentList;
        private ExtentInfoList _yInfoList;

        /// <summary>
        /// The collision pairs
        /// </summary>
        public CollisionPairDictionary CollisionPairs;

        public SweepAndPruneCollider(PhysicsSimulator physicsSimulator)
        {
            _physicsSimulator = physicsSimulator;
            _xExtentList = new ExtentList(this);
            _yExtentList = new ExtentList(this);
            _xInfoList = new ExtentInfoList(this);
            _yInfoList = new ExtentInfoList(this);
            CollisionPairs = new CollisionPairDictionary();
        }

        #region IBroadPhaseCollider Members

        /// <summary>
        /// Fires when a broad phase collision occurs
        /// </summary>
        public event BroadPhaseCollisionHandler OnBroadPhaseCollision;

#if (!SILVERLIGHT)
        /// <summary>
        /// Used by the <see cref="PhysicsSimulator"/> to remove geometry from Sweep and Prune once it
        /// has been disposed.
        /// </summary>
        public void ProcessDisposedGeoms()
        {
            //TODO: Could use lamda expressions. Need to test the performance first.
            if (_xInfoList.RemoveAll(delegate(ExtentInfo i) { return i.Geometry.IsDisposed; }) > 0)
            {
                _xExtentList.RemoveAll(delegate(Extent n) { return n.Info.Geometry.IsDisposed; });
            }

            if (_yInfoList.RemoveAll(delegate(ExtentInfo i) { return i.Geometry.IsDisposed; }) > 0)
            {
                _yExtentList.RemoveAll(delegate(Extent n) { return n.Info.Geometry.IsDisposed; });
            }

            // We force a non-incremental update because that will insure that the
            // CollisionPairs get recreated and that the geometry isn't being held
            // by overlaps, etc. Its just easier this way.
            ForceNonIncrementalUpdate();
        }

        /// <summary>
        /// Used by the <see cref="PhysicsSimulator"/> to remove geometry from Sweep and Prune once it
        /// has been removed.
        /// </summary>
        public void ProcessRemovedGeoms()
        {
            if (_xInfoList.RemoveAll(delegate(ExtentInfo i) { return i.Geometry.isRemoved; }) > 0)
            {
                _xExtentList.RemoveAll(delegate(Extent n) { return n.Info.Geometry.isRemoved; });
            }
            if (_yInfoList.RemoveAll(delegate(ExtentInfo i) { return i.Geometry.isRemoved; }) > 0)
            {
                _yExtentList.RemoveAll(delegate(Extent n) { return n.Info.Geometry.isRemoved; });
            }

            // We force a non-incremental update because that will insure that the
            // CollisionPairs get recreated and that the geometry isn't being held
            // by overlaps, etc. Its just easier this way.
            ForceNonIncrementalUpdate();
        }
#else
        private int ExtentInfoListRemoveAllRemoved(ExtentInfoList l)
        {
            int removed = 0;
            for (int i = 0; i < l.Count; i++)
            {
                if (l[i].Geometry.isRemoved)
                {
                    removed++;
                    l.RemoveAt(i);
                    i--;
                }
            }
            return removed;
        }

        private int ExtentListRemoveAllRemoved(ExtentList l)
        {
            int removed = 0;
            for (int i = 0; i < l.Count; i++)
            {
                if (l[i].Info.Geometry.isRemoved)
                {
                    removed++;
                    l.RemoveAt(i);
                    i--;
                }
            }
            return removed;
        }

        private int ExtentInfoListRemoveAllDisposed(ExtentInfoList l)
        {
            int removed = 0;
            for (int i = 0; i < l.Count; i++)
            {
                if (l[i].Geometry.IsDisposed)
                {
                    removed++;
                    l.RemoveAt(i);
                    i--;
                }
            }
            return removed;
        }

        private int ExtentListRemoveAllDisposed(ExtentList l)
        {
            int removed = 0;
            for (int i = 0; i < l.Count; i++)
            {
                if (l[i].Info.Geometry.IsDisposed)
                {
                    removed++;
                    l.RemoveAt(i);
                    i--;
                }
            }
            return removed;
        }

        /// <summary>
        /// Used by the PhysicsSimulator to remove geometry from Sweep and Prune once it
        /// has been disposed.
        /// </summary>
        public void ProcessDisposedGeoms()
        {
            if (ExtentInfoListRemoveAllDisposed(_xInfoList) > 0)
            {
                ExtentListRemoveAllDisposed(_xExtentList);
            }
            if (ExtentInfoListRemoveAllDisposed(_yInfoList) > 0)
            {
                ExtentListRemoveAllDisposed(_yExtentList);
            }


            // We force a non-incremental update because that will insure that the
            // CollisionPairs get recreated and that the geometry isn't being held
            // by overlaps, etc. Its just easier this way.
            ForceNonIncrementalUpdate();
        }
        public void ProcessRemovedGeoms()
        {
            if (ExtentInfoListRemoveAllRemoved(_xInfoList) > 0)
            {
                ExtentListRemoveAllRemoved(_xExtentList);
            }
            if (ExtentInfoListRemoveAllRemoved(_yInfoList) > 0)
            {
                ExtentListRemoveAllRemoved(_yExtentList);
            }

            // We force a non-incremental update because that will insure that the
            // CollisionPairs get recreated and that the geometry isn't being held
            // by overlaps, etc. Its just easier this way.
            ForceNonIncrementalUpdate();
        }
#endif

        /// <summary>
        /// This method is used by the <see cref="PhysicsSimulator"/> to notify Sweep and Prune that 
        /// new geometry is to be tracked.
        /// </summary>
        /// <param name="geom">The geometry to be added</param>
        public void Add(Geom geom)
        {
            ExtentInfo xExtentInfo = new ExtentInfo(geom, geom.AABB.Min.X, geom.AABB.Max.X);
            _xInfoList.Add(xExtentInfo);
            _xExtentList.IncrementalInsertExtent(xExtentInfo);

            ExtentInfo yExtentInfo = new ExtentInfo(geom, geom.AABB.Min.Y, geom.AABB.Max.Y);
            _yInfoList.Add(yExtentInfo);
            _yExtentList.IncrementalInsertExtent(yExtentInfo);
        }

        /// <summary>
        /// Incrementally updates the system. Assumes relatively good frame coherence.
        /// </summary>
        public void Update()
        {
            UpdateExtentValues();

            _xExtentList.IncrementalSort();
            _yExtentList.IncrementalSort();

            _xInfoList.MoveUnderConsiderationToOverlaps();
            _yInfoList.MoveUnderConsiderationToOverlaps();

            HandleCollisions();
        }

        #endregion

        /// <summary>
        /// Test AABB collisions between two geometries. Tests include checking if the
        /// geometries are enabled, static, in the right collision categories, etc.
        /// </summary>
        /// <returns>Returns true if there is a collision, false otherwise</returns>
        public static bool DoCollision(Geom g1, Geom g2)
        {
            if (!g1.body.Enabled || !g2.body.Enabled)
                return false;

            if ((g1.collisionGroup == g2.collisionGroup) &&
                g1.collisionGroup != 0 && g2.collisionGroup != 0)
                return false;

            if (!g1.collisionEnabled || !g2.collisionEnabled)
                return false;

            if (g1.body.isStatic && g2.body.isStatic)
                return false;

            if (g1.body == g2.body)
                return false;

            if (((g1.collisionCategories & g2.collidesWith) ==
                 CollisionCategory.None) & ((g2.collisionCategories &
                                             g1.collidesWith) == CollisionCategory.None))
                return false;

            //TMP
            AABB aabb1 = new AABB();
            AABB aabb2 = new AABB();
            aabb1.min = g1.aabb.min;
            aabb1.max = g1.aabb.max;
            aabb2.min = g2.aabb.min;
            aabb2.max = g2.aabb.max;
            aabb1.min.X -= _floatTolerance;
            aabb1.min.Y -= _floatTolerance;
            aabb1.max.X += _floatTolerance;
            aabb1.max.Y += _floatTolerance;
            aabb2.min.X -= _floatTolerance;
            aabb2.min.Y -= _floatTolerance;
            aabb2.max.X += _floatTolerance;
            aabb2.max.Y += _floatTolerance;

            //NOTE: Changed from
            //            if (AABB.Intersect(g1.aabb, g2.aabb) == false)
            //                return false;
            // return true

            //TO:
            return AABB.Intersect(aabb1, aabb2);
        }

        /// <summary>
        /// Updates the values in the x and y extent lists by the changing AABB values.
        /// </summary>
        private void UpdateExtentValues()
        {
            for (int i = 0; i < _xInfoList.Count; i++)
            {
                ExtentInfo xInfo = _xInfoList[i];
                ExtentInfo yInfo = _yInfoList[i];

                AABB aabb = xInfo.Geometry.aabb;

                xInfo.Min.Value = aabb.min.X - _floatTolerance;
                xInfo.Max.Value = aabb.max.X + _floatTolerance;
                yInfo.Min.Value = aabb.min.Y - _floatTolerance;
                yInfo.Max.Value = aabb.max.Y + _floatTolerance;
            }
        }

        /// <summary>
        /// Iterates over the collision pairs and creates arbiters.
        /// </summary>
        private void HandleCollisions()
        {
            foreach (CollisionPair cp in CollisionPairs.Keys)
            {
                // Note: Possible optimization. Maybe arbiter can be cached into Value of
                // CollisionPairs? Currently, the CollisionPairs hash doesn't use its
                // Value parameter - its just an unused bool Value.
                Arbiter arbiter = _physicsSimulator.arbiterPool.Fetch();
                arbiter.ConstructArbiter(cp.Geom1, cp.Geom2, _physicsSimulator);

                if (!_physicsSimulator.arbiterList.Contains(arbiter))
                    _physicsSimulator.arbiterList.Add(arbiter);
                else
                    _physicsSimulator.arbiterPool.Insert(arbiter);
            }
        }

        /// <summary>
        /// Just calls Update.
        /// </summary>
        public void Run()
        {
            //NOTE: bForce was always false, bug?
            //if (bForce)
            //    ForceNonIncrementalUpdate();
            //else
            Update();
        }

        /// <summary>
        /// This function can be used for times when frame-coherence is temporarily lost
        /// or when it is simply more convenient to completely rebuild all the cached
        /// data instead of incrementally updating it. Currently it is used after
        /// removing disposed/removed geometries. If your application had an object
        /// that teleported across the universe or some other situation where
        /// frame-coherence was lost, you might consider this function.
        /// </summary>
        public void ForceNonIncrementalUpdate()
        {
            UpdateExtentValues();

            // First, wipe out the collision records
            CollisionPairs.Clear();

            // And clear out all the overlap records
            Debug.Assert(_xInfoList.Count == _yInfoList.Count);
            for (int i = 0; i < _xInfoList.Count; i++)
            {
                _xInfoList[i].Overlaps.Clear();
                _xInfoList[i].UnderConsideration.Clear();
                _yInfoList[i].Overlaps.Clear();
                _yInfoList[i].UnderConsideration.Clear();
            }

            // Force sort
            _xExtentList.Sort((l, r) => l.Value.CompareTo(r.Value));
            _yExtentList.Sort((l, r) => l.Value.CompareTo(r.Value));

            // Rebuild overlap information
            List<Geom> overlaps = new List<Geom>();
            for (int i = 0; i < 2; i++)
            {
                overlaps.Clear();

                ExtentList extentList = i == 0 ? _xExtentList : _yExtentList;

                foreach (Extent extent in extentList)
                {
                    if (extent.IsMin)
                    {
                        // Add whatever is currently in overlaps to this
                        extent.Info.Overlaps.InsertRange(0, overlaps);

                        // Now add, this geom to overlaps
                        overlaps.Add(extent.Info.Geometry);
                    }
                    else
                    {
                        // remove this geom from overlaps
                        overlaps.Remove(extent.Info.Geometry);

                        // Test this geom against its overlaps for collisionpairs
                        Geom thisGeom = extent.Info.Geometry;
                        foreach (Geom g in extent.Info.Overlaps)
                        {
                            if (DoCollision(thisGeom, g) == false)
                                continue;

                            //TODO: Should call OnBroadPhaseCollision event here. But not until
                            //the Run() method has been looked after

                            CollisionPairs.AddPair(thisGeom, g);
                        }
                    }
                }
            }
            HandleCollisions();
        }

        #region Nested type: CollisionPair

        /// <summary>
        /// Houses collision pairs as geom1 and geom2. The pairs are always ordered such
        /// that the lower id geometry is first. This allows the <see cref="CollisionPairDictionary"/>
        /// to have a consistent key / hash code for a pair of geometry.
        /// </summary>
        public struct CollisionPair
        {
            public Geom Geom1;
            public Geom Geom2;

            public CollisionPair(Geom g1, Geom g2)
            {
                if (g1 < g2)
                {
                    Geom1 = g1;
                    Geom2 = g2;
                }
                else
                {
                    Geom1 = g2;
                    Geom2 = g1;
                }
            }

            public override int GetHashCode()
            {
                // Arbitrarly choose 10000 as a number of colliders that we won't 
                // approach any time soon.
                return (Geom1.Id*10000 + Geom2.Id);
            }

            public override bool Equals(object obj)
            {
                if (!(obj is CollisionPair))
                    return false;

                return Equals((CollisionPair) obj);
            }

            /// <summary>
            /// Checks to see if the specified <see cref="CollisionPair"/> equals this instance
            /// </summary>
            /// <param name="other">The other.</param>
            /// <returns></returns>
            public bool Equals(CollisionPair other)
            {
                if (Geom1 == other.Geom1)
                    return (Geom2 == other.Geom2);

                return false;
            }

            public static bool operator ==(CollisionPair first, CollisionPair second)
            {
                return first.Equals(second);
            }

            public static bool operator !=(CollisionPair first, CollisionPair second)
            {
                return !first.Equals(second);
            }
        }

        #endregion

        #region Nested type: CollisionPairDictionary

        /// <summary>
        /// This class is used to keep track of the pairs of geometry that need to be
        /// passed on to the narrow phase. The keys stored in the dictionary are
        /// the actual geometry pairs (the boolean Value is currently unused).
        /// NOTE: May eventually want to add OnEnterCollisionState / 
        /// OnExitCollisionState callbacks which might be useful for debugging
        /// or possibly in user applications.
        /// </summary>
        public class CollisionPairDictionary : Dictionary<CollisionPair, bool>
        {
            ///<summary>
            ///Remove a pair of geoms
            ///</summary>
            ///<param name="g1"></param>
            ///<param name="g2"></param>
            public void RemovePair(Geom g1, Geom g2)
            {
                CollisionPair cp = new CollisionPair(g1, g2);
                Remove(cp);

                // May want a OnDeactivatedCollision here.
                // For example, if you were highlighting colliding
                // ABV's for debugging, this callback would let you
                // know to stop.
            }

            /// <summary>
            /// Adds the a pair of geoms.
            /// </summary>
            /// <param name="g1">The g1.</param>
            /// <param name="g2">The g2.</param>
            public void AddPair(Geom g1, Geom g2)
            {
                CollisionPair cp = new CollisionPair(g1, g2);

                // This check is a trade-off. In many cases, we don't need to perform
                // this check and we could just do a "try" block instead, however,
                // when exceptions are thrown, they are mega-slow... so checking for
                // the key is really the best option all round.
                if (ContainsKey(cp))
                    return;

                Add(cp, true);
            }
        }

        #endregion

        #region Nested type: Extent

        /// <summary>
        /// This class represents a single extent of an AABB on a single axis. It has a
        /// reference to <see cref="ExtentInfo"/> which has information about the geometry it belongs
        /// to.
        /// </summary>
        public class Extent
        {
            public ExtentInfo Info;
            public bool IsMin;
            public float Value;

            public Extent(ExtentInfo info, float value, bool isMin)
            {
                Info = info;
                Value = value;
                IsMin = isMin;
            }
        }

        #endregion

        #region Nested type: ExtentInfo

        /// <summary>
        /// This class contains represents additional extent info for a particular axis
        /// It has a reference to the geometry whose extents are being tracked. It
        /// also has a min and max extent reference into the <see cref="ExtentList"/> itself.
        /// The class keeps track of overlaps with other geometries.
        /// </summary>
        public class ExtentInfo
        {
            public Geom Geometry; // Specific to Farseer
            public Extent Max;
            public Extent Min;
            public List<Geom> Overlaps;
            public List<Geom> UnderConsideration;

            public ExtentInfo(Geom g, float min, float max)
            {
                Geometry = g;
                UnderConsideration = new List<Geom>();
                Overlaps = new List<Geom>();
                Min = new Extent(this, min, true);
                Max = new Extent(this, max, false);
            }
        }

        #endregion

        #region Nested type: ExtentInfoList

        /// <summary>
        /// This class keeps a list of information that relates extents to geometries.
        /// </summary>
        private class ExtentInfoList : List<ExtentInfo>
        {
            public SweepAndPruneCollider Owner;

            public ExtentInfoList(SweepAndPruneCollider sap)
            {
                Owner = sap;
            }

            public void MoveUnderConsiderationToOverlaps()
            {
                for (int i = 0; i < Count; i++)
                {
                    if (this[i].UnderConsideration.Count == 0)
                        continue;

                    Geom g1 = this[i].Geometry;

                    // First transfer those under consideration to overlaps,
                    // for, they have been considered...
                    int startIndex = this[i].Overlaps.Count;
                    this[i].Overlaps.AddRange(this[i].UnderConsideration);
                    this[i].UnderConsideration.Clear();

                    for (int j = startIndex; j < this[i].Overlaps.Count; j++)
                    {
                        Geom g2 = this[i].Overlaps[j];

                        // It is possible that we may test the same pair of geometries
                        // for both extents (x and y), however, I'm banking on that
                        // one of the extents has probably already been cached and
                        // therefore, won't be checked.
                        if (DoCollision(g1, g2) == false)
                            continue;

                        //Call the OnBroadPhaseCollision event first. If the user aborts the collision
                        //it will not create an arbiter
                        if (Owner.OnBroadPhaseCollision != null)
                        {
                            if (Owner.OnBroadPhaseCollision(g1, g2))
                                Owner.CollisionPairs.AddPair(g1, g2);
                        }
                        else
                        {
                            Owner.CollisionPairs.AddPair(g1, g2);
                        }
                    }
                }
            }
        }

        #endregion

        #region Nested type: ExtentList

        /// <summary>
        /// Represents a lists of extents for a given axis. This list will be kept
        /// sorted incrementally.
        /// </summary>
        public class ExtentList : List<Extent>
        {
            public SweepAndPruneCollider Owner;

            public ExtentList(SweepAndPruneCollider sap)
            {
                Owner = sap;
            }

            /// <summary>
            /// Inserts a new Extent into the already sorted list. As the <see cref="ExtentList"/>
            /// class is currently derived from the generic List class, insertions
            /// of new geometry (and extents) are going to be somewhat slow right
            /// off the bat. Additionally, this function currently performs 
            /// linear insertion. Two big optimizations in the future would be to
            /// (1) make this function perform a binary search and (2) allow for
            /// a "hint" of what index to start with. The reason for this is because
            /// there is always a min and max extents that need inserting and we
            /// know the max extent is always after the min extent.
            /// </summary>
            private int InsertIntoSortedList(Extent newExtent)
            {
                // List<> is not the most speedy for insertion, however, since
                // we don't plan to do this except for when geometry is added
                // to the system, we go ahead an use List<> instead of LinkedList<>
                // This code, btw, assumes the list is already sorted and that
                // the new entry just needs to be inserted.
                //
                //TODO: Optimization Note: A binary search could be used here and would
                // improve speed for when adding geometry.
                int insertAt = 0;

                // Check for empty list
                if (Count == 0)
                {
                    Add(newExtent);
                    return 0;
                }

                while (insertAt < Count &&
                       (this[insertAt].Value < newExtent.Value ||
                        (this[insertAt].Value == newExtent.Value &&
                         this[insertAt].IsMin && !newExtent.IsMin)))
                {
                    insertAt++;
                }

                Insert(insertAt, newExtent);
                return insertAt;
            }

            /// <summary>
            /// Incrementally inserts the min/max extents into the <see cref="ExtentList"/>. As it
            /// does so, the method ensures that overlap records, the <see cref="CollisionPair"/>
            /// map, and all other book-keeping is up to date.
            /// </summary>
            /// <param name="ourInfo">The extent info for a give axis</param>
            public void IncrementalInsertExtent(ExtentInfo ourInfo)
            {
                Extent min = ourInfo.Min;
                Extent max = ourInfo.Max;

                Debug.Assert(min.Value < max.Value);

                int iMin = InsertIntoSortedList(min);
                int iMax = InsertIntoSortedList(max);

                Geom ourGeom = ourInfo.Geometry;

                // As this is a newly inserted extent, we need to update the overlap 
                // information.

                // RULE 1: Traverse from min to max. Look for other "min" Extents
                // and when found, add our wrapper/geometry to their list.
                int iCurr = iMin + 1;
                while (iCurr != iMax)
                {
                    if (this[iCurr].IsMin)
                        this[iCurr].Info.UnderConsideration.Add(ourGeom);
                    iCurr++;
                }

                // RULE 2: From min, traverse to the left until we encounter
                // another "min" extent. If we find one, we add its geometry
                // to our underConsideration list and go to RULE 3, otherwise
                // there is no more work to do and we can exit.
                iCurr = iMin - 1;
                while (iCurr >= 0 && this[iCurr].IsMin == false)
                    iCurr--;

                if (iCurr < 0)
                    return;

                List<Geom> ourUnderConsideration = ourInfo.UnderConsideration;
                Extent currExtent = this[iCurr];

                ourUnderConsideration.Add(currExtent.Info.Geometry);

                // RULE 3: Now that we have found a "min" extent, we take
                // its existing overlap list and copy it into our underConsideration
                // list. All except for ourselves.
                ourUnderConsideration.AddRange(currExtent.Info.UnderConsideration);
                ourUnderConsideration.Remove(ourGeom); // just in case
                /*LinkedListNode<Geom> currGeomNode = 
                    currExtent.info.underConsideration.First;

                while (currGeomNode != null)
                {
                    if (currGeomNode.Value != ourGeom)
                    {
                        ourUnderConsideration.AddLast(new LinkedListNode<Geom>(
                            currGeomNode.Value));
                    }
                    currGeomNode = currGeomNode.Next;
                }*/

                // RULE 4: Move from the found extent back toward our "min" extent.
                // Whenever and "max" extent is found, we remove its reference
                // from our extents list.
                while (iCurr != iMin)
                {
                    if (currExtent.IsMin == false)
                    {
                        ourUnderConsideration.Remove(currExtent.Info.Geometry);

                        if (ourInfo.Overlaps.Remove(currExtent.Info.Geometry))
                        {
                            Owner.CollisionPairs.RemovePair(ourGeom,
                                                            currExtent.Info.Geometry);
                        }
                    }
                    currExtent = this[++iCurr];
                }
            }

            /// <summary>
            /// Incrementally sorts <see cref="ExtentList"/>. It is assumed that there is a high level
            /// of frame coherence and that much of the list is already fairly well
            /// sorted. This algorithm makes use of "insert sort" which is notoriously
            /// slow - except for when a list is already almost sorted - which is the
            /// case when there is high frame coherence.
            /// </summary>
            public void IncrementalSort()
            {
                if (Count < 2)
                    return;

                for (int i = 0; i < Count - 1; i++)
                {
                    int evalCnt = i + 1;
                    Extent evalExtent = this[evalCnt];
                    Extent currExtent = this[i];

                    if (currExtent.Value <= evalExtent.Value)
                        continue;

                    Extent savedExtent = evalExtent;

                    if (evalExtent.IsMin)
                    {
                        while (currExtent.Value > evalExtent.Value)
                            //while (currExtent.Value >= evalExtent.Value)
                        {
                            if (currExtent.IsMin)
                            {
                                // Begin extent is inserted before another begin extent.
                                // So, Inserted extent's object looses reference to
                                // non-inserted extent and Non-inserted extent gains 
                                // reference to inserted extent's object.
                                evalExtent.Info.UnderConsideration.Remove(
                                    currExtent.Info.Geometry);

                                if (evalExtent.Info.Overlaps.Remove(
                                    currExtent.Info.Geometry))
                                {
                                    Owner.CollisionPairs.RemovePair(
                                        evalExtent.Info.Geometry,
                                        currExtent.Info.Geometry);
                                }

                                // Add extent
                                currExtent.Info.UnderConsideration.Add(
                                    evalExtent.Info.Geometry);
                            }
                            else
                            {
                                // "min" extent inserted before the max extent.
                                // Inserted extent gains reference to non-inserted extent.
                                evalExtent.Info.UnderConsideration.Add(
                                    currExtent.Info.Geometry);
                            }

                            this[evalCnt--] = this[i--];
                            if (i < 0)
                                break;
                            currExtent = this[i];
                        }
                    }
                    else
                    {
                        while (currExtent.Value > evalExtent.Value)
                            //while (currExtent.Value >= evalExtent.Value)
                        {
                            if (currExtent.IsMin)
                            {
                                // Ending extent inserted before a beginning extent
                                // the non inserted extent looses a reference to the
                                // inserted one
                                currExtent.Info.UnderConsideration.Remove(
                                    evalExtent.Info.Geometry);

                                if (currExtent.Info.Overlaps.Remove(
                                    evalExtent.Info.Geometry))
                                {
                                    Owner.CollisionPairs.RemovePair(
                                        evalExtent.Info.Geometry,
                                        currExtent.Info.Geometry);
                                }
                            }
                            this[evalCnt--] = this[i--];
                            if (i < 0)
                                break;
                            currExtent = this[i];
                        }
                    }
                    this[evalCnt] = savedExtent;
                }
            }
        }

        #endregion
    }
}