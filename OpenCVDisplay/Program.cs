using OpenCvSharp;

internal class Program
{
	private static string Desktop => Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

	private static void Main(string[] args)
	{
		bool stop = false;
		int count = 0;
		while (!stop)
		{
			try
			{
				var file = Path.Combine(Desktop, "test.png");
				var file1 = Path.Combine(Desktop, "1.png");

				var writeTask = Task.Run(() =>
				{
					var png = Cv2.ImRead(file1, ImreadModes.Grayscale);
					Cv2.ImWrite(file, png);
				});


				var readTask = Task.Run(() =>
				{
					var read = Cv2.ImRead(file, ImreadModes.Grayscale);
					if (read.Height == 0 && read.Width == 0)
					{
						stop = true;
					}
				});
				Task.WhenAll(writeTask, readTask).Wait();
				Console.WriteLine($"Loop {count}, stop = {stop}");
				Thread.Sleep(1000);
			}
			catch (Exception ex)
			{
				throw;
			}
		}
	}
}