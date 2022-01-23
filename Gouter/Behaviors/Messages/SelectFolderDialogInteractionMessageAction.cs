using System.Windows;
using System.Windows.Forms;
using Gouter.Messaging;
using Livet.Behaviors.Messaging;
using Livet.Messaging;

namespace Gouter.Behaviors.Messages;

internal class SelectFolderDialogInteractionMessageAction : InteractionMessageAction<FrameworkElement>
{
    protected override void InvokeAction(InteractionMessage message)
    {
        if (message is FolderSelectionMessage fsm)
        {
            var dialog = new FolderBrowserDialog
            {
                AutoUpgradeEnabled = fsm.AutoUpgradeEnabled,
                ShowNewFolderButton = fsm.ShowNewFolderButton,
                SelectedPath = fsm.InitialPath,
                Description = fsm.Description,
                UseDescriptionForTitle = fsm.UseDescriptionForTitle,
            };

            if (fsm.RootFolder is not null)
            {
                dialog.RootFolder = fsm.RootFolder.Value;
            }

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                fsm.Response = dialog.SelectedPath;
            }
        }
    }
}
