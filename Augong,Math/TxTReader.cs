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

		public double Average()
		{
			var scores = ReadAllLines();
			return scores.Average();
		}

	}
}
