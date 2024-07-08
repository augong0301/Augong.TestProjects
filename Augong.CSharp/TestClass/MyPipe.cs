using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Augong.CSharp
{
	public class MyPipe
	{
		private Pipe _pipe;

		public bool IsCompleted { get; private set; }

		public MyPipe()
		{
			_pipe = new Pipe();
		}

		public PipeWriter Writer => _pipe.Writer;
		public PipeReader Reader => _pipe.Reader;

		public async Task WriteAsync(byte[] buffer)
		{
			await _pipe.Writer.WriteAsync(buffer);
		}

		public void CompleteWrite()
		{
			_pipe.Writer.Complete();
		}

		public async Task<ReadOnlySequence<byte>> ReadBufferAsync()
		{
			var result = await _pipe.Reader.ReadAsync();
			IsCompleted = result.IsCompleted;
			return result.Buffer;
		}

		public async Task WriteStream(MemoryStream stream)
		{
			stream.TryGetBuffer(out var c);
			await _pipe.Writer.WriteAsync(c.Array);
		}


	}
}
