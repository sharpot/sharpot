using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace SharpOT.Util
{
    public static class Scheduler
    {
        private static Object addEventLock = new Object();

        public static void AddTask(Action action, object[] paramArray, int delayInMilliseconds)
        {
            lock (addEventLock)
            {
                Action<Action, int> myDelegate = new Action<Action, int>(AddTaskDelay);
                myDelegate.BeginInvoke(action, delayInMilliseconds, null, null);
            }
        }

        private static void AddTaskDelay(Action action, int delayInMilliseconds)
        {
            System.Threading.Thread.Sleep(delayInMilliseconds);
            bool bFired;

            if (action != null)
            {
                foreach (Delegate singleCast in action.GetInvocationList())
                {
                    bFired = false;
                    try
                    {
                        ISynchronizeInvoke syncInvoke = (ISynchronizeInvoke)singleCast.Target;
                        if (syncInvoke != null && syncInvoke.InvokeRequired)
                        {
                            bFired = true;
                            syncInvoke.BeginInvoke(singleCast, null);
                        }
                        else
                        {
                            bFired = true;
                            singleCast.DynamicInvoke(null);
                        }
                    }
                    catch (Exception)
                    {
                        if (!bFired)
                            singleCast.DynamicInvoke(null);
                    }
                }
            }
        }
    }
}
