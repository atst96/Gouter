using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;

namespace Gouter.Services
{
    internal class DialogService : IDisposable
    {
        private System.Windows.Application app = System.Windows.Application.Current;

        private Window _view;
        private IntPtr _hwnd;
        private NativeWindow _window;
        private ViewModelBase _viewModel;

        public DialogService()
        {
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
            if (view != null)
            {
                view.Loaded -= this.OnViewLoaded;
                view.Closed -= this.OnViewClosed;
            }
        }

        private void OnViewLoaded(object sender, RoutedEventArgs e)
        {
            this._hwnd = new WindowInteropHelper(this._view).Handle;
            this._window = new NativeWindow();
            this._window.AssignHandle(this._hwnd);
        }

        private void OnViewClosed(object sender, EventArgs e)
        {
            this.OnViewUnregister(this._view);
        }

        private TaskDialogPage CreateTaskDialog(string message, string caption, string instruction)
        {
            return new()
            {
                // OwnerWindowHandle = this._hwnd,
                Text = message,
                Caption = caption,
                Heading = instruction,
            };
        }

        public void WarningMessage(string message, string caption = null, string instruction = null)
        {
            TaskDialog.ShowDialog(this._hwnd, new()
            {
                Caption = caption,
                Icon = TaskDialogIcon.Warning,
                Heading = instruction,
                Text = message,
                Buttons = new()
                {
                    TaskDialogButton.OK,
                },
            });
        }

        public void ErrorMessage(string message, string caption = null, string instruction = null)
        {
            TaskDialog.ShowDialog(this._hwnd, new()
            {
                Caption = caption,
                Icon = TaskDialogIcon.Error,
                Heading = instruction,
                Text = message,
                Buttons = new()
                {
                    TaskDialogButton.OK,
                },
            });
        }

        public void InfoMessage(string message, string caption = null, string instruction = null)
        {
            TaskDialog.ShowDialog(this._hwnd, new()
            {
                Caption = caption,
                Icon = TaskDialogIcon.Information,
                Heading = instruction,
                Text = message,
                Buttons = new()
                {
                    TaskDialogButton.OK,
                },
            });
        }

        public bool ConfirmMessage(string message, string caption = null, string instruction = null)
        {
            var result = TaskDialog.ShowDialog(this._hwnd, new()
            {
                Caption = caption,
                Icon = TaskDialogIcon.Warning,
                Heading = instruction,
                Text = message,
                Buttons = new()
                {
                    TaskDialogButton.Yes,
                    TaskDialogButton.No,
                },
            });

            return result == TaskDialogButton.Yes;
        }

        private static string GetExtensions(IDictionary<string, string[]> filters)
        {
            return string.Join("|", filters.Select(kvp => string.Concat(kvp.Key, "|", string.Join(",", kvp.Value))));
        }

        public string SelectOpenFile(string caption = null, IDictionary<string, string[]> filters = null, string initialDirectory = null)
        {
            using var dialog = new OpenFileDialog()
            {
                Title = caption,
                InitialDirectory = initialDirectory,
                AutoUpgradeEnabled = false,
                Multiselect = false,
                Filter = GetExtensions(filters),
            };

            return dialog.ShowDialog() != DialogResult.OK ? null : dialog.FileName;
        }

        public IEnumerable<string> SelectOpenFiles(string caption = null, IDictionary<string, string[]> filters = null, string initialDirectory = null)
        {
            using var dialog = new OpenFileDialog()
            {
                Title = caption,
                InitialDirectory = initialDirectory,
                AutoUpgradeEnabled = false,
                Multiselect = true,
                Filter = GetExtensions(filters),
            };

            return dialog.ShowDialog() != DialogResult.OK ? null : dialog.FileNames;
        }

        public string SelectSaveFile(string caption = null, IDictionary<string, string[]> filters = null, string initialDirectory = null)
        {
            using var dialog = new SaveFileDialog()
            {
                Title = caption,
                InitialDirectory = initialDirectory,
                AutoUpgradeEnabled = false,
                Filter = GetExtensions(filters),
            };

            return dialog.ShowDialog() != DialogResult.OK ? null : dialog.FileName;
        }

        public string SelectDirectory(string caption = null, string initialDirectory = null)
        {
            using var dialog = new FolderBrowserDialog
            {
                Description = caption,
                AutoUpgradeEnabled = true,
                SelectedPath = initialDirectory,
                UseDescriptionForTitle = true,
            };

            if (dialog.ShowDialog(this._window) == DialogResult.OK)
            {
                return dialog.SelectedPath;
            }

            return null;
        }

        //public IEnumerable<string> SelectDirectories(string caption = null, string initialDirectory = null)
        //{
        //    using var dialog = new FolderBrowserDialog
        //    {
        //        Description = caption,
        //        AutoUpgradeEnabled = true,
        //        SelectedPath = initialDirectory,
        //    };

        //    if (dialog.ShowDialog(this._window) == DialogResult.OK)
        //    {
        //        return dialog.SelectedPath;
        //    }

        //    return null;
        //}

        public void Dispose()
        {
            this._hwnd = IntPtr.Zero;
            this._viewModel = null;
            this._view = null;
        }
    }
}
