using Newtonsoft.Json.Linq;
using ReactiveUI;
using System;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using voicio.Models;

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
        public async Task<bool> SearchAction(byte[] temp_speech_buf)
        {
            string model_path = AppContext.BaseDirectory + "voice_model";
            //var recognition = new SpeechRecognition(model_path, recorder.GetRecorderSampleRate());
            //JObject rss = JObject.Parse(recognition.Recognize(temp_speech_buf));
            //Query = rss.Properties().Last().Value.ToString();

            return await RedirectToSearchResult.Handle(true);
            return await RedirectToSearchResult.Handle(false);
        }
        public VoiceActionViewModel(string query) {
            Query = query;
        }
    }
}
