using System.Reactive;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia;
using AvaloniaEdit;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI;
using voicio.Models;
using System.IO;
using System;
using System.Linq;
using System.Runtime.Loader;

namespace voicio.ViewModels
{
    public class CodeWindowViewModel : ViewModelBase
    {
        public VoiceSetting bindedObject;
        private string _TitleText = "";
        public string TitleText
        {
            get => _TitleText;
            set => this.RaiseAndSetIfChanged(ref _TitleText, value);
        }
        private string _ResultText = "";
        public string ResultText
        {
            get => _ResultText;
            set => this.RaiseAndSetIfChanged(ref _ResultText, value);
        }
        private string _CompileText = "";
        public string CompileText
        {
            get => _CompileText;
            set => this.RaiseAndSetIfChanged(ref _CompileText, value);
        }
        private AvaloniaEdit.Document.TextDocument _SourceCode;
        public AvaloniaEdit.Document.TextDocument SourceCode
        {
            get => _SourceCode;
            set => this.RaiseAndSetIfChanged(ref _SourceCode, value);
        }
        public void RunCode()
        {
            AssemblyLoadContext runContext = new AssemblyLoadContext("");
            runContext.LoadFromStream(new MemoryStream(bindedObject.ActionTreeExpression));
            //System.Runtime.
            //var dll = runContext.LoadFromStream(new MemoryStream());
        }
        public void Compile()
        {
            bindedObject.SourceCode = SourceCode.Text;
            var options = new CSharpCompilationOptions((OutputKind)LanguageVersion.Latest);
            var syntaxTree = CSharpSyntaxTree.ParseText(bindedObject.SourceCode);
            var compilation = CSharpCompilation.Create("DynamicAssembly")
            .AddSyntaxTrees(syntaxTree)
            .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            
            using (var ms = new MemoryStream())
            {
                var result = compilation.Emit(ms);
                if (result.Success == true) ResultText = "Builded without errors";
                bindedObject.ActionTreeExpression = ms.ToArray();
                if (bindedObject.IsSaved)
                {
                    using (var DataSource = new HelpContext())
                    {
                        DataSource.VoiceSettingTable.Attach(bindedObject);
                        DataSource.VoiceSettingTable.Update(bindedObject);
                        DataSource.SaveChanges();
                    }
                }
                else
                {
                    using (var DataSource = new HelpContext())
                    {
                        DataSource.VoiceSettingTable.Attach(bindedObject);
                        DataSource.VoiceSettingTable.Add(bindedObject);
                        DataSource.SaveChanges();
                        bindedObject.IsSaved = true;
                    }
                }
            }
        }
        public ReactiveCommand<Unit, Unit> CompileCommand { get; }
        public CodeWindowViewModel(VoiceSetting obj) {
            CompileCommand = ReactiveCommand.Create(Compile);
            bindedObject = obj;
            if (obj.SourceCode is null) obj.SourceCode = "";
            SourceCode = new AvaloniaEdit.Document.TextDocument(obj.SourceCode);
            TitleText = "Code for \"" + obj.Command + "\" command...";
        }
    }
}
