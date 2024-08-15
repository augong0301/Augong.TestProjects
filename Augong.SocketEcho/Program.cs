// See https://aka.ms/new-console-template for more information
using Augong.SocketEcho;

Console.WriteLine("Hello, World!");
var listener = new SocketEchoServer();
listener.Init(3741);
listener.StartListener(99);
Console.ReadKey();
listener.StopListener();