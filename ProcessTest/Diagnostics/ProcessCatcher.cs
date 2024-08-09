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
        internal Process _process;
        public ProcessCatcher(string processName)
        {
            var processes = Process.GetProcessesByName(processName);

            if (processes.Length <= 0)
            {
                
            }
        }
    }
}
