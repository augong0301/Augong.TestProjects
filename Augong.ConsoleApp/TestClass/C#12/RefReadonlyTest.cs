using Augong.CSharp.Contract;

namespace Augong.CSharp.TestClass
{
    // test for [in] and [ref readonly]
    // https://learn.microsoft.com/zh-cn/dotnet/csharp/whats-new/csharp-12#ref-readonly-parameters
    public class RefReadonlyTest : ITest
	{
		public void DoTest()
		{
			var data = new MyStruct(3, 4);
			DoIn(ref data);
			DoRefRe(in data);
		}

		public void DoIn(in MyStruct myStruct)
		{
			// do something
		}
		public void DoRefRe(ref readonly MyStruct myStruct)
		{
			// do something
		}

	}

	public struct MyStruct(int x, int y)
	{
		public int R2 => x * x + y * y;
	}

}
