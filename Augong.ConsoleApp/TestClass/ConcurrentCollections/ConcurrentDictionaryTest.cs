using Augong.CSharp.Contract;
using System.Collections.Concurrent;

namespace Augong.CSharp.ConsoleApp.TestClass.ConcurrentCollections
{
	public class ConcurrentDictionaryTest : ITest
	{
		public ConcurrentDictionaryTest() { }


		private ConcurrentDictionary<int, byte[]> dic = new ConcurrentDictionary<int, byte[]>();

		public void DoTest()
		{

		}
	}
}
