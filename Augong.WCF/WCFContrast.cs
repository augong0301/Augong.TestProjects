#if NETFRAMEWORK
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Augong.WCF
{
	[DataContract]
	public class PubSubClientInfo
	{
		[DataMember]
		public string Name { get; set; }
		[DataMember]
		public string Channel { get; set; }
	}

	[ServiceContract(CallbackContract = typeof(IPubSubCallback))]
	public interface IPubSubService
	{
		[OperationContract(IsOneWay = true)]
		void Register(string name);
		[OperationContract(IsOneWay = true)]
		void Unregister(string name);
		[OperationContract(IsOneWay = true)]
		void Subscribe(string name, string channel = null);

		[OperationContract(IsOneWay = true)]
		void Publish(PubSubEvent e);

		[OperationContract(IsOneWay = true)]
		void GetAllClients();

		[OperationContract(IsOneWay = false)]
		bool CheckServiceState();
	}

	public interface IPubSubCallback
	{
		[OperationContract(IsOneWay = true)]
		void OnReceive(PubSubEvent e);

		[OperationContract(IsOneWay = true)]
		void OnDiscover(List<PubSubClientInfo> clients);
	}

	[DataContract]
	public class PubSubEvent
	{
		[DataMember]
		public string Name { get; set; }

		[DataMember]
		public string SourceClientName { get; set; }

		[DataMember]
		public bool IsBroadcasting { get; set; }

		[DataMember]
		public string Channel { get; set; }

		[DataMember]
		public string Content { get; set; }

		public static PubSubEvent Create<T>(T obj)
		{
			var serialization = new XmlSerialization();
			var type = typeof(T);
			var element = serialization.Serialize(obj);

			var pubSubEvent = new PubSubEvent();

			pubSubEvent.Name = type.AssemblyQualifiedName;
			pubSubEvent.Content = element.ToString();

			return pubSubEvent;
		}

		public object ToObject()
		{
			var serialization = new XmlSerialization();

			var type = Type.GetType(Name);
			var element = XElement.Parse(Content);

			return serialization.Deserialize(type, element);
		}
	}
}


#endif