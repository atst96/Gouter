using System;
using LiteDB;

namespace Gouter.Components;

/// <summary>
/// トランザクション管理クラス
/// </summary>
internal class DbTransaction : IDisposable
{
    /// <summary>
    /// トランザクション状態
    /// </summary>
    private bool _isTransactionEnabled = false;

    /// <summary>
    /// 接続先のデータベース
    /// </summary>
    private readonly ILiteDatabase _connection;

    /// <summary>
    /// インスタンスを生成する。
    /// </summary>
    /// <param name="connection">DBコネクション</param>
    public DbTransaction(ILiteDatabase connection)
    {
        this._connection = connection;
    }

    /// <summary>
    /// トランザクションを開始する。
    /// </summary>
    /// <returns>トランザクションの開始に成功したかどうかを返す</returns>
    public bool Begin()
    {
        bool started = this._connection.BeginTrans();
        this._isTransactionEnabled = started;

        return started;
    }

    /// <summary>
    /// トランザクションをコミットする。
    /// </summary>
    public void Commit()
    {
        if (this._isTransactionEnabled)
        {
            this._connection.Commit();
        }
    }

    /// <summary>
    /// トランザクションをロールバックする。
    /// </summary>
    public void Rollback()
    {
        if (this._isTransactionEnabled)
        {
            this._connection.Rollback();
        }
    }

    /// <summary>
    /// インスタンスを破棄する。
    /// </summary>
    void IDisposable.Dispose() => this.Commit();
}
