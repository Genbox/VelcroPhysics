using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Factories;

namespace FarseerGames.WaterSampleSilverlight.Models
{
    public class BoxModel
    {
        #region properties

        public Body Body { get; private set; }
        public Geom Geom { get; private set; }
        public float Width { get; private set; }
        public float Height { get; private set; }
        public float Mass { get; private set; }

        #endregion

        #region public methods

        public BoxModel(BoxModelDef def)
        {
            _def = def; //hold on to the _def until Initialize is called
        }

        public void Initialize(PhysicsSimulator physicsSimulator)
        {
            Width = _def.Width;
            Height = _def.Height;
            Mass = _def.Mass;
            Body = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, Width, Height, Mass);
            //Body.RotationalDragCoefficient = .000001f;
            Geom = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, Body, Width, Height);
            Geom.FrictionCoefficient = .2f;
            Body.Position = _def.Position;
            _def = null;
        }

        #endregion

        #region private methods

        #endregion

        #region events

        #endregion

        #region private variables

        private BoxModelDef _def;

        #endregion
    }
}