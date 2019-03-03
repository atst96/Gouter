using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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
    }
}