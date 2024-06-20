using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipelineTest
{
	public class TriTest
	{

		public TriTest() { }

		public async Task Run()
		{
			int dataSize = 10 * 1024 * 1024; // 10 MB data
			int iterations = 10000; // Number of iterations for the test

			Console.WriteLine($"Data bytes of {dataSize / 1024 / 1024} MB, iteration of {iterations}");
			// Generate test data
			byte[] testData = new byte[dataSize];
			new Random().NextBytes(testData);

			// Test ConcurrentQueue
			var concurrentQueueTime = await TestConcurrentQueueAsync(testData, iterations);
			Console.WriteLine($"ConcurrentQueue Time: {concurrentQueueTime} ms");

			// Test Pipelines
			var pipelinesTime = await TestPipelinesAsync(testData, iterations);
			Console.WriteLine($"Pipelines Time: {pipelinesTime} ms");

			// Test ArrayPool
			var arrayPoolTime = await TestArrayPoolAsync(testData, iterations);
			Console.WriteLine($"ArrayPool Time: {arrayPoolTime} ms");

			Console.ReadKey();
		}

		protected async Task<long> TestConcurrentQueueAsync(byte[] testData, int iterations)
		{
			var queue = new ConcurrentQueue<byte[]>();
			var sw = new Stopwatch();
			sw.Start();

			// Writer task
			var writerTask = Task.Run(() =>
			{
				for (int i = 0; i < iterations; i++)
				{
					queue.Enqueue((byte[])testData.Clone());
				}
			});

			// Reader task
			var readerTask = Task.Run(() =>
			{
				for (int i = 0; i < iterations; i++)
				{
					while (!queue.TryDequeue(out _)) { }
				}
			});

			await Task.WhenAll(writerTask, readerTask);

			sw.Stop();
			return sw.ElapsedMilliseconds;
		}

		protected async Task<long> TestPipelinesAsync(byte[] testData, int iterations)
		{
			var pipe = new Pipe();
			var sw = new Stopwatch();
			sw.Start();

			// Writer task
			var writerTask = Task.Run(async () =>
			{
				for (int i = 0; i < iterations; i++)
				{
					Memory<byte> memory = pipe.Writer.GetMemory(testData.Length);
					testData.CopyTo(memory);
					pipe.Writer.Advance(testData.Length);

					FlushResult result = await pipe.Writer.FlushAsync();
					if (result.IsCompleted)
					{
						break;
					}
				}
				pipe.Writer.Complete();
			});

			// Reader task
			var readerTask = Task.Run(async () =>
			{
				for (int i = 0; i < iterations; i++)
				{
					ReadResult result = await pipe.Reader.ReadAsync();
					ReadOnlySequence<byte> buffer = result.Buffer;

					// Process the buffer
					buffer = buffer.Slice(buffer.End);

					pipe.Reader.AdvanceTo(buffer.Start, buffer.End);

					if (result.IsCompleted)
					{
						break;
					}
				}
				pipe.Reader.Complete();
			});

			await Task.WhenAll(writerTask, readerTask);

			sw.Stop();
			return sw.ElapsedMilliseconds;
		}

		public async Task<long> TestArrayPoolAsync(byte[] testData, int iterations)
		{
			var pool = ArrayPool<byte>.Shared;
			var sw = new Stopwatch();
			sw.Start();

			// Writer task
			var writerTask = Task.Run(() =>
			{
				for (int i = 0; i < iterations; i++)
				{
					byte[] rentedArray = pool.Rent(testData.Length);
					Array.Copy(testData, rentedArray, testData.Length);

					// Simulate processing
					pool.Return(rentedArray);
				}
			});

			// Reader task
			var readerTask = Task.Run(() =>
			{
				for (int i = 0; i < iterations; i++)
				{
					byte[] rentedArray = pool.Rent(testData.Length);

					// Simulate processing
					pool.Return(rentedArray);
				}
			});

			await Task.WhenAll(writerTask, readerTask);

			sw.Stop();
			return sw.ElapsedMilliseconds;
		}
	}
}
