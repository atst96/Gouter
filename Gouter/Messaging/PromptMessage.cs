using System.Windows;
using System.Windows.Controls;
using Livet.Messaging;

namespace Gouter.Messaging;

/// <summary>
/// プロンプトウィンドウ用InteractionMessage
/// </summary>
internal class PromptMessage : ResponsiveInteractionMessage<string>
{

    /// <summary>
    /// ウィンドウタイトル
    /// </summary>
    public string Title
    {
        get => this.GetValue(TitleProperty) as string;
        set => this.SetValue(TitleProperty, value);
    }

    /// <summary>
    /// <seealso cref="Title"/>の依存関係プロパティ
    /// </summary>
    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(nameof(Title), typeof(string), typeof(PromptMessage), new PropertyMetadata(null));

    /// <summary>
    /// ダイアログの説明文
    /// </summary>
    public string Description
    {
        get => this.GetValue(DescriptionProperty) as string;
        set => this.SetValue(DescriptionProperty, value);
    }

    /// <summary>
    /// <seealso cref="Description"/>の依存関係プロパティ
    /// </summary>
    public static readonly DependencyProperty DescriptionProperty =
        DependencyProperty.Register(nameof(Description), typeof(string), typeof(PromptMessage), new PropertyMetadata(null));

    /// <summary>
    /// 初期入力文字列
    /// </summary>
    public string Text
    {
        get => this.GetValue(TextProperty) as string;
        set => this.SetValue(TextProperty, value);
    }

    /// <summary>
    /// <seealso cref="Text"/>の依存関係プロパティ
    /// </summary>
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(Text), typeof(string), typeof(PromptMessage), new PropertyMetadata(null));

    /// <summary>
    /// 空文字を許容するかどうかのフラグを取得または設定する
    /// </summary>
    public bool IsAllowEmpty
    {
        get => (bool)this.GetValue(IsAllowEmptyProperty);
        set => this.SetValue(IsAllowEmptyProperty, value);
    }

    /// <summary>
    /// <seealso cref="IsAllowEmpty"/>の依存関係プロパティ
    /// </summary>
    public static readonly DependencyProperty IsAllowEmptyProperty =
        DependencyProperty.Register(nameof(IsAllowEmpty), typeof(bool), typeof(PromptMessage), new PropertyMetadata(false));


    /// <summary>
    /// バリデーションを取得または設定する
    /// </summary>
    public ValidationRule Validation
    {
        get => (ValidationRule)this.GetValue(ValidationProperty);
        set => this.SetValue(ValidationProperty, value);
    }

    /// <summary>
    /// <seealso cref="Validation"/>の依存関係プロパティ
    /// </summary>
    public static readonly DependencyProperty ValidationProperty =
        DependencyProperty.Register(nameof(Validation), typeof(ValidationRule), typeof(PromptMessage), new PropertyMetadata(null));

    /// <summary>
    /// インスタンスを複製する
    /// </summary>
    /// <returns></returns>
    protected override Freezable CreateInstanceCore() => new PromptMessage
    {
        Title = this.Title,
        Description = this.Description,
        Text = this.Text,
        IsAllowEmpty = this.IsAllowEmpty,
        Validation = this.Validation,
    };
}
