using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Augong.CSharp
{
	public class BenchmarkTest
	{
		public void DoTest()
		{
			var dic = new Dictionary<int, byte[]>();

			// 写入字典
			for (int i = 0; i < 1024; i++)
			{
				var nb = new byte[1024 * 1024];
				dic.Add(i, nb);
			}

			Console.ReadKey();

			// 删除元素
			for (int i = 0; i < 512; i++)
			{
				dic.Remove(i);
			}

			Console.ReadKey();
		}

	}
}
