using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Mulan.UI.Recipes.Arcturus
{
	class ChannelToStringConverter : IValueConverter
	{
		public static string[] DefaultChannel { get; } = { "Default" };

		public static string[] GetAvailableChannels(ScanTechnology[] Technologies)
		{
			var list = Technologies.Select(t => t.ToString()).ToList();

			list.Insert(0, "Default");

			return list.ToArray();
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return (int)value == 0 ? "Default" : ((ScanTechnology)value).ToString();
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (Enum.TryParse<ScanTechnology>((string)value, out var channel))
			{
				return (int)channel;
			}

			return 0;
		}
	}

	public enum ScanTechnology
	{
		BF = 1,
		DF = 2,
		BF_R = 3,
		BF_G = 4,
		BF_B = 5,
		PL = 6,
		BF_TR = 7,
	}
}
