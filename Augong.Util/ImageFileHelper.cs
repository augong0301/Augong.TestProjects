namespace Augong.Util
{
	public class ImageFileHelper(string folder) : FileHelper(folder)
	{
		private string _folder = folder;

		public override void CopyAllFilesTo(string destination)
		{
			destination = Path.Combine(destination, Const.ImageFolder);
			base.CopyAllFilesTo(destination);
		}
	}
}
