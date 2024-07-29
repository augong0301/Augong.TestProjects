using Augong.CSharp.Contract;
using OpenCvSharp;
using System;

namespace Augong.OpenCVDisplay
{
	public class HistTest(string filePath) : ITest
	{
		public void DoTest()
		{
			var image = Cv2.ImRead(filePath);
			ImageConvert16to8(ref image);
			Mat hist = new Mat(image.Rows, image.Cols, image.Type());
			int range = 256;

			Cv2.CalcHist(new Mat[] { image },
						 new int[] { 0 },
						 null,
						 hist,
						 1,
						 new int[] { range },
						 new Rangef[] { new Rangef(0, range) });


			List<double> histogram = new List<double>();

            for (int i = 0; i < hist.Rows; i++)
            {
                histogram.Add(hist.Get<float>(i, 0));
            }
            double v = histogram.Max();
            var total = histogram.Sum();

            if (total != 0)
            {
                for (int i = 0; i < histogram.Count; i++)
                {
                    histogram[i] /= total;
                    histogram[i] = Math.Round(histogram[i], 4) * 100;
                }
            }

			Console.ReadKey();
		}

		public void ImageConvert16to8(ref Mat mat)
		{
			Cv2.MinMaxIdx(mat, out double minVal, out double maxVal);
			mat.ConvertTo(mat, MatType.CV_8UC1, 255.0 / (maxVal - minVal), -255.0 * minVal / (maxVal - minVal));
		}
	}
}
