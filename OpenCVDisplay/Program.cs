using Augong.OpenCVDisplay;

internal class Program
{
	private static string Desktop => Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

	private static void Main(string[] args)
	{
		var hist = new HistTest(Path.Combine(Desktop, "test.png"));
		hist.DoTest();
	}
}