using System.IO.Pipelines;

namespace Augong.NewPipe
{
    public class NewPipeReader(Pipe pipe) : PipeReader
    {

        public override void AdvanceTo(SequencePosition consumed)
        {
            pipe.Reader.AdvanceTo(consumed);
        }

        public override void AdvanceTo(SequencePosition consumed, SequencePosition examined)
        {
            pipe.Reader.AdvanceTo(consumed, examined);
        }

        public override void CancelPendingRead()
        {
            pipe.Reader.CancelPendingRead();
        }

        //
        public override void Complete(Exception? exception = null)
        {
            var c = pipe.Reader.CompleteAsync(exception);

            pipe.Reader.Complete(exception);
        }

        public override ValueTask<ReadResult> ReadAsync(CancellationToken cancellationToken = default)
        {
            return pipe.Reader.ReadAsync(cancellationToken);
        }

        public override bool TryRead(out ReadResult result)
        {
            return pipe.Reader.TryRead(out result);
        }

    }
}
