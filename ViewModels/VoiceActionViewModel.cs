using ReactiveUI;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace voicio.ViewModels
{
    public class VoiceActionViewModel : ViewModelBase
    {
        public Interaction<bool, bool> RedirectToSearchResult { get; } = new Interaction<bool, bool>();
        private string _Query = "";
        public string Query
        {
            get => _Query;
            set => this.RaiseAndSetIfChanged(ref _Query, value);
        }
        private string _Exception = "";
        public string Exception
        {
            get => _Exception;
            set => this.RaiseAndSetIfChanged(ref _Exception, value);
        }
        public async Task<bool> ConfirmAction()
        {


            var result = await RedirectToSearchResult.Handle(true);
            return result;
        }
        public VoiceActionViewModel(string query) {
            Query = query;
        }
    }
}
