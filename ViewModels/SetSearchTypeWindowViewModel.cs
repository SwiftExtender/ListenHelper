using ReactiveUI;

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
        public SetSearchTypeWindowViewModel(string searchType)
        {
            SearchType = searchType;
        }
    }
}
