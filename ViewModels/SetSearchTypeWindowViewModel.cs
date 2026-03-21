using ReactiveUI;
using voicio.Services;

namespace voicio.ViewModels
{
    public class SetSearchTypeWindowViewModel : ViewModelBase
    {
        private string? _SearchType;
        public string? SearchType
        {
            get => _SearchType;
            set => this.RaiseAndSetIfChanged(ref _SearchType, value);
        }
        public SetSearchTypeWindowViewModel(AudioRecorderService recorder, SpeechRecognitionService recognition)
        {

            SearchType = searchType;
        }
    }
}
