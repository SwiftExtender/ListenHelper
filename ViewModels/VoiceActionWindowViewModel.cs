using AvaloniaEdit.Editing;
using ReactiveUI;
using System;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading.Tasks;
using voicio.Services;
using voicio.SpeechService;

namespace voicio.ViewModels
{
    public class VoiceActionWindowViewModel : ViewModelBase
    {
        public Interaction<bool, bool> RedirectToSearchResult { get; } = new Interaction<bool, bool>();
        private string _Query = "";
        public string Query
        {
            get => _Query;
            set => this.RaiseAndSetIfChanged(ref _Query, value);
        }
        private string _Exception;
        public string Exception
        {
            get => _Exception;
            set => this.RaiseAndSetIfChanged(ref _Exception, value);
        }
        private string _InputWord = "";
        public string InputWord
        {
            get => _InputWord;
            set => this.RaiseAndSetIfChanged(ref _InputWord, value);
        }
        public void SearchAction()
        {
            
        }
        public void ProcessAction()
        {
            try
            {
                Assembly asm = Assembly.Load(dllArray);
                Type type = asm.GetType("FastActionPlugin.Plugin");
                MethodInfo entrypoint = type.GetMethod("Handler");
                if (entrypoint != null)
                {
                    Delegate.CreateDelegate(typeof(Action<TextArea>), entrypoint);

                }
                else
                {
                    
                }
            }
            catch (Exception ex)
            {
                Exception = ex.Message;
            }
        }
        public VoiceActionWindowViewModel(bool isExecute, AudioRecorderService recorder, SpeechRecognitionService recognition, SearchService searchService)
        {
            byte[] audio = recorder.StartRecord(2);
            Query = recognition.GetRecognizeTextResult(audio);
            if (isExecute)
            {
                ProcessAction();
            }
            else
            {
                SearchAction();
            }
        }
    }
}
