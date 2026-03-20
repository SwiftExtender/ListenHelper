using AvaloniaEdit;
using AvaloniaEdit.Editing;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Runtime.InteropServices;
using voicio.Models;
using TextDocument = AvaloniaEdit.Document.TextDocument;

namespace voicio.ViewModels
{
    public class ScriptWindowViewModel : ViewModelBase
    {
        const string templateCode = "using System;\r\nusing AvaloniaEdit.Editing;\r\n\r\nnamespace ContextItemPlugin {\r\nclass Plugin\r\n{\r\n}\r\n}\r\n";
        public void CopyMouseCommand(TextArea textArea)
        {
            ApplicationCommands.Copy.Execute(null, textArea);
        }
        public void CutMouseCommand(TextArea textArea)
        {
            ApplicationCommands.Cut.Execute(null, textArea);
        }
        public void PasteMouseCommand(TextArea textArea)
        {
            ApplicationCommands.Paste.Execute(null, textArea);
        }
        public void SelectAllMouseCommand(TextArea textArea)
        {
            ApplicationCommands.SelectAll.Execute(null, textArea);
        }
        public void SaveCode()
        {
            if (SelectedRow != null)
            {
                SelectedRow.SourceCode = SourceCode.Text;
                SaveMacros(SelectedRow);
                if (SelectedRow.Name != "")
                {
                    CompileStatusText = "Macros " + SelectedRow.Name + " saved";
                }
                else
                {
                    CompileStatusText = "Macros saved";
                }
            }
        }
        public void RemoveMacros(ScriptCodeModel remHint)
        {
            MacrosGridData.Remove(remHint);
            if (remHint.IsSaved)
            {
                using (var DataSource = new HelpContext())
                {
                    DataSource.ScriptTable.Attach(remHint);
                    DataSource.ScriptTable.Remove(remHint);
                    DataSource.SaveChanges();
                }
            }
        }
        public void SaveMacros(ScriptCodeModel updateHint)
        {
            if (updateHint.IsSaved)
            {
                using (var DataSource = new HelpContext())
                {
                    DataSource.ScriptTable.Attach(updateHint);
                    DataSource.ScriptTable.Update(updateHint);
                    DataSource.SaveChanges();
                }
            }
            else
            {
                using (var DataSource = new HelpContext())
                {
                    DataSource.ScriptTable.Attach(updateHint);
                    DataSource.ScriptTable.Add(updateHint);
                    DataSource.SaveChanges();
                }
                updateHint.IsSaved = true;
            }
        }
        private string _MacrosNameText = "";
        public string MacrosNameText
        {
            get => _MacrosNameText;
            set => this.RaiseAndSetIfChanged(ref _MacrosNameText, value);
        }
        private string _SourceCodeRunOutputText = "";
        public string SourceCodeRunOutputText
        {
            get => _SourceCodeRunOutputText;
            set => this.RaiseAndSetIfChanged(ref _SourceCodeRunOutputText, value);
        }
        private string _CompileStatusText = "Status: No code was compiled";
        public string CompileStatusText
        {
            get => _CompileStatusText;
            set => this.RaiseAndSetIfChanged(ref _CompileStatusText, value);
        }
        private TextDocument _SourceCode = new("");
        public TextDocument SourceCode
        {
            get => _SourceCode;
            set => this.RaiseAndSetIfChanged(ref _SourceCode, value);
        }
        private ObservableCollection<ScriptCodeModel>? _MacrosGridData;
        public ObservableCollection<ScriptCodeModel>? MacrosGridData
        {
            get => _MacrosGridData;
            set => this.RaiseAndSetIfChanged(ref _MacrosGridData, value);
        }
        private ObservableCollection<PortableExecutableReference>? _RefsGridData;
        public ObservableCollection<PortableExecutableReference>? RefsGridData
        {
            get => _RefsGridData;
            set => this.RaiseAndSetIfChanged(ref _RefsGridData, value);
        }
        public void AddMacros()
        {
            try
            {
                MacrosGridData.Add(new ScriptCodeModel(false, templateCode));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        public static long TimeStamp()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }
        public static string GenerateChecksum(MemoryStream ms)
        {
            ms.Position = 0;
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            byte[] hash = sha256.ComputeHash(ms);
            return BitConverter.ToString(hash).Replace("-", "");
        }
        public List<PortableExecutableReference> GetRefs()
        {
            string framework = RuntimeEnvironment.GetRuntimeDirectory();
            string customImportsDir = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "imports");
            List<PortableExecutableReference> refs = new List<PortableExecutableReference>();
            refs.Add(AssemblyMetadata.CreateFromFile(typeof(object).Assembly.Location).GetReference());
            refs.Add(AssemblyMetadata.CreateFromFile(Path.Combine(framework, "System.Private.Corelib.dll")).GetReference());
            refs.Add(AssemblyMetadata.CreateFromFile(Path.Combine(framework, "System.Runtime.dll")).GetReference());
            //refs.Add(AssemblyMetadata.CreateFromFile("Avalonia.dll").GetReference());
            //refs.Add(AssemblyMetadata.CreateFromFile("Avalonia.Base.dll").GetReference());
            //refs.Add(AssemblyMetadata.CreateFromFile("Avalonia.Desktop.dll").GetReference());
            //refs.Add(AssemblyMetadata.CreateFromFile("Avalonia.Controls.dll").GetReference());
            //refs.Add(AssemblyMetadata.CreateFromFile("Avalonia.Dialogs.dll").GetReference());
            //refs.Add(AssemblyMetadata.CreateFromFile("AvaloniaEdit.dll").GetReference());
            //refs.Add(AssemblyMetadata.CreateFromFile("AvaloniaEdit.TextMate.dll").GetReference());
            foreach (string file in Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll"))
            {
                try
                {
                    PortableExecutableReference defaultImport = MetadataReference.CreateFromFile(file);
                    refs.Add(defaultImport);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
            foreach (string file in Directory.GetFiles(customImportsDir, "*.dll"))
            {
                try
                {
                    PortableExecutableReference customImport = MetadataReference.CreateFromFile(file);
                    refs.Add(customImport);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
            return refs;
        }
        public void CompileSourceCode()
        {
            CSharpCompilationOptions options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true, optimizationLevel: OptimizationLevel.Release); //deterministic: true, platform: Platform.AnyCpu, optimizationLevel: OptimizationLevel.Release, 
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(SourceCode.Text);
            CSharpCompilation compilation = CSharpCompilation.Create(SelectedRow.Name + "_" + TimeStamp().ToString(), references: RefsGridData.ToArray(), options: options)
                .AddSyntaxTrees(syntaxTree);
            SaveSourceCode(compilation);
        }
        public void SaveSourceCode(CSharpCompilation compilation)
        {
            using (var ms = new MemoryStream())
            {
                EmitResult result = compilation.Emit(ms);
                if (!result.Success)
                {
                    //result.Diagnostics.
                    CompileStatusText = "Error of compilation";
                    var fails = result.Diagnostics.Where(e => e.Severity == DiagnosticSeverity.Error);
                    foreach (var error in fails)
                    {
                        CompileStatusText += error.Id + ":" + error.GetMessage();
                    }
                    return;
                }
                CompileStatusText = "Compiled successfully";
                SelectedRow.Checksum = GenerateChecksum(ms);
                SelectedRow.BinaryExecutable = ms.ToArray();
                if (SelectedRow.IsSaved)
                {
                    using (var DataSource = new HelpContext())
                    {
                        DataSource.ScriptTable.Attach(SelectedRow);
                        DataSource.ScriptTable.Update(SelectedRow);
                        DataSource.SaveChanges();
                    }
                }
                else
                {
                    using (var DataSource = new HelpContext())
                    {
                        SelectedRow.IsSaved = true;
                        DataSource.ScriptTable.Attach(SelectedRow);
                        DataSource.ScriptTable.Add(SelectedRow);
                        DataSource.SaveChanges();
                    }
                }
            }
        }
        public ReactiveCommand<Unit, Unit> CompileSourceCodeCommand { get; }
        public ReactiveCommand<ScriptCodeModel, Unit> SaveMacrosCommand { get; }
        public ReactiveCommand<ScriptCodeModel, Unit> RemoveMacrosCommand { get; }
        public ReactiveCommand<Unit, Unit> SaveCodeCommand { get; }
        public ReactiveCommand<Unit, Unit> AddMacrosCommand { get; }
        private ScriptCodeModel _SelectedRow;
        public ScriptCodeModel SelectedRow
        {
            get => _SelectedRow;
            set
            {
                this.RaiseAndSetIfChanged(ref _SelectedRow, value);
                SourceCode.Text = _SelectedRow.SourceCode;
            }
        }
        public void SetRefsGrid()
        {
            RefsGridData = new ObservableCollection<PortableExecutableReference>(GetRefs());
        }
        public ScriptWindowViewModel()
        {
            CompileSourceCodeCommand = ReactiveCommand.Create(CompileSourceCode);
            RemoveMacrosCommand = ReactiveCommand.Create<ScriptCodeModel>(RemoveMacros);
            SaveMacrosCommand = ReactiveCommand.Create<ScriptCodeModel>(SaveMacros);
            SaveCodeCommand = ReactiveCommand.Create(SaveCode);
            AddMacrosCommand = ReactiveCommand.Create(AddMacros);
            using (var DataSource = new HelpContext())
            {
                List<ScriptCodeModel> selectedMacros = DataSource.ScriptTable.ToList();
                MacrosGridData = new ObservableCollection<ScriptCodeModel>(selectedMacros);
                SetRefsGrid();
            }
        }
    }
}