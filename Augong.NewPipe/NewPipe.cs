using System.IO.Pipelines;

namespace Augong.NewPipe
{
    public class NewPipe
    {
        private readonly Pipe _pipe;
        public NewPipe(Pipe pipe)
        {
            _pipe = pipe;
            _reader = new NewPipeReader(pipe);
            _writer = new NewPipeWriter(pipe);
        }

        public NewPipeReader Reader => _reader;

        private NewPipeReader _reader;

        public NewPipeWriter Writer => _writer;

        private NewPipeWriter _writer;


        public void Reset()
        {
            try
            {
                _reader.Complete();
                _writer.Complete();
            }
            catch (Exception ex)
            {

            }
            finally
            {
                _pipe.Reset();
            }
        }
    }
}
