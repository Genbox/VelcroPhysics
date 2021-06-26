namespace Genbox.VelcroPhysics.Dynamics
{
    public struct Profile
    {
        /// <summary>
        /// The time it takes to complete the full World.Step()
        /// </summary>
        public long Step;

        /// <summary>
        /// The time it takes to find collisions in the CollisionManager
        /// </summary>
        public long Collide;

        /// <summary>
        /// The time it takes to solve integration of velocities, constraints and integrate positions
        /// </summary>
        public long Solve;

        /// <summary>
        /// Timings from the island solver. The time it takes to initialize velocity constraints.
        /// </summary>
        public long SolveInit;

        /// <summary>
        /// Timings from the island solver. It includes the time it takes to solve joint velocity constraints.
        /// </summary>
        public long SolveVelocity;

        /// <summary>
        /// Timings from the island solver. In includes the time it takes to solve join positions.
        /// </summary>
        public long SolvePosition;

        /// <summary>
        /// The time it takes for the broad-phase to update
        /// </summary>
        public long Broadphase;

        /// <summary>
        /// The time it takes for the time-of-impact solver
        /// </summary>
        public long SolveTOI;

        /// <summary>
        /// Time it takes to process newly added and removed bodies/joints/controllers from the world
        /// </summary>
        public long AddRemoveTime;

        /// <summary>
        /// The time it takes for the contact manager to find new contacts in the world
        /// </summary>
        public long NewContactsTime;

        /// <summary>
        /// The time it takes to update controller logic
        /// </summary>
        public long ControllersUpdateTime;

        /// <summary>
        /// The time it takes to update breakable bodies
        /// </summary>
        public long BreakableBodies;
    }
}
