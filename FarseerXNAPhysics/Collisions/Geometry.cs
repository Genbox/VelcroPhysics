using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using Microsoft.Xna.Framework;

using FarseerGames.FarseerXNAPhysics.Mathematics;

namespace FarseerGames.FarseerXNAPhysics.Collisions {
    /// <remark>
    /// The geometry class is the unit of collision detection.
    /// </remark>
    public abstract class Geometry  {
        //private Grid _grid;
        private Vertices _localVertices;
        private Vertices _worldVertices;
        private Matrix _orientation = Matrix.Identity;
        private Vector2 _position = new Vector2();
        private AABB _aabb = new AABB();

        public Geometry() {
        }

        public Geometry(Vertices vertices) {
            SetVertices(vertices);                     
        }

        public Vertices LocalVertices {
            get { return _localVertices; }
        }

        public Vertices WorldVertices {
            get { return _worldVertices; }
        }

        public Vector2 Position {
            get { return _position; }
        }

        public Matrix Orientation {
            get { return _orientation; }
        }

        public Matrix WorldMatrix {
            get {
                Matrix translationMatrix = Matrix.CreateTranslation(_position.X, _position.Y, 0);
                return Matrix.Multiply(_orientation, translationMatrix);
            }
        }

        public AABB AABB {
            get { return _aabb; }
        }
      
        protected void SetVertices(Vertices vertices) {
            _localVertices = new Vertices(vertices);
            _worldVertices = new Vertices(vertices);
            _aabb.Update(_localVertices);
        }

        protected void SubDivideEdges(float maxEdgeLength){
            //TODO: Implement SubDivideEdges()
        }

        public Vector2 GetEdge(Int32 index) {
            Int32 nextIndex = NextIndex(index);
            Vector2 edge = Vector2.Subtract(_localVertices[nextIndex], _localVertices[index]);
            return edge;
        }

        public Vector2 GetEdge(Int32 index, bool worldCoordinates) {
            if (worldCoordinates) {
                Int32 nextIndex = NextIndex(index);
                return Vector2.Subtract(_worldVertices[nextIndex], _worldVertices[index]);
            }
            else {
                return GetEdge(index);
            }
        }

        public Vector2 GetEdgeMidPoint(Int32 index) {
            Vector2 edge = GetEdge(index);
            edge = Vector2.Multiply(edge, .5f);
            Vector2 midPoint = Vector2.Add(_localVertices[index], edge);
            return midPoint;
        }

        public Vector2 GetEdgeMidPoint(Int32 index, bool worldCoordinates) {
            if (worldCoordinates) {
                Vector2 edge = GetEdge(index,worldCoordinates);
                edge=Vector2.Multiply(edge, .5f);
                Vector2 midPoint = Vector2.Add(_worldVertices[index], edge);
                return midPoint;
            }
            else {
                return GetEdgeMidPoint(index);
            }
        }

        public Vector2 GetEdgeNormal(Int32 index) {
            Vector2 edge = GetEdge(index);
            Vector2 edgeNormal = new Vector2(-edge.Y, edge.X);
            edgeNormal.Normalize();
            return edgeNormal;
        }

        public Vector2 GetEdgeNormal(Int32 index, bool worldCoordinates) {
            if (worldCoordinates) {
                Vector2 edge = GetEdge(index,worldCoordinates);
                Vector2 edgeNormal = new Vector2(-edge.Y, edge.X);
                edgeNormal.Normalize();
                return edgeNormal;
            }
            else {
                return GetEdgeNormal(index);
            }
        }

        public Vector2 GetVertexNormal(Int32 index) {
            Vector2 nextEdge = GetEdgeNormal(index);
            Vector2 prevEdge = GetEdgeNormal(PreviousIndex(index));
            Vector2 vertexNormal = Vector2.Add(nextEdge,prevEdge);
            vertexNormal.Normalize();
            return vertexNormal;            
        }

        public Vector2 GetVertexNormal(Int32 index, bool worldCoordinates) {
            if (worldCoordinates) {
                Vector2 nextEdge = GetEdgeNormal(index, true);
                Vector2 prevEdge = GetEdgeNormal(PreviousIndex(index), true);
                Vector2 vertexNormal = Vector2.Add(nextEdge, prevEdge);
                vertexNormal.Normalize();
                return vertexNormal;
            }
            else {
                return GetVertexNormal(index);
            }
        }

        public void SetPosition(float x, float y) {
            Update(new Vector2(x, y), _orientation);
        }

        public void SetRotation(float angle) {
            
            Matrix orientation = Matrix.CreateRotationZ(angle);
            Update(_position,orientation);
        
        }

        public void Move(float x, float y) {
            Vector2 moveVector = new Vector2(x, y);
            _position = Vector2.Add(_position, moveVector);
            Update();            
        }

        public void Rotate(float angle) {
            Matrix rotationMatrix = Matrix.CreateRotationZ(angle);
            _orientation = Matrix.Multiply(_orientation ,rotationMatrix);
            Update();
        }

        public Vector2 ConvertToLocalCoordinates(Vector2 point) {
           Vector2 localPoint = Vector2.Subtract(point, _position);
            Matrix inverseOrientation = Matrix.Transpose(_orientation);
            localPoint = Vector2.Transform(localPoint,inverseOrientation);
            return localPoint;
        }

        public Vector2 ConvertToWorldOrientation(Vector2 point) {
            Vector2 worldPoint = Vector2.TransformNormal(point, _orientation);
            return worldPoint;
        }

        public void Update(Vector2 position, float orientation) {
            _position = position;
            _orientation = Matrix.CreateRotationZ(orientation);
            Update();
        }

        private void Update(Vector2 position,Matrix orientation) {
            //update worldVertices;
            for (int i = 0; i < _localVertices.Count; i++) {
                _worldVertices[i] = Vector2.Transform(_localVertices[i], orientation);
                _worldVertices[i] = Vector2.Add(_worldVertices[i], position);
            }
            _aabb.Update(_worldVertices);
        }

        private void Update() {
            Update(_position, _orientation);
        }

        internal Feature GetNearestFeature(Vector2 point) {
            Feature nearestFeature = new Feature();
            nearestFeature.Distance = float.MaxValue;

            for (int i = 0; i < _localVertices.Count; i++) {
                Feature feature = GetNearestFeature(point, i);
                if (feature.Distance < nearestFeature.Distance) {
                    nearestFeature = feature;
                }
            }
            return nearestFeature;
        }

        private Feature GetNearestFeature(Vector2 point, Int32 index) {
            Feature feature;
            Vector2 v = GetEdge(index);
            Vector2 w = Vector2.Subtract(point, _localVertices[index]);

            float c1 = Vector2.Dot(w, v);
            if (c1 < 0) {
                feature.Position = _localVertices[index];
                feature.Normal = GetVertexNormal(index);
                feature.Distance = Math.Abs(w.Length());
                
                return feature;
            }
            
            float c2 = Vector2.Dot(v, v);
            if (c2 <= c1) {
                Vector2 d1 = Vector2.Subtract(point, _localVertices[NextIndex(index)]);
                feature.Position = _localVertices[NextIndex(index)];
                feature.Normal = GetVertexNormal(NextIndex(index));
                feature.Distance = Math.Abs(d1.Length());
                return feature;
            }

            float b = c1 / c2;
            v = Vector2.Multiply(v,b);
            Vector2 Pb = Vector2.Add(_localVertices[index], v);
            Vector2 d2 = Vector2.Subtract(point, Pb);
            feature.Position = Pb;
            feature.Normal = GetEdgeNormal(index);
            feature.Distance = d2.Length();
            return feature;
        }

        private Int32 NextIndex(Int32 index) {
            if (index == _localVertices.Count-1) {
                return 0;
            }else{
                return index + 1;
            }
        }

        private Int32 PreviousIndex(Int32 index) {
            if (index == 0) {
                return _localVertices.Count - 1;
            }
            else {
                return index - 1;
            }
        }

    }
}
