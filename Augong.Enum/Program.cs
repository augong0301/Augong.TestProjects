// See https://aka.ms/new-console-template for more information
using Augong.Enum;

Console.WriteLine("Hello, World!");
var sen = SensorType.ScN & SensorType.ScO;

var dictionary = new Dictionary<SensorType,int>();
dictionary.Add(sen,1);


Console.WriteLine(dictionary[sen]);

Console.ReadKey();