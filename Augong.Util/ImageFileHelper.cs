namespace Augong.Util
{
	public class ImageFileHelper(params string[] folders) : FileHelper(folders)
	{
		public override void CopyAllFilesTo(string destination)
		{
			destination = Path.Combine(destination, Const.ImageFolder);
			if (!Directory.Exists(destination))
			{
				Directory.CreateDirectory(destination);
			}
			base.CopyAllFilesTo(destination);
		}

		public void CopyAllFilesAsync(string destination)
		{
			Task.Run(() => { CopyAllFilesTo(destination); });
		}
	}
}
