using System;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Interfaces;
using Microsoft.Xna.Framework;

namespace FarseerGames.FarseerPhysics.Collisions
{
    /// <remark>
    /// The geometry class is the unit of collision detection.
    /// </remark>
    public class Geom : IEquatable<Geom>, IIsDisposable
    {
        #region Delegates

        public delegate bool CollisionEventHandler(Geom geometry1, Geom geometry2, ContactList contactList);

        #endregion

        internal AABB aabb = new AABB();
        internal Body body;
        private EventHandler<EventArgs> _bodyDisposed;
        private Body.UpdatedEventHandler _bodyUpdated;

        internal CollisionCategories collidesWith = CollisionCategories.All;
                                           //collides with all categories by default

        public CollisionEventHandler Collision;

        internal CollisionCategories collisionCategories = CollisionCategories.All;
                                           //member off all categories by default

        internal bool collisionEnabled = true;
        internal int collisionGroup;
        private float _collisonGridCellSize;
        internal bool collsionResponseEnabled = true;
        internal float frictionCoefficient;

        internal Grid grid;
        private int _id;
        protected bool isDisposed;
        internal bool isRemoved = true; //true=>geometry removed from simulation
        internal Vertices localVertices;
        private Matrix _matrix = Matrix.Identity;
        private Matrix _matrixInverse = Matrix.Identity;
        private Matrix _matrixInverseTemp;
        private Vector2 _offset = Vector2.Zero;
        private Vector2 _position = new Vector2(0, 0);
        internal float restitutionCoefficient;
        private float _rotation;
        private float _rotationOffset;
        private Vector2 _vert;
        internal Vertices worldVertices;

        public Geom()
        {
            _id = GetNextId();
            grid = new Grid();
        }

        public Geom(Body body, Vertices vertices, float collisionGridCellSize)
        {
            Construct(body, vertices, Vector2.Zero, 0, collisionGridCellSize);
        }

        public Geom(Body body, Vertices vertices, Vector2 offset, float rotationOffset, float collisionGridCellSize)
        {
            Construct(body, vertices, offset, rotationOffset, collisionGridCellSize);
        }

        public Geom(Body body, Geom geometry)
        {
            ConstructClone(body, geometry, geometry._offset, geometry._rotationOffset);
        }

        public Geom(Body body, Geom geometry, Vector2 offset, float rotationOffset)
        {
            ConstructClone(body, geometry, offset, rotationOffset);
        }

        public Vector2 Position
        {
            get { return _position; }
        }

        public float Rotation
        {
            get { return _rotation; }
        }

        public float CollisonGridCellSize
        {
            get { return _collisonGridCellSize; }
            set { _collisonGridCellSize = value; }
        }

        public Vertices LocalVertices
        {
            get { return localVertices; }
            set { localVertices = value; }
        }

        public Vertices WorldVertices
        {
            get { return worldVertices; }
            set { worldVertices = value; }
        }

        public Matrix Matrix
        {
            get { return _matrix; }
            set
            {
                _matrix = value;
                Update();
            }
        }

        public Matrix MatrixInverse
        {
            get
            {
                Matrix.Invert(ref _matrix, out _matrixInverse);
                return _matrixInverse;
            }
        }

        public AABB AABB
        {
            get { return aabb; }
            set { aabb = value; }
        }

        public int CollisionGroup
        {
            get { return collisionGroup; }
            set { collisionGroup = value; }
        }

        public bool CollisionEnabled
        {
            get { return collisionEnabled; }
            set { collisionEnabled = value; }
        }

        public bool CollisionResponseEnabled
        {
            get { return collsionResponseEnabled; }
            set { collsionResponseEnabled = value; }
        }

        public CollisionCategories CollisionCategories
        {
            get { return collisionCategories; }
            set { collisionCategories = value; }
        }

        public CollisionCategories CollidesWith
        {
            get { return collidesWith; }
            set { collidesWith = value; }
        }

        public Grid Grid
        {
            get { return grid; }
        }

        public Body Body
        {
            get { return body; }
        }

        /// <summary>
        /// The coefficient of restitution of the geometry.
        /// <para>This parameter controls how bouncy an object is when it collides with other
        /// geometries. Valid values range from 0 to 1 inclusive.  1 implies 100% restitution (perfect bounce)
        /// 0 implies no restitution (think a ball of clay)</para>
        /// </summary>
        public float RestitutionCoefficient
        {
            get { return restitutionCoefficient; }
            set { restitutionCoefficient = value; }
        }

        /// <summary>
        /// Controls the amount of friction a geometry has when in contact with another geometry. A value of zero implies
        /// no friction.  When two geometries collide, the minimum friction coeficent between the two bodies is used.
        /// </summary>
        public float FrictionCoefficient
        {
            get { return frictionCoefficient; }
            set { frictionCoefficient = value; }
        }

        public Object Tag { get; set; }

        internal int Id
        {
            get { return _id; }
        }

        #region GetNextId variables

        private static int _newId = -1;

        #endregion

        #region IEquatable<Geom> Members

        public bool Equals(Geom other)
        {
            if ((object) other == null)
            {
                return false;
            }
            return _id == other._id;
        }

        #endregion

        #region IIsDisposable Members

        public bool IsDisposed
        {
            get { return isDisposed; }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        private void Construct(Body body, Vertices vertices, Vector2 offset, float rotationOffset,
                               float collisionGridCellSize)
        {
            _id = GetNextId();
            _collisonGridCellSize = collisionGridCellSize;
            _offset = offset;
            _rotationOffset = rotationOffset;
            grid = new Grid();
            SetVertices(vertices);
            ComputeCollisonGrid();
            SetBody(body);
        }

        private void ConstructClone(Body body, Geom geometry, Vector2 offset, float rotationOffset)
        {
            _id = GetNextId();
            _collisonGridCellSize = geometry._collisonGridCellSize;
            grid = geometry.grid.Clone();
            restitutionCoefficient = geometry.restitutionCoefficient;
            frictionCoefficient = geometry.frictionCoefficient;
            collisionGroup = geometry.collisionGroup;
            collisionEnabled = geometry.collisionEnabled;
            collsionResponseEnabled = geometry.collsionResponseEnabled;
            _collisonGridCellSize = geometry._collisonGridCellSize;
            this._offset = offset;
            this._rotationOffset = rotationOffset;
            collisionCategories = geometry.collisionCategories;
            collidesWith = geometry.collidesWith;
            SetVertices(geometry.localVertices);
            SetBody(body);
        }

        public void SetVertices(Vertices vertices)
        {
            vertices.ForceCounterClockWiseOrder();
            localVertices = new Vertices(vertices);
            worldVertices = new Vertices(vertices);

            aabb.Update(ref vertices);
        }

        public void SetBody(Body body)
        {
            this.body = body;
            _bodyUpdated = body_OnChange;
            body.Updated += _bodyUpdated;

            _bodyDisposed = body_OnDisposed;
            body.Disposed += _bodyDisposed;
            Update(ref body.position, ref body.rotation);
        }

        public void ComputeCollisonGrid()
        {
            if (localVertices.Count > 2)
            {
                grid.ComputeGrid(this, _collisonGridCellSize);
            }
            else
            {
                grid = null;
            }
        }

        public Vector2 GetWorldPosition(Vector2 localPosition)
        {
            Vector2 retVector = Vector2.Transform(localPosition, _matrix);
            return retVector;
        }

        public float GetNearestDistance(Vector2 point)
        {
            float distance;
            Feature nearestFeature = new Feature(point);
            nearestFeature.Distance = float.MaxValue;

            for (int i = 0; i < localVertices.Count; i++)
            {
                Feature feature = GetNearestFeature(point, i);
                if (feature.Distance < nearestFeature.Distance)
                {
                    nearestFeature = feature;
                }
            }

            //determine if inside or outside of geometry.
            Vector2 diff = Vector2.Subtract(point, nearestFeature.Position);
            float dot = Vector2.Dot(diff, nearestFeature.Normal);

            if (dot < 0)
            {
                distance = -nearestFeature.Distance;
            }
            else
            {
                distance = nearestFeature.Distance;
            }
            return distance;
        }

        public Feature GetNearestFeature(Vector2 point, Int32 index)
        {
            Feature feature = new Feature();
            Vector2 v = localVertices.GetEdge(index);
            Vector2 w = Vector2.Subtract(point, localVertices[index]);

            float c1 = Vector2.Dot(w, v);
            if (c1 < 0)
            {
                feature.Position = localVertices[index];
                feature.Normal = localVertices.GetVertexNormal(index);
                feature.Distance = Math.Abs(w.Length());

                return feature;
            }

            float c2 = Vector2.Dot(v, v);
            if (c2 <= c1)
            {
                Vector2 d1 = Vector2.Subtract(point, localVertices[localVertices.NextIndex(index)]);
                feature.Position = localVertices[localVertices.NextIndex(index)];
                feature.Normal = localVertices.GetVertexNormal(localVertices.NextIndex(index));

                feature.Distance = Math.Abs(d1.Length());
                return feature;
            }

            float b = c1/c2;
            v = Vector2.Multiply(v, b);
            Vector2 pb = Vector2.Add(localVertices[index], v);
            Vector2 d2 = Vector2.Subtract(point, pb);
            feature.Position = pb;
            feature.Normal = localVertices.GetEdgeNormal(index); // GetEdgeNormal(index);
            feature.Distance = d2.Length();
            return feature;
        }

        public bool Collide(Vector2 point)
        {
            Feature feature;
            point = Vector2.Transform(point, MatrixInverse);
            grid.Intersect(ref point, out feature);
            if (feature.Distance < 0)
            {
                return true;
            }
                return false;
        }

        public bool Collide(Geom geometry)
        {
            for (int i = 0; i < worldVertices.Count; i++)
            {
                _vert = worldVertices[i];
                if (geometry.Collide(_vert))
                {
                    return true;
                }
            }
            for (int i = 0; i < geometry.worldVertices.Count; i++)
            {
                _vert = geometry.worldVertices[i];
                if (Collide(_vert))
                {
                    return true;
                }
            }
            return false;
        }

        public bool Intersect(ref Vector2 localVertex, out Feature feature)
        {
            return grid.Intersect(ref localVertex, out feature);
        }

        public void TransformToLocalCoordinates(ref Vector2 worldVertex, out Vector2 localVertex)
        {
            _matrixInverseTemp = MatrixInverse;
            Vector2.Transform(ref worldVertex, ref _matrixInverseTemp, out localVertex);
        }

        public void TransformNormalToWorld(ref Vector2 localNormal, out Vector2 worldNormal)
        {
            Vector2.TransformNormal(ref localNormal, ref _matrix, out worldNormal);
        }

        private void Update(ref Vector2 position, ref float orientation)
        {
            Matrix.CreateRotationZ(orientation + _rotationOffset, out _matrix);

            #region INLINE: Vector2.Transform(ref _offset, ref _matrix, out _newPos);

            float num2 = ((_offset.X*_matrix.M11) + (_offset.Y*_matrix.M21)) + _matrix.M41;
            float num = ((_offset.X*_matrix.M12) + (_offset.Y*_matrix.M22)) + _matrix.M42;
            _newPos.X = num2;
            _newPos.Y = num;

            #endregion

            _matrix.M41 = position.X + _newPos.X;
            _matrix.M42 = position.Y + _newPos.Y;
            _matrix.M44 = 1;
            _position.X = _matrix.M41;
            _position.Y = _matrix.M42;
            _rotation = body.rotation + _rotationOffset;
            Update();
        }

        private void Update()
        {
            for (int i = 0; i < localVertices.Count; i++)
            {
                #region INLINE: worldVertices[i] = Vector2.Transform(localVertices[i], _matrix);

                _localVertice = localVertices[i];
                float num2 = ((_localVertice.X*_matrix.M11) + (_localVertice.Y*_matrix.M21)) + _matrix.M41;
                float num = ((_localVertice.X*_matrix.M12) + (_localVertice.Y*_matrix.M22)) + _matrix.M42;
                _vertice.X = num2;
                _vertice.Y = num;
                worldVertices[i] = _vertice;

                #endregion
            }

            aabb.Update(ref worldVertices);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            Geom g = obj as Geom;
            if ((object) g == null)
            {
                return false;
            }
            return Equals(g);
        }

        public static bool operator ==(Geom geometry1, Geom geometry2)
        {
            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(geometry1, geometry2))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object) geometry1 == null) || ((object) geometry2 == null))
            {
                return false;
            }

            return geometry1.Equals(geometry2);
        }

        public static bool operator !=(Geom geometry1, Geom geometry2)
        {
            return !(geometry1 == geometry2);
        }

        public static bool operator <(Geom geometry1, Geom geometry2)
        {
            return geometry1.Id < geometry2.Id;
        }

        public static bool operator >(Geom geometry1, Geom geometry2)
        {
            return geometry1.Id > geometry2.Id;
        }

        public static int GetNextId()
        {
            _newId += 1;
            return _newId;
        }

        private void body_OnChange(ref Vector2 position, ref float rotation)
        {
            Update(ref position, ref rotation);
        }

        private void body_OnDisposed(object sender, EventArgs e)
        {
            Dispose();
        }

        protected virtual void Dispose(bool disposing)
        {
            //subclasses can override incase they need to dispose of resources
            //otherwise do nothing.
            if (!isDisposed)
            {
                if (disposing)
                {
                    //dispose managed resources 
                    if (body != null && _bodyUpdated != null)
                    {
                        body.Updated -= _bodyUpdated;
                        body.Disposed -= _bodyDisposed;
                    }
                }
                
                //dispose unmanaged resources
            }
            isDisposed = true;
            //base.Dispose(disposing)        
        }

        #region Update variables

        private Vector2 _localVertice;
        private Vector2 _vertice = Vector2.Zero;

        #endregion

        #region Update variables

        private Vector2 _newPos = Vector2.Zero;

        #endregion
    }
}