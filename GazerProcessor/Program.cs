using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.ImageSharp;
using OxyPlot.Series;
using System.Text.RegularExpressions;

internal class program
{

	private static void Main(string[] args)
	{
		DDDD();
		D2();
	}


	public static void DDDD()
	{
		// 设置文件夹路径和输出文件路径
		string folderPath = "C:\\Works\\Logs\\Gazer";
		string outputFilePath = "C:\\Works\\Logs\\Gazer\\Output\\rst.txt";

		// 查找所有 txt 文件
		var txtFiles = Directory.GetFiles(folderPath, "*.txt");

		// 创建一个 StreamWriter 用于写入输出文件
		using (StreamWriter outputFile = new StreamWriter(outputFilePath))
		{
			foreach (var file in txtFiles)
			{
				// 读取每个文件的内容
				var lines = File.ReadAllLines(file);

				// 过滤出包含指定字符串的行
				var matchingLines = lines.Where(line => line.Contains("Edge alignment center of wafer"));

				// 将匹配的行写入输出文件
				foreach (var line in matchingLines)
				{
					outputFile.WriteLine(line);
				}
			}
		}

		Console.WriteLine("完成！");
	}


	public static void D2()
	{
		// 输入和输出文件路径
		string inputFilePath = @"C:\Works\Logs\Gazer\Output\rst.txt";
		string outputFilePath = @"C:\Works\Logs\Gazer\Output\output.txt";
		List<double> angles = new List<double>();
		List<double> xs = new List<double>();
		List<double> ys = new List<double>();
		// 读取所有行
		var lines = File.ReadAllLines(inputFilePath);

		// 创建一个 StreamWriter 用于写入输出文件
		using (StreamWriter outputFile = new StreamWriter(outputFilePath))
		{
			foreach (var line in lines)
			{
				// 使用正则表达式提取数据
				var match = Regex.Match(line, @"After : Edge alignment center of wafer \(([^,]+),\s*(-?\d+\.?\d*),\s*(-?\d+\.?\d*)\)");

				if (match.Success)
				{
					// 提取角度、x 和 y 坐标
					var angle = match.Groups[1].Value;
					if (double.Parse(angle) > 1 && line.Contains("2024-09-09"))
					{
						continue;
					}
					var x = match.Groups[2].Value;
					var y = match.Groups[3].Value;
					angles.Add(double.Parse(angle));
					xs.Add(double.Parse(x));
					ys.Add(double.Parse(y));
					// 将结果写入输出文件
					outputFile.WriteLine($"{angle}\t{x}\t{y}");
				}
			}
		}
		Cal(angles, "angle offset");
		Cal(xs, "x offset");
		Cal(ys, "y offset");
		//var aa = angles.Average();
		//var xa = xs.Average();
		//var ya = ys.Average();
		//angles = angles.Select(a => a - aa).ToList();
		//xs = xs.Select(a => a - xa).ToList();
		//ys = ys.Select(a => a - ya).ToList();


		ExportUsageHist(angles, "angle offset");
		ExportUsageHist(xs, "x offset");
		ExportUsageHist(ys, "y offset");

		Console.WriteLine("处理完成！");
	}

	public static void ExportUsageHist(List<double> data, string title)
	{
		// Define the file paths


		// Read CPU usage data from the file

		// Create a plot model
		var plotModel = new PlotModel { Title = title };

		var histogramData = GenerateHistogramData(data, 100);

		// 定义区间数
		int numberOfIntervals = 100;

		// 计算区间宽度
		double intervalWidth = (data.Max() - data.Min()) / numberOfIntervals;

		// 初始化频数列表
		List<int> frequency = new List<int>(new int[numberOfIntervals]);

		// 统计每个区间的频数
		foreach (double value in data)
		{
			if (value == data.Max())
			{
				frequency[numberOfIntervals - 1]++;
			}
			else
			{
				int index = (int)((value - data.Min()) / intervalWidth);
				frequency[index]++;
			}
		}

		// Create a bar series
		var barSeries = new BarSeries
		{
			Title = "Value",
			FillColor = OxyColors.SkyBlue
		};

		// Add data points to the bar series
		foreach (var d in frequency)
		{
			barSeries.Items.Add(new BarItem { Value = d });
		}

		// 添加 Y 轴（类别轴）
		plotModel.Axes.Add(new CategoryAxis
		{
			Position = AxisPosition.Left,
			Title = "Value Range",
			ItemsSource = Enumerable.Range(0, numberOfIntervals).Select(i => $"{data.Min() + i * intervalWidth:F2} - {data.Min() + (i + 1) * intervalWidth:F2}").ToList()
		});

		// 添加 X 轴（线性轴）
		var xAxis = new LinearAxis
		{
			Position = AxisPosition.Bottom,
			Title = "Count",
			Minimum = 0,
			Maximum = frequency.Max() * 1.1, // 设定 x 轴的最大值，略大于最大频数
			IntervalLength = Math.Max(1, frequency.Max() / 10.0) // 设定 x 轴的刻度间隔
		};

		plotModel.Background = OxyColors.White;

		// Add the bar series to the plot model
		plotModel.Series.Add(barSeries);

		string outputFilePath = Directory.GetParent(@"C:\Works\Logs\Gazer\Output").FullName;

		ExportToPng(plotModel, Path.Combine(outputFilePath, $"{title}.png"));
	}

	private static void ExportToPng(PlotModel plotModel, string outputFilePath)
	{
		PngExporter.Export(plotModel, outputFilePath, 1920, 1280);
	}

	private static List<double> GenerateHistogramData(List<double> data, int binCount)
	{
		var max = data.Max();
		var min = data.Min();
		var range = max - min;
		var binSize = range / binCount;

		var bins = new double[binCount];
		int count = 1;
		foreach (var value in data)
		{
			var bin = binSize * count + min;
			bins[count - 1] = bin;
			count++;
			if (count > 100)
			{
				break;
			}
		}

		return new List<double>(bins);
	}

	public static void Cal(List<double> data, string title)
	{
		// 计算均值
		double mean = data.Average();

		// 计算方差
		double variance = data.Select(value => Math.Pow(value - mean, 2)).Average();

		// 计算最大值
		double max = data.Max();

		// 计算最小值
		double min = data.Min();

		double cv = Math.Sqrt(variance) / mean;

		// 输出结果
		Console.WriteLine($"{title}均值: {mean}");
		Console.WriteLine($"{title}方差: {variance}");
		Console.WriteLine($"{title}最大值: {max}");
		Console.WriteLine($"{title}最小值: {min}");
		Console.WriteLine($"{title} CV: {cv}");
	}
}
