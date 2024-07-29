using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Augong.OpenCVDisplay
{
	// c#12 feature Primary constructors
	// https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-12#primary-constructors
	public class DisplayDemo(string filePath)
	{
		public void Dotest()
		{
			var mat = Cv2.ImRead(filePath, ImreadModes.AnyColor);

			using (new Window("Image", mat))
			{
				Cv2.WaitKey();
			}

			Console.ReadKey();
		}
	}
}
