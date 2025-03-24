using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;

namespace OpenCVWpf
{
	public class MainViewModel : INotifyPropertyChanged
	{
		public MainViewModel()
		{
			Task.Run(() =>
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
						App.Current.Dispatcher.Invoke(() =>
						{
							Image = subMat.ToBitmapSource();
						});
					}
					catch (Exception ex)
					{
						Debug.WriteLine($"Exception caught: {ex.Message}");
					}
				}
			});
		}


		public event PropertyChangedEventHandler? PropertyChanged;

		public void NotifyChanged([CallerMemberName] string propName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
		}

		private BitmapSource _image;

		public BitmapSource Image
		{
			get { return _image; }
			set { _image = value; NotifyChanged(); }
		}



	}
}
