using Augong.CSharp.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Augong.ConsoleApp.LazyTest
{
	internal class LazyTest : ITest
	{
		public void DoTest()
		{
			var _lazyInstance = new Lazy<LazyObject>(() => new LazyObject());

			Console.WriteLine("程序开始");

			// 在这里不会初始化 MyClass，只有调用 _lazyInstance.Value 时才会初始化
			Console.WriteLine("访问 Lazy 实例之前");

			// 触发 Lazy 初始化
			LazyObject lzObject = _lazyInstance.Value;
			Console.WriteLine("获取 LazyObject 的值: " + lzObject.GetValue());

			Console.WriteLine("程序结束");
		}
	}

	internal class LazyObject
	{
		private int _value;

		public LazyObject()
		{
			Console.WriteLine("LazyObject 初始化");
			_value = 42;
		}

		public int GetValue()
		{
			return _value;
		}
	}
}
