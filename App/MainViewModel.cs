using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Augong.App
{
	public class MainViewModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler? PropertyChanged;

		protected void NotifyChanged([CallerMemberName] string propName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
		}



		public MainViewModel()
		{

		}
	}
}
