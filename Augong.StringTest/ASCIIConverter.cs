using System.Text;

namespace Augong.StringTest
{
	public static class ASCIIConverter
	{
		public static string ConvertToASCII(string command)
		{
			var ascBytes = Encoding.ASCII.GetBytes(command);
			return Encoding.ASCII.GetString(ascBytes);
		}
	}
}
