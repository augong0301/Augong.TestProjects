using System;
using Augong.CSharp.Contract;

namespace Augong.ConsoleApp
{
	public class AwaitVoidTest : ITest
	{

		public void DoTest()
		{
			AsyncVoidExceptions_CannotBeCaughtByCatch();
		}

		private async void ThrowExceptionAsync()
		{
			throw new InvalidOperationException();
		}
		public void AsyncVoidExceptions_CannotBeCaughtByCatch()
		{
			try
			{
				ThrowExceptionAsync();
			}
			catch (Exception ex)
			{
				// The exception is never caught here!
				throw ex;
			}
		}
	}
}
