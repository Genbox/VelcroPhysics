using System;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Interfaces;

#if (XNA)
using Microsoft.Xna.Framework;
#else
using FarseerGames.FarseerPhysics.Mathematics;
#endif

namespace FarseerGames.FarseerPhysics.Collisions
{
    /// <summary>
    /// The geometry class is the heart of collision detection.
    /// A Geom need a body and a set of vertices. The vertices should define the edge of the shape.
    /// </summary>
    public class Geom : IEquatable<Geom>, IIsDisposable
    {
        #region Delegates

        /// <summary>
        /// This delegate is called when a collision between 2 geometries occurs
        /// </summary>
        public delegate bool CollisionEventHandler(Geom geometry1, Geom geometry2, ContactList contactList);

        /// <summary>
        /// This delegate is called when a separation between 2 geometries occurs
        /// </summary>
        public delegate void SeparationEventHandler(Geom geometry1, Geom geometry2);

        #endregion

        private bool _isSensor;
        private Matrix _matrix = Matrix.Identity;
        private Matrix _matrixInverse = Matrix.Identity;
        private Matrix _matrixInverseTemp;
        private Vector2 _offset = Vector2.Zero;
        private Vector2 _position = Vector2.Zero;
        private float _rotation;
        private float _rotationOffset;
        private bool _isDisposed;
        internal Body body;
        public bool InSimulation = true; //true=>geometry removed from simulation
        internal Vertices localVertices;
        internal Vertices worldVertices;
        internal INarrowPhaseCollider narrowPhaseCollider;

        /// <summary>
        /// Gets or sets the collision categories that this geom collides with.
        /// </summary>
        /// <Value>The collides with.</Value>
        public CollisionCategory CollidesWith = CollisionCategory.All;

        /// <summary>
        /// Gets or sets the collision categories.
        ///member off all categories by default
        /// </summary>
        /// <Value>The collision categories.</Value>
        public CollisionCategory CollisionCategories = CollisionCategory.All;

        /// <summary>
        /// Gets or sets a value indicating whether collision response is enabled.
        /// If 2 geoms collide and CollisionResponseEnabled is false, then impulses will not be calculated
        /// for the 2 colliding geoms. They will pass through each other, but will still be able to fire the
        /// <see cref="OnCollision"/> event.
        /// </summary>
        /// <Value>
        /// 	<c>true</c> if collision response enabled; otherwise, <c>false</c>.
        /// </Value>
        public bool CollisionResponseEnabled = true;

        /// <summary>
        /// Controls the amount of friction a geometry has when in contact with another geometry. A Value of zero implies
        /// no friction. When two geometries collide, the minimum friction coefficient between the two bodies is used.
        /// </summary>
        public float FrictionCoefficient;

        /// <summary>
        /// Fires when a collision occurs with the geom
        /// </summary>
        public CollisionEventHandler OnCollision;

        /// <summary>
        /// Fires when a separation between this and another geom occurs
        /// </summary>
        public SeparationEventHandler OnSeparation;

        /// <summary>
        /// The coefficient of restitution of the geometry.
        /// <para>This parameter controls how bouncy an object is when it collides with other
        /// geometries. Valid values range from 0 to 1 inclusive.  1 implies 100% restitution (perfect bounce)
        /// 0 implies no restitution (think a ball of clay)</para>
        /// </summary>
        public float RestitutionCoefficient;

        /// <summary>
        /// Gets or sets the distance grid collision grid size.
        /// ONLY used when the distance grid narrow phase collider is used.
        /// Be sure to run <see cref="PrepareNarrowPhaseCollider"/>() if you change this for changes to take effect.
        /// </summary>
        public float CollisionGridSize;

        /// <summary>
        /// Gets or sets the collision group.
        /// If 2 geoms are in the same collision group, they will not collide.
        /// </summary>
        /// <Value>The collision group.</Value>
        public int CollisionGroup;

        /// <summary>
        /// Gets or sets the Axis Aligned Bounding Box of the geom.
        /// </summary>
        /// <Value>The AABB.</Value>
        public AABB AABB = new AABB();

        /// <summary>
        /// Gets or sets a Value indicating whether collision is enabled.
        /// </summary>
        /// <Value><c>true</c> if collision is enabled; otherwise, <c>false</c>.</Value>
        public bool CollisionEnabled = true;

        /// <summary>
        /// Gets or sets the tag. A tag is used to attach a custom object to the Geom.
        /// </summary>
        /// <Value>The custom object.</Value>
        public Object Tag;

        /// <summary>
        /// Initializes a new instance of the <see cref="Geom"/> class.
        /// </summary>
        public Geom()
        {
            Id = GetNextId();
            narrowPhaseCollider = new Grid();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Geom"/> class.
        /// </summary>
        /// <param name="body">The body.</param>
        /// <param name="vertices">The vertices.</param>
        /// <param name="collisionGridSize">Size of the collision grid.</param>
        public Geom(Body body, Vertices vertices, float collisionGridSize)
        {
            Construct(body, vertices, Vector2.Zero, 0, collisionGridSize);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Geom"/> class.
        /// </summary>
        /// <param name="body">The body.</param>
        /// <param name="vertices">The vertices.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="rotationOffset">The rotation offset.</param>
        /// <param name="collisionGridSize">Size of the collision grid.</param>
        public Geom(Body body, Vertices vertices, Vector2 offset, float rotationOffset, float collisionGridSize)
        {
            Construct(body, vertices, offset, rotationOffset, collisionGridSize);
        }

        /// <summary>
        /// Creates a clone of an already existing geometry
        /// </summary>
        /// <param name="body">The body</param>
        /// <param name="geometry">The geometry to clone</param>
        public Geom(Body body, Geom geometry)
        {
            ConstructClone(body, geometry, geometry._offset, geometry._rotationOffset);
        }

        /// <summary>
        /// Creates a clone of an already existing geometry
        /// </summary>
        /// <param name="body">The body</param>
        /// <param name="geometry">The geometry to clone</param>
        /// <param name="offset">The offset.</param>
        /// <param name="rotationOffset">The rotation offset.</param>
        public Geom(Body body, Geom geometry, Vector2 offset, float rotationOffset)
        {
            ConstructClone(body, geometry, offset, rotationOffset);
        }

        /// <summary>
        /// Gets the position.
        /// </summary>
        /// <Value>The position.</Value>
        public Vector2 Position
        {
            get { return _position; }
        }

        /// <summary>
        /// Gets the rotation.
        /// </summary>
        /// <Value>The rotation.</Value>
        public float Rotation
        {
            get { return _rotation; }
        }

        /// <summary>
        /// Gets the local vertices of the geom. Local vertices are relative to the center of the vertices.
        /// </summary>
        /// <Value>The local vertices.</Value>
        public Vertices LocalVertices
        {
            get { return localVertices; }
        }

        /// <summary>
        /// Gets the world vertices.
        /// </summary>
        /// <Value>The world vertices.</Value>
        public Vertices WorldVertices
        {
            get { return worldVertices; }
        }

        /// <summary>
        /// Gets or sets the matrix.
        /// </summary>
        /// <Value>The matrix.</Value>
        public Matrix Matrix
        {
            get { return _matrix; }
            set
            {
                _matrix = value;
                Update();
            }
        }

        /// <summary>
        /// Gets the inverse matrix.
        /// </summary>
        /// <Value>The matrix inverse.</Value>
        public Matrix MatrixInverse
        {
            get
            {
                Matrix.Invert(ref _matrix, out _matrixInverse);
                return _matrixInverse;
            }
        }

        /// <summary>
        /// Gets or sets a Value indicating whether this instance is a sensor.
        /// A sensor does not calculate impulses and does not change position (it's static)
        /// it does however detect collisions. Sensors can be used to sense other geoms.
        /// </summary>
        /// <Value><c>true</c> if this instance is sensor; otherwise, <c>false</c>.</Value>
        public bool IsSensor
        {
            get { return _isSensor; }
            set
            {
                _isSensor = value;
                if (_isSensor)
                {
                    body.IsStatic = true;
                    CollisionResponseEnabled = false;
                }
                else
                {
                    body.IsStatic = false;
                    CollisionResponseEnabled = true;
                }
            }
        }

        /// <summary>
        /// Gets the narrow phase collider currently in use.
        /// </summary>
        /// <Value>The narrow phase collider.</Value>
        public INarrowPhaseCollider NarrowPhaseCollider
        {
            get { return narrowPhaseCollider; }
        }

        /// <summary>
        /// Gets the body attached to the geom.
        /// </summary>
        /// <Value>The body.</Value>
        public Body Body
        {
            get { return body; }
        }

        /// <summary>
        /// Gets the id of this geom.
        /// </summary>
        /// <Value>The id.</Value>
        internal int Id { get; private set; }

        private void Construct(Body bodyToSet, Vertices vertices, Vector2 offset, float rotationOffset,
                               float collisionGridSize)
        {
            Id = GetNextId();
            CollisionGridSize = collisionGridSize;
            _offset = offset;
            _rotationOffset = rotationOffset;
            narrowPhaseCollider = new Grid();
            SetVertices(vertices);
            PrepareNarrowPhaseCollider();
            SetBody(bodyToSet);
        }

        private void ConstructClone(Body bodyToSet, Geom geometry, Vector2 offset, float rotationOffset)
        {
            Id = GetNextId();
            narrowPhaseCollider = geometry.NarrowPhaseCollider.Clone();
            RestitutionCoefficient = geometry.RestitutionCoefficient;
            FrictionCoefficient = geometry.FrictionCoefficient;
            CollisionGroup = geometry.CollisionGroup;
            CollisionEnabled = geometry.CollisionEnabled;
            CollisionResponseEnabled = geometry.CollisionResponseEnabled;
            CollisionGridSize = geometry.CollisionGridSize;
            _offset = offset;
            _rotationOffset = rotationOffset;
            CollisionCategories = geometry.CollisionCategories;
            CollidesWith = geometry.CollidesWith;
            SetVertices(geometry.localVertices);
            SetBody(bodyToSet);
        }

        /// <summary>
        /// Sets the vertices of the geom.
        /// </summary>
        /// <param name="vertices">The vertices.</param>
        public void SetVertices(Vertices vertices)
        {
            vertices.ForceCounterClockWiseOrder();
            localVertices = new Vertices(vertices);
            worldVertices = new Vertices(vertices);

            AABB.Update(ref vertices);
        }

        /// <summary>
        /// Sets the body.
        /// </summary>
        /// <param name="bodyToSet">The body.</param>
        public void SetBody(Body bodyToSet)
        {
            body = bodyToSet;

            bodyToSet.Updated += Update;
            bodyToSet.Disposed += BodyOnDisposed;

            Update(ref bodyToSet.position, ref bodyToSet.rotation);
        }

        /// <summary>
        /// Prepares the narrow phase collider
        /// </summary>
        public void PrepareNarrowPhaseCollider()
        {
            if (localVertices.Count > 2)
            {
                narrowPhaseCollider.Prepare(this, CollisionGridSize);
            }
            else
            {
                narrowPhaseCollider = null;
            }
        }

        /// <summary>
        /// Gets the world position.
        /// </summary>
        /// <param name="localPosition">The local position.</param>
        /// <returns></returns>
        public Vector2 GetWorldPosition(Vector2 localPosition)
        {
            Vector2 retVector = Vector2.Transform(localPosition, _matrix);
            return retVector;
        }

        /// <summary>
        /// Gets the nearest distance relative to the point given.
        /// </summary>
        /// <param name="point">The point that should be calculated against.</param>
        /// <returns>The distance</returns>
        public float GetNearestDistance(Vector2 point)
        {
            float distance = float.MaxValue;
            int nearestIndex = 0;

            for (int i = 0; i < localVertices.Count; i++)
            {
                float pointDistance = GetDistanceToEdge(ref point, i);

                if (pointDistance < distance)
                {
                    distance = pointDistance;
                    nearestIndex = i;
                }
            }

            Feature nearestFeature = GetNearestFeature(point, nearestIndex);

            //Determine if inside or outside of geometry.
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

        /// <summary>
        /// Gets the distance to edge.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        private float GetDistanceToEdge(ref Vector2 point, int index)
        {
            Vector2 edge = localVertices.GetEdge(index);
            Vector2 diff = Vector2.Subtract(point, localVertices[index]);

            float c1 = Vector2.Dot(diff, edge);
            if (c1 < 0)
            {
                return Math.Abs(diff.Length());
            }

            float c2 = Vector2.Dot(edge, edge);
            if (c2 <= c1)
            {
                return Vector2.Distance(point, localVertices[localVertices.NextIndex(index)]);
            }

            float b = c1 / c2;
            edge = Vector2.Multiply(edge, b);
            Vector2 pb = Vector2.Add(localVertices[index], edge);

            return Vector2.Distance(point, pb);
        }

        /// <summary>
        /// Gets the nearest feature relative to a point.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <param name="index">The index of a vector in the vertices.</param>
        /// <returns></returns>
        public Feature GetNearestFeature(Vector2 point, int index)
        {
            Feature feature = new Feature();
            Vector2 edge = localVertices.GetEdge(index);
            Vector2 diff = Vector2.Subtract(point, localVertices[index]);

            float c1 = Vector2.Dot(diff, edge);
            if (c1 < 0)
            {
                feature.Position = localVertices[index];
                feature.Normal = localVertices.GetVertexNormal(index);
                feature.Distance = Math.Abs(diff.Length());

                return feature;
            }

            float c2 = Vector2.Dot(edge, edge);
            if (c2 <= c1)
            {
                Vector2 d1 = Vector2.Subtract(point, localVertices[localVertices.NextIndex(index)]);
                feature.Position = localVertices[localVertices.NextIndex(index)];
                feature.Normal = localVertices.GetVertexNormal(localVertices.NextIndex(index));

                feature.Distance = Math.Abs(d1.Length());
                return feature;
            }

            float b = c1 / c2;
            edge = Vector2.Multiply(edge, b);
            Vector2 pb = Vector2.Add(localVertices[index], edge);
            Vector2 d2 = Vector2.Subtract(point, pb);
            feature.Position = pb;
            feature.Normal = localVertices.GetEdgeNormal(index); // GetEdgeNormal(index);
            feature.Distance = d2.Length();
            return feature;
        }

        /// <summary>
        /// Checks to see if the geom collides with the specified point.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <returns>true if colliding</returns>
        public bool Collide(Vector2 point)
        {
            //Check first if the AABB contains the point
            if (AABB.Contains(ref point))
            {
                Matrix matrixInverse = MatrixInverse;
                Vector2.Transform(ref point, ref matrixInverse, out point);

                Feature feature;
                narrowPhaseCollider.Intersect(ref point, out feature);

                if (feature.Distance < 0)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Exactly the same as Collide(), but does not do the AABB check because it was done elsewhere.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        private bool FastCollide(Vector2 point)
        {
            Matrix matrixInverse = MatrixInverse;
            Vector2.Transform(ref point, ref matrixInverse, out point);

            Feature feature;
            narrowPhaseCollider.Intersect(ref point, out feature);

            if (feature.Distance < 0)
                return true;

            return false;
        }

        /// <summary>
        /// Checks to see if the geom collides with the specified geom.
        /// </summary>
        /// <param name="geometry">The geometry.</param>
        /// <returns></returns>
        public bool Collide(Geom geometry)
        {
            //Check first if the AABB intersects the other geometry's AABB. If they
            //do not intersect, there can be no collision.
            if (AABB.Intersect(AABB, geometry.AABB))
            {
                //Check each vertice (of self) against the provided geometry
                int count = worldVertices.Count;
                for (int i = 0; i < count; i++)
                {
                    if (geometry.FastCollide(worldVertices[i]))
                        return true;
                }

                //Check each vertice of the provided geometry, against itself
                count = geometry.worldVertices.Count;
                for (int i = 0; i < count; i++)
                {
                    if (FastCollide(geometry.worldVertices[i]))
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Transforms a world vertex to local vertex.
        /// </summary>
        /// <param name="worldVertex">The world vertex.</param>
        /// <param name="localVertex">The local vertex.</param>
        public void TransformToLocalCoordinates(ref Vector2 worldVertex, out Vector2 localVertex)
        {
            _matrixInverseTemp = MatrixInverse;
            Vector2.Transform(ref worldVertex, ref _matrixInverseTemp, out localVertex);
        }

        /// <summary>
        /// Transforms a local normal to a world normal.
        /// </summary>
        /// <param name="localNormal">The local normal.</param>
        /// <param name="worldNormal">The world normal.</param>
        public void TransformNormalToWorld(ref Vector2 localNormal, out Vector2 worldNormal)
        {
            Vector2.TransformNormal(ref localNormal, ref _matrix, out worldNormal);
        }

        private void Update(ref Vector2 position, ref float rotation)
        {
            Matrix.CreateRotationZ(rotation + _rotationOffset, out _matrix);

            #region INLINE: Vector2.Transform(ref _offset, ref _matrix, out _newPos);

            float num2 = ((_offset.X * _matrix.M11) + (_offset.Y * _matrix.M21)) + _matrix.M41;
            float num = ((_offset.X * _matrix.M12) + (_offset.Y * _matrix.M22)) + _matrix.M42;
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

        /// <summary>
        /// Transform the local vertices to world vertices.
        /// Also updates the AABB of the geometry to the new values.
        /// </summary>
        private void Update()
        {
            for (int i = 0; i < localVertices.Count; i++)
            {
                #region INLINE: worldVertices[i] = Vector2.Transform(localVertices[i], _matrix);

                _localVertice = localVertices[i];
                float num2 = ((_localVertice.X * _matrix.M11) + (_localVertice.Y * _matrix.M21)) + _matrix.M41;
                float num = ((_localVertice.X * _matrix.M12) + (_localVertice.Y * _matrix.M22)) + _matrix.M42;
                _vertice.X = num2;
                _vertice.Y = num;
                worldVertices[i] = _vertice;

                #endregion
            }

            AABB.Update(ref worldVertices);
        }

        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
        /// </returns>
        /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>. 
        ///                 </param><exception cref="T:System.NullReferenceException">The <paramref name="obj"/> parameter is null.
        ///                 </exception><filterpriority>2</filterpriority>
        public override bool Equals(object obj)
        {
            Geom g = obj as Geom;
            if ((object)g == null)
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
            if (((object)geometry1 == null) || ((object)geometry2 == null))
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

        private static int GetNextId()
        {
            _newId += 1;
            return _newId;
        }

        private void BodyOnDisposed(object sender, EventArgs e)
        {
            Dispose();
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// Subclasses can override incase they need to dispose of resources otherwise do nothing.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    //dispose managed resources 
                    if (body != null)
                    {
                        //Release event subscriptions
                        body.Updated -= Update;
                        body.Disposed -= BodyOnDisposed;
                    }
                }
            }
            IsDisposed = true;
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region IEquatable<Geom> Members

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.
        ///                 </param>
        public bool Equals(Geom other)
        {
            if ((object)other == null)
            {
                return false;
            }
            return Id == other.Id;
        }

        #endregion

        #region GetNextId variables

        private static int _newId = -1;

        #endregion

        #region Update variables

        private Vector2 _localVertice;
        private Vector2 _vertice = Vector2.Zero;
        private Vector2 _newPos = Vector2.Zero;

        #endregion

        #region IIsDisposable Members

        public bool IsDisposed
        {
            get { return _isDisposed; }
            set { _isDisposed = value; }
        }

        #endregion
    }
}