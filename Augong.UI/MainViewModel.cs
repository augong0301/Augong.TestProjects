using Augong.CSharp.Diagnostics;
using SocketTest;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
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



		#endregion

		private TestClient _client;
		private ProcessMonitor pm = new ProcessMonitor();
		private List<string> _results;
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
			Task.Run(() =>
			{
				for (int i = 0; i < LoopCount && !cts.IsCancellationRequested; i++)
				{
					var result = _client.SendAndGetBytes("T");
					result = result.Replace("\0", "");
					_results.Add(result);
					if (result.Contains(Match))
					{
						SuccessCount++;
					}

					Msg += result;
					Msg += "\r\n";
				}
			}).ContinueWith(task =>
			{
				// 确保任务成功完成后才调用 SaveData
				if (task.Status == TaskStatus.RanToCompletion)
				{
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
			});

		}

		private Action<Task> SaveData(int freq)
		{
			try
			{
				var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "OCRResults", DateTime.Now.ToString("HH-mm-ss") + $"Succ {SuccessCount} freq {freq} Percent {SuccessCount / LoopCount * 100}%");
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
