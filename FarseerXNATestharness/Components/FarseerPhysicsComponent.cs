using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Components;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;

using FarseerGames.FarseerXNAPhysics;

namespace FarseerGames.FarseerXNATestharness.Components {
    public partial class FarseerPhysicsComponent : Microsoft.Xna.Framework.GameComponent {
        private PhysicsSimulator _physicsSimulator;

        private Vector2 _gravity = new Vector2(0, 0);
        private int _iterationsPerCollision = 1;
        private float _allowedCollisionPenetration = .01f;
        private float _impulseBiasFactor = .8f;

        [DescriptionAttribute("Gravity (+X=Right, +Y=Down)")]
        public Vector2 Gravity {
            get { return _gravity ; }
            set { _gravity  = value; }
        }

        [DescriptionAttribute("Number of iterations that will be used to try and resolve collisions. High numbers for accuracy, low numbers for performance. Value should be between 1 and 10.")]
        public int IterationsPerCollision {
            get { return _iterationsPerCollision ; }
            set { _iterationsPerCollision  = value; }
        }

        [DescriptionAttribute("Represents how much one body can penetrate another before the impulse bias mechanism kicks in.")]
        public float AllowedCollisionPenetration {
            get { return _allowedCollisionPenetration; }
            set { _allowedCollisionPenetration = value; }
        }

        [DescriptionAttribute("A factor that represents how quickly intersecting bodies resolved. Read Erin Catto's paper on Box2D for more info.")]
        public float ImpulseBiasFactor {
            get { return _impulseBiasFactor ; }
            set { _impulseBiasFactor  = value; }
        }	
	
        public FarseerPhysicsComponent() {
            InitializeComponent();
        }

        public override void Start() {
            _gravity = new Vector2(0, 0); //TODO: change this once Vector2's are editable in the property grid
            _physicsSimulator = new PhysicsSimulator(_gravity);
            _physicsSimulator.Iterations = _iterationsPerCollision;
            _physicsSimulator.SetAllowedPenetration(_allowedCollisionPenetration);
            _physicsSimulator.SetBiasFactor(_impulseBiasFactor);
            Game.GameServices.AddService(typeof(PhysicsSimulator), _physicsSimulator);
        }

        public override void Update() {
           _physicsSimulator.Update(Game.ElapsedTime.Milliseconds*.001f);
        }

        private void UpdatePhysics(float elapsedTime) {

        }

        public override void Draw() {
            // TODO: Add some diagnostic drawings here
        }
    }
}