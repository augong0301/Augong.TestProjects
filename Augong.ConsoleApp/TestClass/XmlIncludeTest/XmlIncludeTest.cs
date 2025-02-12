using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Augong.ConsoleApp.TestClass.XmlIncludeTest
{
	internal class XmlIncludeTest
	{
	}

	public interface IAnimal
	{
		string Name { get; set; }
	}

	public class Dog : IAnimal
	{
		public string Name { get; set; }
		public string Breed { get; set; }
	}

	public class Cat : IAnimal
	{
		public string Name { get; set; }
		public int Lives { get; set; }
	}

	[XmlRoot("Zoo")]
	[XmlInclude(typeof(Dog)), XmlInclude(typeof(Cat))] 
	public class Animal
	{
	}
}
