using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using BepuPhysics;
using BepuUtilities;
using BepuUtilities.Memory;

namespace LeaderEngine
{
    public static class PhysicsController
    {
        public static Simulation Simulation;

        public static event Action<Simulation> OnPhysicsUpdate;

        public static bool PausePhysics = false;

        private static IThreadDispatcher threadDispatcher;

        public static void Init()
        {
            Simulation = Simulation.Create(new BufferPool(), new NarrowPhaseCallbacks(), new PoseIntegratorCallbacks(new Vector3(0.0f, -9.8f, 0.0f)), new PositionLastTimestepper());

            threadDispatcher = new SimpleThreadDispatcher(Environment.ProcessorCount);
        }

        public static void Update()
        {
            if (PausePhysics)
                return;

            Simulation.Timestep(Time.deltaTime, threadDispatcher);
            OnPhysicsUpdate?.Invoke(Simulation);
        }
    }
}
