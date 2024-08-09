using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Augong.ThreadTest
{
    public class ThreadAbortTest : ThreadTestBase
    {
        public override void DoTest()
        {
            var thread = new Thread(new ThreadStart(DoLoop));
        }

        private void DoLoop()
        {
            while (true) { var t = 1; };
        }
    }
}
