using System;
using System.Xml;
using System.Xml.Serialization;

namespace Augong.ConsoleApp
{


	[XmlRoot("SerializableInterface"), Serializable]
	public class SerializableInterface<TInterface> : IXmlSerializable
	{
		public TInterface Source { get { return (TInterface)mSource; } }
		string mTypeName;
		object mSource;


		public SerializableInterface()
		{

		}

		public SerializableInterface(object interfaceObj)
		{
			this.mSource = interfaceObj;
			mTypeName = mSource.GetType().FullName;
		}

		public System.Xml.Schema.XmlSchema GetSchema()
		{
			return null;
		}

		public void ReadXml(XmlReader reader)
		{
			var typeSer = new XmlSerializer(typeof(string));
			bool wasEmpty = reader.IsEmptyElement;
			reader.Read();

			if (wasEmpty)
				return;
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				reader.ReadStartElement("Type");
				mTypeName = typeSer.Deserialize(reader) as string;

				reader.ReadEndElement();

				var sourceSer = new XmlSerializer(Type.GetType(mTypeName));
				reader.ReadStartElement("Object");
				mSource = sourceSer.Deserialize(reader);
				reader.ReadEndElement();
				reader.MoveToContent();
			}
			reader.ReadEndElement();
		}

		public void WriteXml(XmlWriter writer)
		{
			var typeSer = new XmlSerializer(typeof(string));
			var sourceSer = new XmlSerializer(mSource.GetType());

			writer.WriteStartElement("Type");
			typeSer.Serialize(writer, mTypeName);
			writer.WriteEndElement();
			writer.WriteStartElement("Object");
			sourceSer.Serialize(writer, mSource);
			writer.WriteEndElement();
		}
	}
}
