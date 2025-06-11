using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Windows.Media;
using OpenCvSharp;

public static class ArcBoundaryOptimizer
{

	private static string mFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Arcturus Exp Data");

	public static Mat OptimizeBoundary(Mat boundaryMask,
									 double circleThreshold = 0.85,
									 int minArcPoints = 10,
									 double lineSimplificationRatio = 0.02)
	{
		Mat processedMask = new Mat();
		Cv2.MorphologyEx(boundaryMask, processedMask, MorphTypes.Close,
			Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(11, 11)));
		Cv2.ImWrite(Path.Combine(mFolder, "12_distTransform.png"), processedMask);

		processedMask = EnhanceContourConnectivity(processedMask);
		Cv2.ImWrite(Path.Combine(mFolder, "13_EnhanceContourConnectivity.png"), processedMask);

		Point[][] contours;
		HierarchyIndex[] hierarchy;
		Cv2.FindContours(
			boundaryMask.Clone(),
			out contours,
			out hierarchy,
			RetrievalModes.Tree,  // 改为Tree模式获取层级
			ContourApproximationModes.ApproxSimple,  // 使用简单近似
			offset: null
		);

		var realContours = PreProcessContours(contours, boundaryMask);

		// 4. 绘制最终边界
		Mat result = new Mat(boundaryMask.Size(), MatType.CV_8UC1, Scalar.Black);
		//result = DrawDiscreteContours(result, realContours);
		return result;
	}

	public static Mat EnhanceContourConnectivity(Mat boundaryMask)
	{
		Mat processed = boundaryMask.Clone();

		// 1. 自适应结构元素（根据图像尺寸动态计算）
		int kernelSize = Math.Max(3, boundaryMask.Width / 300); // 300像素对应3x3核
		Mat kernel = Cv2.GetStructuringElement(
			MorphShapes.Cross,  // 十字形比椭圆更能连接对角线
			new Size(kernelSize, kernelSize)
		);

		// 2. 多阶段形态学处理
		// 第一阶段：强力闭合间隙（水平/垂直方向）
		Cv2.MorphologyEx(processed, processed, MorphTypes.Close, kernel, iterations: 2);

		// 3. 去除孤立噪点（面积过滤）
		Mat labels = new Mat();
		int numLabels = Cv2.ConnectedComponents(processed, labels);

		// 统计各连通域面积（使用Mat.At<int>）
		var blobAreas = new Dictionary<int, int>();
		for (int i = 0; i < labels.Rows; i++)
		{
			for (int j = 0; j < labels.Cols; j++)
			{
				int label = labels.At<int>(i, j);
				if (label > 0)
				{
					if (!blobAreas.ContainsKey(label))
						blobAreas[label] = 1;
					else
						blobAreas[label]++;
				}
			}
		}

		// 过滤小面积区域
		double minArea = boundaryMask.Width * boundaryMask.Height * 0.0005; // 0.05%面积阈值
		for (int i = 0; i < labels.Rows; i++)
		{
			for (int j = 0; j < labels.Cols; j++)
			{
				int label = labels.At<int>(i, j);
				if (label > 0 && blobAreas[label] < minArea)
				{
					processed.Set(i, j, 0);
				}
			}
		}

		return processed;
	}

	private static List<Point[]> PreProcessContours(Point[][] contours, Mat boundaryMask)
	{
		List<Point[]> uniqueContours = new List<Point[]>();
		double shapeThreshold = 0.05; // 相似度阈值（0表示完全匹配）

		for (int i = 0; i < contours.Length; i++)
		{
			bool isDuplicate = false;
			for (int j = 0; j < i; j++)
			{
				double similarity = Cv2.MatchShapes(contours[i], contours[j], ShapeMatchModes.I1, 0);
				Moments m1 = Cv2.Moments(contours[i]);
				Point2f center1 = new Point2f((float)(m1.M10 / m1.M00), (float)(m1.M01 / m1.M00));
				Moments m2 = Cv2.Moments(contours[j]);
				Point2f center2 = new Point2f((float)(m2.M10 / m2.M00), (float)(m2.M01 / m2.M00));
				if (similarity < shapeThreshold && center1.DistanceTo(center2)< 10)
				{
					isDuplicate = true;
					break;
				}
			}

			if (!isDuplicate)
			{
				uniqueContours.Add(contours[i]);
			}
		}

		var result = new List<Point[]>();
		Mat blank = new Mat(boundaryMask.Size(), MatType.CV_8UC1, Scalar.Black);
		int count = 0;
		foreach (var contour in uniqueContours)
		{
			//Mat contourImage = blank.Clone();

			// 1. 预处理
			double epsilon = 0.02 * Cv2.ArcLength(contour, true);
			Point[] approx = Cv2.ApproxPolyDP(contour, epsilon, true);

			if (approx.Length == 2 || Cv2.ContourArea(contour) < boundaryMask.Rows * boundaryMask.Cols * 0.0005)
			{
				continue;
			}

			// 2. 分类与平滑
			double area = Cv2.ContourArea(contour);
			double perimeter = Cv2.ArcLength(contour, true);
			double circularity = 4 * Math.PI * area / (perimeter * perimeter);

			if (circularity > 0.85) // 接近1表示圆形
			{
				RotatedRect ellipse = Cv2.FitEllipse(contour);
				// 绘制平滑后的圆
				//Cv2.Ellipse(contourImage, ellipse, Scalar.White, 10);
			}
			else if (approx.Length == 4 && Math.Abs(Cv2.ContourArea(approx) - area) < area * 0.1)
			{
				// 绘制平滑后的矩形
				//Cv2.Polylines(contourImage, new[] { approx }, true, Scalar.White, 10);
				result.Add(approx);
			}
			else
			{
				var smoothed = SmoothCurve(contour);
				result.Add(smoothed);
				//Cv2.Polylines(contourImage, new[] { smoothed }, false, Scalar.White, 10);
			}
			//string path = Path.Combine(mFolder, $"PreProcessContours{count:000}.png");
			//Cv2.ImWrite(path, contourImage);
			count++;
		}

		return result;
	}

	private static Point[] SmoothCurve(Point[] contour, int windowSize = 5)
	{
		List<Point> smoothed = new List<Point>();
		for (int i = 0; i < contour.Length; i++)
		{
			// 取滑动窗口内的点求平均
			int start = Math.Max(0, i - windowSize / 2);
			int end = Math.Min(contour.Length - 1, i + windowSize / 2);
			float x = 0, y = 0;
			for (int j = start; j <= end; j++)
			{
				x += contour[j].X;
				y += contour[j].Y;
			}
			smoothed.Add(new Point(
				(int)(x / (end - start + 1)),
				(int)(y / (end - start + 1))
			));
		}
		return smoothed.ToArray();
	}

	private static bool IsCircle(Point[] contour, out RotatedRect ellipse)
	{
		double area = Cv2.ContourArea(contour);
		double perimeter = Cv2.ArcLength(contour, true);
		double circularity = 4 * Math.PI * area / (perimeter * perimeter);

		if (circularity > 0.85) // 接近1表示圆形
		{
			ellipse = Cv2.FitEllipse(contour);
			return true;
			// 绘制平滑后的圆
			//Cv2.Ellipse(img, ellipse, Scalar.Red, 2);
		}
		ellipse = new RotatedRect();
		return false;
	}






	public static Mat DrawDiscreteContours(Mat boundaryMask, List<Point[]> contours)
	{
		// 创建三通道结果图
		Mat result = new Mat(boundaryMask.Size(), MatType.CV_8UC3, Scalar.Black);

		// 为每个轮廓生成唯一颜色
		Random rnd = new Random();
		Scalar[] colors = contours.Select(_ =>
			new Scalar(rnd.Next(100, 255), rnd.Next(100, 255), rnd.Next(100, 255))
		).ToArray();

		// 绘制所有轮廓（确保不连接）
		for (int i = 0; i < contours.Count; i++)
		{
			// 方法1：直接绘制（可能轻微重叠）
			Cv2.DrawContours(
				result,
				contours,
				contourIdx: i,
				color: colors[i],
				thickness: 2,
				lineType: LineTypes.AntiAlias
			);
			Cv2.ImWrite(Path.Combine(mFolder, $"DrawDiscreteContours_{i}.png"), result);
		}

		return result;
	}


	private static CircleSegment FitCircleRANSAC(Point[] points, int iterations, double threshold)
	{
		Random rnd = new Random();
		CircleSegment bestCircle = new CircleSegment();
		double bestScore = double.MaxValue;

		for (int i = 0; i < iterations; i++)
		{
			// 随机采样3点
			var samples = points.OrderBy(x => rnd.Next()).Take(3).ToArray();

			// 计算圆参数 (几何法)
			var circle = CalculateCircleFrom3Points(samples[0], samples[1], samples[2]);

			// 评估拟合质量
			double score = points.Average(p =>
				Math.Abs(Distance(p, circle.Center) - circle.Radius));

			if (score < bestScore)
			{
				bestScore = score;
				bestCircle = circle;
			}
		}
		return bestCircle;
	}


	// 几何计算辅助方法
	private static double Distance(Point p, Point2f center) =>
		Math.Sqrt(Math.Pow(p.X - center.X, 2) + Math.Pow(p.Y - center.Y, 2));

	private static double CalculateAngle(Point2f center, Point p) =>
		Math.Atan2(p.Y - center.Y, p.X - center.X);

	private static double CalculateArcFitError(Point[] points, CircleSegment circle) =>
		points.Average(p => Math.Pow(Distance(p, circle.Center) - circle.Radius, 2));

	private static CircleSegment CalculateCircleFrom3Points(Point p1, Point p2, Point p3)
	{
		// 计算垂直平分线交点
		Point2f mid1 = new Point2f((p1.X + p2.X) / 2f, (p1.Y + p2.Y) / 2f);
		Point2f mid2 = new Point2f((p2.X + p3.X) / 2f, (p2.Y + p3.Y) / 2f);

		float slope1 = -(p2.X - p1.X) / (float)(p2.Y - p1.Y);
		float slope2 = -(p3.X - p2.X) / (float)(p3.Y - p2.Y);

		float centerX = (mid2.Y - mid1.Y + slope1 * mid1.X - slope2 * mid2.X) / (slope1 - slope2);
		float centerY = mid1.Y + slope1 * (centerX - mid1.X);

		return new CircleSegment
		{
			Center = new Point2f(centerX, centerY),
			Radius = (float)Math.Sqrt(Math.Pow(centerX - p1.X, 2) + Math.Pow(centerY - p1.Y, 2))
		};
	}


	public static Point[] SmoothContour(Point[] contour,
									  double simplificationRatio = 0.02,
									  bool useCurveFitting = false,
									  Size? imageSize = null)
	{
		// Step 1: 过滤极小轮廓（可选）
		if (contour.Length < 5)
			return contour;

		// Step 2: 轮廓简化
		double epsilon = simplificationRatio * Cv2.ArcLength(contour, true);
		Point[] simplified = Cv2.ApproxPolyDP(contour, epsilon, true);

		// Step 3: 可选曲线拟合
		if (useCurveFitting && simplified.Length > 10)
		{
			simplified = FitCurveToContour(simplified);
		}

		// Step 4: 后处理优化
		return PostProcessContour(simplified, imageSize);
	}

	private static Point[] FitCurveToContour(Point[] contour)
	{
		List<Point> smoothedPoints = new List<Point>();
		int segmentSize = Math.Max(20, contour.Length / 5);

		for (int i = 0; i < contour.Length; i += segmentSize)
		{
			int endIdx = Math.Min(i + segmentSize, contour.Length);
			int currentSize = endIdx - i;
			if (currentSize < 3) continue;

			// 提取并处理分段
			Point[] segment = new Point[currentSize];
			Array.Copy(contour, i, segment, 0, currentSize);

			// 转换为相对坐标（避免数值溢出）
			Point refPoint = segment[0];
			double[] t = segment.Select((_, idx) => (double)idx).ToArray();
			double[] x = segment.Select(p => (double)(p.X - refPoint.X)).ToArray();
			double[] y = segment.Select(p => (double)(p.Y - refPoint.Y)).ToArray();

			// 三次样条拟合
			var spline = MathNet.Numerics.Interpolation.CubicSpline.InterpolateAkimaSorted(t, y);

			// 生成平滑点（双倍密度）
			for (double ti = 0; ti < currentSize; ti += 0.5)
			{
				int px = refPoint.X + (int)Math.Round(x[0] + ti * (x[currentSize - 1] - x[0]) / (currentSize - 1));
				int py = refPoint.Y + (int)Math.Round(spline.Interpolate(ti));
				smoothedPoints.Add(new Point(px, py));
			}
		}

		return smoothedPoints.Count > 0 ? smoothedPoints.ToArray() : contour;
	}

	private static Point[] PostProcessContour(Point[] contour, Size? imageSize = null)
	{
		// 方法1：几何平滑（快速）
		if (!imageSize.HasValue)
		{
			// 基于顶点距离的简化
			double minDist = Cv2.ArcLength(contour, true) * 0.01;
			return contour.Where((p, i) =>
				i == 0 ||
				i == contour.Length - 1 ||
				Point.Distance(p, contour[i - 1]) > minDist
			).ToArray();
		}

		// 方法2：基于图像形态学的精确处理（需要图像尺寸）
		else
		{
			// 在临时图像上处理
			using (Mat temp = new Mat(imageSize.Value, MatType.CV_8UC1, Scalar.Black))
			{
				Cv2.DrawContours(temp, new[] { contour }, -1, Scalar.White, -1);

				// 形态学操作
				var kernel = Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(3, 3));
				Cv2.MorphologyEx(temp, temp, MorphTypes.Close, kernel);

				// 重新提取轮廓
				var processedContours = Cv2.FindContoursAsArray(
					temp,
					RetrievalModes.External,
					ContourApproximationModes.ApproxSimple
				);

				return processedContours.Length > 0 ? processedContours[0] : contour;
			}
		}
	}




	public static void SaveIndividualContours(Mat boundaryMask, string outputFolder)
	{
		// 确保输出目录存在
		Directory.CreateDirectory(outputFolder);

		// 获取轮廓及层级信息
		Point[][] contours;
		HierarchyIndex[] hierarchy;
		Cv2.FindContours(
			boundaryMask.Clone(),
			out contours,
			out hierarchy,
			RetrievalModes.Tree,
			ContourApproximationModes.ApproxSimple
		);

		// 创建纯黑背景模板
		Mat blank = new Mat(boundaryMask.Size(), MatType.CV_8UC1, Scalar.Black);

		// 处理每个轮廓
		for (int i = 0; i < contours.Length; i++)
		{
			// 创建当前轮廓的图像
			Mat contourImage = blank.Clone();

			// 绘制当前轮廓（白色填充）
			Cv2.DrawContours(
				contourImage,
				contours,
				contourIdx: i,
				color: Scalar.White,
				thickness: -1,  // 填充轮廓内部
				lineType: LineTypes.AntiAlias,
				hierarchy: hierarchy,
				maxLevel: 0  // 只绘制当前轮廓
			);

			// 保存为PNG文件
			string path = Path.Combine(outputFolder, $"contour_{i:000}.png");
			Cv2.ImWrite(path, contourImage);
		}
	}
}
public struct ArcSegment
{
	public CircleSegment Circle;
	public double StartAngle;
	public double EndAngle;
	public Point[] Points;
}
// 辅助结构体
public struct CircleSegment
{
	public Point2f Center;
	public float Radius;
}

public struct ContourSegment
{
	public bool IsArc;
	public CircleSegment Circle;
	public double StartAngle;
	public double EndAngle;
	public Point[] Points;
}
