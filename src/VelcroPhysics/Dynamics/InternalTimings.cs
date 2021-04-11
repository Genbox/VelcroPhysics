namespace Genbox.VelcroPhysics.Dynamics
{
    public struct InternalTimings
    {
        public static long AddRemoveTime;

        internal static long NewContactsTimeDelta;
        public static long NewContactsTime => NewContactsTimeDelta - AddRemoveTime;

        internal static long ControllersUpdateTimeDelta;
        public static long ControllersUpdateTime => ControllersUpdateTimeDelta - NewContactsTimeDelta;

        internal static long ContactsUpdateTimeDelta;
        public static long ContactsUpdateTime => ContactsUpdateTimeDelta - ControllersUpdateTimeDelta;

        internal static long SolveUpdateTimeDelta;
        public static long SolveUpdateTime => SolveUpdateTimeDelta - ContactsUpdateTimeDelta;

        internal static long ContinuousPhysicsTimeDelta;
        public static long ContinuousPhysicsTime => ContinuousPhysicsTimeDelta - SolveUpdateTimeDelta;

        public static long JointUpdateTime;
        public static long UpdateTime;
    }
}
