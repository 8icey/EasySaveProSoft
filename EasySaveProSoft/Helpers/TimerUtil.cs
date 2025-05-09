using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySaveProSoft.Helpers
{
    // TimerUtil is a helper class used to measure the execution time of an action (code block).
    internal static class TimerUtil
    {
        /// <summary>
        /// Measures the time it takes to execute the provided action.
        /// </summary>
        /// <param name="action">The block of code to measure.</param>
        /// <returns>TimeSpan representing how long the action took to execute.</returns>
        public static TimeSpan MeasureExecution(Action action)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew(); // Start the stopwatch
            action();                                                 // Execute the provided action
            stopwatch.Stop();                                         // Stop the stopwatch
            return stopwatch.Elapsed;                                 // Return the elapsed time
        }
    }
}
