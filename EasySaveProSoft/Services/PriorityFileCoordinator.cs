using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace EasySaveProSoft.Services
{
    public static class PriorityFileCoordinator
    {
        private static readonly object _lock = new object();
        private static readonly HashSet<string> _remainingPriorityFiles = new HashSet<string>();
        private static readonly AutoResetEvent _priorityChanged = new AutoResetEvent(false);

        public static void RegisterPriorityFile(string path)
        {
            lock (_lock)
            {
                if (!_remainingPriorityFiles.Contains(path))
                {
                    _remainingPriorityFiles.Add(path);
                    _priorityChanged.Set(); // signal
                }
            }
        }

        public static void MarkAsProcessed(string path)
        {
            lock (_lock)
            {
                if (_remainingPriorityFiles.Remove(path))
                {
                    _priorityChanged.Set(); // signal that something has changed
                }
            }
        }

        public static bool HasRemainingPriorityFiles()
        {
            lock (_lock)
            {
                return _remainingPriorityFiles.Count > 0;
            }
        }

        public static void WaitUntilNoPriorityLeft()
        {
            while (true)
            {
                lock (_lock)
                {
                    if (_remainingPriorityFiles.Count == 0)
                        return;
                }

                // Wait for a signal that the state might have changed
                _priorityChanged.WaitOne(200); // check every 200ms
            }
        }
    }
}
