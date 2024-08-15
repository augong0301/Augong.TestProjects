using Augong.CSharp.Contract;
namespace Augong.CSharp.ConsoleApp.BasicTests
{
	public class ParamsTest : ITest
	{
		public void DoTest()
		{
			SetNoParams();
		}

		public void SetNoParams(params string[] input)
		{
			if (input != null)
			{
				Console.WriteLine("Input is null");
				Console.WriteLine($"Input Length = {input.Length}");
			}
		}
	}
}
