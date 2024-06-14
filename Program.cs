#define mem
using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Runtime.InteropServices;
using System.Text;

internal class Program
{
	private static void Main(string[] args)
	{
		var op = new PipeOptions(pauseWriterThreshold: 1024 * 1024 * 4 * 50, resumeWriterThreshold: 1024 * 1024 * 4 * 20);
		var pipe = new Pipe(op);
		int sizeKb = 1024 * 4;
		int iteration = 1024;
		var cts = new CancellationTokenSource();
		var ct = cts.Token;

		Console.WriteLine($"byte array size of {sizeKb} Kb, iteration of {iteration}, total in {sizeKb * iteration / 1024} Mb");

		DateTime now = DateTime.Now;
		Thread writeThd = new Thread(new ThreadStart(() => WriteData(pipe, sizeKb, iteration, ct)));
		writeThd.Start();

		Thread dataProcess = new Thread(new ThreadStart(() => ReadData(pipe, now, iteration)));
		dataProcess.Start();

		if (Console.ReadLine() == string.Empty)
		{
			cts.Cancel();
		}

		GC.Collect(2);

		GC.WaitForPendingFinalizers();

		Console.ReadLine();
	}



	private static void WriteData(Pipe pipe, int sizeKb, int iteration, CancellationToken ct)
	{
		WriteDataAsync(pipe, sizeKb, iteration, ct).GetAwaiter().GetResult();
	}

	private static async Task WriteDataAsync(Pipe pipe, int sizeKb, int iteradtion, CancellationToken ct)
	{
		var writer = pipe.Writer;
		var kilobyte = new byte[1024 * sizeKb];
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
				Console.WriteLine($"write = {count}");
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

	private static void ReadData(Pipe pipe, DateTime now, int iteration)
	{
		ReadDataAsync(pipe).GetAwaiter().GetResult();
		Console.WriteLine($"{iteration} tasks done by {(DateTime.Now - now).TotalMilliseconds} ms , average is {(DateTime.Now - now).TotalMilliseconds / iteration}");
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
				Processbuffer(in buffer, out consumed, out examined,reader);
			}
			finally
			{
				reader.AdvanceTo(consumed);
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


		if (buffer.Length == 0)
		{
			return;
		}

		if (buffer.IsSingleSegment)
		{
			var span = buffer.FirstSpan;
			using var mo = ms.Rent((int)buffer.Length / 2);
			var shortArr = mo.Memory.Span;

			ProcessData(in span, shortArr, loc);
			// get buffer length
			var lengthMem = span.Slice(0, 8);
			//if (!BitConverter.IsLittleEndian)
			//{
			//	Array.Reverse(lengthMem.Slice(0, 4).ToArray());
			//}
			int length = BitConverter.ToInt32([lengthMem[0], lengthMem[1], lengthMem[2], lengthMem[3]], 0);
			int index = BitConverter.ToInt32([lengthMem[4], lengthMem[5], lengthMem[6], lengthMem[7]], 0);
			Console.WriteLine($"read index = {index}");

			consumed = buffer.End;
			examined = consumed;
		}
		else
		{
			foreach (var seg in buffer)
			{
				using var mo = ms.Rent((int)buffer.Length / 2);
				var shortArr = mo.Memory.Span;
				var span = seg.Span;

				var lengthMem = span.Slice(0, 8);

				int length = BitConverter.ToInt32([lengthMem[0], lengthMem[1], lengthMem[2], lengthMem[3]], 0);
				int index = BitConverter.ToInt32([lengthMem[4], lengthMem[5], lengthMem[6], lengthMem[7]], 0);
				Console.WriteLine($"read index = {index}");
				ProcessData(in span, shortArr, loc);
				loc += seg.Length;
				consumed = buffer.GetPosition(seg.Length);
				reader.AdvanceTo(consumed);
			}

		}
	}

	private static void ProcessData(in ReadOnlySpan<byte> span, Span<short> shortArr, int loc)
	{
		if (span.Length == 0)
		{
			return;
		}

		if (loc < 0 || loc + span.Length / 2 > shortArr.Length)
		{
			throw new ArgumentOutOfRangeException(nameof(loc), "The location is out of range.");
		}

		unsafe
		{
			fixed (byte* bytesss = &MemoryMarshal.GetReference(span))
			fixed (short* shortPtr = &shortArr[loc])
			{
				short* shortArrPtr = shortPtr;
				for (int i = 0; i < span.Length / 2; i++)
				{
					shortArrPtr[i] = *(short*)(bytesss + i * 2);
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