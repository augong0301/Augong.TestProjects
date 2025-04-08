using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Augong.TxtReader
{
	internal class Program
	{
		static void Main(string[] args)
		{
			string logData = @"
  1132,102: 2025-04-02 18:07:06.298 <Trace      > [FocusTrain]YPosition[mm] 50 FocusIlluminate 100 FocusRPM 1000 FocusPosition 5338.35
  3613,102: 2025-04-02 18:15:03.776 <Trace      > [FocusTrain]YPosition[mm] 50 FocusIlluminate 100 FocusRPM 1000 FocusPosition 5338.1
  6090,102: 2025-04-02 18:23:08.246 <Trace      > [FocusTrain]YPosition[mm] 50 FocusIlluminate 100 FocusRPM 1000 FocusPosition 5337.9
  8576,102: 2025-04-02 18:31:27.624 <Trace      > [FocusTrain]YPosition[mm] 50 FocusIlluminate 100 FocusRPM 1000 FocusPosition 5338.1
  11024,102: 2025-04-02 18:39:45.706 <Trace      > [FocusTrain]YPosition[mm] 50 FocusIlluminate 100 FocusRPM 1000 FocusPosition 5338.05
  13475,102: 2025-04-02 18:48:12.527 <Trace      > [FocusTrain]YPosition[mm] 50 FocusIlluminate 100 FocusRPM 1000 FocusPosition 5337.6
  15916,102: 2025-04-02 18:56:27.961 <Trace      > [FocusTrain]YPosition[mm] 50 FocusIlluminate 100 FocusRPM 1000 FocusPosition 5337.45
  18364,102: 2025-04-02 19:04:46.982 <Trace      > [FocusTrain]YPosition[mm] 50 FocusIlluminate 100 FocusRPM 1000 FocusPosition 5338.05
  20806,102: 2025-04-02 19:13:02.746 <Trace      > [FocusTrain]YPosition[mm] 50 FocusIlluminate 100 FocusRPM 1000 FocusPosition 5337.95
  23251,102: 2025-04-02 19:21:21.202 <Trace      > [FocusTrain]YPosition[mm] 50 FocusIlluminate 100 FocusRPM 1000 FocusPosition 5337.85
  25693,102: 2025-04-02 19:29:38.592 <Trace      > [FocusTrain]YPosition[mm] 50 FocusIlluminate 100 FocusRPM 1000 FocusPosition 5337.75
  28132,102: 2025-04-02 19:37:35.416 <Trace      > [FocusTrain]YPosition[mm] 50 FocusIlluminate 100 FocusRPM 1000 FocusPosition 5334.899405
  30568,102: 2025-04-02 19:45:48.216 <Trace      > [FocusTrain]YPosition[mm] 50 FocusIlluminate 100 FocusRPM 1000 FocusPosition 5337.7
  33005,102: 2025-04-02 19:53:55.946 <Trace      > [FocusTrain]YPosition[mm] 50 FocusIlluminate 100 FocusRPM 1000 FocusPosition 5337.45
  35446,102: 2025-04-02 20:02:10.479 <Trace      > [FocusTrain]YPosition[mm] 50 FocusIlluminate 100 FocusRPM 1000 FocusPosition 5337.45
  37894,102: 2025-04-02 20:10:15.809 <Trace      > [FocusTrain]YPosition[mm] 50 FocusIlluminate 100 FocusRPM 1000 FocusPosition 5334.46898
  40336,102: 2025-04-02 20:18:21.626 <Trace      > [FocusTrain]YPosition[mm] 50 FocusIlluminate 100 FocusRPM 1000 FocusPosition 5335.006055
  42778,102: 2025-04-02 20:26:41.826 <Trace      > [FocusTrain]YPosition[mm] 50 FocusIlluminate 100 FocusRPM 1000 FocusPosition 5337.45
  45210,102: 2025-04-02 20:34:55.952 <Trace      > [FocusTrain]YPosition[mm] 50 FocusIlluminate 100 FocusRPM 1000 FocusPosition 5337.5
  47648,102: 2025-04-02 20:43:08.332 <Trace      > [FocusTrain]YPosition[mm] 50 FocusIlluminate 100 FocusRPM 1000 FocusPosition 5337.85
";

			List<double> focusPos = new List<double>();

			string[] lines = logData.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (string line in lines)
			{
				if (line.Contains("FocusPosition"))
				{
					string timeStr = line.Split(new[] { ": " }, StringSplitOptions.RemoveEmptyEntries)[1].Split(new[] { " FocusPosition" }, StringSplitOptions.RemoveEmptyEntries)[1];
					focusPos.Add(double.Parse(timeStr));
				}
			}


			double mean = focusPos.Average();
			double maxDifference = focusPos.Select(x => Math.Pow(x - mean, 2)).Average() / mean;

			Console.WriteLine($"对焦位置平均: {mean:F2} um");
			Console.WriteLine($"最大: {focusPos.Max():F2} um");
			Console.WriteLine($"最小: {focusPos.Min():F2} um");
			Console.WriteLine($"标准差: {Math.Sqrt(focusPos.Sum(x => Math.Pow(x - mean, 2)) / (focusPos.Count - 1)):F2}");
			Console.WriteLine($"变异系数: {maxDifference:F6}");
			Console.ReadLine();
		}


		static void MainTimer(string[] args)
		{
			string logData = @"
999,40: 2025-04-02 18:06:33.159 <Information> [TrainService]Begin auto train 
1789,40: 2025-04-02 18:10:40.683 <Information> [TrainService]Auto train all done
3481,40: 2025-04-02 18:14:33.704 <Information> [TrainService]Begin auto train 
4265,40: 2025-04-02 18:18:40.139 <Information> [TrainService]Auto train all done
5957,40: 2025-04-02 18:22:37.734 <Information> [TrainService]Begin auto train 
6741,40: 2025-04-02 18:26:50.805 <Information> [TrainService]Auto train all done
8444,40: 2025-04-02 18:30:57.586 <Information> [TrainService]Begin auto train 
9227,40: 2025-04-02 18:35:07.612 <Information> [TrainService]Auto train all done
10918,40: 2025-04-02 18:39:15.631 <Information> [TrainService]Begin auto train 
11671,40: 2025-04-02 18:43:29.785 <Information> [TrainService]Auto train all done
13369,40: 2025-04-02 18:47:42.644 <Information> [TrainService]Begin auto train 
14123,40: 2025-04-02 18:51:55.624 <Information> [TrainService]Auto train all done
15809,40: 2025-04-02 18:55:57.977 <Information> [TrainService]Begin auto train 
16565,40: 2025-04-02 19:00:11.687 <Information> [TrainService]Auto train all done
18258,40: 2025-04-02 19:04:17.132 <Information> [TrainService]Begin auto train 
19013,40: 2025-04-02 19:08:28.666 <Information> [TrainService]Auto train all done
20700,40: 2025-04-02 19:12:32.072 <Information> [TrainService]Begin auto train 
21453,40: 2025-04-02 19:16:45.600 <Information> [TrainService]Auto train all done
23145,40: 2025-04-02 19:20:51.289 <Information> [TrainService]Begin auto train 
23899,40: 2025-04-02 19:25:05.278 <Information> [TrainService]Auto train all done
25587,40: 2025-04-02 19:29:08.640 <Information> [TrainService]Begin auto train 
26342,40: 2025-04-02 19:33:21.525 <Information> [TrainService]Auto train all done
28026,40: 2025-04-02 19:37:19.069 <Information> [TrainService]Begin auto train 
28780,40: 2025-04-02 19:41:20.475 <Information> [TrainService]Auto train all done
30462,40: 2025-04-02 19:45:18.284 <Information> [TrainService]Begin auto train 
31217,40: 2025-04-02 19:49:30.157 <Information> [TrainService]Auto train all done
32899,40: 2025-04-02 19:53:25.423 <Information> [TrainService]Begin auto train 
33653,40: 2025-04-02 19:57:39.365 <Information> [TrainService]Auto train all done
35339,40: 2025-04-02 20:01:40.664 <Information> [TrainService]Begin auto train 
36094,40: 2025-04-02 20:05:54.998 <Information> [TrainService]Auto train all done
37787,40: 2025-04-02 20:09:59.478 <Information> [TrainService]Begin auto train 
38542,40: 2025-04-02 20:14:02.323 <Information> [TrainService]Auto train all done
40229,40: 2025-04-02 20:18:05.285 <Information> [TrainService]Begin auto train 
40985,40: 2025-04-02 20:22:06.439 <Information> [TrainService]Auto train all done
42672,40: 2025-04-02 20:26:11.876 <Information> [TrainService]Begin auto train 
43425,40: 2025-04-02 20:30:24.161 <Information> [TrainService]Auto train all done
45103,40: 2025-04-02 20:34:25.946 <Information> [TrainService]Begin auto train 
45859,40: 2025-04-02 20:38:39.133 <Information> [TrainService]Auto train all done
47542,40: 2025-04-02 20:42:38.497 <Information> [TrainService]Begin auto train 
48296,40: 2025-04-02 20:46:49.964 <Information> [TrainService]Auto train all done
50148,40: 2025-04-03 09:18:05.334 <Information> [TrainService]Begin auto train 
50903,40: 2025-04-03 09:22:00.374 <Information> [TrainService]Auto train all done
";

			List<DateTime> startTimes = new List<DateTime>();
			List<DateTime> endTimes = new List<DateTime>();

			string[] lines = logData.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (string line in lines)
			{
				if (line.Contains("Begin auto train"))
				{
					string timeStr = line.Split(new[] { ": " }, StringSplitOptions.RemoveEmptyEntries)[1].Split(new[] { " <" }, StringSplitOptions.RemoveEmptyEntries)[0];
					startTimes.Add(DateTime.ParseExact(timeStr, "yyyy-MM-dd HH:mm:ss.fff", null));
				}
				else if (line.Contains("Auto train all done"))
				{
					string timeStr = line.Split(new[] { ": " }, StringSplitOptions.RemoveEmptyEntries)[1].Split(new[] { " <" }, StringSplitOptions.RemoveEmptyEntries)[0];
					endTimes.Add(DateTime.ParseExact(timeStr, "yyyy-MM-dd HH:mm:ss.fff", null));
				}
			}

			List<double> timeDifferences = new List<double>();
			for (int i = 0; i < startTimes.Count; i++)
			{
				timeDifferences.Add((endTimes[i] - startTimes[i]).TotalSeconds);
			}

			double averageTime = timeDifferences.Average();
			double maxTimeDifference = timeDifferences.Max() - timeDifferences.Min();

			Console.WriteLine($"平均时间: {averageTime:F2} 秒");
			Console.WriteLine($"最大时间误差: {maxTimeDifference:F2} 秒");
			Console.ReadLine();
		}
		static void Main1(string[] args)
		{
			var _oldPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "old.txt");
			var _newPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "new.txt");

			var aveOld = AverageTick(_oldPath);
			var aveNew = AverageTick(_newPath);
			Console.WriteLine($"old ticks average [{aveOld}], new ticks average [{aveNew}]");
			Console.ReadKey();
		}

		static double AverageTick(string filePath)
		{
			var ticks = new List<int>();

			using (var fs = File.OpenRead(filePath))
			{
				using (var sr = new StreamReader(fs))
				{
					while (!sr.EndOfStream)
					{
						var l = sr.ReadLine();
						if (!l.Contains("invoke cost"))
						{
							continue;
						}

						var tick = l.Split(' ')[2];
						ticks.Add(int.Parse(tick));
					}
				}
			}
			return ticks.Average();

		}
	}
}
