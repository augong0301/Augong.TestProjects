using Augong.CSharp.Contract;
using System.Collections.Generic;

namespace Augong.ConsoleApp.BasicTests
{
	class IEnumerableTest : ITest
	{
		public void DoTest()
		{
			var data = new List<ClassInt>();
			for (int i = 0; i < 10; i++)
			{
				data.Add(new ClassInt(i));
			}
			var valueObj = (object)data;
			var parse = (IEnumerable<object>)valueObj;
		}
	}

	class ClassInt(int i)
	{
		public int Value => i;
	}
}
