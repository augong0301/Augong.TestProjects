using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Augong.Util
{
	public class FileHelper(string folder)
	{
		protected string _folder = folder;

		public virtual string[] GetAllFileNames()
		{
			if (_folder == null)
			{
				return null;
			}

			return Directory.GetFiles(_folder);
		}

		public virtual void CopyAllFilesTo(string destination)
		{
			if (destination == null)
			{
				throw new ArgumentNullException("Copy destination is null");
			}

			foreach (var name in GetAllFileNames())
			{
				File.Move(name, Path.Combine(destination, name));
			}
		}
	}
}
