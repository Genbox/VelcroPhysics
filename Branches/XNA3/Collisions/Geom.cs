using System;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Interfaces;
using FarseerGames.FarseerPhysics.Mathematics;

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

        private EventHandler<EventArgs> _bodyDisposed;
        private Body.UpdatedEventHandler _bodyUpdated;
        private Matrix _matrix = Matrix.Identity;
        private Matrix _matrixInverse = Matrix.Identity;
        private Matrix _matrixInverseTemp;
        private Vector2 _offset = Vector2.Zero;
        private Vector2 _position = new Vector2(0, 0);
        private float _rotationOffset;
        private Vector2 _vert;
        private Vertices _worldVertices;
        public CollisionEventHandler Collision;
        internal bool IsRemoved = true; //true=>geometry removed from simulation

        public Geom()
        {
            AABB = new AABB();
            CollisionEnabled = true;
            CollisionResponseEnabled = true;
            CollisionCategories = Enums.CollisionCategories.All;
            CollidesWith = Enums.CollisionCategories.All;
            Id = GetNextId();
            Grid = new Grid();
        }

        public Geom(Body body, Vertices vertices, float collisionGridCellSize)
        {
            AABB = new AABB();
            CollisionEnabled = true;
            CollisionResponseEnabled = true;
            CollisionCategories = Enums.CollisionCategories.All;
            CollidesWith = Enums.CollisionCategories.All;
            Construct(body, vertices, Vector2.Zero, 0, collisionGridCellSize);
        }

        public Geom(Body body, Vertices vertices, Vector2 offset, float rotationOffset, float collisionGridCellSize)
        {
            AABB = new AABB();
            CollisionEnabled = true;
            CollisionResponseEnabled = true;
            CollisionCategories = Enums.CollisionCategories.All;
            CollidesWith = Enums.CollisionCategories.All;
            Construct(body, vertices, offset, rotationOffset, collisionGridCellSize);
        }

        public Geom(Body body, Geom geometry)
        {
            AABB = new AABB();
            CollisionEnabled = true;
            CollisionResponseEnabled = true;
            CollisionCategories = Enums.CollisionCategories.All;
            CollidesWith = Enums.CollisionCategories.All;
            ConstructClone(body, geometry, geometry._offset, geometry._rotationOffset);
        }

        public Geom(Body body, Geom geometry, Vector2 offset, float rotationOffset)
        {
            AABB = new AABB();
            CollisionEnabled = true;
            CollisionResponseEnabled = true;
            CollisionCategories = Enums.CollisionCategories.All;
            CollidesWith = Enums.CollisionCategories.All;
            ConstructClone(body, geometry, offset, rotationOffset);
        }

        public Vector2 Position
        {
            get { return _position; }
        }

        public float Rotation { get; private set; }
        public float CollisonGridCellSize { get; set; }
        public Vertices LocalVertices { get; set; }

        public Vertices WorldVertices
        {
            get { return _worldVertices; }
            set { _worldVertices = value; }
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

        public AABB AABB { get; set; }
        public int CollisionGroup { get; set; }
        public bool CollisionEnabled { get; set; }
        public bool CollisionResponseEnabled { get; set; }
        public Enums.CollisionCategories CollisionCategories { get; set; }
        public Enums.CollisionCategories CollidesWith { get; set; }
        public Grid Grid { get; internal set; }
        public Body Body { get; internal set; }

        /// <summary>
        /// The coefficient of restitution of the geometry.
        /// <para>This parameter controls how bouncy an object is when it collides with other
        /// geometries. Valid values range from 0 to 1 inclusive.  1 implies 100% restitution (perfect bounce)
        /// 0 implies no restitution (think a ball of clay)</para>
        /// </summary>
        public float RestitutionCoefficient { get; set; }

        /// <summary>
        /// Controls the amount of friction a geometry has when in contact with another geometry. A value of zero implies
        /// no friction.  When two geometries collide, the minimum friction coeficent between the two bodies is used.
        /// </summary>
        public float FrictionCoefficient { get; set; }

        public Object Tag { get; set; }

        internal int Id { get; private set; }

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
            return Id == other.Id;
        }

        #endregion

        #region IIsDisposable Members

        public bool IsDisposed { get; protected set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        private void Construct(Body body, Vertices vertices, Vector2 offset, float rotationOffset,
                               float collisionGridCellSize)
        {
            Id = GetNextId();
            CollisonGridCellSize = collisionGridCellSize;
            _offset = offset;
            _rotationOffset = rotationOffset;
            Grid = new Grid();
            SetVertices(vertices);
            ComputeCollisonGrid();
            SetBody(body);
        }

        private void ConstructClone(Body body, Geom geometry, Vector2 offset, float rotationOffset)
        {
            Id = GetNextId();
            CollisonGridCellSize = geometry.CollisonGridCellSize;
            Grid = geometry.Grid.Clone();
            RestitutionCoefficient = geometry.RestitutionCoefficient;
            FrictionCoefficient = geometry.FrictionCoefficient;
            CollisionGroup = geometry.CollisionGroup;
            CollisionEnabled = geometry.CollisionEnabled;
            CollisionResponseEnabled = geometry.CollisionResponseEnabled;
            CollisonGridCellSize = geometry.CollisonGridCellSize;
            _offset = offset;
            _rotationOffset = rotationOffset;
            CollisionCategories = geometry.CollisionCategories;
            CollidesWith = geometry.CollidesWith;
            SetVertices(geometry.LocalVertices);
            SetBody(body);
        }

        public void SetVertices(Vertices vertices)
        {
            vertices.ForceCounterClockWiseOrder();
            LocalVertices = new Vertices(vertices);
            _worldVertices = new Vertices(vertices);

            AABB.Update(ref vertices);
        }

        public void SetBody(Body body)
        {
            Body = body;
            _bodyUpdated = body_OnChange;
            body.Updated += _bodyUpdated;

            _bodyDisposed = body_OnDisposed;
            body.Disposed += _bodyDisposed;
            Update(ref body.position, ref body.rotation);
        }

        public void ComputeCollisonGrid()
        {
            if (LocalVertices.Count > 2)
            {
                Grid.ComputeGrid(this, CollisonGridCellSize);
            }
            else
            {
                Grid = null;
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

            for (int i = 0; i < LocalVertices.Count; i++)
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
            Vector2 v = LocalVertices.GetEdge(index);
            Vector2 w = Vector2.Subtract(point, LocalVertices[index]);

            float c1 = Vector2.Dot(w, v);
            if (c1 < 0)
            {
                feature.Position = LocalVertices[index];
                feature.Normal = LocalVertices.GetVertexNormal(index);
                feature.Distance = Math.Abs(w.Length());

                return feature;
            }

            float c2 = Vector2.Dot(v, v);
            if (c2 <= c1)
            {
                Vector2 d1 = Vector2.Subtract(point, LocalVertices[LocalVertices.NextIndex(index)]);
                feature.Position = LocalVertices[LocalVertices.NextIndex(index)];
                feature.Normal = LocalVertices.GetVertexNormal(LocalVertices.NextIndex(index));

                feature.Distance = Math.Abs(d1.Length());
                return feature;
            }

            float b = c1/c2;
            v = Vector2.Multiply(v, b);
            Vector2 Pb = Vector2.Add(LocalVertices[index], v);
            Vector2 d2 = Vector2.Subtract(point, Pb);
            feature.Position = Pb;
            feature.Normal = LocalVertices.GetEdgeNormal(index); // GetEdgeNormal(index);
            feature.Distance = d2.Length();
            return feature;
        }

        public bool Collide(Vector2 point)
        {
            Feature feature;
            point = Vector2.Transform(point, MatrixInverse);
            Grid.Intersect(ref point, out feature);
            if (feature.Distance < 0)
            {
                return true;
            }

            return false;
        }

        public bool Collide(Geom geometry)
        {
            for (int i = 0; i < _worldVertices.Count; i++)
            {
                _vert = _worldVertices[i];
                if (geometry.Collide(_vert))
                {
                    return true;
                }
            }
            for (int i = 0; i < geometry._worldVertices.Count; i++)
            {
                _vert = geometry._worldVertices[i];
                if (Collide(_vert))
                {
                    return true;
                }
            }
            return false;
        }

        public bool Intersect(ref Vector2 localVertex, out Feature feature)
        {
            return Grid.Intersect(ref localVertex, out feature);
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

            #region INLINE: Vector2.Transform(ref offset, ref matrix, out newPos);

            float num2 = ((_offset.X*_matrix.M11) + (_offset.Y*_matrix.M21)) + _matrix.M41;
            float num = ((_offset.X*_matrix.M12) + (_offset.Y*_matrix.M22)) + _matrix.M42;
            newPos.X = num2;
            newPos.Y = num;

            #endregion

            _matrix.M41 = position.X + newPos.X;
            _matrix.M42 = position.Y + newPos.Y;
            _matrix.M44 = 1;
            _position.X = _matrix.M41;
            _position.Y = _matrix.M42;
            Rotation = Body.rotation + _rotationOffset;
            Update();
        }

        private void Update()
        {
            for (int i = 0; i < LocalVertices.Count; i++)
            {
                #region INLINE: worldVertices[i] = Vector2.Transform(localVertices[i], matrix);

                localVertice = LocalVertices[i];
                float num2 = ((localVertice.X*_matrix.M11) + (localVertice.Y*_matrix.M21)) + _matrix.M41;
                float num = ((localVertice.X*_matrix.M12) + (localVertice.Y*_matrix.M22)) + _matrix.M42;
                vertice.X = num2;
                vertice.Y = num;
                _worldVertices[i] = vertice;

                #endregion
            }

            AABB.Update(ref _worldVertices);
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
            //BUG: Was:
            //if (((object) geometry1 == null) || ((object) geometry1 == null))
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
            if (!IsDisposed)
            {
                if (disposing)
                {
                    //dispose managed resources 
                    if (Body != null && _bodyUpdated != null)
                    {
                        Body.Updated -= _bodyUpdated;
                        Body.Disposed -= _bodyDisposed;
                    }
                }

                //dispose unmanaged resources
            }
            IsDisposed = true;
            //base.Dispose(disposing)        
        }

        #region Update variables

        private Vector2 localVertice;
        private Vector2 vertice = Vector2.Zero;

        #endregion

        #region Update variables

        private Vector2 newPos = Vector2.Zero;

        #endregion
    }
}