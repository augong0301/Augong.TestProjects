using OpenCvSharp;
using System.Diagnostics;
using System.Windows;
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

		private async void StartTest_Click(object sender, RoutedEventArgs e)
		{
			await Task.Run(() => TestCLAHE());
		}

		private void TestCLAHE()
		{
			try
			{
				Mat src = new Mat("input.jpg", ImreadModes.Color);
				Mat dst = new Mat();

				CLAHE clahe = CLAHE.Create(2, new Size(8, 8));
				for (int i = 0; i < 1000; i++)
				{
					clahe.Apply(src, dst);
					Debug.WriteLine($"Applied CLAHE {i + 1} times");
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
	}
}