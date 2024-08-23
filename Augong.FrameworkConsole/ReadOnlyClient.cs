using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Augong.FrameworkConsole
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
				throw;
			}
			return true;
		}

		public void TryReceive(CancellationTokenSource cts)
		{
			int count = 1;
			while (!cts.IsCancellationRequested)
			{
				Console.WriteLine($"Try receive index = {count}");
				var buffer = new byte[1024];
				var len = client.Receive(buffer);
				Console.WriteLine($"Receive length = {len}");
				count++;
				if (len > 0)
				{
					var str = Encoding.ASCII.GetString(buffer);
					Console.WriteLine(Encoding.ASCII.GetString(buffer));
					if (str.Contains("10"))
					{
						break;
					}
				}
				Thread.Sleep(50);
			}
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

		public void DoSend(string command)
		{
			char c = (char)0x0D;
			command += c.ToString();
			byte[] buffer = Encoding.ASCII.GetBytes(command);
			client.Send(buffer);
		}

		public void DoMf()
		{
			var cmd = string.Format("XQ#Mfocus,{0}", 2);
			DoSend(cmd);
		}
	}
}
