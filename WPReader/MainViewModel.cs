using Augong.UI;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using System.Windows.Input;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace WPReader
{
	public class MainViewModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;
		public void NotifyChanged([CallerMemberName] string propName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
		}

		private WaferPatternInfo mPattern;

		public MainViewModel()
		{
		}

		private string mWPFilePath;
		public string WPFilePath
		{
			get { return mWPFilePath; }
			set { mWPFilePath = value; NotifyChanged(); }
		}

		private string mOutputFolder;
		public string OutputFolder
		{
			get { return mOutputFolder; }
			set { mOutputFolder = value; NotifyChanged(); }
		}

		private string mLog;
		public string Log
		{
			get { return mLog; }
			set { mLog = value; NotifyChanged(); }
		}

		private ICommand mSelectWPCommand;
		public ICommand SelectWPCommand => mSelectWPCommand ??
			(mSelectWPCommand = new RelayCommand((o) => SelectWP()));

		private void SelectWP()
		{
			var openFileDialog = new OpenFileDialog
			{
				Filter = "File (*.wp)|*.wp"
			};
			if (openFileDialog.ShowDialog() == true)
			{
				WPFilePath = openFileDialog.FileName;
				mPattern = WPFileLoader.LoadFromLocal(WPFilePath);
				Log = $"Loaded {openFileDialog.FileName}";
			}
		}

		private ICommand mSelectOutputFolderCommand;
		public ICommand SelectOutputFolderCommand => mSelectOutputFolderCommand ??
			(mSelectOutputFolderCommand = new RelayCommand((o) => SelectOutputFolder()));

		private void SelectOutputFolder()
		{
			var dialog = new FolderBrowserDialog();
			var result = dialog.ShowDialog();
			if (result == DialogResult.OK || result == DialogResult.Yes)
			{
				OutputFolder = Path.Combine(dialog.SelectedPath, "wpmap.xls");
				Log = $"输出路径为 {dialog.SelectedPath}";
			}
		}


		private ICommand mExportExcelCommand;
		public ICommand ExportExcelCommand => mExportExcelCommand ??
			(mExportExcelCommand = new RelayCommand((o) => ExportExcel()));

		private void ExportExcel()
		{
			if (mPattern == null)
			{
				Log = "读取的WP文件为空，无法导出excel";
				return;
			}
			var mapExporter = new WPDieMapExporter(mPattern.DieInfos);
			using (FileStream fs = new FileStream(OutputFolder, FileMode.Create, FileAccess.Write))
			{
				mapExporter.DoExport(fs);
			}
		}
	}
}
