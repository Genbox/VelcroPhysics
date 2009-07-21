using System;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Controllers;
using Microsoft.Xna.Framework;

namespace FarseerGames.WaterSampleXNA.Demo1
{
    /// <summary>
    ///The water model in this demo is made up of two parts.  
    ///
    ///The wave controller controls the motion of the wave. It can control
    ///the size of the wave, the speed, the damping, etc..
    ///
    ///The fluid drag controller controls the forces that act upon 
    ///bodies that are IN the water.  It controls how thing float,
    ///the fluid drag, etc..
    ///
    ///Each of these controller could be used by themselves, but 
    ///combining them gives the most full featured water simulation.
    /// </summary>
    public class WaterModel
    {
        public WaveController WaveController { get; private set; }
        public FluidDragController FluidDragController { get; private set; }

        public float WaveGeneratorMax { get; set; }
        public float WaveGeneratorMin { get; set; }
        public float WaveGeneratorStep { get; set; }

        public WaterModel()
        {
            WaveGeneratorMax = 20.0f; //initial value
            WaveGeneratorMin = 0.0f; //initial value
            WaveGeneratorStep = 0.0f;
        }

        public void Initialize(PhysicsSimulator physicsSimulator, Vector2 position, Vector2 size)
        {
            //The wave controller controls how the waves move.. how big, how fast, etc..
            //The wave controller is represented as set of points equally
            //spaced horizontally along the width of the wave.
            WaveController = new WaveController();
            WaveController.Position = position;
            WaveController.Width = size.X;
            WaveController.Height = size.Y;
            WaveController.NodeCount = 100; //how many vertices make up the surface of the wave
            WaveController.DampingCoefficient = .98f; //determines how quickly the wave will disipate
            WaveController.Frequency = .001f; //determines how fast the wave algorithm runs (seconds)

            //The wave generator parameters simply move an end-point of the wave up and down.
            //Think of a string attached to a wall on one end and held by a person on the other.
            //If the person moves the string up and down to make "waves" then the arm is acting
            //similar to the wave generator. The WaveGeneratorStep property controls how fast the "arm"
            //moves.
            WaveController.WaveGeneratorMax = WaveGeneratorMax;
            WaveController.WaveGeneratorMin = WaveGeneratorMin;
            WaveController.WaveGeneratorStep = WaveGeneratorStep;

            WaveController.Initialize();

            //fluid drag controller controls how things move once IN the water.
            FluidDragController = new FluidDragController();
            FluidDragController.Initialize(WaveController, 0.0040f, 0.98f, 0.002f, physicsSimulator.Gravity);
            FluidDragController.Entry += EntryEventHandler;
            //init with default values.
            physicsSimulator.Add(FluidDragController);
        }

        public void Reset()
        {
            FluidDragController.Reset();
        }

        public void Update(TimeSpan elapsedTime)
        {
            WaveController.Update((float)elapsedTime.TotalSeconds, (float)elapsedTime.TotalSeconds);
        }

        public void EntryEventHandler(Geom geom, Vertices verts)
        {
            for (int i = 0; i < verts.Count; i++)
            {
                Vector2 vel, point = verts[i];
                geom.Body.GetVelocityAtWorldPoint(ref point, out vel);
                WaveController.Disturb(verts[i].X, (vel.Y * geom.Body.Mass) / (100.0f * geom.Body.Mass));
            }
        }
    }
}