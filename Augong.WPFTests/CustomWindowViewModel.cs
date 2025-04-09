using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Augong.WPFTests
{
	public class CustomWindowViewModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler? PropertyChanged;

		private MulanMessageWindowData _windowData = new MulanMessageWindowData();


		public MulanMessageWindowData WindowData
		{
			get { return _windowData; }
			set { SetProperty(ref _windowData, value); }
		}

		private bool mIsOkButtonFocus = false;
		public bool IsOkButtonFocus
		{
			get => mIsOkButtonFocus;
			set => SetProperty(ref mIsOkButtonFocus, value);
		}

		private bool mIsOkDefault = false;
		public bool IsOkDefault
		{
			get => mIsOkDefault;
			set => SetProperty(ref mIsOkDefault, value);
		}

		private bool mIsCancelButtonFocus = false;
		public bool IsCancelButtonFocus
		{
			get => mIsCancelButtonFocus;
			set => SetProperty(ref mIsCancelButtonFocus, value);
		}

		private bool mIsCancelDefault = false;
		public bool IsCancelDefault
		{
			get => mIsCancelDefault;
			set => SetProperty(ref mIsCancelDefault, value);
		}
		protected void SetProperty<T>(ref T variable, T value, [CallerMemberName] string propertyName = null)
		{
			if (variable == null && value == null)
			{
				return;
			}

			if (variable == null || !variable.Equals(value))
			{
				variable = value;

				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
			}
		}



		private ICommand mWindowContentRendered;
		public ICommand WindowContentRendered => mWindowContentRendered ??
		  (mWindowContentRendered = new RelayCommand((o) => { OnWindowContentRendered((EventArgs)o); }));

		private void OnWindowContentRendered(EventArgs e)
		{
			switch (_windowData.FocusElementControl)
			{
				case FocusElement.Ok:
					IsOkButtonFocus = true;
					break;
				case FocusElement.Cancel:
					IsCancelButtonFocus = true;
					break;
				default:
					break;
			}

			switch (_windowData.DefaultElementControl)
			{
				case DefaultElement.Ok:
					IsOkDefault = true;
					break;
				case DefaultElement.Cancel:
					IsCancelDefault = true;
					break;
				default:
					break;
			}
		}

		public class RelayCommand : ICommand
		{
			public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
			{
				mExecute = execute;
				mCanExecute = canExecute;
			}

			private Action<object> mExecute;
			private Predicate<object> mCanExecute;

			public event EventHandler CanExecuteChanged
			{
				add => CommandManager.RequerySuggested += value;
				remove => CommandManager.RequerySuggested -= value;
			}

			public bool CanExecute(object parameter) => mCanExecute == null ? true : mCanExecute(parameter);

			public void Execute(object parameter) => mExecute(parameter);

		}

		public enum FocusElement
		{
			None,
			Ok,
			Cancel
		}

		public enum DefaultElement
		{
			None,
			Ok,
			Cancel
		}
	}
}
