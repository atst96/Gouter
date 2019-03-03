using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using Gouter.Components;

namespace Gouter.Services
{
    internal class DialogService
    {
        private Application app = Application.Current;

        private Window _view;
        private IntPtr _hwnd;
        private ViewModelBase _viewModel;
        private Components.MessageBox _messageBox;

        public DialogService()
        {
            this._messageBox = new Components.MessageBox();
        }

        public DialogService(ViewModelBase viewModel) : this()
        {
            this._viewModel = viewModel;
        }

        internal void SetView(Window view)
        {
            if (!object.Equals(this._view, view))
            {
                this.OnViewUnregister(this._view);
                this._view = view;
            }

            if (view != null)
            {
                this._hwnd = new WindowInteropHelper(view).Handle;

                this.OnViewRegistered(view);
            }
        }

        private void OnViewRegistered(Window view)
        {
            view.Loaded += this.OnViewLoaded;
            view.Closed += this.OnViewClosed;
        }

        private void OnViewUnregister(Window view)
        {
            view.Loaded -= this.OnViewLoaded;
            view.Closed -= this.OnViewClosed;

            this._messageBox.Dispose();
        }

        private void OnViewLoaded(object sender, RoutedEventArgs e)
        {
            this._hwnd = new WindowInteropHelper(this._view).Handle;
            this._messageBox.SetHandle(this._hwnd);
        }

        private void OnViewClosed(object sender, EventArgs e)
        {
            this.OnViewUnregister(this._view);
        }

        public void ErrorMessage(string message, string caption = null)
        {
            this._messageBox.Show(message, caption, MessageBoxButtons.Ok, MessageBoxIcon.Error);
        }

        public void InfoMessage(string message, string caption = null)
        {
            this._messageBox.Show(message, caption, MessageBoxButtons.Ok, MessageBoxIcon.Information);
        }

        public bool ConfirmMessage(string message, string caption = null)
        {
            return this._messageBox.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == Components.MessageBoxResult.Yes;
        }
    }
}
