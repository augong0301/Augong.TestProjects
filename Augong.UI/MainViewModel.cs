using Augong.CSharp.Diagnostics;
using Augong.Math;
using Microsoft.Win32;
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


		#endregion

		private TestClient _client;
		private ProcessMonitor pm = new ProcessMonitor();
		private List<string> _results;
		private List<double> _scores;
		private List<(float, float)> _records = new List<(float, float)>();
		private CancellationTokenSource cts = new CancellationTokenSource();

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
			var sw = Stopwatch.StartNew();
			int freq = 10;
			Task.Run(() =>
			{
				pm.DoMonitorOn("GazerWaferIdRead", freq);
			});

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
							string pattern = @"-?\d*\.\d+";

							MatchCollection matches = Regex.Matches(result, pattern);
							var score = double.Parse(matches[0].Value);
							_scores.Add(score);
						}

						Msg += result;
						Msg += "\r\n";
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
					Debug.WriteLine($"Loop {LoopCount} cost {sw.ElapsedMilliseconds} ms");
					SaveData(freq);
				}
				else if (task.Status == TaskStatus.Faulted)
				{
					// 处理错误
					Console.WriteLine("任务执行出错: " + task.Exception);
				}
				else if (task.Status == TaskStatus.Canceled)
				{
					// 处理取消
					Console.WriteLine("任务已取消");
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
				var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "OCRResults", DateTime.Now.ToString("HH-mm-ss") + $"Succ {SuccessCount} freq {freq} Percent {(double)SuccessCount / (double)LoopCount * 100}%");
				Record(SuccessCount, freq, folder);
				pm.Stop(folder);
			}
			catch (Exception ex)
			{
				throw;
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
			(_GetAverageCommand = new RelayCommand((o) => GetAverage()));

		private void GetAverage()
		{

		}

		#endregion

		private void Record(int succ, int freq, string folder = null)
		{
			if (folder == null)
			{
				folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "OCRResults", DateTime.Now.ToString("HH-mm-ss") + $"Succ {succ} freq {freq}");
			}
			if (!Directory.Exists(folder))
			{
				Directory.CreateDirectory(folder);
			}
			var filePath = Path.Combine(folder, "OcrResult.txt");

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
