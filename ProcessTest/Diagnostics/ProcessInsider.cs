using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Augong.ProcessTest.Diagnostics
{

    /// <summary>
    /// Aimed to look inside a process
    /// </summary>
    public class ProcessInsider(string processName) : ProcessCatcher(processName)
    {
        public int GetProcessHandlers()
        {
            return _process.HandleCount;
        }

        public int GetProcessThreads()
        {
            return _process.Threads.Count;
        }

        public void CheckThreadStates()
        {
            var threads = _process.Threads;
        }

    }
}
