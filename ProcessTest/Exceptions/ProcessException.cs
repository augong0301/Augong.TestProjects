using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Augong.ProcessTest.Exceptions
{
    public class ProcessException : Exception
    {
        public ProcessException() : base() { }
        public ProcessException(string msg) : base(msg) { }
    }
}
