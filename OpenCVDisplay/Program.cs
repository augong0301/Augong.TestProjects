using OpenCvSharp;

internal class Program
{
	private static void Main(string[] args)
	{
		var imgPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "test.png");
		var mat = Cv2.ImRead(imgPath, ImreadModes.AnyColor);

		using (new Window("Image", mat))
		{
			Cv2.WaitKey();
		}

		Console.ReadKey();
	}
}