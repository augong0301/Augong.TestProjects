#define mem
using Augong.SocketTest;
using Augong.StringTest;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Runtime.InteropServices;

internal class Program
{
	private static void Main(string[] args)
	{
		DoReadOnlyServerTest();
	}

	private void DoStringTest()
	{
		var st = new StringTester();
		st.DoTest();
		Console.ReadKey();
	}

	private static void DoReadOnlyServerTest()
	{
		var cl = new ReadOnlyClient();
		string input = string.Empty;
		Console.WriteLine("ip =");
		var ip = Console.ReadLine();
		Console.WriteLine("address =");
		var address = Console.ReadLine();
		var suc = cl.Connect(ip, int.Parse(address));
		Console.WriteLine($"Connected is {suc}");
        Console.WriteLine("Enter your commands");
		while (input != null)
		{
			input = Console.ReadLine();
			cl.DoSend(input);

			bool end = false;
			int count = 0;
			var lines = new List<string>();
			while (count < 3)
			{
				var rtn = cl.DoReceive();
				count++;
				end = rtn.Contains("MFOCUS");
				var l = StringHelper.ResortByLine(rtn);
				lines.AddRange(l);
			}
			foreach (var l in lines)
			{
				Console.WriteLine(l);
			}
			Console.WriteLine($"Receive data done by{count}");
		}

		Console.ReadKey();
	}

	private static void DoMemTest()
	{


		var dictionary = new ConcurrentDictionary<int, byte[]>();
		for (int i = 0; i < 1024 * 5; i++)
		{
			var data = new byte[1024 * 1024];
			dictionary.TryAdd(i, data);
		}

		for (int i = 0; i < 1024; i++)
		{
			dictionary.TryRemove(i, out _);
		}


		// 当引用结束时，largeData数组仍然存在，直到它不再被引用并被垃圾回收
		Console.ReadKey();
		dictionary.Clear();
		dictionary = null;
		dictionary = new ConcurrentDictionary<int, byte[]>();

		//var origin = new Dictionary<int, byte[]>();
		//for (int i = 0; i < 1024; i++)
		//{
		//	var data = new byte[1024 * 1024];
		//	origin.TryAdd(i, data);
		//}

		//for (int i = 0; i < 1024; i++)
		//{
		//	origin.Remove(i, out _);
		//}
		//Console.ReadKey();
		//origin.Clear();
		Console.ReadKey();


	}

	private static void DoTest(long p = 1024 * 1024 * 10, long r = 1024 * 1024 * 5, int minimumSegmentSize = 1024 * 1024)
	{
		// 不用同步上下文
		var op = new PipeOptions(pauseWriterThreshold: p, resumeWriterThreshold: r, minimumSegmentSize: minimumSegmentSize);
		//var op = new PipeOptions(useSynchronizationContext: false);
		var pipe = new Pipe(op);
		int sizeKb = 1024;
		int iteration = 1024;
		var cts = new CancellationTokenSource();
		var ct = cts.Token;

		Console.WriteLine($"byte array size of {sizeKb} Kb, iteration of {iteration}, total in {sizeKb * iteration / 1024} Mb");

		DateTime now = DateTime.Now;
		Thread writeThd = new Thread(new ThreadStart(() => WriteData(pipe, sizeKb, iteration, ct)));
		writeThd.Start();

		Thread dataProcess = new Thread(new ThreadStart(() => ReadData(pipe, now, iteration, p, r)));
		dataProcess.Start();

	}

	private static void WriteData(Pipe pipe, int sizeKb, int iteration, CancellationToken ct)
	{
		WriteDataAsync(pipe, sizeKb, iteration, ct).GetAwaiter().GetResult();
	}

	private static async Task WriteDataAsync(Pipe pipe, int sizeKb, int iteradtion, CancellationToken ct)
	{
		var writer = pipe.Writer;
		var kilobyte = new byte[1024 * sizeKb];
		kilobyte[8] = 1;
		kilobyte[10] = 2;

		int length = kilobyte.Length;
		// write length
		BinaryPrimitives.WriteInt32LittleEndian(kilobyte.AsSpan(0, 4), length);
		int count = 0;

		while (count < iteradtion)
		{
			if (ct.IsCancellationRequested)
			{
				Console.WriteLine($"Write data index of {count}");

				break;
			}
			try
			{
				var sw = new Stopwatch();
				sw.Restart();
				BinaryPrimitives.WriteInt32LittleEndian(kilobyte.AsSpan(4, 4), count);

#if mem
				var memory = writer.GetMemory(1024 * sizeKb);
				kilobyte.CopyTo(memory);
				writer.Advance(1024 * sizeKb);
				await writer.FlushAsync();
				sw.Stop();
				//Console.WriteLine($"write = {count}");
				//Console.WriteLine($"Writing speed is  {sizeKb / sw.ElapsedMilliseconds * 1000 / 1024} MB/s");

#else
				sw.Start();

				// write index 
				BinaryPrimitives.WriteInt32LittleEndian(kilobyte.AsSpan(kilobyte.Length - 4, 4), count);

				await writer.WriteAsync(kilobyte);
				sw.Stop();
				Console.WriteLine($"writeasync cost {sw.ElapsedTicks} ticks");
#endif

			}
			catch (Exception ex)
			{

				break;
			}

			count++;
		}
		writer.Complete();
	}

	private static void ReadData(Pipe pipe, DateTime now, int iteration, long p, long r)
	{
		ReadDataAsync(pipe).GetAwaiter().GetResult();
		Console.WriteLine($"{iteration} tasks of p:{p / 1024 / 1024} r:{r / 1024 / 1024} done by {(DateTime.Now - now).TotalMilliseconds} ms , average is {(DateTime.Now - now).TotalMilliseconds / iteration}");
	}

	private static async Task ReadDataAsync(Pipe pipe)
	{
		var reader = pipe.Reader;
		var sw = new Stopwatch();
		while (true)
		{
			sw.Restart();
			var result = await reader.ReadAsync();
			var buffer = result.Buffer;
			SequencePosition consumed = buffer.Start;
			SequencePosition examined = buffer.End;
			if (result.IsCompleted && buffer.Length == 0)
			{
				break;
			}
			//do
			//{
			//bool readDone = GetPosition(ref buffer, ref pos, out var data);
			//if (readDone && pos != null)
			//{
			//	ProcessData(data, pos.Value);

			//}

			//} while (pos != null);

			try
			{
				Processbuffer(in buffer, out consumed, out examined, reader);
			}
			finally
			{
				reader.AdvanceTo(buffer.End);
			}
			sw.Stop();
		}
		reader.Complete();
	}

	private static void Processbuffer(in ReadOnlySequence<byte> buffer, out SequencePosition consumed, out SequencePosition examined, PipeReader reader)
	{
		consumed = buffer.Start;
		examined = buffer.End;
		int loc = 0;

		var ms = MemoryPool<short>.Shared;

		//Thread.Sleep(1);
		if (buffer.Length == 0)
		{
			return;
		}

		if (buffer.IsSingleSegment)
		{
			var span = buffer.FirstSpan;
			using var mo = ms.Rent(span.Length);
			var shortArr = mo.Memory.Span;

			ProcessData(in span, shortArr, loc);
			// get buffer length
			var lengthMem = span.Slice(0, 8);

			int length = BitConverter.ToInt32([lengthMem[0], lengthMem[1], lengthMem[2], lengthMem[3]], 0);
			int index = BitConverter.ToInt32([lengthMem[4], lengthMem[5], lengthMem[6], lengthMem[7]], 0);
			//Console.WriteLine($"read index = {index}");

			consumed = buffer.GetPosition(buffer.Length);

			examined = consumed;
		}
		else
		{
			foreach (var seg in buffer)
			{
				var span = seg.Span;
				using var mo = ms.Rent(span.Length);
				var shortArr = mo.Memory.Span;

				var lengthMem = span.Slice(0, 8);
				int length = BitConverter.ToInt32([lengthMem[0], lengthMem[1], lengthMem[2], lengthMem[3]], 0);
				int index = BitConverter.ToInt32([lengthMem[4], lengthMem[5], lengthMem[6], lengthMem[7]], 0);
				//Console.WriteLine($"read index = {index}");
				ProcessData(in span, shortArr, loc);
				MemoryMarshal.TryGetArray<short>(mo.Memory, out var segment);
				//var sts = ToShortArray(shortArr);
				ProcessShort(segment);
				//loc += seg.Length;
				consumed = buffer.GetPosition(seg.Length);
			}

		}
	}

	private static void ProcessShort(ArraySegment<short> shortArr)
	{
		//var data = shortArr.ToArray();
		var data = new short[shortArr.Count];

		Buffer.BlockCopy(shortArr.Array, 4, data, 0, shortArr.Count);
		unsafe
		{
			fixed (short* src = data)
			{

			}
		}
	}

	private static void ProcessData(in ReadOnlySpan<byte> span, Span<short> shortArr, int loc)
	{
		if (span.Length == 0)
		{
			return;
		}

		unsafe
		{
			fixed (byte* bytesss = &MemoryMarshal.GetReference(span))
			fixed (short* shortPtr = &shortArr[0])
			{
				short* shortArrPtr = shortPtr;
				for (int i = 0; i < span.Length / 2; i++)
				{
					shortArrPtr[i] = *(short*)(bytesss + 8 + i * 2);
				}
			}
		}
	}


	private static bool GetPosition(ref ReadOnlySequence<byte> buffer, ref SequencePosition? pos, out ReadOnlySequence<byte> data)
	{
		data = default;

		#region MyRegion
		pos = null;
		if (buffer.Length < 4)
		{
			return false;
		}

		SequenceReader<byte> reader = new SequenceReader<byte>(buffer.Slice(0, 4));

		if (reader.TryReadLittleEndian(out int length))
		{
			SequenceReader<byte> index = new SequenceReader<byte>(buffer.Slice(length - 4, 4));
			index.TryReadLittleEndian(out int ind);
			//Console.WriteLine($"Read data index of {ind}");

			pos = buffer.GetPosition(length);
			buffer = buffer.Slice(length);
			return true;
		}

		return false;
		#endregion


		//data = default;
		//pos = null;

		//if (buffer.Length < 4)
		//{
		//	return false;
		//}

		//var first = buffer.First.Span;

		//if (!BitConverter.IsLittleEndian)
		//{
		//	Array.Reverse(first.Slice(0, 4).ToArray());
		//}

		//int length = BitConverter.ToInt32(first.Slice(0, 4).ToArray());

		//if (buffer.Length < length)
		//{
		//	return false;
		//}

		//var slice = buffer.Slice(length - 4, 4);
		//var sliceFirst = slice.First.Span;
		////Console.WriteLine($"{sliceFirst.ToArray()[0]} {sliceFirst.ToArray()[1]} {sliceFirst.ToArray()[2]} {sliceFirst.ToArray()[3]}");
		//if (!BitConverter.IsLittleEndian)
		//{
		//	Array.Reverse(sliceFirst.ToArray());
		//}
		////Console.WriteLine($"{sliceFirst.ToArray()[0]} {sliceFirst.ToArray()[1]} {sliceFirst.ToArray()[2]} {sliceFirst.ToArray()[3]}");
		//int ind = BitConverter.ToInt32(sliceFirst.ToArray());

		////Console.WriteLine($"Read data index of {ind}");

		//pos = buffer.GetPosition(length);
		//data = buffer.Slice(0, length);
		//buffer = buffer.Slice(length);

		//return true;
	}
}