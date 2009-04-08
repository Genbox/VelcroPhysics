using System;
using System.Collections.Generic;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Mathematics;

namespace FarseerGames.WaterSampleSilverlight.Models
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
            BoxModels.Add(InitializeBox(350, 190, 200, 20, 1f));
            InitializePyrmid(new Vector2(308, 170), 20, 20, 2, 2, 5);

            foreach (var boxModel in BoxModels)
            {
                boxModel.Initialize(physicsSimulator);
            }

            foreach (var boxModel in PyramidBoxModels)
            {
                boxModel.Initialize(physicsSimulator);
            }
        }

        private void InitializePyrmid(Vector2 bottomRightBlockPosition, float blockWidth, float blockHeight,
                                      float horizontalSpacing, float verticleSpacing, int bottomRowBlockCount)
        {
            PyramidBoxModels = new List<BoxModel>();
            Vector2 rowOffset = new Vector2((blockWidth/2) + (horizontalSpacing/2), -(blockHeight + verticleSpacing));
            Vector2 colOffset = new Vector2(horizontalSpacing + blockWidth, 0);
            for (int i = 0; i < bottomRowBlockCount; i++)
            {
                Vector2 position = bottomRightBlockPosition + rowOffset*i;
                for (int j = 0; j < bottomRowBlockCount - i; j++)
                {
                    Vector2 rowPosition = position + colOffset*j;
                    PyramidBoxModels.Add(InitializeBox(rowPosition.X, rowPosition.Y, blockWidth, blockHeight, 2f));
                }
            }
        }

        private BoxModel InitializeBox(float x, float y, float width, float height, float density)
        {
            BoxModelDef boxModelDef = new BoxModelDef();
            boxModelDef.Width = ConvertUnits.ToSimUnits(width);
            boxModelDef.Height = ConvertUnits.ToSimUnits(height);
            boxModelDef.Position = ConvertUnits.ToSimUnits(x, y);
            boxModelDef.Mass = density*boxModelDef.Width*boxModelDef.Height;

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