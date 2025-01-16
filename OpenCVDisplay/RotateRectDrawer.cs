using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using JsonConverter = Newtonsoft.Json.JsonConverter;
using JsonConverterAttribute = Newtonsoft.Json.JsonConverterAttribute;

namespace Augong.OpenCVDisplay
{
	public class RotateRectDrawer
	{
		public RotateRectDrawer()
		{

		}

		public void DrawCracksInCanvas()
		{
			var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "defects.json");

			var data = JsonConvert.DeserializeObject<DefectPackage>(File.ReadAllText(path));

			var cracks = data.Result.Where(t => t.DefectType.Contains("DF Crack")).ToList();
			var height = data.ImgHeight * 16;
			var width = data.ImgWidth * 16;
			var img = new Mat(height, width, MatType.CV_8UC3, new Scalar(255, 255, 255));

			foreach (var item in cracks)
			{
				var polygonPoints = new List<Point>();

				for (int i = 0; i < item.Polygon.Count; i += 2)
				{
					int x = (int)item.Polygon[i];
					int y = (int)item.Polygon[i + 1];
					polygonPoints.Add(new Point(x + height / 2, y + width / 2));
				}

				// 将坐标绘制成多边形
				//var polygonArray = polygonPoints.ToArray();
				//var color = new Scalar(0, 0, 255); // 红色
				//var thickness = 2; // 线宽
				//Cv2.Polylines(img, new[] { polygonArray }, isClosed: true, color: color, thickness: thickness);

				var rect = Cv2.MinAreaRect(polygonPoints);
				rect.Size = new Size2f(rect.Size.Width + 10 * 2, rect.Size.Height + 10 * 2);

				// 获取矩形的四个角点
				Point2f[] boxPoints = Cv2.BoxPoints(rect);

				// 将角点转换为整数
				Point[] intBoxPoints = Array.ConvertAll(boxPoints, point => new Point((int)point.X, (int)point.Y));

				// 绘制最小外接矩形
				Cv2.Polylines(img, new[] { intBoxPoints }, isClosed: true, color: new Scalar(0, 255, 0), thickness: 2);
			}
			//Cv2.ImShow("Polygon Image", img);
			Cv2.ImWrite(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "defects.png"), img);
		}
	}


	public class Defect
	{

		[JsonProperty("area")]
		public double Area { get; set; }
		public string Bbox { get; set; }
		[JsonProperty("defect_id")]
		public int DefectId { get; set; }
		[JsonProperty("defect_type")]
		public string DefectType { get; set; }
		public double GrayScale { get; set; }
		public string ImgName { get; set; }
		[JsonProperty("polygon"), JsonConverter(typeof(PolygonConverter))]
		public List<double> Polygon { get; set; }
		public double Score { get; set; }
	}

	public class DefectPackage
	{
		[JsonProperty("img_height")]
		public int ImgHeight { get; set; }
		[JsonProperty("img_width")]
		public int ImgWidth { get; set; }
		public bool Last { get; set; }
		public List<Defect> Result { get; set; }
	}


	public class PolygonConverter : Newtonsoft.Json.JsonConverter<List<double>>
	{
		public override List<double> ReadJson(JsonReader reader, Type objectType, List<double> existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			var polygonString = (string)JToken.Load(reader);

			// 将字符串转换为 List<double>
			var polygonList = new List<double>();

			// 清除掉字符串中的括号
			var numbers = polygonString.Trim('[', ']').Split(',');

			foreach (var number in numbers)
			{
				if (double.TryParse(number.Trim(), out double value))
				{
					polygonList.Add(value);
				}
			}

			return polygonList;
		}

		public override void WriteJson(JsonWriter writer, List<double> value, JsonSerializer serializer)
		{
			writer.WriteStartArray();
			foreach (var num in value)
			{
				writer.WriteValue(num);
			}
			writer.WriteEndArray();
		}
	}
}
