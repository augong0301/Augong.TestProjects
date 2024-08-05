using Augong.CSharp.Diagnostics;
using Augong.Math;
using Augong.Util;
using Microsoft.Win32;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.ImageSharp;
using OxyPlot.Series;
using SocketTest;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace Augong.UI
{
	public class MainViewModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler? PropertyChanged;

		public void NotifyChanged([CallerMemberName] string propName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
		}

		#region props
		private string _IP = "127.0.0.1";

		public string IP
		{
			get { return _IP; }
			set
			{
				_IP = value;
				this.NotifyChanged();
			}
		}

		private int _port = 7789;

		public int Port
		{
			get { return _port; }
			set { _port = value; this.NotifyChanged(); }
		}

		private string _sendMsg;

		public string SendMsg
		{
			get { return _sendMsg; }
			set { _sendMsg = value; this.NotifyChanged(); }
		}

		private string _match = "PA0226231113-14";
		//private string _match = "CN6N38301804";

		public string Match
		{
			get { return _match; }
			set { _match = value; this.NotifyChanged(); }
		}

		private int _loopCount;

		public int LoopCount
		{
			get { return _loopCount; }
			set { _loopCount = value; this.NotifyChanged(); }
		}


		private int _successCount;

		public int SuccessCount
		{
			get { return _successCount; }
			set { _successCount = value; this.NotifyChanged(); }
		}

		private string _msg;

		public string Msg
		{
			get { return _msg; }
			set { _msg = value; this.NotifyChanged(); }
		}

		private string _isConnect = "Offline";

		public string IsConnect
		{
			get { return _isConnect; }
			set { _isConnect = value; this.NotifyChanged(); }
		}

		private double _avereage;

		public double Average
		{
			get { return _avereage; }
			set { _avereage = value; this.NotifyChanged(); }
		}

		private string _Signals;

		public string Signals
		{
			get { return _Signals; }
			set { _Signals = value; this.NotifyChanged(); }
		}


		#endregion

		private TestClient _client;
		private ProcessMonitor pm;

		private string _currentResultFolder;
		private List<string> _results = new List<string>();
		private List<double> _scores;
		private List<(float, float)> _records = new List<(float, float)>();
		private CancellationTokenSource cts = new CancellationTokenSource();
		private ImageFileHelper _imageFileHelper;

		public MainViewModel()
		{
			_client = new TestClient();
		}

		#region commands
		private ICommand _DoConnectCommand;
		public ICommand DoConnectCommand => _DoConnectCommand ??
			(_DoConnectCommand = new RelayCommand((o) => DoConnect()));

		private void DoConnect()
		{
			_client.Connect(IP, Port);
			if (_client.IsConnected)
			{
				IsConnect = "Connected";
			}
			else
			{
				IsConnect = "Offline";
			}
			_results = new List<string>();
		}

		private ICommand _DoLoopCommand;
		public ICommand DoLoopCommand => _DoLoopCommand ??
			(_DoLoopCommand = new RelayCommand((o) => DoLoop()));

		private void DoLoop()
		{
			int freq = 10;
			var sw = Stopwatch.StartNew();
			var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), Const.OCRFolder, DateTime.Now.ToString("HH-mm-ss"));
			Setup(folder, LoopCount, freq);

			pm = new ProcessMonitor("GazerWaferIdRead");
			Task.Run(() =>
			{
				pm.DoMonitorOn(freq);
			});
			var root = pm.GetProcessRoot();
			_imageFileHelper = new ImageFileHelper(Path.Combine(root, "OK"));

			SuccessCount = 0;
			cts = new CancellationTokenSource();
			Msg = "Start";
			_results.Clear();
			_scores = new List<double>();

			Task.Run(() =>
			{
				for (int i = 0; i < LoopCount && !cts.IsCancellationRequested; i++)
				{
					try
					{
						var result = _client.SendAndGetBytes("T");
						result = result.Replace("\0", "");
						_results.Add(result);
						if (result.Contains(Match))
						{
							SuccessCount++;
						}
						MatchCollection matches = Regex.Matches(result, Const.DoubleRegex);
						var score = double.Parse(matches[0].Value);
						_scores.Add(score);

						Msg += result;
						Msg += "\r\n";
						_imageFileHelper.CopyAllFilesTo(_currentResultFolder);
					}
					catch (Exception ex)
					{
						Debug.WriteLine($"exception in OCR result {ex.Message}");
						continue;
					}

				}
			}).ContinueWith(task =>
			{
				// 确保任务成功完成后才调用 SaveData
				if (task.Status == TaskStatus.RanToCompletion)
				{
					sw.Stop();
					_results.Add($"Total cost time {sw.ElapsedMilliseconds} ms, average = {(double)sw.ElapsedMilliseconds / LoopCount} ms");
					_results.Add($"Task Done Succ {SuccessCount} freq {freq} Percent {(double)SuccessCount / (double)LoopCount * 100}%");
					Debug.WriteLine($"Loop {LoopCount} cost {sw.ElapsedMilliseconds} ms");
					SaveData(freq);
				}
				CalculateAverageScore();
			});

		}

		private void CalculateAverageScore()
		{
			Average = _scores.Average();
		}

		private Action<Task> SaveData(int freq)
		{
			try
			{
				Record(SuccessCount, freq);
				pm.Stop(_currentResultFolder);
			}
			catch (Exception ex)
			{
				return null;
			}
			return null;
		}

		private ICommand _CancelCommand;
		public ICommand CancelCommand => _CancelCommand ??
			(_CancelCommand = new RelayCommand((o) => Cancel()));

		private void Cancel()
		{
			cts.Cancel();
		}

		List<string> txtScores = new List<string>();
		string _txtPath;
		private ICommand _SelectFileCommand;
		public ICommand SelectFileCommand => _SelectFileCommand ??
			(_SelectFileCommand = new RelayCommand((o) => SelectFile()));

		private void SelectFile()
		{
			txtScores.Clear();
			var sd = new OpenFileDialog();
			sd.FileName = "Documents";
			sd.DefaultExt = ".txt";
			sd.Filter = "Text documents (.txt)|*.txt";
			var rs = sd.ShowDialog();

			_txtPath = string.Empty;
			if (rs == true)
			{
				_txtPath = sd.FileName;
			}

			var reader = new TxTReader(_txtPath);
			Average = reader.Average();
		}

		private ICommand _GetAverageCommand;
		public ICommand GetAverageCommand => _GetAverageCommand ??
			(_GetAverageCommand = new RelayCommand((o) => ExportUsageHist()));


		private void ExportUsageHist()
		{
			// Define the file paths
			var sd = new OpenFileDialog();
			sd.FileName = "Documents";
			sd.DefaultExt = ".txt";
			sd.Filter = "Text documents (.txt)|*.txt";
			var rs = sd.ShowDialog();
			if (rs == true)
			{
				_txtPath = sd.FileName;
			}

			var reader = new TxTReader(_txtPath);

			// Read CPU usage data from the file
			var cpuPercentages = reader.GetAllDouble(_txtPath);

			// Create a plot model
			var plotModel = new PlotModel { Title = "CPU Usage" };

			var histogramData = GenerateHistogramData(cpuPercentages, 100);

			// Create a bar series
			var barSeries = new BarSeries
			{
				Title = "CPU %",
				FillColor = OxyColors.SkyBlue
			};

			// Add data points to the bar series
			foreach (var data in histogramData)
			{
				barSeries.Items.Add(new BarItem { Value = data });
			}

			// Add axes
			plotModel.Axes.Add(new LinearAxis
			{
				Position = AxisPosition.Bottom,
				Title = "Frequency"
			});
			plotModel.Axes.Add(new CategoryAxis
			{
				Position = AxisPosition.Left,
				Key = "CategoryAxis",
				Title = "CPU % Range"
			});
			plotModel.Background = OxyColors.White;

			// Add the bar series to the plot model
			plotModel.Series.Add(barSeries);


			string outputFilePath = Directory.GetParent(_txtPath).FullName;

			ExportToPng(plotModel, Path.Combine(outputFilePath, "Usage.png"));
		}
		private void ExportUsage()
		{
			// Define the file paths
			var sd = new OpenFileDialog();
			sd.FileName = "Documents";
			sd.DefaultExt = ".txt";
			sd.Filter = "Text documents (.txt)|*.txt";
			var rs = sd.ShowDialog();
			if (rs == true)
			{
				_txtPath = sd.FileName;
			}

			var reader = new TxTReader(_txtPath);

			// Read CPU usage data from the file
			var cpuPercentages = reader.GetAllDouble(_txtPath);

			// Create a plot model
			var plotModel = new PlotModel { Title = "CPU Usage" };

			// Create a line series
			var lineSeries = new LineSeries
			{
				Title = "CPU %",
				MarkerType = MarkerType.Circle,
				MarkerSize = 4,
				MarkerStroke = OxyColors.White
			};

			// Add data points to the line series
			for (int i = 0; i < cpuPercentages.Count; i++)
			{
				lineSeries.Points.Add(new DataPoint(i, cpuPercentages[i]));
			}

			// Add the line series to the plot model
			plotModel.Series.Add(lineSeries);
			string outputFilePath = Directory.GetParent(_txtPath).FullName;

			// Export the plot model to a PNG file
			ExportToPng(plotModel, Path.Combine(outputFilePath, "Usage.png"));
		}

		private void ExportToPng(PlotModel plotModel, string outputFilePath)
		{
			PngExporter.Export(plotModel, outputFilePath, 1920, 1280);
		}

		private List<double> GenerateHistogramData(List<double> data, int binCount)
		{
			var max = data.Max();
			var min = data.Min();
			var range = max - min;
			var binSize = range / binCount;

			var bins = new double[binCount];
			foreach (var value in data)
			{
				var binIndex = (int)((value - min) / binSize);
				if (binIndex == binCount) binIndex--; // Handle edge case where value == max
				bins[binIndex]++;
			}

			return new List<double>(bins);
		}

		private ICommand _SendCommand;
		public ICommand SendCommand => _SendCommand ??
			(_SendCommand = new RelayCommand((o) => SendToServer()));

		private void SendToServer()
		{
			try
			{
				if (Signals != null)
				{
					Msg = _client.SendAndGetBytes(Signals);
				}
			}
			catch (Exception ex)
			{
				throw;
			}
		}

		#endregion

		private void Setup(string folder, int count, int freq)
		{
			_currentResultFolder = SetupFolder(folder, count, freq);
		}

		private string SetupFolder(string folder, int count, int freq)
		{
			if (folder == null)
			{
				folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "OCRResults", DateTime.Now.ToString("HH-mm-ss") + $"Count {count} freq {freq}");
			}
			if (!Directory.Exists(folder))
			{
				Directory.CreateDirectory(folder);
			}
			return folder;
		}

		private void Record(int succ, int freq)
		{
			var filePath = Path.Combine(_currentResultFolder, "OcrResult.txt");

			using (var fs = new FileStream(filePath, FileMode.Append, FileAccess.Write))
			{
				using (var sw = new StreamWriter(fs, Encoding.UTF8))
				{
					foreach (var rs in _results)
					{
						sw.WriteLine(rs);
					}
				}
			}
		}

		public void OnExit()
		{
			_client.Close();
		}
	}
}
