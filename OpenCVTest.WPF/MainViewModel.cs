using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using UI.Utitlities;

namespace OpenCVTest.WPF
{
	public class MainViewModel : INotifyPropertyChanged
	{
		private string mFolder => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Arcturus Exp Data");
		private const string mDefaultImageName = "test_1.png";
		public event PropertyChangedEventHandler PropertyChanged;

		public void NotifyChanged([CallerMemberName] string propName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
		}

		public MainViewModel()
		{
			LoadDefaultImage();
		}

		private Mat mOriginMat;

		private void LoadDefaultImage(string fileName = mDefaultImageName)
		{
			mOriginMat = Cv2.ImRead(Path.Combine(mFolder, fileName), ImreadModes.Grayscale);
			OriginImage = mOriginMat.ToBitmapSource();

			return;

			using var loaded = Cv2.ImRead(Path.Combine(mFolder, fileName), ImreadModes.Grayscale);
			using var gray = loaded.Clone();
			using var enhanced = new Mat();
			if (loaded.Channels() > 1)
			{
				Cv2.CvtColor(loaded, gray, ColorConversionCodes.BGR2GRAY);
			}
			Cv2.EqualizeHist(gray, enhanced);
			mOriginMat = enhanced.Clone();
			OriginImage = mOriginMat.ToBitmapSource();
		}

		private string mFileName = mDefaultImageName;

		public string FileName
		{
			get { return mFileName; }
			set { mFileName = value; NotifyChanged(); }
		}


		private BitmapSource mOriginImage;

		public BitmapSource OriginImage
		{
			get { return mOriginImage; }
			set { mOriginImage = value; NotifyChanged(); }
		}


		private BitmapSource mAfterImage;

		public BitmapSource AfterImage
		{
			get { return mAfterImage; }
			set { mAfterImage = value; NotifyChanged(); }
		}


		private List<BitmapSource> mProcessedImages;

		public List<BitmapSource> ProcessedImages
		{
			get { return mProcessedImages; }
			set { mProcessedImages = value; NotifyChanged(); }
		}

		private ICommand mEdgeDectionCommand;
		public ICommand EdgeDectionCommand => mEdgeDectionCommand ??
			(mEdgeDectionCommand = new RelayCommand((o) => PerformEdgeDection()));

		private void PerformEdgeDection()
		{
			LoadDefaultImage(FileName);
			using var blur = new Mat(mOriginMat.Size(), mOriginMat.Type(), Scalar.Black);
			using var canny = new Mat(mOriginMat.Size(), mOriginMat.Type());
			using var binary = new Mat(mOriginMat.Size(), mOriginMat.Type());

			Cv2.Canny(mOriginMat, canny, 50, 150);

			Cv2.ImWrite(Path.Combine(mFolder, "canny.png"), canny);

			float[,] kernelX = { { 1f, 0f, -1f }, { 2f, 0f, -2f }, { 1f, 0f, -1f } }; // 水平梯度
			float[,] kernelY = { { 1f, 2f, 1f }, { 0f, 0f, 0f }, { -1f, -2f, -1f } }; // 垂直梯度

			// 3. 正确创建Mat对象（通过Mat.FromArray）
			Mat sobelX = Mat.FromArray(kernelX);
			Mat sobelY = Mat.FromArray(kernelY);

			// 4. 计算梯度
			Mat gx = new Mat(), gy = new Mat();
			Cv2.Filter2D(mOriginMat, gx, MatType.CV_32F, sobelX);
			Cv2.Filter2D(mOriginMat, gy, MatType.CV_32F, sobelY);

			// 5. 合并梯度：G = sqrt(Gx² + Gy²)
			Mat gxSquared = new Mat(), gySquared = new Mat(), sumSquared = new Mat();
			Cv2.Pow(gx, 2, gxSquared);
			Cv2.Pow(gy, 2, gySquared);
			Cv2.Add(gxSquared, gySquared, sumSquared);
			Mat edges = new Mat();
			Cv2.Sqrt(sumSquared, edges);

			// 6. 转换为8位图像（与ImageJ一致）
			edges.ConvertTo(edges, MatType.CV_8U);
			Cv2.ImWrite(Path.Combine(mFolder, $"edges1.png"), edges);




			using var contourImage = blur.Clone();

			Cv2.FindContours(edges, out Point[][] contours, out HierarchyIndex[] hierarchy,
							RetrievalModes.List, ContourApproximationModes.ApproxNone);
			int count = 0;
			foreach (var contour in contours)
			{
				Cv2.DrawContours(contourImage, contours, Array.IndexOf(contours, contour), Scalar.White, thickness: 1);
				Cv2.ImWrite(Path.Combine(mFolder, $"counterImage_{count}.png"), contourImage);
				count++;

			}
			Cv2.ImWrite(Path.Combine(mFolder, $"counterImage.png"), contourImage);
			//Cv2.ImWrite(Path.Combine(mFolder, "lines.png"), resultMat);
			AfterImage = contourImage.ToBitmapSource();
		}

		private int mRange = 50;

		public int Range
		{
			get { return mRange; }
			set { mRange = value; NotifyChanged(); }
		}

		private int mStep = 25;

		public int Step
		{
			get { return mStep; }
			set { mStep = value; NotifyChanged(); }
		}


		private ICommand mSteppedBinaryCommand;
		public ICommand SteppedBinaryCommand => mSteppedBinaryCommand ??
			(mSteppedBinaryCommand = new RelayCommand((o) => SteppedBinary()));


		private List<Mat> mBinaries = [];
		private Dictionary<int, Point[][]> mContours = [];
		private string mComparisons;

		public string Comparisons
		{
			get { return mComparisons; }
			set { mComparisons = value; NotifyChanged(); }
		}


		private void SteppedBinary()
		{
			try
			{
				LoadDefaultImage(FileName);
				mBinaries = [];
				mContours = [];

				Scalar meanGray = Cv2.Mean(mOriginMat);
				double averageGray = meanGray.Val0;

				var blurhw = Math.Min((int)mOriginMat.Cols / 100, (int)mOriginMat.Rows / 100);
				blurhw = blurhw % 2 == 0 ? blurhw + 1 : blurhw;
				var morphSize = new Size(blurhw, blurhw);
				var kernel1 = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(5, 5));
				var blurSize = new Size(blurhw, blurhw);

				var folder = mFolder.ToNow();
				if (!Directory.Exists(folder))
				{
					Directory.CreateDirectory(folder);
				}

				using var blur = new Mat(mOriginMat.Size(), mOriginMat.Type());

				Cv2.GaussianBlur(mOriginMat, blur, blurSize, 10);
				Cv2.MorphologyEx(blur, blur, MorphTypes.Open, kernel1);
				Cv2.MorphologyEx(blur, blur, MorphTypes.Close, kernel1);


				for (int i = 100 - Range; i <= 100 + Range; i += Step)
				{
					using var canny = new Mat(mOriginMat.Size(), mOriginMat.Type());
					using var binary = new Mat(mOriginMat.Size(), mOriginMat.Type());
					using var counterImage = mOriginMat.Clone();
					double stepG = Math.Min(Math.Max(0, averageGray * i / 100), 255);
					Cv2.Threshold(blur, binary, stepG, 255, ThresholdTypes.Binary);

					// post process
					var kernel2 = Cv2.GetStructuringElement(MorphShapes.Rect, morphSize);
					Cv2.MorphologyEx(binary, binary, MorphTypes.Open, kernel2);
					Cv2.MorphologyEx(binary, binary, MorphTypes.Close, kernel2);
					Cv2.GaussianBlur(binary, binary, blurSize, 10);



					Cv2.ImWrite(Path.Combine(folder, $"binary_{i}%.png"), binary);
					mBinaries.Add(binary.Clone());

					Cv2.Canny(binary, canny, 50, 150);
					Cv2.ImWrite(Path.Combine(folder, $"Canny_{i}%.png"), canny);
					Cv2.FindContours(canny, out Point[][] contours, out HierarchyIndex[] hierarchy,
							 RetrievalModes.List, ContourApproximationModes.ApproxSimple);
					foreach (var contour in contours)
					{
						Cv2.DrawContours(counterImage, contours, Array.IndexOf(contours, contour), new Scalar(0, 0, 255), thickness: 3);
					}
					Cv2.ImWrite(Path.Combine(folder, $"Contours_{i}%.png"), counterImage);
				}

				//FilterContours();
				//ComputeBinaryImageStatistics();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}

		private Mat HE(Mat mat)
		{
			mat.ConvertTo(mat, MatType.CV_8UC1);
			var output = new Mat(mat.Size(), mat.Type());
			Cv2.EqualizeHist(mat.Clone(), output);
			return output;
		}

		private void ComputeBinaryImageStatistics()
		{
			for (int i = 0; i < mBinaries.Count; i++)
			{
				for (int j = i + 1; j < mBinaries.Count; j++)
				{
					var (mse, psnr) = CalculateMSEAndPSNR(mBinaries[i], mBinaries[j]);
					Comparisons += $"Comparing image {i + 1} with image {j + 1}: MSE = {mse}, PSNR = {psnr} \n\r";
				}
			}
		}

		private (double MSE, double PSNR) CalculateMSEAndPSNR(Mat image1, Mat image2)
		{
			if (image1.Size() != image2.Size())
			{
				throw new ArgumentException("Images must have the same dimensions");
			}

			var diff = new Mat();
			Cv2.Absdiff(image1, image2, diff);
			var diffSquared = new Mat();
			Cv2.Pow(diff, 2, diffSquared);
			double mse = Cv2.Mean(diffSquared).Val0;

			double psnr = 10 * Math.Log10(255 * 255 / mse);

			return (mse, psnr);
		}

		private ICommand mWaterShedCommand;
		public ICommand WaterShedCommand => mWaterShedCommand ??
			(mWaterShedCommand = new RelayCommand((o) => WaterShed1()));

		private void WaterShed()
		{
			LoadDefaultImage(FileName);

			// 转换为灰度图像
			Mat gray = mOriginMat.Clone();

			// 应用阈值获取标记
			Mat thresh = new Mat();
			Cv2.Threshold(gray, thresh, 0, 255, ThresholdTypes.BinaryInv | ThresholdTypes.Otsu);
			Cv2.ImWrite(Path.Combine(mFolder, "thresh.png"), thresh);


			// 形态学操作去除噪声
			Mat kernel = Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(3, 3));
			Mat opening = new Mat();
			Cv2.MorphologyEx(thresh, opening, MorphTypes.Open, kernel, iterations: 2);
			Cv2.ImWrite(Path.Combine(mFolder, "opening.png"), opening);

			// 确定背景区域
			Mat sureBg = new Mat();
			Cv2.Dilate(opening, sureBg, kernel, iterations: 3);
			Cv2.ImWrite(Path.Combine(mFolder, "sureBg.png"), sureBg);


			// 寻找确定的前景区域
			Mat distTransform = new Mat();
			Cv2.DistanceTransform(opening, distTransform, DistanceTypes.L2, DistanceTransformMasks.Mask3);
			Cv2.Normalize(distTransform, distTransform, 0, 1.0, NormTypes.MinMax);
			Mat distVisual = new Mat();
			Cv2.Normalize(distTransform, distVisual, 0, 255, NormTypes.MinMax);
			Cv2.ImShow("DistanceTransform", distVisual); // 应呈现灰度渐变效果
			Cv2.ImWrite(Path.Combine(mFolder, "distVisual.png"), distVisual);

			// 动态阈值
			double minVal, maxVal;
			Cv2.MinMaxLoc(distTransform, out minVal, out maxVal);

			Mat sureFg = new Mat();
			double dynamicTh = 0.4 * maxVal;
			Cv2.Threshold(distTransform, sureFg, dynamicTh, 255, ThresholdTypes.Binary);
			sureFg.ConvertTo(sureFg, MatType.CV_8U);
			Cv2.ImWrite(Path.Combine(mFolder, "sureFg.png"), sureFg);

			// 2. 创建过滤后的前景图
			Mat filteredFG = Mat.Zeros(sureFg.Size(), sureFg.Type());
			var cc = Cv2.ConnectedComponentsEx(sureFg,
	PixelConnectivity.Connectivity8, ConnectedComponentsAlgorithmsTypes.Default);

			// 3. 只保留大面积的blob
			foreach (var blob in cc.Blobs.Skip(1))
			{
				if (blob.Area >= 50 && blob.Area <= mOriginMat.Cols * mOriginMat.Rows * 0.2) // 面积阈值
				{
					sureFg[blob.Rect].CopyTo(filteredFG[blob.Rect]);
				}
			}
			Cv2.ImWrite(Path.Combine(mFolder, "filteredFG.png"), filteredFG);


			// 找到未知区域
			Mat unknown = new Mat();
			Cv2.Subtract(sureBg, filteredFG, unknown);
			Cv2.ImWrite(Path.Combine(mFolder, "unknown.png"), unknown);

			// 标记连通区域
			Mat markers = new Mat();
			Cv2.ConnectedComponents(filteredFG, markers);

			Cv2.Add(markers, Scalar.All(1), markers);

			// 标记未知区域为0
			for (int i = 0; i < markers.Rows; i++)
			{
				for (int j = 0; j < markers.Cols; j++)
				{
					if (unknown.At<byte>(i, j) == 255)
					{
						markers.Set<int>(i, j, 0);
					}
				}
			}

			// 应用分水岭算法
			Mat colorSrc = gray.CvtColor(ColorConversionCodes.GRAY2BGR);
			Cv2.Watershed(colorSrc, markers);

			// 可视化结果
			Mat result = new Mat();
			colorSrc.CopyTo(result);
			Mat boundaryMask = new Mat();
			Cv2.Compare(markers, Scalar.All(-1), boundaryMask, CmpType.EQ);

			Cv2.ImWrite(Path.Combine(mFolder, "boundaryMask.png"), boundaryMask);
		}

		private void WaterShed1()
		{
			try
			{
				var sw = Stopwatch.StartNew();
				LoadDefaultImage(FileName);

				// 转换为灰度图像并去噪
				Mat gray = mOriginMat.Clone();

				// 亮度标准化（CLAHE算法）
				Mat normalized = new Mat();
				var clahe = Cv2.CreateCLAHE(clipLimit: 2.0, tileGridSize: new Size(8, 8));
				clahe.Apply(gray, normalized);
				Cv2.ImWrite(Path.Combine(mFolder, "0_normalized.png"), normalized);

				Cv2.GaussianBlur(normalized, normalized, new Size(5, 5), 0);
				Cv2.ImWrite(Path.Combine(mFolder, "1_gray_blur.png"), normalized);

				// 自适应阈值处理
				double meanVal = Cv2.Mean(gray)[0];
				Mat thresh = new Mat();
				if (meanVal < 50) // 低亮度图像
				{
					// 提高对比度后再阈值
					Cv2.Normalize(gray, gray, 0, 255, NormTypes.MinMax);
					Cv2.Threshold(gray, thresh, meanVal * 0.7, 255, ThresholdTypes.BinaryInv);
				}
				else
				{
					Cv2.Threshold(gray, thresh, 0, 255, ThresholdTypes.BinaryInv | ThresholdTypes.Otsu);
				}
				Cv2.ImWrite(Path.Combine(mFolder, "2_thresh.png"), thresh);

				// 改进的形态学处理
				Mat kernel = Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(5, 5));
				Mat opening = new Mat();
				Cv2.MorphologyEx(thresh, opening, MorphTypes.Close, kernel, iterations: 1); // 先闭后开
				Cv2.MorphologyEx(opening, opening, MorphTypes.Open, kernel, iterations: 2);
				Cv2.ImWrite(Path.Combine(mFolder, "3_opening.png"), opening);

				// 精确的背景区域提取
				Mat sureBg = new Mat();
				Cv2.Dilate(opening, sureBg, kernel, iterations: 3);
				Cv2.ImWrite(Path.Combine(mFolder, "4_sureBg.png"), sureBg);

				// 改进的距离变换处理
				Mat distTransform = new Mat();
				Cv2.DistanceTransform(opening, distTransform, DistanceTypes.L2, DistanceTransformMasks.Mask3);

				// 动态标准化并可视化距离变换
				Mat distVisual = new Mat();
				Cv2.Normalize(distTransform, distVisual, 0, 255, NormTypes.MinMax);
				Cv2.ImWrite(Path.Combine(mFolder, "5_distTransform.png"), distVisual);

				// 自适应前景提取
				double minVal, maxVal;
				Cv2.MinMaxLoc(distTransform, out minVal, out maxVal);
				double dynamicTh = maxVal * 0.3; // 优化阈值比例
				Mat sureFg = new Mat();
				Cv2.Threshold(distTransform, sureFg, dynamicTh, 255, ThresholdTypes.Binary);
				sureFg.ConvertTo(sureFg, MatType.CV_8U);
				Cv2.ImWrite(Path.Combine(mFolder, "6_sureFg_raw.png"), sureFg);

				// 改进的连通域过滤
				Mat filteredFG = Mat.Zeros(sureFg.Size(), MatType.CV_8U);
				var cc = Cv2.ConnectedComponentsEx(sureFg,
					PixelConnectivity.Connectivity8,
					ConnectedComponentsAlgorithmsTypes.WU);

				// 基于形状和面积的多条件过滤
				foreach (var blob in cc.Blobs.Skip(1))
				{
					double circularity = 4 * Math.PI * blob.Area / Math.Pow(blob.Width * 2 + blob.Height * 2, 2);
					if (blob.Area >= 50 &&
						blob.Area <= mOriginMat.Cols * mOriginMat.Rows * 0.3 &&
						circularity > 0.3)
					{
						using (Mat blobMask = new Mat(sureFg, blob.Rect))
						{
							blobMask.CopyTo(filteredFG[blob.Rect]);
						}
					}
				}
				Cv2.ImWrite(Path.Combine(mFolder, "7_filteredFG.png"), filteredFG);

				// 精确的未知区域计算
				Mat unknown = new Mat();
				Cv2.Subtract(sureBg, filteredFG, unknown);
				Cv2.ImWrite(Path.Combine(mFolder, "8_unknown.png"), unknown);

				// 改进的标记生成
				Mat markers = new Mat();
				int numObjects = Cv2.ConnectedComponents(filteredFG, markers, PixelConnectivity.Connectivity8, MatType.CV_32S);

				// 标记调整（使用安全加法）
				using (Mat ones = Mat.Ones(markers.Size(), MatType.CV_32S))
				{
					Cv2.Add(markers, ones, markers); // markers += 1的替代
				}

				// 标记未知区域
				for (int i = 0; i < markers.Rows; i++)
				{
					for (int j = 0; j < markers.Cols; j++)
					{
						if (unknown.At<byte>(i, j) == 255)
						{
							markers.Set<int>(i, j, 0);
						}
					}
				}

				// 分水岭算法（使用彩色图像）
				Mat colorSrc = mOriginMat.Clone().CvtColor(ColorConversionCodes.GRAY2BGR);
				Cv2.Watershed(colorSrc, markers);

				// 改进的边界提取和可视化
				Mat boundaryMask = new Mat();
				Cv2.Compare(markers, Scalar.All(-1), boundaryMask, CmpType.EQ);

				// 边界闭合处理
				Mat kernel3 = Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(3, 3));
				Cv2.MorphologyEx(boundaryMask, boundaryMask, MorphTypes.Close, kernel3, iterations: 1);

				// 结果可视化
				Mat result = colorSrc.Clone();
				result.SetTo(new Scalar(0, 0, 255), boundaryMask);

				// 标记不同物体区域
				Random rnd = new Random();
				for (int i = 2; i <= numObjects + 1; i++)
				{
					using (Mat regionMask = new Mat())
					{
						Cv2.Compare(markers, Scalar.All(i), regionMask, CmpType.EQ);
						result.SetTo(new Scalar(
							rnd.Next(0, 255),
							rnd.Next(0, 255),
							rnd.Next(0, 255)),
							regionMask);
					}
				}
				Cv2.ImWrite(Path.Combine(mFolder, "9_boundaryMask.png"), boundaryMask);
				var smoothBs = ArcBoundaryOptimizer.OptimizeBoundary(boundaryMask, 0.85, 40);
				Cv2.ImWrite(Path.Combine(mFolder, "10_SmoothboundaryMask.png"), smoothBs);

				Cv2.ImWrite(Path.Combine(mFolder, "11_result.png"), result);
				sw.Stop();
				Debug.WriteLine($"WaterShed1 in {sw.ElapsedMilliseconds}ms in mat {mOriginMat}");
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex + ex.Message);
			}

		}

		private void SmoothBoundary(ref Mat boundaryMask)
		{
			// 步骤1：降采样处理（大尺寸图像）
			if (boundaryMask.Width > 1500)
			{
				Mat resized = new Mat();
				Cv2.Resize(boundaryMask, resized, new Size(1024, 1024));
				Cv2.MorphologyEx(resized, resized, MorphTypes.Close,
					Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(5, 5)));
				Cv2.Resize(resized, boundaryMask, boundaryMask.Size());
			}

			// 步骤2：亚像素平滑
			Mat floatMask = new Mat();
			boundaryMask.ConvertTo(floatMask, MatType.CV_32F);
			Cv2.GaussianBlur(floatMask, floatMask, new Size(0, 0), sigmaX: 1.2);
			Cv2.Threshold(floatMask, boundaryMask, 0.8, 255, ThresholdTypes.Binary);

			// 步骤3：形态学优化
			int kernelSize = Math.Max(3, boundaryMask.Width / 500);
			var kernel = Cv2.GetStructuringElement(MorphShapes.Ellipse,
				new Size(kernelSize, kernelSize));
			Cv2.MorphologyEx(boundaryMask, boundaryMask, MorphTypes.Close, kernel);
		}

		#region Line
		public Mat SmoothBoundary(Mat boundaryMask, double simplificationRatio = 0.02, bool useCurveFitting = false)
		{
			// Step 1: 提取有效轮廓
			Point[][] contours;
			HierarchyIndex[] hierarchy;
			Cv2.FindContours(
				boundaryMask.Clone(),
				out contours,
				out hierarchy,
				RetrievalModes.List,
				ContourApproximationModes.ApproxNone
			);

			// Step 2: 过滤小轮廓
			double minArea = boundaryMask.Width * boundaryMask.Height * 0.001;
			var validContours = contours.Where(c => Cv2.ContourArea(c) >= minArea).ToList();

			// Step 3: 轮廓简化与拟合
			List<Point[]> processedContours = new List<Point[]>();
			foreach (var contour in validContours)
			{
				// 3.1 Douglas-Peucker简化
				double epsilon = simplificationRatio * Cv2.ArcLength(contour, true);
				Point[] approx = Cv2.ApproxPolyDP(contour, epsilon, true);

				// 3.2 可选曲线拟合
				if (useCurveFitting && approx.Length > 10)
				{
					approx = FitCurveToContour(approx);
				}

				processedContours.Add(approx);
			}

			// Step 4: 生成平滑边界
			Mat smoothBoundary = new Mat(boundaryMask.Size(), MatType.CV_8UC1, Scalar.Black);
			Cv2.DrawContours(
				smoothBoundary,
				processedContours,
				-1,
				Scalar.White,
				thickness: 1,
				lineType: LineTypes.AntiAlias
			);

			// Step 5: 后处理优化
			//PostProcessBoundary(ref smoothBoundary);

			return smoothBoundary;
		}

		private Point[] FitCurveToContour(Point[] contour)
		{
			// 分段三次样条拟合
			List<Point> smoothedPoints = new List<Point>();
			int segmentSize = Math.Max(20, contour.Length / 5);

			for (int i = 0; i < contour.Length; i += segmentSize)
			{
				int endIdx = Math.Min(i + segmentSize, contour.Length);
				int currentSize = endIdx - i;
				if (currentSize < 3) continue;

				// 提取当前分段
				Point[] segment = new Point[currentSize];
				Array.Copy(contour, i, segment, 0, currentSize);

				// 转换为向量便于计算
				double[] x = segment.Select(p => (double)p.X).ToArray();
				double[] y = segment.Select(p => (double)p.Y).ToArray();

				// 使用Akima插值（需安装MathNet.Numerics）
				var spline = MathNet.Numerics.Interpolation.CubicSpline.InterpolateAkimaSorted(
					x.Select((v, idx) => (double)idx).ToArray(),
					y
				);

				// 生成平滑点（双倍密度）
				for (double t = 0; t < currentSize; t += 0.5)
				{
					double val = spline.Interpolate(t);
					int px = (int)Math.Round(x[0] + t * (x[currentSize - 1] - x[0]) / (currentSize - 1));
					int py = (int)Math.Round(val);
					smoothedPoints.Add(new Point(px, py));
				}
			}

			return smoothedPoints.ToArray();
		}

		private void PostProcessBoundary(ref Mat boundary)
		{
			// 形态学闭合小缺口
			Mat kernel = Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(3, 3));
			Cv2.MorphologyEx(boundary, boundary, MorphTypes.Close, kernel, iterations: 1);

			// 亚像素平滑
			Mat floatBoundary = new Mat();
			boundary.ConvertTo(floatBoundary, MatType.CV_32F);
			Cv2.GaussianBlur(floatBoundary, floatBoundary, new Size(0, 0), sigmaX: 0.8);
			Cv2.Threshold(floatBoundary, boundary, 0.5, 255, ThresholdTypes.Binary);
		}

		#endregion
	}

	public static class TimeFolder
	{
		public static string ToNow(this string folder)
		{
			return Path.Combine(folder, DateTime.Now.ToString("HH-mm-ss"));
		}
	}
}
