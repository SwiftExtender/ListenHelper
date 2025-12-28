using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using DynamicData;
using Newtonsoft.Json.Linq;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using voicio.Models;

namespace voicio.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private InteractiveAudioRecorder recorder;
        //private System.Timers.Timer RecordTimer;
        private TaskCompletionSource<bool> voiceSearchTask;
        private ObservableCollection<string>? _LastSearches;
        public ObservableCollection<string>? LastSearches
        {
            get => _LastSearches;
            set => this.RaiseAndSetIfChanged(ref _LastSearches, value);
        }
        //private List<Tag>? _TagsForChoice;
        //public List<Tag>? TagsForChoice
        //{
        //    get => _TagsForChoice;
        //    set => this.RaiseAndSetIfChanged(ref _TagsForChoice, value);
        //}
        private ObservableCollection<Hint>? _HintsRows;
        public ObservableCollection<Hint>? HintsRows
        {
            get => _HintsRows;
            set => this.RaiseAndSetIfChanged(ref _HintsRows, value);
        }
        private FlatTreeDataGridSource<Hint>? _HintsGridData;
        public FlatTreeDataGridSource<Hint>? HintsGridData
        {
            get => _HintsGridData;
            set => this.RaiseAndSetIfChanged(ref _HintsGridData, value);
        }
        private string _query = "";
        public string Query
        {
            get => _query;
            set => this.RaiseAndSetIfChanged(ref _query, value);
        }
        private string? _StatusText;
        public string? StatusText
        {
            get => _StatusText;
            set => this.RaiseAndSetIfChanged(ref _StatusText, value);
        }
        private bool _IsPinnedWindow = false;
        public bool IsPinnedWindow
        {
            get => _IsPinnedWindow;
            set => this.RaiseAndSetIfChanged(ref _IsPinnedWindow, value);
        }
        private bool _IsTextSearch = true;
        private bool _IsCommentSearch = true;
        //private bool _IsTagSearch = true;
        private bool _IsFuzzy = true;
        private bool _IsGridEditable = false;
        private bool _IsAddButtonVisible = false;
        private bool _IsHighlighting = false; //to do when i will understand the way to assign some struct to every table field in TreeDataGrid
        private bool _IsVoiceSearching = false;
        public bool IsVoiceSearching
        {
            get => _IsVoiceSearching;
            set => this.RaiseAndSetIfChanged(ref _IsVoiceSearching, value);
        }
        public bool IsTextSearch
        {
            get => _IsTextSearch;
            set => this.RaiseAndSetIfChanged(ref _IsTextSearch, value);
        }
        public bool IsCommentSearch
        {
            get => _IsCommentSearch;
            set => this.RaiseAndSetIfChanged(ref _IsCommentSearch, value);
        }
        //public bool IsTagSearch
        //{
        //    get => _IsTagSearch;
        //    set => this.RaiseAndSetIfChanged(ref _IsTagSearch, value);
        //}
        public bool IsHighlighting
        {
            get => _IsHighlighting;
            set => this.RaiseAndSetIfChanged(ref _IsHighlighting, value);
        }
        public bool IsFuzzy
        {
            get => _IsFuzzy;
            set => this.RaiseAndSetIfChanged(ref _IsFuzzy, value);
        }
        public bool IsAddButtonVisible
        {
            get => _IsAddButtonVisible;
            set => this.RaiseAndSetIfChanged(ref _IsAddButtonVisible, value);
        }
        public bool IsGridEditable
        {
            get => _IsGridEditable;
            set
            {
                this.RaiseAndSetIfChanged(ref _IsGridEditable, value);
                TreeDataGridInit();
            }
        }
        public ReactiveCommand<Unit, Unit> StartSearchCommand { get; }
        public ReactiveCommand<Unit, Unit> StartVoiceSearchCommand { get; }   
        public void StartVoiceSearch()
        {
            if (!IsVoiceSearching)
            {
                IsVoiceSearching = true;
                recorder = new InteractiveAudioRecorder();
                Dispatcher.UIThread.Invoke(() =>
                {
                    recorder.StartRecord();
                });
            } else
            {
                Dispatcher.UIThread.Invoke(() =>
                {
                    recorder.StopRecord();
                    var temp_speech_buf = recorder.GetByteArray();
                    string model_path = AppContext.BaseDirectory + "voice_model";
                    var recognition = new SpeechRecognition(model_path, recorder.GetRecorderSampleRate());
                    JObject rss = JObject.Parse(recognition.Recognize(temp_speech_buf));
                    Query = rss.Properties().Last().Value.ToString();
                });
                
                StartSearch();
                IsVoiceSearching = false;
                
            }
        }
        private void RemoveHint(object sender, RoutedEventArgs e)
        {
            Button removeButton = (Button)sender;
            Hint removedHint = (Hint)removeButton.DataContext;
            HintsRows.Remove(removedHint);
            if (removedHint.IsSaved)
            {
                using (var DataSource = new HelpContext())
                {
                    DataSource.HintTable.Attach(removedHint);
                    DataSource.HintTable.Remove(removedHint);
                    DataSource.SaveChanges();
                }
            }
        }
        private void UpdateHint(object sender, RoutedEventArgs e)
        {
            Button updateButton = (Button)sender;
            Hint updateHint = (Hint)updateButton.DataContext;
            List<Tag> assosiatedTags = new List<Tag>();
            if (updateHint.IsSaved)
            {
                using (var DataSource = new HelpContext())
                {
                    DataSource.HintTable.Attach(updateHint);
                    DataSource.HintTable.Update(updateHint);
                    DataSource.SaveChanges();
                }
            } else
            {
                using (var DataSource = new HelpContext())
                {
                    DataSource.HintTable.Attach(updateHint);
                    DataSource.HintTable.Add(updateHint);
                    DataSource.SaveChanges();
                }
                updateHint.IsSaved = true;
            }
        }
        public void AddTempHint()
        {
            Hint NewHint = new Hint(false);
            HintsRows.Add(NewHint);
        }
        private Button UpdateButtonInit()
        {
            var b = new Button();
            b.Background = new SolidColorBrush() { Color = new Color(255, 34, 139, 34) };
            b.Content = "Add";
            b.Click += UpdateHint;
            return b;
        }
        private Button RemoveButtonInit()
        {
            var b = new Button();
            b.Background = new SolidColorBrush() { Color = new Color(255, 80, 00, 20) };
            b.Content = "Remove";
            b.Click += RemoveHint;
            return b;
        }
        private DockPanel ButtonsPanelInit()
        {
            var panel = new DockPanel();
            panel.Children.Add(UpdateButtonInit());
            panel.Children.Add(RemoveButtonInit());
            panel.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
            return panel;
        }
        //private ComboBox TagControlInit()
        //{
        //    var addTag = new ComboBox();
        //    //addTag.SelectedItem = new string[] { "all", "not all" };
        //    addTag.ItemsSource = TagsForChoice;
        //    addTag.SelectedItem = TagsForChoice[0];
        //    addTag.SelectedValueBinding = new Binding("TagText") { Converter = TagGocntrolConverter };
        //    addTag.ItemTemplate = new FuncDataTemplate<Tag>((value, namescope) => new Label
        //    {   
        //            [!Label.ContentProperty] = new Binding("TagText") {}
        //    });
        //    return addTag;
        //}

        public void TreeDataGridInit()
        {
            var TextColumnLength = new GridLength(1, GridUnitType.Star);
            var TemplateColumnLength = new GridLength(125, GridUnitType.Pixel);

            if (IsGridEditable)
            {
                var EditOptions = new TextColumnOptions<Hint>
                {
                    BeginEditGestures = BeginEditGestures.Tap,
                    MinWidth = new GridLength(80, GridUnitType.Pixel),
                    IsTextSearchEnabled = true,
                    
                };
                TextColumn<Hint, string> HintTextColumn = new TextColumn<Hint, string>("Text", x => x.HintText, (r, v) => r.HintText = v, options: EditOptions, width: TextColumnLength);
                TextColumn<Hint, string> HintCommentColumn = new TextColumn<Hint, string>("Comment", x => x.Comment, (r, v) => r.Comment = v, options: EditOptions, width: TextColumnLength);
                HintsGridData = new FlatTreeDataGridSource<Hint>(HintsRows)
                {
                    Columns =
                    {
                        HintTextColumn,
                        HintCommentColumn,
                        //new TemplateColumn<Hint>("", new FuncDataTemplate<Hint>((a, e) => TagControlInit(), supportsRecycling: true), width: TemplateColumnLength),
                        new TemplateColumn<Hint>("", new FuncDataTemplate<Hint>((a, e) => ButtonsPanelInit(), supportsRecycling: true), width: TemplateColumnLength),
                    },
                };
                IsAddButtonVisible = true;
            }
            else
            {
                var ReadOptions = new TextColumnOptions<Hint>
                {
                    MinWidth = new GridLength(80, GridUnitType.Pixel)
                };
                TextColumn<Hint, string> HintTextColumn = new TextColumn<Hint, string>("Text", x => x.HintText, options: ReadOptions, width: TextColumnLength);
                TextColumn<Hint, string> HintCommentColumn = new TextColumn<Hint, string>("Comment", x => x.Comment, options: ReadOptions, width: TextColumnLength);
                HintsGridData = new FlatTreeDataGridSource<Hint>(HintsRows)
                {
                    Columns =
                    {
                        HintTextColumn,
                        HintCommentColumn,
                        //new TemplateColumn<Hint>("", new FuncDataTemplate<Hint>((a, e) => TagControlInit(), supportsRecycling: true), width: TemplateColumnLength)
                    },
                };
                IsAddButtonVisible = false;
                
            }
            HintsGridData.Selection = new TreeDataGridCellSelectionModel<Hint>(HintsGridData);
        }

        public void StartSearch()
        {
            using (var DataSource = new HelpContext())
            {
                if (Query.Trim() != "") LastSearches.Insert(0, Query);
                List<Hint> hints = new List<Hint>();
                //TagsForChoice = DataSource.TagTable.ToList();
                if (IsFuzzy)
                {
                    if (IsTextSearch) hints.Add(DataSource.HintTable.Where(b => b.HintText.Contains(Query)).ToList());
                    if (IsCommentSearch) hints.Add(DataSource.HintTable.Where(b => b.Comment.Contains(Query)).ToList());
                    //if (IsTagSearch) hints.Add(DataSource.HintTable.Where(b => b.HintTag.Any(pz => pz.Tag.TagText.Contains(Query))).ToList());
                }
                else
                {
                    if (IsTextSearch) hints.Add(DataSource.HintTable.Where(b => b.HintText == Query).ToList());
                    if (IsCommentSearch) hints.Add(DataSource.HintTable.Where(b => b.Comment == Query).ToList());
                    //if (IsTagSearch) hints.Add(DataSource.HintTable.Where(b => b.HintTag.Any(pz => pz.Tag.TagText == Query)).ToList());
                }
                HintsRows = new ObservableCollection<Hint>(hints.Distinct());
                if (HintsRows.Count > 0)
                {
                    StatusText = "Search result: " + HintsRows.Count.ToString();
                } else
                {
                    StatusText = "No results found";
                }
                
            }
            TreeDataGridInit();
        }
        public MainWindowViewModel()
        {
            StartSearchCommand = ReactiveCommand.Create(StartSearch);
            //StartVoiceSearchCommand = ReactiveCommand.CreateFromTask(StartVoiceSearch);
            StartVoiceSearchCommand = ReactiveCommand.Create(StartVoiceSearch);
            HintsRows = new ObservableCollection<Hint>();
            LastSearches = new ObservableCollection<string>();
            TreeDataGridInit();
            List<Hint> hints = new List<Hint>();
            using (var DataSource = new HelpContext())
            {
                hints.Add(DataSource.HintTable);
                HintsRows = new ObservableCollection<Hint>(hints.Distinct());
            }
                
        }
    }
}