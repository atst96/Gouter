using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Gouter.Components.Mvvm;

/// <summary>
/// コマンド管理クラス
/// </summary>
internal class MvvmCommandManager : IDisposable
{
    /// <summary>
    /// 生成コマンド情報
    /// </summary>
    private List<IDisposableCommand> _commands = new();

    /// <summary>
    /// 内部リストにコマンドを登録する
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="newCommand"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private T Add<T>(T newCommand) where T : IDisposableCommand
    {
        if (this._isDisposed)
        {
            throw new InvalidOperationException("This instance was already disposed.");
        }

        this._commands.Add(newCommand);
        return newCommand;
    }

    /// <summary>
    /// コマンドを生成する。
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    public Command Create(Action action)
        => this.Add(new DelegateCommand(action));

    /// <summary>
    /// コマンドを生成する。
    /// </summary>
    /// <param name="action"></param>
    /// <param name="hookRequerySuggested">RequerySuggestedイベントにフックするかどうかのフラグ</param>
    /// <returns></returns>
    public Command Create(Action action, bool hookRequerySuggested)
        => this.Add(new DelegateCommand(action, hookRequerySuggested));

    /// <summary>
    /// コマンドを生成する。
    /// </summary>
    /// <param name="action"></param>
    /// <param name="canExecute"></param>
    /// <returns></returns>
    public Command Create(Action action, Func<bool> canExecute)
        => this.Add(new DelegateCommand(action, canExecute));

    /// <summary>
    /// コマンドを生成する。
    /// </summary>
    /// <param name="action"></param>
    /// <param name="canExecute"></param>
    /// <param name="hookRequerySuggested">RequerySuggestedイベントにフックするかどうかのフラグ</param>
    /// <returns></returns>
    public Command Create(Action action, Func<bool> canExecute, bool hookRequerySuggested)
        => this.Add(new DelegateCommand(action, canExecute, hookRequerySuggested));

    /// <summary>
    /// コマンドを生成する。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="action"></param>
    /// <returns></returns>
    public Command<T> Create<T>(Action<T> action)
        => this.Add(new DelegateCommand<T>(action));

    /// <summary>
    /// コマンドを生成する。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="action"></param>
    /// <param name="hookRequerySuggested">RequerySuggestedイベントにフックするかどうかのフラグ</param>
    /// <returns></returns>
    public Command<T> Create<T>(Action<T> action, bool hookRequerySuggested)
        => this.Add(new DelegateCommand<T>(action, hookRequerySuggested));

    /// <summary>
    /// コマンドを生成する。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="action"></param>
    /// <param name="canExecute"></param>
    /// <returns></returns>
    public Command<T> Create<T>(Action<T> action, Predicate<T> canExecute)
        => this.Add(new DelegateCommand<T>(action, canExecute));

    /// <summary>
    /// コマンドを生成する。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="action"></param>
    /// <param name="canExecute"></param>
    /// <param name="hookRequerySuggested">RequerySuggestedイベントにフックするかどうかのフラグ</param>
    /// <returns></returns>
    public Command<T> Create<T>(Action<T> action, Predicate<T> canExecute, bool hookRequerySuggested)
        => this.Add(new DelegateCommand<T>(action, canExecute, hookRequerySuggested));

    private bool _isDisposed;

    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                this._commands.ForEach(c => c.Dispose());
                this._commands.Clear();
            }

            // TODO: アンマネージド リソース (アンマネージド オブジェクト) を解放し、ファイナライザーをオーバーライドします
            // TODO: 大きなフィールドを null に設定します
            this._isDisposed = true;
            this._commands = null;
        }
    }

    public void Dispose()
    {
        // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
        this.Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
