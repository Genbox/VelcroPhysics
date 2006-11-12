using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;

using FarseerGames.FarseerXNAPhysics.Mathematics;
using FarseerGames.FarseerXNAPhysics.Collisions;

namespace FarseerGames.FarseerXNAPhysics.Collisions
{
    public sealed class Grid
    {
        private Geometry _geometry;
        private Vector2 _min = new Vector2(0,0);
        private Vector2 _max = new Vector2(0,0);
        float _gridWidth;
        float _gridHeight;
        private float _padding = 0;
        private float _gridCellSize = .1f;
        private float _gridCellSizeInverse = 10f;
        private GridPoint[][] _gridPoints;
        private GridCell[][] _gridCells;
        private Int32 _gridPointsWide; //# of grid points wide
        private Int32 _gridPointsHigh; //# of grid points high
        private Int32 _gridCellsWide; //# of grid cells wide
        private Int32 _gridCellsHigh; //# of grid cell high

        internal Geometry Geometry {
            get { return _geometry; }
            set { _geometry = value; }
        }

        internal float GridCellSize {
            get { return _gridCellSize; }
            set { 
                _gridCellSize = value;
                _gridCellSizeInverse = 1f / value;                
            }
        }

        internal Vector2 Min
        {
            get { return _min; }
        }

        internal Vector2 Max
        {
            get { return _max; }
        }

        public GridPoint[][] GridPoints {
            get { return _gridPoints; }
        }

        public Int32 GridPointsWide {
            get { return _gridPointsWide; }
        }

        public Int32 GridPointsHigh {
            get { return _gridPointsHigh; }
        }

        internal GridCell[][] GridCells {
            get { return _gridCells; }
        }

        internal Int32 GridCellsWide {
            get { return _gridCellsWide; }
        }

        internal Int32 GridCellsHigh {
            get { return _gridCellsWide; }
        }

        internal Grid(Geometry geometry, float gridCellSize, bool primeTheGrid)
        {
            if (geometry.LocalVertices.Count < 3) { throw new InvalidOperationException("A grid can only be constructed for geometries with 3 vertices or more"); }
            GridConstructor(geometry,gridCellSize, 0, primeTheGrid);
        }

        internal Grid(Geometry geometry, float gridCellSize, float padding, bool primeTheGrid) {
            GridConstructor(geometry, gridCellSize, padding, primeTheGrid);
        }

        private void GridConstructor(Geometry geometry, float gridCellSize, float padding, bool primeTheGrid){
            _geometry = geometry;
            _padding = padding;
            GridCellSize = gridCellSize;
            Initialize();
            if (primeTheGrid) {
                Prime();
            }
        }

        internal void Initialize()
        {
            if (_geometry == null) {
                throw new MissingFieldException("Geometry was not properly set");
            }
            InitializeMinMax();
            InitializeGridPoints();
            InitializeGridCells(); 
        }

        internal void Prime() {
            for  (int i = 0; i < _gridCellsWide; i++){
                for (int j = 0; j < _gridCellsHigh; j++) {
                    _gridCells[i][j].Prime(_geometry);
                }
            }
        }

        internal Feature Evaluate(Vector2 point) {
            Feature feature = new Feature(point);
            if(!Contains(point)){
                return feature;
            }

            //find the cell
            GridCell gridCell = FindContainingCell(point);
            
            //if grid is outide the geometry, return feature with default values (distance=float.MaxValue)
            if (gridCell.IsOutside) {
                return feature;
            }             

            //point is known to be inside the geometry so some work is needed to get the 'Feature' details.
            feature = gridCell.Evaluate(point);
            return feature;
        }

        internal bool Contains(Vector2 point) {
            if ((point.X >= _min.X && point.X <= _max.X) && (point.Y >= _min.Y && point.Y <= _max.Y)) {
                return true;
            }
            else {
                return false;
            }
        }

        private GridCell FindContainingCell(Vector2 point) {
#if (DEBUG)
            if(!Contains(point)){
                throw new Exception("Grid must contain point. Call 'Contains(...)' method prior to calling 'FindContainingCell'");
            }
#endif        
            Vector2 pointRelativeToMin = Vector2.Subtract(point, _min);
            int cellX = (int)Math.Floor(pointRelativeToMin.X * _gridCellSizeInverse);
            int cellY = (int)Math.Floor(pointRelativeToMin.Y * _gridCellSizeInverse);
            cellX = Math.Min(cellX, _gridCellsWide - 1);
            cellY = Math.Min(cellY, _gridCellsHigh - 1);
            return _gridCells[cellX][cellY];
        }    

        private void InitializeMinMax() {
            AABB aabb = _geometry.AABB;

            _gridWidth = (aabb.Width - aabb.Width % _gridCellSize) + 2*_gridCellSize;
            _gridHeight = (aabb.Height - aabb.Height % _gridCellSize) + 2 * _gridCellSize;

            _min = new Vector2(-_gridWidth / 2, -_gridHeight / 2);
            _max = new Vector2(_gridWidth / 2, _gridHeight / 2);

            //AABB aabb = _geometry.AABB;

            //_min.X = (aabb.Min.X - aabb.Min.X % _gridCellSize) - _gridCellSize - _padding;
            //_min.Y = (aabb.Min.Y - aabb.Min.Y % _gridCellSize) - _gridCellSize - _padding;
            //_max.X = (aabb.Max.X - aabb.Max.X % _gridCellSize) + _gridCellSize + _padding;
            //_max.Y = (aabb.Max.Y - aabb.Max.Y % _gridCellSize) + _gridCellSize + _padding;
        }

        private void InitializeGridPoints() { 
            _gridCellsWide = (int)Math.Round(_gridWidth / _gridCellSize);
            //_gridCellsWideInverse = 1f / _gridCellsWide;
            _gridPointsWide = _gridCellsWide + 1;

            _gridCellsHigh = (int)Math.Round(_gridHeight / _gridCellSize);
            //_gridCellsHighInverse = 1f / _gridCellsHigh; 
            _gridPointsHigh = _gridCellsHigh + 1;

            _gridPoints = new GridPoint[_gridPointsWide][];
            for(int k=0; k<_gridPointsWide; k++){
                _gridPoints[k] = new GridPoint[_gridPointsHigh];               
            }

            for  (int i = 0; i < _gridPointsWide; i++){
                for (int j = 0; j < _gridPointsHigh; j++) {
                    GridPoint gridPoint = new GridPoint(_min.X + i * _gridCellSize, _min.Y + j * _gridCellSize);
                    _gridPoints[i][j] = gridPoint;
                }
            }
        }

        private void InitializeGridCells() {
            _gridCells = new GridCell[_gridCellsWide][];
            for (int k = 0; k < _gridCellsWide; k++) {
                _gridCells[k] = new GridCell[_gridCellsHigh];
            }

            for (int j = 0; j < _gridCellsHigh; j++) {
                for (int i = 0; i < _gridCellsWide; i++) {
                    GridCell gridCell = new GridCell();
                    gridCell.GridPoints[0] = _gridPoints[i][j];
                    gridCell.GridPoints[1] = _gridPoints[i][j+ 1];
                    gridCell.GridPoints[2] = _gridPoints[i + 1][j + 1];
                    gridCell.GridPoints[3] = _gridPoints[i + 1][j];
                    _gridCells[i][j] = gridCell;
                }
            }
        }

        private Int32 GetGridPointIndex(Int32 i, Int32 j) {
            return j * _gridPointsWide + i;
        }

    }
}
