using Augong.OpenCVDisplay;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

internal class Program
{
	private static string Desktop => Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

	private static void Main(string[] args)
	{

		RotateRectDrawer rotateRectDrawer = new RotateRectDrawer();
		rotateRectDrawer.DrawCracksInCanvas();
		bool stop = false;
		var file = Path.Combine(Desktop, "test", "test.bmp");
		var png = Cv2.ImRead(file, ImreadModes.Grayscale);
		var mat = new Mat(png.Rows, png.Cols, png.Type());
		try
		{
			var png1 = png.Clone();
			mat[new Rect(0, 0, mat.Width / 2, mat.Height / 2)] = png[new Rect(0, 0, png.Width / 2, png.Height / 2)];
			png.Dispose();
			var clahe = CLAHE.Create();
			clahe.Apply(mat, png1);
		}
		catch (Exception ex)
		{
			throw;
		}


		try
		{
			int edgeWidth = 1;
			Mat image = Cv2.ImRead(file, ImreadModes.Grayscale);
			Mat borderedImage = Mat.FromPixelData(image.Rows + edgeWidth *2, image.Cols + edgeWidth *2 , image.Depth(), image.Channels());
			Cv2.CopyMakeBorder(image.Clone(), borderedImage, edgeWidth, edgeWidth, edgeWidth, edgeWidth, BorderTypes.Constant, new Scalar(255, 255, 255));


			List<Mat> images = new List<Mat>(); // Add a number of Mats to the List. 
			Mat combinedImage = null;
			List<int> expectedVerticalPositions = new List<int> { 0 };
			Mat imageSoFar = images.ElementAt(0);
			for (int count = 1; count < images.Count; count++)
			{
				Mat imageToAdd = images.ElementAt(count);
				expectedVerticalPositions.Add(expectedVerticalPositions.Last() + imageToAdd.Cols);

				Mat newlyCombined = Mat.FromPixelData(imageSoFar.Rows, imageSoFar.Cols + imageToAdd.Cols, imageSoFar.Depth(), imageSoFar.Channels());
				Cv2.HConcat(imageSoFar, imageToAdd, newlyCombined);
				imageSoFar = newlyCombined;
			}
		}
		catch (Exception)
		{

			throw;
		}
	}


	private static void Test()
	{
		//var writeTask = Task.Run(() =>
		//{
		//	var png = Cv2.ImRead(file1, ImreadModes.AnyColor);
		//	Cv2.ImWrite(file, png);
		//});


		//var readTask = Task.Run(() =>
		//{
		//	var read = Cv2.ImRead(file, ImreadModes.Grayscale);
		//	if (read.Height == 0 && read.Width == 0)
		//	{
		//		stop = true;
		//	}
		//});
		//Task.WhenAll(writeTask, readTask).Wait();
		//Console.WriteLine($"Loop {count}, stop = {stop}");
		//Thread.Sleep(1000);
	}
}