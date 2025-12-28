using Avalonia.Controls;
using Avalonia.ReactiveUI;
using System.Threading;
using voicio.ViewModels;

namespace voicio;

public partial class VoiceActionWindow : ReactiveWindow<VoiceActionViewModel>
{
    public VoiceActionWindow()
    {
        InitializeComponent();
        //ViewModel.RedirectToSearchResult.RegisterHandler(async interaction =>
        //{
        //    // View-specific code to show the dialog
        //    var dialog = new ConfirmationDialog(interaction.Input);
        //    var result = await dialog.ShowAsync(TopLevel.GetTopLevel(this));
        //    interaction.SetOutput(result);
        //});
        Thread.Sleep(3000);
        Close();
    }
}