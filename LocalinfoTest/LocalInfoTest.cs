using System.Diagnostics;
using System.Globalization;
using System.Management;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Win32;

namespace LocalinfoTest
{
	public class LocalInfoTest
	{

		public void DoTest()
		{

			var localInfo = GetAndSaveLocalInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "LocalInfo.key"));
			var t = DateTime.Now;
			var startupInfoFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "StartupInfo.key");
			DecryptAndValidate(startupInfoFile, localInfo);

		}

		private void DecryptAndValidate(string fileToDecrypt, string[] localInfo)
		{
			string encryptInfo;
			string decryptInfo;

			if (!File.Exists(fileToDecrypt))
			{
				return;
			}

			using (StreamReader sr = new StreamReader(fileToDecrypt))
			{
				encryptInfo = sr.ReadToEnd();
				decryptInfo = mRSAHelper.Decrypt(encryptInfo);
			}

			var segments = decryptInfo.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
			var secretKey = segments[0];

			if (!localInfo.Any(i => i == secretKey))
			{
				return;
			}


#if !DEBUG
			var expiration = DateTime.Now.Date.AddDays(30);
#else
			var expiration = File.GetCreationTime(Assembly.GetEntryAssembly().Location).Date.AddDays(30);
#endif
			var expSegment = segments.FirstOrDefault(s => s.StartsWith("EXP:"));

			if (expSegment != null)
			{
				expiration = DateTime.ParseExact(expSegment.Substring("EXP:".Length), "yyyyMMdd", CultureInfo.InvariantCulture);
			}

			var expTime = expiration;
		}

		RSAHelper mRSAHelper = new RSAHelper();
		private string[] GetAndSaveLocalInfo(string localFilePath)
		{
			var computer = new ComputerInfo();
			var computerInfo = computer.GetComputerInfo();

			var shaInfo = computerInfo.Select(i => mRSAHelper.GetSHA1Hash(i)).ToArray();

			EnsureParentDirectory(localFilePath);
			ValidateOnTimeStamp();

			using (StreamWriter sw = new StreamWriter(localFilePath))
			{
				foreach (var sha in shaInfo)
				{
					sw.WriteLine(sha);
				}
			}

			return shaInfo;
		}

		public void EnsureParentDirectory(string fileName)
		{
			var dir = Directory.GetParent(fileName);

			if (!dir.Exists)
			{
				dir.Create();
			}
		}

		private readonly string mMulanRegistry = @"SOFTWARE\Mulan";
		private readonly string mTimeStampRegName = "AppStatus";
		private void ValidateOnTimeStamp()
		{
			var currentDate = DateTime.Today;
			var dataInfo = GetRegistryValue(mMulanRegistry, mTimeStampRegName);
			if (string.IsNullOrEmpty(dataInfo))
			{
				// create registry if not exist
				CreateRegistryValue(mMulanRegistry, mTimeStampRegName, currentDate.ToString("yyyyMMdd"));
				dataInfo = GetRegistryValue(mMulanRegistry, mTimeStampRegName);
			}

			if (DateTime.TryParseExact(dataInfo, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime preTimeStamp))
			{
				if (currentDate < preTimeStamp)
				{
					throw new Exception("System time exception: The current system time is earlier than the last timestamp.");
				}
			}
			else
			{
				throw new Exception("Invalid timestamp format in the registry.");
			}
			CreateRegistryValue(mMulanRegistry, mTimeStampRegName, currentDate.ToString("yyyyMMdd"));
		}


		public void CreateRegistryValue(string subKey, string valueName, string value)
		{
			using (RegistryKey key = Registry.CurrentUser.CreateSubKey(subKey))
			{
				key.SetValue(valueName, Encoding.ASCII.GetBytes(value));
			}
		}

		public string GetRegistryValue(string subKey, string valueName)
		{
			using (RegistryKey key = Registry.CurrentUser.OpenSubKey(subKey))
			{
				if (key != null && key.GetValue(valueName) != null)
				{
					return Encoding.ASCII.GetString((byte[])key.GetValue(valueName));
				}
				return null;
			}
		}
	}
}

public class ComputerInfo
{
	public string[] GetComputerInfo()
	{
		string cpu = GetHardWareInfo("Win32_Processor", "ProcessorId");
		string baseBoard = GetHardWareInfo("Win32_BaseBoard", "SerialNumber");
		string bios = GetHardWareInfo("Win32_BIOS", "SerialNumber");
		string[] macs = GetMacAddressByNetworkInformation();

		if (macs.Length == 0)
		{
			return new string[] { string.Concat(cpu, baseBoard, bios) };
		}

		return macs.Select(mac => string.Concat(cpu, baseBoard, bios, mac)).ToArray();
	}

	public string[] GetComputerMacs()
	{
		return GetMacAddressByNetworkInformation();
	}

	private string GetHardWareInfo(string typePath, string key)
	{
		try
		{
			var managementClass = new ManagementClass(typePath);
			var managementInstances = managementClass.GetInstances();
			var managementProperties = managementClass.Properties;

			foreach (var property in managementProperties)
			{
				if (property.Name == key)
				{
					foreach (ManagementObject obj in managementInstances)
					{
						return obj.Properties[property.Name].Value.ToString();
					}
				}
			}
		}
		catch (Exception ex)
		{
			Debug.WriteLine(ex.Message);
		}

		return string.Empty;
	}

	private string[] GetMacAddressByNetworkInformation()
	{
		var ifaces = NetworkInterface.GetAllNetworkInterfaces().Where(i =>
			(i.NetworkInterfaceType == NetworkInterfaceType.Ethernet ||
			 i.NetworkInterfaceType == NetworkInterfaceType.Wireless80211));

		var macs = new List<string>();

		foreach (var iface in ifaces)
		{
			var mac = string.Concat(iface.GetPhysicalAddress().GetAddressBytes().Select(b => $"{b:x2}"));

			macs.Add(mac);
		}

		return macs.ToArray();
	}
}


public class RSAHelper
{
	private RSACryptoServiceProvider mRSA = new RSACryptoServiceProvider();
	private SHA1CryptoServiceProvider mSHA1 = new SHA1CryptoServiceProvider();

	private string mSHA1Begin = "Mulan";
	private string mSHA1End = "Successed";

	public RSAHelper()
	{
		mPrivateKey = "<RSAKeyValue><Modulus>xbyr8LZTB3RnGvng7cUWwYaAmQbG544tO5Z9OiT3F02PQ7VSHP9WbjDqjpSK/oPSaD9TF2YTYWm3fT0uKrV8Ca1w1YTKIBbXvmxj5lVUmJHR+ST0/OZ/7VG1ng/IxleSTibQPKAqTIzrC8RRUDlaa/HxF/02Cu//tBL2VjMZqfE=</Modulus><Exponent>AQAB</Exponent><P>+pUrzr95WVo8bt8asmPJ7VEqVSnLQL0xE8jGVBW0Cl6hL15A0+2DUNNo9lAPBWAmxNdhvHM1H860cBukoXn+rw==</P><Q>ygMHykNWmtrRp9GmNlwas2wkll+O1/H/yZu9eRJhKjh2V/WLsdXaBJfFr+kseZvlV22zlrpiNTVzM7OzCSAJXw==</Q><DP>JVcxTf/Ob3g45TSt38NrBchAjKxLs3v94jrbAxCw4ZK0ZkCfXHVaSiMW0w2fD2gCnvaRg+mPEwxUaxhTchSh1Q==</DP><DQ>RHtcg7PqQxrVIigPSbRVlOefS57fZNN0HBuA0u9pIw/7BnyGO+Y8P3xYFvdcDWnNCKN20y0iJ9mT0T4k/n3uzw==</DQ><InverseQ>HerATBw2zwSIf1sHyge+yDQJGuHDhNcCZK+/9w0j3W2DjMtwxWvx1YUTBykvL6yAwUTcqAekWUFc5sttYwBBkg==</InverseQ><D>QbqyyQRCW1MFRwFTFJaUNuZX7wZCrgwj2w/uNpq9DCD7A33NetghyeU2wwh7n5kAIykRnNCQlqwGk3n307iaLyLmiASRL4Ej5UvHjiQm1h3IScFmlyqlPU/I+dq2AcvoB8NCIR2Vo+Ul+sGbLaC56E9lq0kMICmffpIgOzHyg0U=</D></RSAKeyValue>";
		mPublicKey = "<RSAKeyValue><Modulus>xbyr8LZTB3RnGvng7cUWwYaAmQbG544tO5Z9OiT3F02PQ7VSHP9WbjDqjpSK/oPSaD9TF2YTYWm3fT0uKrV8Ca1w1YTKIBbXvmxj5lVUmJHR+ST0/OZ/7VG1ng/IxleSTibQPKAqTIzrC8RRUDlaa/HxF/02Cu//tBL2VjMZqfE=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
	}

	private readonly string mPrivateKey;
	private readonly string mPublicKey;

	public string Encrypt(string encryptstring)
	{
		mRSA.FromXmlString(mPublicKey);

		byte[] plainTextBArray = Encoding.ASCII.GetBytes(encryptstring);
		byte[] cypherTextBArray = mRSA.Encrypt(plainTextBArray, false);

		return Convert.ToBase64String(cypherTextBArray);
	}

	public string Decrypt(string decryptstring)
	{
		mRSA.FromXmlString(mPrivateKey);

		byte[] plainTextBArray = Convert.FromBase64String(decryptstring);
		byte[] cypherTextBArray = mRSA.Decrypt(plainTextBArray, false);

		return Encoding.ASCII.GetString(cypherTextBArray);
	}

	public string GetSHA1Hash(string str)
	{
		str = string.Concat(mSHA1Begin, str, mSHA1End);

		byte[] fromData = Encoding.Unicode.GetBytes(str);
		byte[] targetData = mSHA1.ComputeHash(fromData);

		string mSHA1str = string.Empty;

		foreach (var b in targetData)
		{
			mSHA1str += b.ToString("x2");
		}

		return mSHA1str;
	}
}
