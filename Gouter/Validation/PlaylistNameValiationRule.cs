using System.Collections.Generic;
using System.Globalization;
using System.Windows.Controls;
using System.Linq;
using System;

namespace Gouter.Validation;

internal class PlaylistNameValiationRule : ValidationRule
{
    public IEnumerable<string> PlaylistNames { get; set; }

    public PlaylistNameValiationRule() : base()
    {
    }


    public PlaylistNameValiationRule(IEnumerable<string> names) : base()
    {
        this.PlaylistNames = names;
    }

    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        if (value is null)
        {
            return new ValidationResult(false, "プレイリスト名を入力してください。");
        }

        if (value is not string inputValue)
        {
            throw new NotSupportedException();
        }

        if (this.PlaylistNames?.Any(n => n == inputValue) ?? true)
        {
            return new ValidationResult(false, "同じ名前のプレイリストが既に登録されています。");
        }
        else
        {
            return ValidationResult.ValidResult;
        }
    }
}
