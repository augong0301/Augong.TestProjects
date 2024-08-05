using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Augong.Util
{
	public class FileHelper(params string[] folders)
	{

		public virtual List<string> GetAllFileNames()
		{
			List<string> files = new List<string>();
			if (folders == null)
			{
				return null;
			}
			foreach (string folder in folders)
			{
				files.AddRange(Directory.GetFiles(folder));
			}
			return files;
		}

		public virtual void CopyAllFilesTo(string destination)
		{
			if (destination == null)
			{
				throw new ArgumentNullException("Copy destination is null");
			}

			foreach (var file in GetAllFileNames())
			{
				try
				{
					var savePath = Path.Combine(destination, Path.GetFileName(file));
					//File.Copy(name, Path.Combine(destination, name));
					using (FileStream fs = new FileStream(file, FileMode.Open))
					{
						using (FileStream destinationStream = new FileStream(savePath, FileMode.Create, FileAccess.Write))
						{
							byte[] buffer = new byte[1024];
							int bytesRead;

							while ((bytesRead = fs.Read(buffer, 0, buffer.Length)) > 0)
							{
								destinationStream.Write(buffer, 0, bytesRead);
							}
						}
					}
				}
				catch (Exception ex)
				{
					var ms = ex.Message;
					continue;
				}
			}
		}
	}
}
