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

		public static List<string> ResortByLine(params string[] data)
		{
			List<string> rst = new List<string>();
			foreach (var d in data)
			{
				var lines = d.Split("\r\n");
				rst.AddRange(lines);
			}
			return rst;
		}
	}
}
