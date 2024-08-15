using Augong.CSharp.Contract;

namespace Augong.CSharp.ConsoleApp.LinqTest
{
	public class LinqType
	{

		public LinqType()
		{

		}

		public string WaferType { get; set; }

		/// <summary>
		/// 定义的WaferType所在的程序集
		/// </summary>
		public string AssemblyInfo { get; set; }

		public long FeatureID { get; set; }

		public double Revision { get; set; } = 1.0;

		/// <summary>
		/// WaferType index
		/// </summary>
		public int Index { get; set; }
		public string Description { get; set; }
		public string GetDescription()
		{
			return WaferType;
		}




	}

	public class LinqTest : ITest
	{
		public LinqTest() { }


		public void DoTest()
		{
			var waferType = "new";
			var t = new List<LinqType>();
			for (int i = 0; i < 4; i++)
			{
				t.Add(new LinqType() { Description = $"{i}", WaferType = $"{i}" });
			}


			var c = t.Where(t => t.WaferType == waferType).FirstOrDefault()?.GetDescription() ?? " exception";
			var cc = t.Where(t => t.WaferType == waferType).FirstOrDefault()?.Description ?? " description";
			Console.WriteLine($"result is {c}");
			Console.WriteLine($"ds is {cc}");
			Console.ReadKey();
		}

	}
}
