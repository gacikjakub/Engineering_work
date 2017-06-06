using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace ServoMonitoring_with_Control
{
    public static class ExtensionMethods
    {
        // function which mapping value from one range of values to another range 
        public static float Remap(this float value, float from1, float to1, float from2, float to2)
        {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }

        public static void Wait(int milis)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            while (true)
            {
                //some other processing to do STILL POSSIBLE
                if (stopwatch.ElapsedMilliseconds >= milis)
                {
                    break;
                }
            }
        }
    }
}
