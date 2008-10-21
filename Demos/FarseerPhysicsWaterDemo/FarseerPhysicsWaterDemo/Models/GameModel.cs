using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;

using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Mathematics;

namespace FarseerPhysicsWaterDemo.Models
{
    public class GameModel
    {
        #region properties
        public WaterModel WaterModel { get; private set; }
        public List<BoxModel> BoxModels { get; private set; }
        public List<BoxModel> PyramidBoxModels { get; private set; }
        #endregion

        #region public methods
        public void Initialize(PhysicsSimulator physicsSimulator)
        {
            InitializeBoxes(physicsSimulator);
            InitializeWater(physicsSimulator);
        }

        public void Update(TimeSpan elapsedTime)
        {
            WaterModel.Update(elapsedTime);
        }
        #endregion

        #region private methods
        private void InitializeWater(PhysicsSimulator physicsSimulator)
        {
            WaterModel = new WaterModel();
            WaterModel.Initialize(physicsSimulator);
        
            foreach (var boxModel in BoxModels)
            {
                WaterModel.FluidDragController.AddGeom(boxModel.Geom);
            }

            foreach (var boxModel in PyramidBoxModels)
            {
                WaterModel.FluidDragController.AddGeom(boxModel.Geom);
            }
        }

        private void InitializeBoxes(PhysicsSimulator physicsSimulator)
        {
            BoxModels = new List<BoxModel>();
            BoxModels.Add(InitializeBox(physicsSimulator, 350, 190, 200, 20, 1f));
            InitializePyrmid(physicsSimulator, new Vector2(308, 170), 15, 15, 2, 2,6);
            
            foreach (var boxModel in BoxModels)
            {
                boxModel.Initialize(physicsSimulator);
            }

            foreach (var boxModel in PyramidBoxModels)
            {
                boxModel.Initialize(physicsSimulator);
            }
        }

        private void InitializePyrmid(PhysicsSimulator physicsSimulator, Vector2 bottomRightBlockPosition, float blockWidth, float blockHeight, float horizontalSpacing, float verticleSpacing, int bottomRowBlockCount)
        {
            PyramidBoxModels = new List<BoxModel>();
            Vector2 rowOffset = new Vector2((blockWidth / 2) + (horizontalSpacing / 2), -(blockHeight + verticleSpacing));
            Vector2 colOffset = new Vector2(horizontalSpacing + blockWidth, 0);
            Vector2 position = Vector2.Zero;
            int blockCounter = 0;
            for (int i = 0; i < bottomRowBlockCount; i++)
            {
                position = bottomRightBlockPosition + rowOffset * i;
                for (int j = 0; j < bottomRowBlockCount - i; j++)
                {
                    Vector2 rowPosition = position + colOffset * j;
                    PyramidBoxModels.Add(InitializeBox(physicsSimulator, rowPosition.X, rowPosition.Y, blockWidth, blockHeight, 2f));
                    blockCounter += 1;
                }
            }
        }

        private BoxModel InitializeBox(PhysicsSimulator physicsSimulator, float x, float y, float width, float height, float density)
        {
            BoxModelDef boxModelDef = new BoxModelDef();
            boxModelDef.Width = ConvertUnits.ToSimUnits(width);
            boxModelDef.Height = ConvertUnits.ToSimUnits(height);
            boxModelDef.Position = ConvertUnits.ToSimUnits(x, y);
            boxModelDef.Mass = density * boxModelDef.Width * boxModelDef.Height;

            BoxModel model = new BoxModel(boxModelDef);
            return model;
        }
        #endregion

        #region events
        #endregion

        #region private variables

        #endregion
    }
}
