using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.IO;
using System.Threading.Tasks;

namespace WPReader
{
	public class WPDieMapExporter
	{
		private IWorkbook mWorkbook;
		private int[,] mOriginGrid;

		public WPDieMapExporter(int[,] originGrid)
		{
			mOriginGrid = originGrid;
		}

		public async Task DoExportAsync(Stream stream)
		{
			await Task.Run(() => DoExport(stream));
		}

		public void DoExport(Stream stream)
		{
            mWorkbook = new XSSFWorkbook();

			try
			{
				var rows = mOriginGrid.GetLength(0);
				var cols = mOriginGrid.GetLength(1);

				var sheet = mWorkbook.CreateSheet("Sheet1");
				for (int i = 0; i < rows; i++)
				{
					var row = sheet.CreateRow(i);
					for (int j = 0; j < cols; j++)
					{
						if (mOriginGrid[i, j] != 0)
						{
							var cell = row.CreateCell(j);
							cell.SetCellValue(mOriginGrid[i, j]);
						}
					}
				}

				mWorkbook.Write(stream, false);
			}
			catch (Exception)
			{
				throw;
			}
			finally
			{
				mWorkbook.Close();
			}
		}
	}
}

