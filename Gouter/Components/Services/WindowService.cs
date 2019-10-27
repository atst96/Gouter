using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Gouter.Views;

namespace Gouter.Services
{
    internal class WindowService
    {
        private Window _view;
        private readonly ViewModelBase _viewModel;

        public WindowService()
        {
        }

        public WindowService(ViewModelBase viewModel) : this()
        {
            this._viewModel = viewModel;
        }

        internal void SetView(Window view)
        {
            if (!object.Equals(this._view, view))
            {
                this._view = view;
            }
        }

        private static IEnumerable<TWindow> GetOpenedWindows<TWindow>() where TWindow : Window
        {
            foreach (Window window in App.Instance.Windows)
            {
                if (window is TWindow tWindow)
                {
                    yield return tWindow;
                }
            }
        }

        private static SettingWindow GetOpenedSettingWindow()
        {
            return GetOpenedWindows<SettingWindow>().FirstOrDefault();
        }

        internal void OpenSettingWindow()
        {
            var opennedWindow = GetOpenedSettingWindow();
            if (opennedWindow != null)
            {
                opennedWindow.Activate();
            }
            else
            {
                opennedWindow = new SettingWindow
                {
                    Owner = this._view,
                };

                opennedWindow.Show();
            }

        }
    }
}
