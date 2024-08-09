using Augong.CSharp.Contract;
using System.Buffers.Binary;
using System.Buffers;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Runtime.InteropServices;

namespace Augong.NewPipe
{
    public class NewPipeTest : ITest
    {
        public CancellationTokenSource cts = new CancellationTokenSource();

        private NewPipe Pipe;

        public void DoTest()
        {
            StartPipes();
        }

        private void StartPipes(long p = 1024 * 1024 * 10, long r = 1024 * 1024 * 5, int minimumSegmentSize = 1024 * 1024)
        {
            // 不用同步上下文
            var op = new PipeOptions(pauseWriterThreshold: 0, resumeWriterThreshold: -1, minimumSegmentSize: 1024 * 1024);
            //var op = new PipeOptions(useSynchronizationContext: false);
            var pipe = new Pipe(op);
            Pipe = new NewPipe(pipe);
            int sizeKb = 1024;
            int iteration = 1024;

            Console.WriteLine($"byte array size of {sizeKb} Kb, iteration of {iteration}, total in {sizeKb * iteration / 1024} Mb");

            DateTime now = DateTime.Now;
            Thread writeThd = new Thread(new ThreadStart(() => WriteData(Pipe, sizeKb, iteration, cts.Token)));
            writeThd.Start();

            Thread dataProcess = new Thread(new ThreadStart(() => ReadData(Pipe, now, iteration, p, r)));
            dataProcess.Start();
            var c = Console.ReadKey();
            if (c.Key.ToString() != null)
            {
                cts.Cancel();
                Pipe.Reader.CancelPendingRead();
                Pipe.Reader.Complete();
                Pipe.Reset();
            }

            Console.ReadKey();
        }

        private void WriteData(NewPipe pipe, int sizeKb, int iteration, CancellationToken ct)
        {
            WriteDataAsync(pipe, sizeKb, iteration, ct).GetAwaiter().GetResult();
        }

        private async Task WriteDataAsync(NewPipe pipe, int sizeKb, int iteration, CancellationToken ct)
        {
            var writer = pipe.Writer;
            var kilobyte = new byte[1024 * sizeKb];
            kilobyte[8] = 1;
            kilobyte[10] = 2;

            int length = kilobyte.Length;
            // write length
            BinaryPrimitives.WriteInt32LittleEndian(kilobyte.AsSpan(0, 4), length);
            int count = 0;

            while (count < iteration)
            {
                if (count == 10)
                {
                    //cts.Cancel();
                }
                if (ct.IsCancellationRequested)
                {
                    Console.WriteLine($"IsCancellationRequested index of {count}");

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
                    Thread.Sleep(50);

                    sw.Stop();
                    Console.WriteLine($"writeasync {count} cost {sw.ElapsedTicks} ticks");
#endif

                }
                catch (Exception ex)
                {

                    break;
                }

                count++;
            }
        }

        private void ReadData(NewPipe pipe, DateTime now, int iteration, long p, long r)
        {
            ReadDataAsync(pipe).GetAwaiter().GetResult();
            Console.WriteLine($"{iteration} tasks of p:{p / 1024 / 1024} r:{r / 1024 / 1024} done by {(DateTime.Now - now).TotalMilliseconds} ms , average is {(DateTime.Now - now).TotalMilliseconds / iteration}");
        }

        private async Task ReadDataAsync(NewPipe pipe)
        {
            var reader = pipe.Reader;
            var sw = new Stopwatch();
            var c = new object();
            var count = 0;

            while (!cts.IsCancellationRequested)
            {
                try
                {
                    sw.Restart();
                    var result = await reader.ReadAsync(cts.Token);
                    var buffer = result.Buffer;
                    c = buffer;
                    SequencePosition consumed = buffer.Start;
                    SequencePosition examined = buffer.End;
                    if (result.IsCompleted || buffer.Length == 0 || result.IsCanceled)
                    {
                        Console.WriteLine("read break IsCompleted or IsCanceled");
                        break;
                    }

                    Processbuffer(in buffer, out consumed, out examined, reader);
                    if (buffer.Length == 0)
                        break;

                    Task.Run(() =>
                    {
                        reader.AdvanceTo(buffer.End);
                        Console.WriteLine($"read count = {count}");
                    }, cts.Token).ConfigureAwait(false).GetAwaiter().GetResult();

                    count++;
                    sw.Stop();
                }
                catch (OperationCanceledException ex)
                {
                    Console.WriteLine("Abort task");
                }
                catch (Exception)
                {
                    break;
                }
            }
            var t = 1;
        }

        private void Processbuffer(in ReadOnlySequence<byte> buffer, out SequencePosition consumed, out SequencePosition examined, PipeReader reader)
        {
            Thread.Sleep(1);
            consumed = buffer.Start;
            examined = buffer.End;
            return;
            int loc = 0;

            var ms = MemoryPool<short>.Shared;

            Thread.Sleep(10);

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
                    if (seg.Length == 0)
                    {
                        return;
                    }
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

        private void ProcessShort(ArraySegment<short> shortArr)
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

        private void ProcessData(in ReadOnlySpan<byte> span, Span<short> shortArr, int loc)
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

    }
}
