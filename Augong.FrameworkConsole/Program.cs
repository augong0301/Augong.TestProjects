using Augong.Framework.Shared;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Augong.FrameworkConsole
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var s = new SomeHelper();
            var rst = s.DoSomething("ccccc");
            var c= JsonConvert.SerializeObject(rst);

            var assembly = Assembly.GetExecutingAssembly();
            var referencedAssemblies = assembly.GetReferencedAssemblies();

            foreach (var referencedAssembly in referencedAssemblies)
            {
                Console.WriteLine($"\tReferenced Assembly: {referencedAssembly.Name}, Version: {referencedAssembly.Version}");
            }
        }
        private static void DoReadOnlyServerTest()
        {
            var cl = new ReadOnlyClient();
            string input = string.Empty;
            Console.WriteLine("ip =");
            var ip = "192.168.42.100";
            //var ip = "127.0.0.1";
            Console.WriteLine("port =");
            var port = 23;
            var suc = cl.Connect(ip, port);
            Console.WriteLine($"Connected {ip} : {port} is {suc}");
            Console.WriteLine("Enter your commands");
            while (input != null)
            {
                input = Console.ReadLine();
                TryReceive(cl, input);
            }

            Console.ReadKey();
        }

        public static void TryReceive(ReadOnlyClient cl, string input)
        {
            cl.DoSend(input);
            Console.WriteLine($"Send = {input}");
            var cts = new CancellationTokenSource();
            cl.TryReceive(cts);

        }

        public static void DoReceive(ReadOnlyClient cl, string input)
        {
            cl.DoSend(input);

            bool end = false;
            int count = 0;
            var lines = new List<string>();
            var rtn = cl.DoReceive();
            count++;
            end = rtn.Contains("MFOCUS");
            Console.WriteLine("Received = " + rtn);
            //lines.AddRange(l);
            foreach (var l in lines)
            {
                Console.WriteLine(l);
            }
            Console.WriteLine($"Receive data done by{count}");
        }
    }
}
