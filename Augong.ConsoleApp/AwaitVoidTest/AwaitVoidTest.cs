using System;
using System.Threading;
using System.Threading.Tasks;
using Augong.CSharp.Contract;

namespace Augong.ConsoleApp
{
	public class AwaitVoidTest : ITest
	{

		public void DoTest()
		{
			AsyncVoidExceptions_CannotBeCaughtByCatch();
		}

		private async void RunSomeTask()
		{
			var task = Task.Run(() =>
			{
				Thread.Sleep(10000);
			});
			await Task.WhenAll(task);

			Console.WriteLine("done task");
		}
		public void AsyncVoidExceptions_CannotBeCaughtByCatch()
		{
			try
			{
				RunSomeTask();
				Console.WriteLine("After some task");
			}
			catch (Exception ex)
			{
				// The exception is never caught here!
				throw ex;
			}
		}
	}
}
