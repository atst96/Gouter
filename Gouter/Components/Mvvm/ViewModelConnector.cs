using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Markup;

namespace Gouter
{
    [MarkupExtensionReturnType(typeof(ViewModelBase))]
    internal class ViewModelConnector : MarkupExtension
    {
        private Window _view;

        [ConstructorArgument("viewModel")]
        public ViewModelBase ViewModel { get; private set; }

        [ConstructorArgument("instanceType")]
        public Type InstanceType { get; private set; }

        [DefaultValue(true)]
        public bool ConnectDialogService { get; set; } = true;

        [DefaultValue(true)]
        public bool ConnectWindowService { get; set; } = true;

        public ViewModelConnector()
        {
        }

        public ViewModelConnector(Type instanceType)
        {
            this.InstanceType = instanceType;
        }

        public ViewModelConnector(ViewModelBase viewModel)
        {
            this.ViewModel = viewModel;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var viewModel = this.ViewModel;

            if (this.InstanceType != null)
            {
                viewModel = Activator.CreateInstance(this.InstanceType) as ViewModelBase;

                this.ViewModel = viewModel ?? throw new NotSupportedException();
            }
            else if (this.ViewModel != null)
            {
                this.InstanceType = viewModel.GetType();
            }
            else
            {
                throw new NullReferenceException();
            }

            var valueTarget = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
            if (valueTarget?.TargetObject is Window view)
            {
                this._view = view;

                var dialogService = viewModel.DialogService;
                var windowService = viewModel.WindowService;

                if (this.ConnectDialogService)
                {
                    dialogService.SetView(view);
                }

                if (this.ConnectWindowService)
                {
                    windowService.SetView(view);
                }
            }

            return viewModel;
        }
    }
}
