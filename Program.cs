using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics;
using System.IO.Pipelines;

internal class Program
{
	private static void Main(string[] args)
	{
		var pipe = new Pipe();
		int sizeKb = 1000;
		int iteration = 1024 * 16;
		Console.WriteLine($"byte array size of {sizeKb} Kb, iteration of {iteration}, total in {sizeKb * iteration / 1024} Mb");

		DateTime now = DateTime.Now;
		Thread writeThd = new Thread(new ThreadStart(() => WriteData(pipe, sizeKb, iteration)));
		writeThd.Start();

		Thread dataProcess = new Thread(new ThreadStart(() => ReadData(pipe, now)));
		dataProcess.Start();

		Console.ReadLine();

		GC.Collect(2);
		GC.WaitForPendingFinalizers();

		Console.ReadLine();


	}

	private static void ReadData(Pipe pipe, DateTime now)
	{
		ReadDataAsync(pipe).GetAwaiter().GetResult();
		Console.WriteLine($"All task done by {(DateTime.Now - now).TotalMilliseconds} ms");
	}

	private static void WriteData(Pipe pipe, int sizeKb, int iteration)
	{
		WriteDataAsync(pipe, sizeKb, iteration).GetAwaiter().GetResult();
	}

	private static async Task WriteDataAsync(Pipe pipe, int sizeKb, int iteradtion)
	{
		var writer = pipe.Writer;
		var kilobyte = new byte[1024 * sizeKb];
		int length = kilobyte.Length;
		// write length
		BinaryPrimitives.WriteInt32LittleEndian(kilobyte.AsSpan(0, 4), length);

		int count = 0;
		while (count < iteradtion)
		{
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
				bool readDone = GetPosition(ref buffer, ref pos);
				if (readDone && pos != null)
				{
					ProcessData(buffer, pos.Value);
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
	}

	private static bool GetPosition(ref ReadOnlySequence<byte> buffer, ref SequencePosition? pos)
	{
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
	}
}