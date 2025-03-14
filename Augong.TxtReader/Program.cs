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
