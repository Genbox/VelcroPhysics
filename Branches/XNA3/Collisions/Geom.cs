using System;
using FarseerGames.FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace FarseerGames.FarseerPhysics.Collisions
{
    /// <remark>
    /// The geometry class is the unit of collision detection.
    /// </remark>
    public class Geom : IEquatable<Geom>, IIsDisposable
    {
        //public class Geom : ICollideable<Geom>, IEquatable<Geom>, IIsDisposable {

        #region Delegates

        public delegate bool CollisionEventHandler(Geom geometry1, Geom geometry2, ContactList contactList);

        #endregion

        internal AABB aabb = new AABB();
        internal Body body;
        private EventHandler<EventArgs> bodyDisposed;
        private Body.UpdatedEventHandler bodyUpdated;

        internal CollisionCategories collidesWith = CollisionCategories.All;
                                           //collides with all categories by default

        public CollisionEventHandler Collision;

        internal CollisionCategories collisionCategories = CollisionCategories.All;
                                           //member off all categories by default

        internal bool collisionEnabled = true;
        internal int collisionGroup;
        private float collisonGridCellSize;
        internal bool collsionResponseEnabled = true;
        internal float frictionCoefficient;

        internal Grid grid;
        private int id;
        protected bool isDisposed;
        internal bool isRemoved = true; //true=>geometry removed from simulation
        internal Vertices localVertices;
        private Matrix matrix = Matrix.Identity;
        private Matrix matrixInverse = Matrix.Identity;
        private Matrix matrixInverseTemp;
        private Vector2 offset = Vector2.Zero;
        private Vector2 position = new Vector2(0, 0);
        internal float restitutionCoefficient;
        private float rotation;
        private float rotationOffset;
        private Vector2 vert;
        internal Vertices worldVertices;

        public Geom()
        {
            id = GetNextId();
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
            ConstructClone(body, geometry, geometry.offset, geometry.rotationOffset);
        }

        public Geom(Body body, Geom geometry, Vector2 offset, float rotationOffset)
        {
            ConstructClone(body, geometry, offset, rotationOffset);
        }

        public Vector2 Position
        {
            get { return position; }
        }

        public float Rotation
        {
            get { return rotation; }
        }

        public float CollisonGridCellSize
        {
            get { return collisonGridCellSize; }
            set { collisonGridCellSize = value; }
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
            get { return matrix; }
            set
            {
                matrix = value;
                Update();
            }
        }

        public Matrix MatrixInverse
        {
            get
            {
                Matrix.Invert(ref matrix, out matrixInverse);
                return matrixInverse;
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
            get { return id; }
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
            return id == other.id;
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
            id = GetNextId();
            collisonGridCellSize = collisionGridCellSize;
            this.offset = offset;
            this.rotationOffset = rotationOffset;
            grid = new Grid();
            SetVertices(vertices);
            ComputeCollisonGrid();
            SetBody(body);
        }

        private void ConstructClone(Body body, Geom geometry, Vector2 offset, float rotationOffset)
        {
            id = GetNextId();
            collisonGridCellSize = geometry.collisonGridCellSize;
            grid = geometry.grid.Clone();
            restitutionCoefficient = geometry.restitutionCoefficient;
            frictionCoefficient = geometry.frictionCoefficient;
            collisionGroup = geometry.collisionGroup;
            collisionEnabled = geometry.collisionEnabled;
            collsionResponseEnabled = geometry.collsionResponseEnabled;
            collisonGridCellSize = geometry.collisonGridCellSize;
            this.offset = offset;
            this.rotationOffset = rotationOffset;
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
            bodyUpdated = body_OnChange;
            body.Updated += bodyUpdated;

            bodyDisposed = body_OnDisposed;
            body.Disposed += bodyDisposed;
            Update(ref body.position, ref body.rotation);
        }

        public void ComputeCollisonGrid()
        {
            if (localVertices.Count > 2)
            {
                grid.ComputeGrid(this, collisonGridCellSize);
            }
            else
            {
                grid = null;
            }
        }

        public Vector2 GetWorldPosition(Vector2 localPosition)
        {
            Vector2 retVector = Vector2.Transform(localPosition, matrix);
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
            Vector2 Pb = Vector2.Add(localVertices[index], v);
            Vector2 d2 = Vector2.Subtract(point, Pb);
            feature.Position = Pb;
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
                vert = worldVertices[i];
                if (geometry.Collide(vert))
                {
                    return true;
                }
            }
            for (int i = 0; i < geometry.worldVertices.Count; i++)
            {
                vert = geometry.worldVertices[i];
                if (Collide(vert))
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
            matrixInverseTemp = MatrixInverse;
            Vector2.Transform(ref worldVertex, ref matrixInverseTemp, out localVertex);
        }

        public void TransformNormalToWorld(ref Vector2 localNormal, out Vector2 worldNormal)
        {
            Vector2.TransformNormal(ref localNormal, ref matrix, out worldNormal);
        }

        private void Update(ref Vector2 position, ref float orientation)
        {
            Matrix.CreateRotationZ(orientation + rotationOffset, out matrix);

            #region INLINE: Vector2.Transform(ref offset, ref matrix, out newPos);

            float num2 = ((offset.X*matrix.M11) + (offset.Y*matrix.M21)) + matrix.M41;
            float num = ((offset.X*matrix.M12) + (offset.Y*matrix.M22)) + matrix.M42;
            newPos.X = num2;
            newPos.Y = num;

            #endregion

            matrix.M41 = position.X + newPos.X;
            matrix.M42 = position.Y + newPos.Y;
            matrix.M44 = 1;
            this.position.X = matrix.M41;
            this.position.Y = matrix.M42;
            rotation = body.rotation + rotationOffset;
            Update();
        }

        private void Update()
        {
            for (int i = 0; i < localVertices.Count; i++)
            {
                #region INLINE: worldVertices[i] = Vector2.Transform(localVertices[i], matrix);

                localVertice = localVertices[i];
                float num2 = ((localVertice.X*matrix.M11) + (localVertice.Y*matrix.M21)) + matrix.M41;
                float num = ((localVertice.X*matrix.M12) + (localVertice.Y*matrix.M22)) + matrix.M42;
                vertice.X = num2;
                vertice.Y = num;
                worldVertices[i] = vertice;

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
                    if (body != null && bodyUpdated != null)
                    {
                        body.Updated -= bodyUpdated;
                        body.Disposed -= bodyDisposed;
                    }
                }
                
                //dispose unmanaged resources
            }
            isDisposed = true;
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