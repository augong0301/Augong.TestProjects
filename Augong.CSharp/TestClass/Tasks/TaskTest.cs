using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Augong.CSharp.TestClass.Tasks
{
	public class TaskTest : ITest
	{
		private CancellationTokenSource cts = new CancellationTokenSource();
		public void DoTest()
		{

			try
			{
				var ts = new List<Task>();
				var task1 = Task.Factory.StartNew(() => { DoJob1(); }, TaskCreationOptions.LongRunning );
				var task2 = Task.Factory.StartNew(() => { DoJob2(); },  TaskCreationOptions.LongRunning);

				task1.ContinueWith(t => { Console.WriteLine("TASK 1 done , continue"); }).Wait();
				var completed =  task2.Wait(1000 * 10);
				if (!completed)
				{
					cts.Cancel();
				}
				Console.WriteLine($"passed wait , task2 completed status = {completed}");

			}
			catch (AggregateException ex)
			{
				Console.WriteLine("Catch AggregateException");
				foreach (var innerEx in ex.InnerExceptions)
				{
					Console.WriteLine(innerEx.Message);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Catch Exception: " + ex.Message);
			}

			Console.ReadLine();
		}

		private void DoJob2()
		{
			try
			{
				var index = 0;
				while (index < 30 && !cts.IsCancellationRequested)
				{
					Thread.Sleep(1000);
					index++;
					Console.WriteLine("Job2 running");
				}
				Console.WriteLine("Job2 done");
			}
			catch (OperationCanceledException)
			{
				Console.WriteLine("Job2 cancelled");
			}
		}

		private void DoJob1()
		{
			Thread.Sleep(4000);
			throw new Exception("job1 ex");
		}
	}
}
