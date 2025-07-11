using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Augong.TxtReader
{
	public class DefectInfo
	{
		public DefectInfo()
		{
		}

		public DefectInfo(int id)
		{
			ID = id;
		}

		[JsonSerializableProperty("ID")]
		public int ID { get; set; }

		[JsonSerializableProperty("DefectType")]
		public int DefectType { get; set; }

		[JsonSerializableProperty("FrameID")]
		public int FrameID { get; set; }

		[JsonSerializableProperty("Channel")]
		public string Channel { get; set; }

		[JsonSerializableProperty("InDie")]
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public bool InDie { get; set; }
		[JsonSerializableProperty("DieIndex")]
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public int DieIndex { get; set; }
		[JsonSerializableProperty("FieldIndexX")]
		public int FieldIndexX { get; set; }
		[JsonSerializableProperty("FieldIndexY")]
		public int FieldIndexY { get; set; }
		[JsonSerializableProperty("SectorID")]
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public int SectorID { get; set; }


		//based on Scan coordinator, right is Xpositive and up is Ypositive (Theta ignored)
		[JsonSerializableProperty("CenterX")]
		public double CenterX { get; set; }
		[JsonSerializableProperty("CenterY")]
		public double CenterY { get; set; }
		//based on wafer coordinator (Theta corrected, and primary flat is Ynegtive(semi) )
		[JsonSerializableProperty("CorrectedCenterX")]
		public double CorrectedCenterX { get; set; } = double.NaN;
		[JsonSerializableProperty("CorrectedCenterY")]
		public double CorrectedCenterY { get; set; } = double.NaN;

		//based on WaferMap coordinator
		[JsonSerializableProperty("InDieCenterX")]
		public double InDieCenterX { get; set; }
		[JsonSerializableProperty("InDieCenterY")]
		public double InDieCenterY { get; set; }
		[JsonSerializableProperty("IsCluster")]
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public bool IsCluster { get; set; }
		[JsonSerializableProperty("Width")]
		public double Width { get; set; }
		[JsonSerializableProperty("Height")]
		public double Height { get; set; }
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		[JsonSerializableProperty("ActualSize")]
		public double ActualSize { get; set; }
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		[JsonSerializableProperty("Depth")]
		public double Depth { get; set; }
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		[JsonSerializableProperty("Inclination")]
		public double Inclination { get; set; }
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		[JsonSerializableProperty("RadialDistance")]
		public double RadialDistance { get; set; }
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		[JsonSerializableProperty("RadialAngle")]
		public double RadialAngle { get; set; }
		[JsonSerializableProperty("Length")]
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public double Length { get; set; }
		[JsonSerializableProperty("Density")]
		public double Density { get; set; }
		[JsonSerializableProperty("GrayScale")]
		public double GrayScale { get; set; }
		[JsonSerializableProperty("Score")]
		public double Score { get; set; }
		[JsonSerializableProperty("Area")]
		public double Area { get; set; }

		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public bool IsExcluded { get; set; }

		//based on Scan coordinator (Theta ignored)
		[JsonSerializable(Handler = typeof(ArrayJsonSerializationHandler<double>))]
		public double[] BoundingMask { get; set; }

		public string GetDiePos(bool includeDieIndex = true)
		{
			if (!InDie)
			{
				return "";
			}

			if (includeDieIndex)
			{
				return $"({FieldIndexX}, {FieldIndexY}, {DieIndex})";
			}

			return $"({FieldIndexX}, {FieldIndexY})";
		}

		public static bool operator ==(DefectInfo left, DefectInfo right)
		{
			if (left is null)
			{
				return right is null;
			}
			return left.Equals(right);
		}

		public static bool operator !=(DefectInfo left, DefectInfo right)
		{
			return !(left == right);
		}

		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is DefectInfo defect))
			{
				return false;
			}

			bool equal = DefectType == defect.DefectType &&
						 Channel == defect.Channel &&
						 CenterX == defect.CenterX &&
						 CenterY == defect.CenterY &&
						 Width == defect.Width &&
						 Height == defect.Height &&
						 Score == defect.Score &&
						 Area == defect.Area &&
						 IsExcluded == defect.IsExcluded;

			return equal;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hash = 17;
				hash = hash * 23 + DefectType.GetHashCode();
				hash = hash * 23 + Channel == null ? 0 : Channel.GetHashCode();
				hash = hash * 23 + CenterX.GetHashCode();
				hash = hash * 23 + CenterY.GetHashCode();
				hash = hash * 23 + Width.GetHashCode();
				hash = hash * 23 + Height.GetHashCode();
				hash = hash * 23 + Score.GetHashCode();
				hash = hash * 23 + Area.GetHashCode();
				hash = hash * 23 + IsExcluded.GetHashCode();
				return hash;
			}
		}

		public DefectInfo Copy()
		{
			return new DefectInfo()
			{
				ID = ID,
				DefectType = DefectType,
				FrameID = FrameID,
				Channel = Channel,
				CenterX = CenterX,
				CenterY = CenterY,
				CorrectedCenterX = CorrectedCenterX,
				CorrectedCenterY = CorrectedCenterY,
				IsCluster = IsCluster,
				Width = Width,
				Height = Height,
				ActualSize = ActualSize,
				Depth = Depth,
				Inclination = Inclination,
				RadialDistance = RadialDistance,
				Length = Length,
				Density = Density,
				GrayScale = GrayScale,
				Score = Score,
				Area = Area,
				IsExcluded = IsExcluded,
				BoundingMask = BoundingMask,
			};
		}
	}

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public class JsonSerializableAttribute : Attribute
	{
		public Type Handler { get; set; }
	}

	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public class JsonSerializablePropertyAttribute : Attribute
	{
		public JsonSerializablePropertyAttribute(string name)
		{
			Name = name;
		}

		public string Name { get; private set; }
		public bool Explicit { get; set; } = true;
		public object DefaultValue { get; set; }
	}

	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public class JsonSerializableCollectionAttribute : Attribute
	{
		public JsonSerializableCollectionAttribute(Type collectionType)
		{
			CollectionType = collectionType;
		}

		public Type CollectionType { get; private set; }
	}

	public interface IJsonSerializationHandler<T>
	{
		JToken Serialize(T obj);

		T Deserialize(JToken element);
	}

	public class ArrayJsonSerializationHandler<T> : IJsonSerializationHandler<T[]>
	{
		public JToken Serialize(T[] obj)
		{
			if (obj == null)
			{
				return null;
			}

			var builder = new StringBuilder();

			if (obj.Length > 0)
			{
				builder.Append($"{obj[0]}");
			}

			for (int i = 1; i < obj.Length; i++)
			{
				builder.Append($",{obj[i]}");
			}

			return new JValue($"[{builder.ToString()}]");
		}

		public T[] Deserialize(JToken element)
		{
			var text = element.ToObject<string>();

			if (string.IsNullOrWhiteSpace(text))
			{
				return new T[] { };
			}

			var list = new List<T>();

			foreach (var segment in text.Trim('[', ']').Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
			{
				list.Add((T)Convert.ChangeType(segment, typeof(T)));
			}

			return list.ToArray();
		}
	}
}
