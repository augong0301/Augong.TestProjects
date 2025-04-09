using System.Diagnostics;
using System.Windows;

namespace Augong.WPFTests
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		private Mutex mutex = new Mutex(false);

		private void LoopBtnClick(object sender, RoutedEventArgs e)
		{
			Task.Run(() =>
			{
				while (true)
				{
					App.Current.Dispatcher.Invoke(() =>
					{
						var window = new CustomWindow();
						window.ShowDialog();
					});
				}

			});

			//Task.Run(() =>
			//{
			//	while (true)
			//	{
			//		Thread.Sleep(1000);
			//		var process = Process.GetProcesses().FirstOrDefault(c => c.ProcessName.Contains("AUGONG",StringComparison.InvariantCultureIgnoreCase));
			//	}
			//});
		}
	}
}