using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

class Program
{
	static void Main(string[] args)
	{
		Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "start merging");

		Console.WriteLine("Enter anything to start process, 'end' to exit");
		while (Console.ReadLine() != "end")
		{
			try
			{
				// get output folder
				Console.WriteLine("Please enter output folder");
				string outputFolder = Console.ReadLine();
				string outputPath = Path.Combine(outputFolder);
				string[] selChannels = new string[3];
				double[] ratios = new double[3];

				Console.WriteLine("Select merge mode : all ; selected channels (sc)");
				var mergeMode = Console.ReadLine().ToLower();
				switch (mergeMode)
				{
					case "all":
						break;
					case "selected channels":
						ReadChannels(ref selChannels, ref ratios);
						break;
					case "sc":
						ReadChannels(ref selChannels, ref ratios);
						break;
					default:
						Console.WriteLine($"Unrecognized mode of {mergeMode}");
						break;
				}
				// switch mode
				Console.WriteLine("Select file mode : zip(z) ; folder(f)");
				var mode = Console.ReadLine().ToLower();
				if (mode == "png" || mode == "p")
				{
					MergePng(outputPath);
				}
				else if (mode == "zip" || mode == "z")
				{
					MergeZips(outputPath, selChannels, ratios);
				}
				else if (mode == "folder" || mode == "f")
				{
					MergeChannels(outputPath, selChannels, ratios);
				}
				else
				{
					Console.WriteLine($"Unsupported mode {mode}");
					continue;
				}

			}
			catch (Exception ex)
			{
				Console.WriteLine($"An error occurred: {ex.Message}");
				Console.WriteLine("Please try again.");
			}
			finally
			{
				Console.WriteLine("Enter 'start' to start process, 'end' to exit");
			}

		}
	}

	private static void MergeChannels(string outputPath, string[] selChannels, double[] ratios)
	{
		try
		{
			Console.WriteLine("Please enter input folder directory");
			var inputPath = Console.ReadLine();
			var inputFolder = Path.Combine(inputPath);

			string[] folders = Directory.GetDirectories(inputFolder);
			folders = folders.Where(f => f.Contains("Channel")).ToArray();
			var imgNames = GetCommonImageNames(folders, false);

			// Use Parallel.ForEach to process combinations in parallel
			//Parallel.ForEach(combinations, combination =>
			//{
			//    ProcessZipFiles(combination, outputPath, imgNames);
			//});

			ProcessZipFiles(inputFolder, selChannels, outputPath, imgNames, ratios, false);
			Console.WriteLine($"Finished mergeing {string.Join("_", selChannels)} with ratio {string.Join("_", ratios)}");
		}
		catch (Exception ex)
		{
			throw;
		}
	}

	private static void ReadChannels(ref string[] selChannels, ref double[] ratios)
	{
		Console.WriteLine("Supproted channels : sco scn sp1 sp2 zco zro pl");
		Console.WriteLine("Please enter channels ordered by B G R, seperate with comma");
		var channels = Console.ReadLine();
		selChannels = ConvertToChannel(channels);

		Console.WriteLine("Please enter ratio, seperate with comma");
		var ratio = Console.ReadLine();
		ratios = ratio.Split(',').Select(c => double.Parse(c)).ToArray();
	}

	private static string[] ConvertToChannel(string channels)
	{
		var cArray = channels.Split(',');
		if (cArray.Length > 3)
		{
			throw new ArgumentOutOfRangeException("channel count > 3");
		}
		var rst = new string[cArray.Length];
		for (int i = 0; i < cArray.Length; i++)
		{
			var ch = cArray[i];
			rst[i] = Verify(ch);
		}
		return rst;
	}

	private static string Verify(string ch)
	{
		switch (ch.ToLower())
		{
			case "sco":
				return "ScO";
			case "scn":
				return "ScN";
			case "zco":
				return "ZcO2";
			case "zco2":
				return "ZcO2";
			case "zro":
				return "ZrO2";
			case "zro2":
				return "ZrO2";
			case "sp1":
				return "Sp1";
			case "sp2":
				return "Sp2";
			case "pl":
				return "PL";
			default:
				throw new NotSupportedException("current channel not supported");
		}
	}

	private static void MergeZips(string outputPath, string[] channels, double[] ratios)
	{
		try
		{
			Console.WriteLine("Please enter input folder on desktop");
			var foldeName = Console.ReadLine();
			var inputFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), foldeName);

			string[] zipFiles = Directory.GetFiles(inputFolder, "*.zip");
			var imgNames = GetCommonImageNames(zipFiles, true);

			ProcessZipFiles(inputFolder, channels, outputPath, imgNames, ratios, true);
			Console.WriteLine($"Finished mergeing {string.Join("_", channels)} with ratio {string.Join("_", ratios)}");
			Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
		}
		catch (Exception ex)
		{
			throw;
		}
	}

	private static void MergePng(string outputPath)
	{
		Console.WriteLine("Merging Png Not supported yet");
	}

	private static void ProcessZipFiles(string inputPath, string[] channels, string outputPath, IEnumerable<string> imgNames, double[] ratios, bool isZip)
	{
		// get merged name
		string compositeName = GetFolderName(channels, ratios);
		var totalCount = imgNames.Count();
		int finished = 0;
		Parallel.ForEach(imgNames, imageFileName =>
		{
			using (var bStream = GetFileStream(inputPath, channels[0], imageFileName, isZip))
			using (var gStream = GetFileStream(inputPath, channels[1], imageFileName, isZip))
			using (var rStream = GetFileStream(inputPath, channels[2], imageFileName, isZip))
			{
				if (bStream == null || gStream == null || rStream == null)
				{
					Console.WriteLine($"Image {imageFileName} not found in one of the channel.");
				}
				Mat bImage = ReadImageFromStream(bStream);
				Mat gImage = ReadImageFromStream(gStream);
				Mat rImage = ReadImageFromStream(rStream);

				// Merge images to create a color image
				Mat[] channelMats = new Mat[] { bImage * ratios[0], gImage * ratios[1], rImage * ratios[2] };
				Mat colorImage = new Mat();
				Cv2.Merge(channelMats, colorImage);


				if (!Directory.Exists(Path.Combine(outputPath, compositeName)))
				{
					Directory.CreateDirectory(Path.Combine(outputPath, compositeName));
				}
				// Save the merged image
				string outputImagePath = Path.Combine(outputPath, compositeName, imageFileName);
				finished++;
				if (finished % 500 == 0)
				{
					Console.WriteLine($"Merged count = {finished}");
				}
				Cv2.ImWrite(outputImagePath, colorImage);
			}
		});
	}


	private static string GetFolderName(string[] channelNames, double[] ratios)
	{
		var rst = string.Join("_", channelNames);
		var sRatios = string.Join("_", ratios);
		return string.Join("_", rst, sRatios); ;
	}
	static string GetLastPartWithoutExtension(string path)
	{
		string fileName = Path.GetFileNameWithoutExtension(path);
		int lastIndex = fileName.LastIndexOf('_');
		if (lastIndex != -1)
		{
			return fileName.Substring(lastIndex + 1);
		}
		return fileName; // In case there is no underscore, return the whole name
	}

	static Stream GetFileStream(string filePath, string fileName, string imageFileName, bool isZip)
	{
		string extension = "Channel_Q_";
		if (isZip)
		{
			var zip = ZipFile.OpenRead(extension + filePath);
			var entry = zip.GetEntry(fileName);
			return entry?.Open();
		}
		var fs = File.OpenRead(Path.Combine(filePath, extension + fileName, imageFileName));
		return fs;
	}

	static IEnumerable<string> GetCommonImageNames(string[] files, bool isZip)
	{
		if (isZip)
		{
			var commonImages = new HashSet<string>();

			for (int i = 0; i < files.Length; i++)
			{
				using (var zip = ZipFile.OpenRead(files[i]))
				{
					var imageNames = new HashSet<string>(zip.Entries
						.Where(entry => entry.FullName.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
						.Select(entry => entry.FullName));

					if (i == 0)
					{
						commonImages = imageNames;
					}
					else
					{
						commonImages.IntersectWith(imageNames);
					}
				}
			}

			return commonImages;
		}
		else
		{
			var commonImages = new HashSet<string>();

			for (int i = 0; i < files.Length; i++)
			{
				var imageFiles = Directory.GetFiles(files[i], "*.png", SearchOption.AllDirectories)
					.Select(Path.GetFileName);

				if (i == 0)
				{
					commonImages = new HashSet<string>(imageFiles);
				}
				else
				{
					commonImages.IntersectWith(imageFiles);
				}
			}

			return commonImages;
		}
	}

	static Mat ReadImageFromStream(Stream stream)
	{
		using (var ms = new MemoryStream())
		{
			stream.CopyTo(ms);
			byte[] data = ms.ToArray();
			return Mat.FromImageData(data, ImreadModes.Grayscale);
		}
	}
}
