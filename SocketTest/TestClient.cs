using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Augong.SocketTest
{
	public class TestClient
	{
		private TcpClient client;
		public TestClient()
		{

		}

		public void Connect(string address, int port)
		{
			client = new TcpClient();
			var ad = IPAddress.Parse(address);
			client.Connect(ad, port);
		}

		public bool IsConnected => client.Connected;

		public void Close()
		{
			client?.Close();
			client?.Dispose();
		}

		public string SendAndGetBytes(string msg)
		{
			var buffer = new byte[1024];
			var bytes = Encoding.UTF8.GetBytes(msg);
			client.GetStream().Write(bytes);
			Task.Run(() => { });
			while (client.GetStream().Read(buffer, 0, buffer.Length) < 4)
			{
				Thread.Sleep(100);
			}
			return Encoding.UTF8.GetString(buffer);
		}

		public string GetString()
		{
			var str = string.Empty;
			var bytes = new byte[1024];
			var length = client.GetStream().Read(bytes);
			return Encoding.ASCII.GetString(bytes, 0, length);
		}

	}
}
