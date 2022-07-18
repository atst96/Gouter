using System.Globalization;
using System.Windows.Controls;

namespace Gouter.ViewModels;

/// <summary>
/// プロンプトのViewModel
/// </summary>
internal class PromptViewModel : ViewModelBase
{
    /// <summary>
    /// タイトル
    /// </summary>
    public string Title { get; init; }

    /// <summary>
    /// 説明文
    /// </summary>
    public string Description { get; init; }

    private string _text;

    /// <summary>
    /// 入力文字列
    /// </summary>
    public string Text
    {
        get => this._text;
        set
        {
            if (this.SetProperty(ref this._text, value))
            {
                this.ValidateText();
            }
        }
    }

    private bool _isAllowEmpty;

    /// <summary>
    /// 未入力を許容する
    /// </summary>
    public bool IsAllowEmpty
    {
        get => this._isAllowEmpty;
        init
        {
            if (this.SetProperty(ref this._isAllowEmpty, value))
            {
                this.ValidateText();
            }
        }
    }

    /// <summary>
    /// バリデーションルールを取得または設定する
    /// </summary>
    public ValidationRule Validation { get; init; }

    private bool _isAccept;
    public bool IsAccept
    {
        get => this._isAccept;
        private set => this.SetProperty(ref this._isAccept, value);
    }

    private string _validationMessage;

    /// <summary>
    /// バリデーションメッセージを取得または設定する
    /// </summary>
    public string ValidationMessage
    {
        get => this._validationMessage;
        private set => this.SetProperty(ref this._validationMessage, value);
    }

    /// <summary>
    /// <see cref="Text"/>のバリデーションチェックを行う
    /// </summary>
    private void ValidateText()
    {
        var value = this.Text;

        if (string.IsNullOrEmpty(value))
        {
            if (!this.IsAllowEmpty)
            {
                this.ValidationMessage = null;
                this.IsAccept = false;
                return;
            }
            else
            {
                this.ClearValidationStatus();
            }
        }

        var validation = this.Validation;
        if (validation is not null)
        {
            var result = validation.Validate(value, CultureInfo.CurrentCulture);
            if (result.IsValid)
            {
                this.ClearValidationStatus();
            }
            else
            {
                this.ValidationMessage = result.ErrorContent.ToString();
                this.IsAccept = false;
            }
        }
        else
        {
            this.ClearValidationStatus();
        }
    }

    /// <summary>
    /// バリデーションの状態を初期化する
    /// </summary>
    private void ClearValidationStatus()
    {
        this.ValidationMessage = null;
        this.IsAccept = true;
    }
}
