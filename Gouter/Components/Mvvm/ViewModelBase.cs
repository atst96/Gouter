using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Gouter
{
    /// <summary>
    /// ViewModelのベースクラス
    /// </summary>
    internal abstract class ViewModelBase : INotifyPropertyChanged, IDisposable
    {
        private List<IDisposable> _disposables = new();

        /// <summary>
        /// プロパティ変更通知イベントハンドラ
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// プロパティの変更通知を行う
        /// </summary>
        /// <param name="propertyName">プロパティ名</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            this.PropertyChanged?.Invoke(this, new(propertyName));
        }

        /// <summary>
        /// インスタンスを破棄する
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="changedValue"></param>
        /// <param name="newValue"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool SetProperty<T>(ref T changedValue, T newValue, [CallerMemberName] string propertyName = "")
        {
            if (object.Equals(changedValue, newValue))
            {
                return false;
            }

            changedValue = newValue;
            this.RaisePropertyChanged(propertyName);

            return true;
        }

        #region Commands

        /// <summary>
        /// ViewModelにコマンドを登録する
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected T RegisterCommand<T>(T command)
            where T : IDisposableCommand
        {
            this._disposables.Add(command);
            return command;
        }

        #endregion

        /// <summary>
        /// インスタンス破棄時
        /// </summary>
        protected virtual void OnDispose()
        {
            this._disposables.ForEach(cmd => cmd.Dispose());
            this._disposables.Clear();
        }

        /// <summary>
        /// インスタンス破棄時
        /// </summary>
        ~ViewModelBase()
        {
            this.OnDispose();
        }

        /// <summary>
        /// インスタンスを破棄する
        /// </summary>
        void IDisposable.Dispose()
        {
            this.OnDispose();
            GC.SuppressFinalize(this);
        }
    }
}
