using Augong.ProcessTest.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Augong.ProcessTest.Diagnostics
{
    /// <summary>
    /// Base class to manage process
    /// </summary>
    public class ProcessCatcher
    {
        protected Process _process;
        public ProcessCatcher(string processName)
        {
            var processes = Process.GetProcessesByName(processName);
            var ps = Process.GetProcessById(1384);

            if (processes.Length <= 0)
            {
                throw new ProcessException($"Process {processName} not found!");
            }

            _process = processes[0];
        }
    }
}
