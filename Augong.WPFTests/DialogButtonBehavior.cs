using Microsoft.Xaml.Behaviors;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Augong.WPFTests
{
	public class DialogButtonBehavior : Behavior<System.Windows.Controls.Button>
    {
        public enum DialogButtonRoles
        {
            OK,
            Cancel,
            Close,
        }

        public DialogButtonRoles Role
        {
            get { return (DialogButtonRoles)GetValue(RoleProperty); }
            set { SetValue(RoleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Role.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RoleProperty =
            DependencyProperty.RegisterAttached("Role", typeof(DialogButtonRoles), typeof(DialogButtonBehavior), new PropertyMetadata(DialogButtonRoles.Close));

        public int AutoClickTimeout
        {
            get { return (int)GetValue(AutoClickTimeoutProperty); }
            set { SetValue(AutoClickTimeoutProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AutoTriggerTimeout.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AutoClickTimeoutProperty =
            DependencyProperty.RegisterAttached("AutoClickTimeout", typeof(int), typeof(DialogButtonRoles), new FrameworkPropertyMetadata(0, new PropertyChangedCallback(OnAutoClickTimeoutChanged)));

        private static void OnAutoClickTimeoutChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as DialogButtonBehavior).DoOnAutoClickTimeoutChanged();
        }

        private DispatcherTimer mTimer;

        private void DoOnAutoClickTimeoutChanged()
        {
            if (AutoClickTimeout <= 0)
            {
                mTimer?.Stop();
                return;
            }

            mTimer = new DispatcherTimer();
            mTimer.Interval  = TimeSpan.FromMilliseconds(AutoClickTimeout);
            mTimer.Tick     += (s, e) => DoOnButtonClick();

            mTimer.Start();
        }

        protected override void OnAttached()  => AssociatedObject.Click += OnButtonClick;

        protected override void OnDetaching() => AssociatedObject.Click -= OnButtonClick;
        
        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            DoOnButtonClick();
        }

        private void DoOnButtonClick()
        {
            mTimer?.Stop();
            
            var window = Window.GetWindow(AssociatedObject);

            bool isModal = (bool)typeof(Window).GetField("_showingAsDialog", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(window);
            if(!isModal)
            {
                window.Close();
                return;
            }

            switch (Role)
            {
                case DialogButtonRoles.OK:
                    window.DialogResult = true;
                    break;
                case DialogButtonRoles.Cancel:
                    window.DialogResult = false;
                    break;
                case DialogButtonRoles.Close:
                    window.Close();
                    break;
                default:
                    window.Close();
                    break;
            }
        }
    }

}
