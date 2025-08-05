using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPReader
{
	public class WaferPatternInfo()
	{
		public int[,] DieInfos { get; set; }

		public int Diameter { get; set; }

		public int DieLimit { get; set; } = 90000;

		public List<double> Cols { get; set; } = [];

		public List<double> Rows { get; set; } = [];

		public DateTime EndTime { get; set; }

		public string Version { get; set; }

		public string Type { get; set; }

		public int RowCount { get; set; }

		public int ColCount { get; set; }

		public int[,] GetSubMatrix(int rowStart, int colStart, int rowCount, int colCount)
		{
			if (DieInfos == null)
			{
				throw new ArgumentNullException(nameof(DieInfos));
			}

			int maxRow = DieInfos.GetLength(0), maxCol = DieInfos.GetLength(1);
			if (rowStart < 0 || colStart < 0
				|| rowStart + rowCount > maxRow
				|| colStart + colCount > maxCol)
			{
				throw new ArgumentOutOfRangeException("Submat out of range");
			}

			var sub = new int[rowCount, colCount];
			for (int i = 0; i < rowCount; i++)
			{
				for (int j = 0; j < colCount; j++)
				{
					sub[i, j] = DieInfos[rowStart + i, colStart + j];
				}
			}
			return sub;
		}
	}
}
