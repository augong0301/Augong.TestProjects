using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Augong.CSharp
{
	public class ListMemTest
	{
		public ListMemTest() { }


		public void DoTest()
		{
			List<ushort[]> testCollection = new List<ushort[]>();

			for (int i = 0; i < 1024; i++)
			{
				testCollection.Add(new ushort[1024 * 1024]);
			}
			Console.ReadKey();


			for (int i = 0; i < 512; i++)
			{
				testCollection[i] = new ushort[1];
			}


			Console.ReadKey();
		}

	}
}
