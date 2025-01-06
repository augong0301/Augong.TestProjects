#if NET80
using System;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Threading.Tasks;

namespace Augong.WCF
{
	public class WCFTest
	{


	}
	class ServiceHosting
	{
		public ServiceHosting()
		{

		}
		private ServiceHost mHost;


		public string BaseAddress { get; set; } = "net.tcp://localhost:8088/Augong";

		public void RegisterService<T, K>() where K : class
		{
			var uri = new Uri($"{BaseAddress}/Augong");


			var address = new EndpointAddress(uri);
			var binding = new NetTcpBinding();

			binding.ReceiveTimeout = TimeSpan.MaxValue;
			binding.MaxReceivedMessageSize = 200000000;
			binding.MaxBufferPoolSize = 200000000;

		}

		public async Task StartServiceAsync()
		{
			try
			{

				await Task.CompletedTask;
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		public async Task StopServiceAsync()
		{
			try
			{
				await Task.CompletedTask;
			}
			catch (Exception ex)
			{
			}
		}
	}
}
#endif
#if NETFRAMEWORK


using System;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Threading.Tasks;

public class ServiceHosting
{
	public ServiceHosting()
	{
	}

	private ServiceHost mHost;

	public string BaseAddress { get; set; } = "net.tcp://localhost:8088/augong";

	public void RegisterService<T, K>() where K : class
	{
		var uri = new Uri($"{BaseAddress}/augong/");

		mHost = new ServiceHost(typeof(K), uri);

		var contract = ContractDescription.GetContract(typeof(T), typeof(K));
		var address = new EndpointAddress(uri);
		var binding = new NetTcpBinding();

		binding.ReceiveTimeout = TimeSpan.MaxValue;
		binding.MaxReceivedMessageSize = 200000000;
		binding.MaxBufferPoolSize = 200000000;

		mHost.AddServiceEndpoint(new ServiceEndpoint(contract, binding, address));

	}

	public async Task StartServiceAsync()
	{
		try
		{
			mHost?.Open();

			await Task.CompletedTask;
		}
		catch (Exception ex)
		{
		}
	}

	public async Task StopServiceAsync()
	{
		try
		{
			mHost?.Close();

			await Task.CompletedTask;
		}
		catch (Exception ex)
		{
		}
	}
}
#endif

