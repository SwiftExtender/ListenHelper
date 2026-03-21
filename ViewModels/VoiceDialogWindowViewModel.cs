using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reflection;
using voicio.Models;
using voicio.Services;

namespace voicio.ViewModels
{
    public class VoiceDialogWindowViewModel : ViewModelBase
    {
        public SearchService SearchService { get; set; }
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
            List<Hint> hints = SearchService.SearchHint(Query, true, false);
        }
        public void ProcessAction()
        {
            try
            {
                List<ScriptCodeModel> hints = SearchService.SearchScript(Query, true, false);
                if (hints.Count > 0)
                {
                    Assembly asm = Assembly.Load(hints[0].BinaryExecutable);
                    Type type = asm.GetType("VoiceViewActionPlugin.Plugin");
                    MethodInfo entrypoint = type.GetMethod("Handler");
                    if (entrypoint != null)
                    {
                        entrypoint.Invoke(null, null);
                    }
                }
            }
            catch (Exception ex)
            {
                Exception = ex.Message;
            }
        }
        public VoiceDialogWindowViewModel(bool isExecute, AudioRecorderService recorder, SpeechRecognitionService recognition, SearchService searchService)
        {
            byte[] audio = recorder.StartRecord(2);
            Query = recognition.GetRecognizeTextResult(audio);
            SearchService = searchService;
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
