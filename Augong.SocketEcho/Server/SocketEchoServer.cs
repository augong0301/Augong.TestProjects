﻿using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Augong.SocketEcho
{
    public class SocketEchoServer
    {
        public SocketEchoServer()
        {

        }
        private readonly CancellationTokenSource cts = new CancellationTokenSource();

        public Socket Listener;

        public void Init(int port)
        {
            Listener = new Socket(SocketType.Stream, ProtocolType.Tcp);
            Listener.Bind(new IPEndPoint(IPAddress.Loopback, port));
        }

        public async void StartListener(int max)
        {
            Listener.Listen(max);
            var socket = Listener.Accept();
            var stm = new NetworkStream(socket);
            while (!cts.IsCancellationRequested)
            {
                var buffer = new byte[1024 * 1024];
                var len = stm.Read(buffer);
                if (len > 0)
                {
                    if (!IsCommand(buffer))
                    {
                        stm.Write(buffer);
                    }
                    else
                    {
                        SendSimData(stm);
                    }
                }

            }
        }
        private void SendSimData(NetworkStream stm)
        {
            var data = " 0.0000 2.6000 0.5997 -0.5997 0.0000 0.0000 0.0000 0.0717\r\n 1.0000 2.6000 0.6097 -0.6097 0.0000 0.0000 0.0000 0.0717\r\n 2.0000 2.6000 0.6197 -0.6197 0.0000 0.0000 0.0000 0.0717\r\n 3.0000 2.6000 0.6297 -0.6297 0.0000 0.0000 0.0000 0.0717\r\n 4.0000 2.6000 0.6396 -0.6396 0.0000 0.0000 0.0000 0.0717\r\n 5.0000 2.6000 0.6496 -0.6496 0.0000 0.0000 0.0000 0.0717\r\n 6.0000 2.6000 0.6596 -0.6596 0.0000 0.0000 0.0000 0.0717\r\n 7.0000 2.6000 0.6696 -0.6696 0.0000 0.0000 0.0000 0.0717\r\n 8.0000 2.6000 0.6796 -0.6796 0.0000 0.0000 0.0000 0.0717\r\n 9.0000 2.6000 0.6896 -0.6896 0.0000 0.0000 0.0000 0.0717\r\n 10.0000 2.6000 0.6996 -0.6996 0.0000 0.0000 0.0000 0.0717\r\n 11.0000 2.6000 0.7096 -0.7096 0.0000 0.0000 0.0000 0.0717\r\n 12.0000 2.6000 0.7196 -0.7196 0.0000 0.0000 0.0000 0.0717\r\n 13.0000 2.6000 0.7296 -0.7296 0.0000 0.0000 0.0000 0.0717\r\n 14.0000 2.6000 0.7396 -0.7396 0.0000 0.0000 0.0000 0.0717\r\n 15.0000 2.6000 0.7496 -0.7496 0.0000 0.0000 0.0000 0.0717\r\n 16.0000 2.6000 0.7596 -0.7596 0.0000 0.0000 0.0000 0.0717\r\n 17.0000 2.6000 0.7696 -0.7696 0.0000 0.0000 0.0000 0.0717\r\n 18.0000 2.6000 0.7796 -0.7796 0.0000 0.0000 0.0000 0.0717\r\n 19.0000 2.6000 0.7896 -0.7896 0.0000 0.0000 0.0000 0.0717\r\n 20.0000 2.6000 0.7996 -0.7996 0.0000 0.0000 0.0000 0.0717\r\n 21.0000 2.6000 0.8096 -0.8096 0.0000 0.0000 0.0000 0.0717\r\n 22.0000 2.6000 0.8195 -0.8195 0.0000 0.0000 0.0000 0.0717\r\n 23.0000 2.6000 0.8295 -0.8295 0.0000 0.0000 0.0000 0.0717\r\n 24.0000 2.6000 0.8395 -0.8395 0.0000 0.0000 0.0000 0.0717\r\n 25.0000 2.6000 0.8495 -0.8495 0.0000 0.0000 0.0000 0.0717\r\n 26.0000 2.6000 0.8595 -0.8595 0.0000 0.0000 0.0000 0.0717\r\n 27.0000 2.6000 0.8695 -0.8695 0.0000 0.0000 0.0000 0.0717\r\n 28.0000 2.6000 0.8795 -0.8795 0.0000 0.0000 0.0000 0.0717\r\n 29.0000 2.6000 0.8895 -0.8895 0.0000 0.0000 0.0000 0.0717\r\n 30.0000 2.6000 0.8995 -0.8995 0.0000 0.0000 0.0000 0.0717\r\n 31.0000 2.6000 0.9095 -0.9095 0.0000 0.0000 0.0000 0.0717\r\n 32.0000 2.6000 0.9195 -0.9195 0.0000 0.0000 0.0000 0.0717\r\n 33.0000 2.6000 0.9295 -0.9295 0.0000 0.0000 0.0000 0.0717\r\n 34.0000 2.6000 0.9395 -0.9395 0.0000 0.0000 0.0000 0.0717\r\n 35.0000 2.6000 0.9495 -0.9495 0.0000 0.0000 0.0000 0.0717\r\n 36.0000 2.6000 0.9595 -0.9595 0.0000 0.0000 0.0000 0.0717\r\n 37.0000 2.6000 0.9695 -0.9695 0.0000 0.0000 0.0000 0.0717\r\n 38.0000 2.6000 0.9795 -0.9795 0.0000 0.0000 0.0000 0.0717\r\n 39.0000 2.6000 0.9895 -0.9895 0.0000 0.0000 0.0000 0.0717\r\n 40.0000 2.6000 0.9995 -0.9995 0.0000 0.0000 0.0000 0.0717\r\n 41.0000 2.6000 1.0094 -1.0094 0.0000 0.0000 0.0000 0.0717\r\n 42.0000 2.6000 1.0194 -1.0194 0.0000 0.0000 0.0000 0.0717\r\n 43.0000 2.6000 1.0294 -1.0294 0.0000 0.0000 0.0000 0.0717\r\n 44.0000 2.6000 1.0394 -1.0394 0.0000 0.0000 0.0000 0.0717\r\n 45.0000 2.6000 1.0494 -1.0494 0.0000 0.0000 0.0000 0.0717\r\n 46.0000 2.6000 1.0594 -1.0594 0.0000 0.0000 0.0000 0.0717\r\n 47.0000 2.6000 1.0694 -1.0694 0.0000 0.0000 0.0000 0.0717\r\n 48.0000 2.6000 1.0794 -1.0794 0.0000 0.0000 0.0000 0.0717\r\n 49.0000 2.6000 1.0894 -1.0894 0.0000 0.0000 0.0000 0.0717\r\n 50.0000 2.6000 1.0994 -1.0994 0.0000 0.0000 0.0000 0.0717\r\n 51.0000 2.6000 1.1094 -1.1094 0.0000 0.0000 0.0000 0.0717\r\n 52.0000 2.6000 1.1194 -1.1194 0.0000 0.0000 0.0000 0.0717\r\n 53.0000 2.6000 1.1294 -1.1294 0.0000 0.0000 0.0000 0.0717\r\n 54.0000 2.6000 1.1394 -1.1394 0.0000 0.0000 0.0000 0.0717\r\n 55.0000 2.6000 1.1494 -1.1494 0.0000 0.0000 0.0000 0.0717\r\n 56.0000 2.6000 1.1594 -1.1594 0.0000 0.0000 0.0000 0.0717\r\n 57.0000 2.6000 1.1694 -1.1694 0.0000 0.0000 0.0000 0.0717\r\n 58.0000 2.6000 1.1794 -1.1794 0.0000 0.0000 0.0000 0.0717\r\n 59.0000 2.6000 1.1893 -1.1893 0.0000 0.0000 0.0000 0.0717\r\nDone FSGL01 Find  125860.0000 RatioPre  100.0000 RatioNow  -100.0000\r\nDone MFOCUS at 52.0000";

            var split = data.Split("\r\n");

                stm.Write(Encoding.ASCII.GetBytes(data));

            //foreach (var sl in split)
            //{
            //    stm.Write(Encoding.ASCII.GetBytes(sl));
            //    Console.WriteLine("Send:" + sl);
            //}

        }

        private bool IsCommand(byte[] buffer)
        {
            var cmd = Encoding.ASCII.GetString(buffer);
            if (cmd.Contains("XQ#Mfocus"))
            {
                return true;
            }
            return false;
        }

        public void StopListener()
        {
            cts.Cancel();
            cts.Dispose();
        }
    }
}
