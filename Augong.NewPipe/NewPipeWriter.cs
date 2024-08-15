using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Augong.NewPipe
{
    public class NewPipeWriter(Pipe pipe) : PipeWriter
    {
        public override void Advance(int bytes)
        {
            pipe.Writer.Advance(bytes);
        }

        public override void CancelPendingFlush()
        {
            pipe.Writer.CancelPendingFlush();
        }

        public override void Complete(Exception? exception = null)
        {
            pipe.Writer.Complete(exception);
        }

        public override ValueTask<FlushResult> FlushAsync(CancellationToken cancellationToken = default)
        {
            return pipe.Writer.FlushAsync(cancellationToken);
        }

        public override Memory<byte> GetMemory(int sizeHint = 0)
        {
            return pipe.Writer.GetMemory(sizeHint);
        }

        public override Span<byte> GetSpan(int sizeHint = 0)
        {
            return pipe.Writer.GetSpan(sizeHint);
        }
    }
}
