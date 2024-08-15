using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SocketTest
{
	public class ReadOnlyClient
	{
		private Socket client;

		public ReadOnlyClient()
		{
			client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		}

		public bool Connect(string _ip, int port)
		{
			if (client.Connected) return true;
			try
			{
				IPAddress ip = IPAddress.Parse(_ip);
				IPEndPoint point = new IPEndPoint(ip, port);
				client.SendTimeout = 10000;
				client.Connect(point);
			}
			catch (Exception)
			{
				return false;
			}
			return true;
		}


		public string DoReceive()
		{
			string str = "";
			try
			{
				byte[] buffer = new byte[1024 * 1024 * 2];
				int r = client.Receive(buffer);
				if (r != 0)
				{
					str = Encoding.ASCII.GetString(buffer, 0, r);
				}
			}
			catch (Exception ex)
			{
				throw;
			}
			return str;
		}
	}
}
