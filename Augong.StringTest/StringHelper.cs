using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Augong.StringTest
{
    public static class StringHelper
    {
        public static string[] GetStartFlag(string flag, string input)
        {
            return input.Split(flag);
        }
    }
}
