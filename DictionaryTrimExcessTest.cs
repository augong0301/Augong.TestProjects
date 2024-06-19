using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipelineTest
{
	[MemoryDiagnoser]
	public class DictionaryTrimExcessTest
	{
		public DictionaryTrimExcessTest() { }

		[Benchmark(Baseline = true)]
		public void DoTest()
		{
			var dictionary = new Dictionary<int, byte[]>();

			for (int i = 0; i < 1024; i++)
			{
				var bytes = new byte[1024 * 1024 * 1];
				dictionary.Add(i, bytes);
			}

			for (int i = 0; i < 512; i++)
			{
				dictionary.Remove(i);
			}

			dictionary.TrimExcess();
		}

		[Benchmark]
		public void DoTest1()
		{
			var dictionary = new Dictionary<int, byte[]>();

			for (int i = 0; i < 1024; i++)
			{
				var bytes = new byte[1024 * 1024 * 1];
				dictionary.Add(i, bytes);
			}


			for (int i = 0; i < 1024; i++)
			{
				dictionary.Remove(i);
			}
		}
	}
}
