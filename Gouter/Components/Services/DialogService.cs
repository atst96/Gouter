using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace Gouter.Services
{
    internal class DialogService : IDisposable
    {
        private Application app = Application.Current;

        private Window _view;
        private IntPtr _hwnd;
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
        }

        private void OnViewClosed(object sender, EventArgs e)
        {
            this.OnViewUnregister(this._view);
        }

        private TaskDialog CreateTaskDialog(string message, string caption, string instruction)
        {
            return new TaskDialog
            {
                OwnerWindowHandle = this._hwnd,
                Text = message,
                Caption = caption,
                InstructionText = instruction,
            };
        }

        public void WarningMessage(string message, string caption = null, string instruction = null)
        {
            using (var dialog = CreateTaskDialog(message, caption, instruction))
            {
                dialog.StandardButtons = TaskDialogStandardButtons.Ok;
                dialog.Icon = TaskDialogStandardIcon.Warning;

                dialog.Show();
            }
        }

        public void ErrorMessage(string message, string caption = null, string instruction = null)
        {
            using (var dialog = CreateTaskDialog(message, caption, instruction))
            {
                dialog.StandardButtons = TaskDialogStandardButtons.Ok;
                dialog.Icon = TaskDialogStandardIcon.Error;

                dialog.Show();
            }
        }

        public void InfoMessage(string message, string caption = null, string instruction = null)
        {
            using (var dialog = CreateTaskDialog(message, caption, instruction))
            {
                dialog.StandardButtons = TaskDialogStandardButtons.Ok;
                dialog.Icon = TaskDialogStandardIcon.Information;

                dialog.Show();
            }
        }

        public bool ConfirmMessage(string message, string caption = null, string instruction = null)
        {
            using (var dialog = CreateTaskDialog(message, caption, instruction))
            {
                dialog.StandardButtons = TaskDialogStandardButtons.Yes | TaskDialogStandardButtons.No;
                dialog.Icon = TaskDialogStandardIcon.Information;

                return dialog.Show() == TaskDialogResult.Yes;
            }
        }

        private static void SetCommonFileDialogFilter(CommonFileDialog dialog, IDictionary<string, string[]> filters)
        {
            if (filters == null || filters.Count == 0)
            {
                return;
            }

            var filterCollection = dialog.Filters;

            foreach (var kvp in filters)
            {
                filterCollection.Add(new CommonFileDialogFilter(kvp.Key, string.Join(",", kvp.Value)));
            }
        }

        private bool OpenCommonFileDialog(CommonFileDialog dialog)
        {
            CommonFileDialogResult result = this._hwnd == IntPtr.Zero
                ? dialog.ShowDialog()
                : dialog.ShowDialog(this._hwnd);

            return result == CommonFileDialogResult.Ok;
        }

        public string SelectOpenFile(string caption = null, IDictionary<string, string[]> filters = null, string initialDirectory = null)
        {
            var dialog = new CommonOpenFileDialog
            {
                Title = caption,
                Multiselect = false,
                InitialDirectory = initialDirectory,
            };

            SetCommonFileDialogFilter(dialog, filters);

            using (dialog)
            {
                if (this.OpenCommonFileDialog(dialog))
                {
                    return dialog.FileName;
                }
            }

            return null;
        }

        public IEnumerable<string> SelectOpenFiles(string caption = null, IDictionary<string, string[]> filters = null, string initialDirectory = null)
        {
            var dialog = new CommonOpenFileDialog
            {
                Title = caption,
                Multiselect = true,
                IsFolderPicker = true,
                InitialDirectory = initialDirectory,
            };

            SetCommonFileDialogFilter(dialog, filters);

            using (dialog)
            {
                if (this.OpenCommonFileDialog(dialog))
                {
                    return dialog.FileNames;
                }
            }

            return null;
        }

        public string SelectSaveFile(string caption = null, IDictionary<string, string[]> filters = null, string initialDirectory = null)
        {
            var dialog = new CommonSaveFileDialog
            {
                Title = caption,
                InitialDirectory = initialDirectory,
            };

            SetCommonFileDialogFilter(dialog, filters);

            using (dialog)
            {
                if (this.OpenCommonFileDialog(dialog))
                {
                    return dialog.FileName;
                }
            }

            return null;
        }

        public string SelectDirectory(string caption = null, string initialDirectory = null)
        {
            var dialog = new CommonOpenFileDialog
            {
                Title = caption,
                InitialDirectory = initialDirectory,
                IsFolderPicker = true,
                Multiselect = false,
            };

            using (dialog)
            {
                if (this.OpenCommonFileDialog(dialog))
                {
                    return dialog.FileName;
                }
            }

            return null;
        }

        public IEnumerable<string> SelectDirectories(string caption = null, string initialDirectory = null)
        {
            var dialog = new CommonOpenFileDialog
            {
                Title = caption,
                Multiselect = true,
                IsFolderPicker = true,
                InitialDirectory = initialDirectory,
            };

            using (dialog)
            {
                if (this.OpenCommonFileDialog(dialog))
                {
                    return dialog.FileNames;
                }
            }

            return null;
        }

        public void Dispose()
        {
            this._hwnd = IntPtr.Zero;
            this._viewModel = null;
            this._view = null;
        }
    }
}
