using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Runtime.InteropServices;

internal class Program
{
	private static void Main(string[] args)
	{
		var pipe = new Pipe();
		int sizeKb = 1000 *10 ;
		int iteration = 1024;
		var cts = new CancellationTokenSource();
		var ct = cts.Token;

		Console.WriteLine($"byte array size of {sizeKb} Kb, iteration of {iteration}, total in {sizeKb * iteration / 1024} Mb");

		DateTime now = DateTime.Now;
		Thread writeThd = new Thread(new ThreadStart(() => WriteData(pipe, sizeKb, iteration, ct)));
		writeThd.Start();

		Thread dataProcess = new Thread(new ThreadStart(() => ReadData(pipe, now)));
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
				// write index 
				BinaryPrimitives.WriteInt32LittleEndian(kilobyte.AsSpan(kilobyte.Length - 4, 4), count);

				await writer.WriteAsync(kilobyte);

			}
			catch (Exception ex)
			{

				break;
			}

			count++;
		}
		writer.Complete();
	}

	private static void ReadData(Pipe pipe, DateTime now)
	{
		ReadDataAsync(pipe).GetAwaiter().GetResult();
		Console.WriteLine($"All task done by {(DateTime.Now - now).TotalMilliseconds} ms");
	}

	private static async Task ReadDataAsync(Pipe pipe)
	{
		var reader = pipe.Reader;
		while (true)
		{
			var result = await reader.ReadAsync();
			var buffer = result.Buffer;
			SequencePosition? pos = null;

			do
			{
				bool readDone = GetPosition(ref buffer, ref pos, out var data);
				if (readDone && pos != null)
				{
					ProcessData(data, pos.Value);

				}
			} while (pos != null);

			reader.AdvanceTo(buffer.Start, buffer.End);

			if (result.IsCompleted)
			{
				break;
			}

		}
		reader.Complete();
	}

	private static void ProcessData(ReadOnlySequence<byte> buffer, SequencePosition value)
	{
		//short[] data = new short[buffer.Length / 2];
		//Buffer.BlockCopy(buffer.ToArray(), 0, data, 0, (int)buffer.Length / 2);


		int length = (int)buffer.Length / 2;
		short[] data = new short[length];

		Span<byte> byteSpan = buffer.ToArray().AsSpan();
		Span<short> shortSpan = MemoryMarshal.Cast<byte, short>(byteSpan);

		shortSpan.Slice(0, length).CopyTo(data);
	}

	private static bool GetPosition(ref ReadOnlySequence<byte> buffer, ref SequencePosition? pos, out ReadOnlySequence<byte> data)
	{
		#region MyRegion
		//pos = null;
		//if (buffer.Length < 4)
		//{
		//	return false;
		//}

		//SequenceReader<byte> reader = new SequenceReader<byte>(buffer.Slice(0, 4));


		//if (reader.TryReadLittleEndian(out int length))
		//{
		//	SequenceReader<byte> index = new SequenceReader<byte>(buffer.Slice(length - 4, 4));
		//	index.TryReadLittleEndian(out int ind);
		//	//Console.WriteLine($"Read data index of {ind}");

		//	pos = buffer.GetPosition(length);
		//	buffer = buffer.Slice(length);
		//	return true;
		//}

		//return false;
		#endregion


		data = default;
		pos = null;

		if (buffer.Length < 4)
		{
			return false;
		}

		var first = buffer.First.Span;

		if (!BitConverter.IsLittleEndian)
		{
			Array.Reverse(first.Slice(0, 4).ToArray());
		}

		int length = BitConverter.ToInt32(first.Slice(0, 4).ToArray());

		if (buffer.Length < length)
		{
			return false;
		}

		var slice = buffer.Slice(length - 4, 4);
		var sliceFirst = slice.First.Span;
		//Console.WriteLine($"{sliceFirst.ToArray()[0]} {sliceFirst.ToArray()[1]} {sliceFirst.ToArray()[2]} {sliceFirst.ToArray()[3]}");
		if (!BitConverter.IsLittleEndian)
		{
			Array.Reverse(sliceFirst.ToArray());
		}
		//Console.WriteLine($"{sliceFirst.ToArray()[0]} {sliceFirst.ToArray()[1]} {sliceFirst.ToArray()[2]} {sliceFirst.ToArray()[3]}");
		int ind = BitConverter.ToInt32(sliceFirst.ToArray());

		//Console.WriteLine($"Read data index of {ind}");

		pos = buffer.GetPosition(length);
		data = buffer.Slice(0, length);
		buffer = buffer.Slice(length);

		return true;
	}
}