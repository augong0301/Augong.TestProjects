using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipelineTest.TestClass.Collections
{
	public class DictionaryTest : ITest
	{
		public DictionaryTest() { }

		private Dictionary<int, byte[]> collection = new Dictionary<int, byte[]>();

		public void DoTest()
		{
			for (int i = 0; i < 1024; i++)
			{
				var bytes = new byte[1024 * 1024 * 2];
				collection.Add(i, bytes);
			}

			Console.ReadKey();

			for (int i = 0; i < 1024; i++)
			{
				collection.Remove(i);
			}
			Console.ReadKey();
			GC.Collect(2);
			Console.ReadKey();


		}
	}
}
