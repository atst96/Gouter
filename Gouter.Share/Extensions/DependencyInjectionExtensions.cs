using Gouter.Constants;
using Gouter.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Gouter.Extensions;

/// <summary>
/// DIの拡張クラス
/// </summary>
public static class DependencyInjectionExtensions
{
    /// <summary>
    /// 共通サービス・コンポーネントを登録する
    /// </summary>
    /// <typeparam name="T">ServiceCollectionの型</typeparam>
    /// <param name="serviceCollection">ServiceCollection</param>
    /// <returns>ServiceCollection</returns>
    public static T RegisterSharedComponents<T>(this T serviceCollection) where T : ServiceCollection
    {
        serviceCollection
            .AddSingleton<SettingsService>();

        return serviceCollection;
    }
}
