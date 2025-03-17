using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System.Diagnostics;
using System.IO;
using System.Runtime;
using System.Windows;
using System.Windows.Media.Imaging;
using Size = OpenCvSharp.Size;

namespace OpenCVWpf
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : System.Windows.Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		public BitmapSource Bitmap { get; set; }

		private async void StartTest_Click(object sender, RoutedEventArgs e)
		{
			await Task.Run(() => TestSubMatrix());
		}

		private void TestCLAHE()
		{
			try
			{
				Mat src = new Mat(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "test", "test.png"), ImreadModes.Color);
				Mat dst = new Mat();

				CLAHE clahe = CLAHE.Create(10, new Size(8, 8));
				for (int i = 0; i < 10000; i++)
				{
					var limit = i % 2 == 0 ? 5 : 10;
					Thread.Sleep(10);
					clahe.ClipLimit = limit;
					clahe.Apply(src, dst);
					Debug.WriteLine($"Applied CLAHE {i + 1} times of {limit} ClipLimit");
				}
			}
			catch (AccessViolationException ex)
			{
				Debug.WriteLine($"AccessViolationException: {ex.Message}");
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Exception: {ex.Message}");
			}
		}

		private void TestSubMatrix()
		{
			var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "test", "test.png");
			var path1 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "test");

			Mat subMat = new Mat(new Size(4000, 4000), MatType.CV_8UC1);
			Debug.WriteLine("SubMat before releasing originalMat:");

			for (int i = 0; i < 3000; i += 100)
			{
				try
				{
					using var originalMat = new Mat(path, ImreadModes.Grayscale);
					subMat[new OpenCvSharp.Rect(i, i, 100, 100)] = originalMat[new OpenCvSharp.Rect(i, i, 100, 100)];
					subMat.SaveImage(Path.Combine(path1, $"{i}.png"));
					var c = subMat.Cols;
					var r = subMat.Rows;
					Debug.WriteLine($"subMat : cols {c} rows {r}");
					GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
					GC.Collect(2);
				}
				catch (Exception ex)
				{
					Debug.WriteLine($"Exception caught: {ex.Message}");
				}
			}
		}
	}
}