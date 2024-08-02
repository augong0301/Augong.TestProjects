using System.Diagnostics;
using System.Text;

namespace Augong.CSharp.Diagnostics
{
	public class ProcessMonitor
	{

		private CancellationTokenSource cts;
		float maxCpu = float.MinValue;
		float maxMem = float.MinValue;
		private List<(float cpu, float mem)> records = new List<(float cpu, float mem)>();

		public Task DoMonitorOnAsync(string processName, int interval)
		{
			return Task.Run(() =>
			{
				DoMonitorOn(processName, interval);
			});
		}

		public void DoMonitorOn(string processName, int interval)
		{
			cts = new CancellationTokenSource();

			// 获取所有名为 processName 的进程
			Process[] processes = Process.GetProcessesByName(processName);

			if (processes.Length == 0)
			{
				Console.WriteLine($"没有找到名为 {processName} 的进程。");
				return;
			}
			while (!cts.IsCancellationRequested)
			{
				Process process = processes[0];

				long memoryUsage = process.PrivateMemorySize64;
				Console.WriteLine($"进程 {processName} 的内存使用: {memoryUsage / 1024} KB");

				PerformanceCounter cpuCounter = new PerformanceCounter("Process", "% Processor Time", process.ProcessName);

				cpuCounter.NextValue();
				Thread.Sleep(interval);
				float cpuUsage = cpuCounter.NextValue() / Environment.ProcessorCount;
				records.Add((cpuUsage, memoryUsage));

				maxCpu = cpuUsage > maxCpu ? cpuUsage : maxCpu;
				maxMem = memoryUsage > maxMem ? memoryUsage : maxMem;

				Console.WriteLine($"进程 {processName} 的CPU使用: {cpuUsage}%");
			}
			records.Add((maxCpu, maxMem));
		}


		public void Stop(string folder = null)
		{
			cts.Cancel();
			Record(folder);
			cts.Dispose();
		}
		private void Record(string folder = null)
		{
			if (folder == null)
			{
				folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "GazerRecorder");
			}
			if (!Directory.Exists(folder))
			{
				Directory.CreateDirectory(folder);
			}
			var filePath = Path.Combine(folder, "Usage.txt");

			using (var fs = new FileStream(filePath, FileMode.Append, FileAccess.Write))
			{
				using (var sw = new StreamWriter(fs, Encoding.UTF8))
				{
					foreach (var r in records)
					{
						sw.WriteLine($"CPU : {r.cpu}%, Memory {r.mem / 1024} KB");
					}
					sw.WriteLine($"CPU max: {maxCpu}%, Memory max {maxMem} KB");

				}
			}
		}
	}
}
