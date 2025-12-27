using Avalonia.Controls;
using System.Threading;

namespace voicio;

public partial class VoiceActionWindow : Window
{
    public VoiceActionWindow()
    {
        InitializeComponent();
        this.WhenActivated(disposables =>
        {
            // Handle the interaction request
            ViewModel.ShowConfirmationDialog.RegisterHandler(async interaction =>
            {
                // View-specific code to show the dialog
                var dialog = new ConfirmationDialog(interaction.Input);
                var result = await dialog.ShowAsync(TopLevel.GetTopLevel(this));
                interaction.SetOutput(result);
            }).DisposeWith(disposables);
        });
        Thread.Sleep(1000);
        Close();
    }
}