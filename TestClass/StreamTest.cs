using Microsoft.IO;
using System.IO.Compression;
using System.Threading.Channels;

namespace PipelineTest
{
	public class StreamTest
	{

		private RecyclableMemoryStreamManager manager = new RecyclableMemoryStreamManager();

		private CompressionLevel compressionLevel = CompressionLevel.Fastest;

		private MemoryStream mStream;

		public Stream GetStream() => mStream;

		public int DataSize = 0;

		public StreamTest()
		{
			mStream = manager.GetStream();
			DoExecutePacking();
		}


		public void DoExecutePacking()
		{
			var stream = manager.GetStream();

			using (var archive = new ZipArchive(mStream, ZipArchiveMode.Create, true))
			{
				var frame = new byte[1014 * 1024];


				string entryName = $"image.bmp";
				var entry = archive.CreateEntry(entryName, compressionLevel);

				using (var entryStream = entry.Open())
				{
					//var bytes = frame.Image;
					entryStream.Write(frame, 0, frame.Length);
				}
			}

			mStream.Seek(0, SeekOrigin.Begin);

			DataSize = (int)mStream.Length;


		}
	}
}
