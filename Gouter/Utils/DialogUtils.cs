using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Gouter.Views;

namespace Gouter.Utils
{
    internal static class DialogUtils
    {
        /// <summary>
        /// ウィンドウを取得する
        /// </summary>
        /// <typeparam name="TWindow"></typeparam>
        /// <returns></returns>
        public static IEnumerable<TWindow> GetWindows<TWindow>() where TWindow : Window
            => App.Instance.Windows.OfType<TWindow>();

        /// <summary>
        /// 設定ウィンドウを開く
        /// </summary>
        public static void OpenSettingWindow(Window owner = null)
        {
            var opennedWindow = GetWindows<SettingWindow>().FirstOrDefault();
            if (opennedWindow is not null)
            {
                _ = opennedWindow.Activate();
            }
            else
            {
                opennedWindow = new SettingWindow
                {
                    Owner = owner,
                };

                opennedWindow.ShowDialog();
            }
        }
    }
}
