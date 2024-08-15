using System.Text;

namespace Augong.CSharp.ConsoleApp.TestClass.Init
{
	using System;
	using System.Runtime.InteropServices;

	public class Example
	{
		private static readonly Encoding Jis8Encoding;
		private static readonly Encoding GB2312Encoding;
		private static readonly Encoding Big5Encoding;


		private static readonly string staticField = InitializeStaticField();


		static Example()
		{
			if (!RuntimeInformation.FrameworkDescription.StartsWith(".NET Framework"))
			{
				Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
			}

		}

		public Example()
		{
			Console.WriteLine("实例构造函数被调用");
		}

		public Example(int i)
		{
			Console.WriteLine($"实例构造函数被调用 int = {i} + {staticField}");
		}

		public Example(string s)
		{
			Console.WriteLine($"实例构造函数被调用 string {s} + {staticField}");
		}

		private static string InitializeStaticField()
		{
			Console.WriteLine("静态字段初始化");
			return "Static Field";
		}

		private string InitializeInstanceField()
		{
			Console.WriteLine("实例字段初始化");
			return "Instance Field";
		}


	}

}
