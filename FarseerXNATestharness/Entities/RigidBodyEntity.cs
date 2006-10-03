//using System;
//using System.Collections.Generic;
//using System.Text;

//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Graphics;

//using FarseerGames.FarseerXNAPhysics;
//using FarseerGames.FarseerXNAPhysics.Dynamics;
//using FarseerGames.FarseerXNAPhysics.Collisions;

//using FarseerGames.FarseerXNAGame.Drawing;

//using FarseerGames.FarseerXNATestharness.EntityViews;

//namespace FarseerGames.FarseerXNATestharness.Entities {
//    public abstract class RigidBodyEntity : RigidBody  {
//        protected IEntityView _entityView;

//        public RigidBodyEntity(Game game) {
            
//        }

//        public void InitializeRigidBodyEntity(Game game){

//            InitializeRigidBody();
//            InitializeEntityView(game);
//            Initialize();
//        }

//        public abstract void InitializeRigidBody();

//        public abstract void InitializeEntityView(Game game);

//        public void Draw() {
//            _entityView.Update(Position, Orientation);
//            _entityView.Draw();
//        }


//    }
//}
