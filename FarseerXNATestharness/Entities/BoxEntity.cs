using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;

using FarseerGames.FarseerXNAPhysics.Collisions;
using FarseerGames.FarseerXNAPhysics.Dynamics;

using FarseerGames.FarseerXNATestharness.EntityViews;

namespace FarseerGames.FarseerXNATestharness.Entities {
    public class BoxEntity : RectangleRigidBody {
        protected IEntityView _entityView;

        public BoxEntity(Game game, float width, float height, float mass, Vector2 position, float orientation, bool isStatic) : base(width,height,mass) {

            Position = position;
            Orientation = orientation;
            IsStatic = isStatic;
            FrictionCoefficient = .3f;
            RestitutionCoefficient = 0f;
            RotationalDragCoefficient = .001f;
            InitializeEntityView(game);
        }

        public void InitializeEntityView(Microsoft.Xna.Framework.Game game) {
            BoxShape boxShape = new BoxShape(game, _width, _height);
            _entityView = boxShape;
        }

        public void Draw() {
            _entityView.Update(Position, Orientation);
            _entityView.Draw();
        }
    }
}
