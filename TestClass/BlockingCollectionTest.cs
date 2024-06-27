using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Augong.CSharp
{
	[MemoryDiagnoser]
	public class BlockingCollectionTest
	{
		public BlockingCollectionTest() { }

		private BlockingCollection<byte[]> blocks = new BlockingCollection<byte[]>();

		private ConcurrentQueue<byte[]> queue = new ConcurrentQueue<byte[]>();

		[Benchmark]
		public void DoTest()
		{
			for (int i = 0; i < 1024; i++)
			{
				var bytes = new byte[1024 * 1024];
				blocks.Add(bytes);
			}


			for (int i = 0; i < 1024; i++)
			{
				blocks.TryTake(out _);
			}

		}

		[Benchmark]
		public void DoQueueTest()
		{
			for (int i = 0; i < 1024; i++)
			{
				var bytes = new byte[1024 * 1024];
				queue.Enqueue(bytes);
			}

			Console.ReadKey();

			queue.Clear();
			queue = null;
			Console.ReadKey();

		}
	}
}
