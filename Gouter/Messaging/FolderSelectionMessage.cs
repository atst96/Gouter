using System;
using System.Windows;
using Livet.Messaging;
using Livet.Messaging.IO;

namespace Gouter.Messaging
{
    internal class FolderSelectionMessage : ResponsiveInteractionMessage<string>
    {
        /// <summary>
        /// Windows Vista以降のダイアログを使用するかどうかを取得または設定する。
        /// </summary>
        public bool AutoUpgradeEnabled
        {
            get => (bool)this.GetValue(AutoUpgradeEnabledProperty);
            set => this.SetValue(AutoUpgradeEnabledProperty, value);
        }

        /// <summary>
        /// <see cref="AutoUpgradeEnabled"/>の依存関係プロパティ
        /// </summary>
        public static readonly DependencyProperty AutoUpgradeEnabledProperty =
            DependencyProperty.Register(nameof(AutoUpgradeEnabled), typeof(bool), typeof(FolderSelectionMessage), new PropertyMetadata(true));


        /// <summary>
        /// フォルダ作成ボタンを表示するかどうかを取得または設定する。
        /// </summary>
        public bool ShowNewFolderButton
        {
            get => (bool)this.GetValue(ShowNewFolderButtonProperty);
            set => this.SetValue(ShowNewFolderButtonProperty, value);
        }

        /// <summary>
        /// <see cref="ShowNewFolderButton"/>の依存関係プロパティ
        /// </summary>
        public static readonly DependencyProperty ShowNewFolderButtonProperty =
            DependencyProperty.Register(nameof(ShowNewFolderButton), typeof(bool), typeof(FolderSelectionMessage), new PropertyMetadata(false));


        /// <summary>
        /// 初期表示するディレクトリを取得または設定する。
        /// </summary>
        public string InitialPath
        {
            get => this.GetValue(InitialPathProperty) as string;
            set => this.SetValue(InitialPathProperty, value);
        }

        /// <summary>
        /// <see cref="InitialPath"/>の依存関係プロパティ
        /// </summary>
        public static readonly DependencyProperty InitialPathProperty =
            DependencyProperty.Register(nameof(InitialPath), typeof(string), typeof(FolderSelectionMessage), new PropertyMetadata(null));

        /// <summary>
        /// ルートフォルダを取得または設定する。
        /// </summary>
        public Environment.SpecialFolder? RootFolder
        {
            get => this.GetValue(RootFolderProperty) as Environment.SpecialFolder?;
            set => this.SetValue(RootFolderProperty, value);
        }

        /// <summary>
        /// <see cref="RootFolder"/>の依存関係プロパティ
        /// </summary>
        public static readonly DependencyProperty RootFolderProperty =
            DependencyProperty.Register(nameof(RootFolder), typeof(Environment.SpecialFolder?), typeof(FolderSelectionMessage), new PropertyMetadata(null));

        /// <summary>
        /// ダイアログの説明文を取得または設定する。
        /// </summary>
        public string Description
        {
            get => this.GetValue(DescriptionProeprty) as string;
            set => this.SetValue(DescriptionProeprty, value);
        }

        /// <summary>
        /// <see cref="Description"/>の依存関係プロパティ
        /// </summary>
        public static readonly DependencyProperty DescriptionProeprty =
            DependencyProperty.Register(nameof(Description), typeof(string), typeof(FileSelectionMessage), new PropertyMetadata(null));


        /// <summary>
        /// ダイアログの説明文をタイトルに表示するかどうかを取得または設定する。
        /// </summary>
        public bool UseDescriptionForTitle
        {
            get => (bool)this.GetValue(UseDescriptionForTitleProperty);
            set => this.SetValue(UseDescriptionForTitleProperty, value);
        }

        /// <summary>
        /// <see cref="UseDescriptionForTitle"/>の依存関係プロパティ
        /// </summary>
        public static readonly DependencyProperty UseDescriptionForTitleProperty =
            DependencyProperty.Register(nameof(UseDescriptionForTitle), typeof(bool), typeof(FolderSelectionMessage), new PropertyMetadata(false));

        /// <summary>
        /// インスタンス生成
        /// </summary>
        /// <returns></returns>
        protected override Freezable CreateInstanceCore() => new FolderSelectionMessage()
        {
            MessageKey = this.MessageKey,
            AutoUpgradeEnabled = this.AutoUpgradeEnabled,
            ShowNewFolderButton = this.ShowNewFolderButton,
            InitialPath = this.InitialPath,
            RootFolder = this.RootFolder,
            Description = this.Description,
            UseDescriptionForTitle = this.UseDescriptionForTitle,
        };
    }

}
