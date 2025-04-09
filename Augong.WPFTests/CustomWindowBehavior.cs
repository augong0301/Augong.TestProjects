using Microsoft.Xaml.Behaviors;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using static Augong.WPFTests.CustomWindowViewModel;
using static Augong.WPFTests.Win32Api;
using WpfUserControl = System.Windows.Controls;

namespace Augong.WPFTests
{
	public class CustomWindowBehavior : Behavior<Window>
	{
		private struct RectLocation
		{
			public double Left, Top, Width, Height;
		}

		private double mScreenRatio = 1.0;
		private double mInitialWidth;
		private double mInitialHeight;
		private int mMoveOffset = 100;
		private IntPtr mMainPtr = IntPtr.Zero;
		private RECT mMoveRectRange;
		private RECT mMaxRect = new RECT() { Left = -5, Top = 0, Right = 2565, Bottom = 1410 };
		private RectLocation mPreviewRect = new RectLocation() { Left = 900, Top = 450, Width = 800, Height = 400 };

		public MulanMessageWindowData WindowData
		{
			get { return (MulanMessageWindowData)GetValue(WindowDataProperty); }
			set { SetValue(WindowDataProperty, value); }
		}

		public static readonly DependencyProperty WindowDataProperty =
			DependencyProperty.RegisterAttached("WindowData", typeof(MulanMessageWindowData), typeof(CustomWindowBehavior), new PropertyMetadata(null));

		public CustomWindowBehavior()
		{
		}

		protected override void OnAttached()
		{
			base.OnAttached();
			if (AssociatedObject != null)
			{
				AssociatedObject.Initialized += AssociatedObject_Initialized;
				AssociatedObject.SourceInitialized += AssociatedObject_SourceInitialized;
				AssociatedObject.MouseLeftButtonDown += AssociatedObject_MouseLeftButtonDown;
				AssociatedObject.ContentRendered += AssociatedObject_ContentRendered;
				AssociatedObject.Closing += AssociatedObject_Closing;
			}
		}

		protected override void OnDetaching()
		{
			base.OnDetaching();
			if (AssociatedObject != null)
			{
				AssociatedObject.Initialized -= AssociatedObject_Initialized;
				AssociatedObject.SourceInitialized -= AssociatedObject_SourceInitialized;
				AssociatedObject.MouseLeftButtonDown -= AssociatedObject_MouseLeftButtonDown;
				AssociatedObject.ContentRendered -= AssociatedObject_ContentRendered;
				AssociatedObject.Closing -= AssociatedObject_Closing;
				AssociatedObject.SizeChanged -= AssociatedObject_SizeChanged;
			}
		}

		private void AssociatedObject_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			Debug.WriteLine("Closing window");
		}

		private void AssociatedObject_ContentRendered(object sender, EventArgs e)
		{
			var controlInfo = WindowData.ElementInfo as WpfUserControl.UserControl;


			if (!WindowData.CanResize)
			{
				RECT lpRect;
				GetWindowRect(mMainPtr, out lpRect);

				mInitialWidth = lpRect.Right - lpRect.Left;
				mInitialHeight = lpRect.Bottom - lpRect.Top;
			}

			AssociatedObject.SizeChanged += AssociatedObject_SizeChanged;
			Debug.WriteLine("AssociatedObject_ContentRendered");
			Thread.Sleep(1000);
			AssociatedObject.Close();
		}

		private void ButtonClose_CanCloseChanged(object sender, EventArgs e)
		{
			AssociatedObject.Close();
		}

		private void AssociatedObject_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			if (WindowData.IsWindowMax)
			{
				WindowData.IsWindowMax = false;
			}
		}

		private void AssociatedObject_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			if (!WindowData.CanResize || e.ClickCount < 2)
			{
				return;
			}

			if (!WindowData.IsWindowMax && AssociatedObject != null)
			{
				mPreviewRect.Left = AssociatedObject.Left;
				mPreviewRect.Top = AssociatedObject.Top;
				mPreviewRect.Width = AssociatedObject.Width;
				mPreviewRect.Height = AssociatedObject.Height;

				AssociatedObject.Left = mMaxRect.Left;
				AssociatedObject.Top = mMaxRect.Top;
				AssociatedObject.Width = mMaxRect.Right - mMaxRect.Left;
				AssociatedObject.Height = mMaxRect.Bottom - mMaxRect.Top;
				WindowData.IsWindowMax = true;
			}
			else
			{
				AssociatedObject.Left = mPreviewRect.Left;
				AssociatedObject.Top = mPreviewRect.Top;
				AssociatedObject.Width = mPreviewRect.Width;
				AssociatedObject.Height = mPreviewRect.Height;
				WindowData.IsWindowMax = false;
			}
		}

		private void AssociatedObject_SourceInitialized(object sender, EventArgs e)
		{
			mMainPtr = new WindowInteropHelper(AssociatedObject).Handle;

			if (WindowData.ElementInfo != null)
			{
				AssociatedObject.MinWidth = 250;
				AssociatedObject.MinHeight = 280;
				AssociatedObject.MaxWidth = 10000;
				AssociatedObject.MaxHeight = 10000;
			}

			LocationCenterScreen();

			if (WindowData.IsWindowMax)
			{
				AssociatedObject.SizeToContent = SizeToContent.Manual;
				AssociatedObject.Left = mMaxRect.Left;
				AssociatedObject.Top = mMaxRect.Top;
				AssociatedObject.Width = mMaxRect.Right - mMaxRect.Left;
				AssociatedObject.Height = mMaxRect.Bottom - mMaxRect.Top;
			}

			mMoveRectRange = GetWindowMoveRange();

			HwndSource hwndSource = (HwndSource)HwndSource.FromVisual((Window)sender);
			if (hwndSource != null)
			{
				hwndSource.AddHook(DragHook);
			}
			else
			{
				throw new InvalidOperationException($"Window {sender.ToString()} hwndSource is null"); ;
			}
			Debug.WriteLine("AssociatedObject_SourceInitialized");
		}

		private void LocationCenterScreen()
		{
			WindowData.Height = WindowData.Height == 0 ? AssociatedObject.Height : WindowData.Height;
			WindowData.Width = WindowData.Width == 0 ? AssociatedObject.Width : WindowData.Width;

			AssociatedObject.SizeToContent = SizeToContent.Manual;

			if (Process.GetCurrentProcess().MainWindowHandle != IntPtr.Zero && !GetMainWindowMinimize())
			{
				RECT lpRect;
				GetWindowRect(Process.GetCurrentProcess().MainWindowHandle, out lpRect);
				var mulanWidth = (lpRect.Right - lpRect.Left) / mScreenRatio;
				var mulanHeight = (lpRect.Bottom - lpRect.Top) / mScreenRatio;

				AssociatedObject.Left = lpRect.Left / mScreenRatio + (mulanWidth - WindowData.Width) / 2;
				AssociatedObject.Top = lpRect.Top / mScreenRatio + (mulanHeight - WindowData.Height) / 2;
			}

			AssociatedObject.Width = WindowData.Width;
			AssociatedObject.Height = WindowData.Height;
		}

		private RECT GetWindowMoveRange()
		{
			int minX = 0, maxX = 0;
			int minY = 0, maxY = 0;

			foreach (Screen screen in Screen.AllScreens)
			{
				Rectangle rectangle = screen.Bounds;
				minX = rectangle.X < minX ? rectangle.X : minX;
				minY = rectangle.Y < minY ? rectangle.Y : minY;
				maxX = rectangle.X + rectangle.Width > maxX ? rectangle.X + rectangle.Width : maxX;
				maxY = rectangle.Y + rectangle.Height > maxY ? rectangle.Y + rectangle.Height : maxY;
			}
			return new RECT { Left = minX - mMoveOffset, Top = minY, Right = maxX - mMoveOffset, Bottom = maxY - mMoveOffset };
		}

		private IntPtr DragHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handeled)
		{
			switch ((WM)msg)
			{
				case WM.WINDOWPOSCHANGING:
					WINDOWPOS pos = (WINDOWPOS)Marshal.PtrToStructure(lParam, typeof(WINDOWPOS));
					if ((pos.flags & (int)0x0002) != 0)
					{
						return IntPtr.Zero;
					}

					if (!WindowData.CanResize)
					{
						pos.cx = (int)mInitialWidth;
						pos.cy = (int)mInitialHeight;
					}

					if (pos.x < mMoveRectRange.Left)
					{
						pos.x = mMoveRectRange.Left;
					}
					if (pos.x > mMoveRectRange.Right)
					{
						pos.x = mMoveRectRange.Right;
					}
					if (pos.y < mMoveRectRange.Top)
					{
						pos.y = mMoveRectRange.Top;
					}
					if (pos.y > mMoveRectRange.Bottom)
					{
						pos.y = mMoveRectRange.Bottom;
					}

					Marshal.StructureToPtr(pos, lParam, true);

					break;
			}
			return IntPtr.Zero;
		}

		private void AssociatedObject_Initialized(object sender, EventArgs e)
		{
			SetOwner(AssociatedObject);
			AssociatedObject.WindowStartupLocation = WindowStartupLocation.CenterScreen;

			Graphics graphics = Graphics.FromHwnd(IntPtr.Zero);
			mScreenRatio = (double)(graphics.DpiX * 1.041666667) / 100;
			Debug.WriteLine("AssociatedObject_Initialized");
		}
	}

	public class Win32Api
	{
		public const int GWL_STYLE = -16;
		public const long MINIMIZE = 0x20000000L;

		[DllImport("user32.dll", EntryPoint = "FindWindow", CharSet = CharSet.Auto)]
		public extern static IntPtr FindWindow(string lpClassName, string lpWindowName);

		[DllImport("user32.dll")]
		public extern static bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

		[DllImport("user32.dll")]
		public static extern long GetWindowLongPtr(IntPtr hWnd, int nIndex);

		[StructLayout(LayoutKind.Sequential)]
		public struct RECT
		{
			public int Left, Top, Right, Bottom;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct WINDOWPOS
		{
			public IntPtr hwnd;
			public IntPtr hwndInsertAfter;
			public int x;
			public int y;
			public int cx;
			public int cy;
			public int flags;
		}

		public enum WM
		{
			WINDOWPOSCHANGING = 0x0046,
			WM_CLOSE = 0x0010,
			WM_SHOWWINDOW = 0x0018,
			WM_SIZING = 0x0214,
		}

		public static void SetOwner(Window childWindow)
		{
			WindowInteropHelper childWin = new WindowInteropHelper(childWindow);

			//IntPtr parentPtr = FindWindow(null, "Mulan");
			IntPtr parentPtr = Process.GetCurrentProcess().MainWindowHandle;

			if (parentPtr != IntPtr.Zero)
			{
				childWin.Owner = parentPtr;
			}
		}

		public static bool GetMainWindowMinimize()
		{
			return (GetWindowLongPtr(Process.GetCurrentProcess().MainWindowHandle, GWL_STYLE) & MINIMIZE) != 0;
		}
	}


	public class MulanMessageWindowData
	{
		public object ElementInfo { get; set; }
		public FocusElement FocusElementControl { get; set; } = FocusElement.Ok;
		public DefaultElement DefaultElementControl { get; set; } = DefaultElement.None;
		public string WindowAutomationID { get; set; } = string.Empty;
		public Type ResourceType { get; set; }
		public string Title { get; set; }
		public string MessageInfo { get; set; }
		public string OkButtonContent { get; set; }
		public string CancelButtonContent { get; set; }
		public bool IsCloseButtonVisible { get; set; } = true;
		public bool IsTopmost { get; set; } = false;
		public bool CanDragWindow { set; get; } = true;
		public bool CanResize { get; set; } = true;
		public bool IsWindowMax { get; set; } = false;
		public double Width { get; set; }
		public double Height { get; set; }

	}
}
