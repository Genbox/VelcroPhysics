using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Components;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;

using FarseerGames.FarseerXNAGame.Components;

using FarseerGames.FarseerXNATestharness.Entities;

namespace FarseerGames.FarseerXNATestharness.Samples {
    public partial class FarseerDemo2 : FarseerDemo {
        private KeyboardInputComponent _keyboardInputComponent;
        
        private GameBorder _gameBorder;
        private Pyrmid _pyrmid;

        private ShipEntity _shipEntity;
        private Vector2 _shipStartPosition;     
        private float _shipWidth = 1;
        private float _shipHeight = 1;
        private float _shipThrust = 50;
        private float _shipTurningTorque = 5;

        public KeyboardInputComponent  KeyboardInputComponent {
            get { return _keyboardInputComponent ; }
            set { _keyboardInputComponent  = value; }
        }	

        public Vector2 ShipStartPosition {
            get { return _shipStartPosition; }
            set { _shipStartPosition = value; }
        }

        public float ShipWidth {
            get { return _shipWidth; }
            set { _shipWidth = value; }
        }

        public float ShipHeight {
            get { return _shipHeight; }
            set { _shipHeight = value; }
        }

        public float  ShipThrust {
            get { return _shipThrust; }
            set { _shipThrust = value; }
        }

        public float ShipTurningTorque {
            get { return _shipTurningTorque; }
            set { _shipTurningTorque = value; }
        }
                
        public FarseerDemo2() {
            InitializeComponent();
        }

        public override void Start() {
            base.Start();
            _shipEntity = new ShipEntity(Game, _shipWidth, _shipHeight, _shipStartPosition, 3.14f);
            _shipEntity.Thrust = _shipThrust;
            _shipEntity.TurningTorque = _shipTurningTorque;
            _physicsSimulator.Add(_shipEntity);

            _gameBorder = new GameBorder(Game, _physicsSimulator, 57.3f, 41.3f);
            _pyrmid = new Pyrmid(Game, _physicsSimulator,new Vector2(-20, 0),1,0);

            this.KeyboardInputComponent.WKeyPressed += new System.EventHandler<FarseerGames.FarseerXNAGame.Components.KeyEventArgs>(this.KeyboardInputComponent_WKeyPressed);
            this.KeyboardInputComponent.DKeyPressed += new System.EventHandler<FarseerGames.FarseerXNAGame.Components.KeyEventArgs>(this.KeyboardInputComponent_DKeyPressed);
            this.KeyboardInputComponent.AKeyPressed += new System.EventHandler<FarseerGames.FarseerXNAGame.Components.KeyEventArgs>(this.KeyboardInputComponent_AKeyPressed);
            this.KeyboardInputComponent.SKeyDown += new System.EventHandler<FarseerGames.FarseerXNAGame.Components.KeyEventArgs>(this.KeyboardInputComponent_SKeyDown);
          }

        public override void Update() {
            // TODO: Add your update code here
        }

        public override void Draw() {
            base.Draw();
            _shipEntity.Draw();
            _gameBorder.Draw();
            _pyrmid.Draw();
        }

        private void KeyboardInputComponent_AKeyPressed(object sender, FarseerGames.FarseerXNAGame.Components.KeyEventArgs e) {
            _shipEntity.TurnLeft();
        }

        private void KeyboardInputComponent_WKeyPressed(object sender, FarseerGames.FarseerXNAGame.Components.KeyEventArgs e) {
            _shipEntity.ApplyThrust();
        }

        private void KeyboardInputComponent_DKeyPressed(object sender, FarseerGames.FarseerXNAGame.Components.KeyEventArgs e) {
            _shipEntity.TurnRight();
        }

        private void KeyboardInputComponent_SKeyDown(object sender, FarseerGames.FarseerXNAGame.Components.KeyEventArgs e) {
            _physicsSimulator.Gravity = new Vector2(0, 2);
        }
    }
}