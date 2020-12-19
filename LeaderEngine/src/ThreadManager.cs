using System;
using System.Collections.Generic;
using System.Text;

namespace LeaderEngine
{
    public static class ThreadManager
    {
        private static Queue<Action> pendingActions = new Queue<Action>();

        public static void ExecuteOnMainThread(Action action)
            => pendingActions.Enqueue(action);

        public static void ExecuteAll()
        {
            while (pendingActions.Count > 0)
                pendingActions.Dequeue().Invoke();
        }
    }
}
