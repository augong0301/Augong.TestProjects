namespace Augong.Math
{
	public class TxTReader
	{

		private string _filePath;
		public TxTReader(string filePath)
		{
			_filePath = filePath;
		}

		public List<double> OutPut = new List<double>();

		public List<double> ReadAllLines()
		{
			using (var fs = File.OpenRead(_filePath))
			{
				using (var sr = new StreamReader(fs))
				{
					while (!sr.EndOfStream)
					{
						OutPut.Add(double.Parse(sr.ReadLine()));
					}
				}
			}
			return OutPut;
		}

		private List<double> ReadDoubleByLine(string filePath)
		{
			var cpuPercentages = new List<double>();
			var lines = File.ReadAllLines(filePath);
			int zeroCount = 0;

			foreach (var line in lines)
			{
				var parts = line.Split(',');
				var cpuPart = parts[0].Trim(); // "CPU : 0%"
				var cpuValueString = cpuPart.Split(':')[1].Trim().TrimEnd('%');
				if (double.TryParse(cpuValueString, out double cpuValue))
				{
					if (cpuValue == 0)
					{
						zeroCount++;
						if (zeroCount <= 2)
						{
							cpuPercentages.Add(cpuValue);
						}
					}
					else
					{
						zeroCount = 0;
						cpuPercentages.Add(cpuValue);
					}
				}
			}

			return cpuPercentages;
		}

		public double Average()
		{
			var scores = ReadAllLines();
			return scores.Average();
		}

		public List<double> GetAllDouble(string filePath)
		{
			return ReadDoubleByLine(filePath);
		}


	}
}
