using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using Avalonia.Media;
using ReactiveUI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using voicio.Models;
using voicio.Views;
using System.Threading;

namespace voicio.ViewModels
{
    public class VoiceSettingViewModel : ViewModelBase
    {
        private bool _IsPinnedWindow = false;
        public bool IsPinnedWindow
        {
            get => _IsPinnedWindow;
            set => this.RaiseAndSetIfChanged(ref _IsPinnedWindow, value);
        }
        private FlatTreeDataGridSource<VoiceSetting>? _VoiceSettingGridData;
        public FlatTreeDataGridSource<VoiceSetting>? VoiceSettingGridData
        {
            get => _VoiceSettingGridData;
            set => this.RaiseAndSetIfChanged(ref _VoiceSettingGridData, value);
        }
        private ObservableCollection<VoiceSetting>? _VoiceSettingRows;
        public ObservableCollection<VoiceSetting>? VoiceSettingRows
        {
            get => _VoiceSettingRows;
            set => this.RaiseAndSetIfChanged(ref _VoiceSettingRows, value);
        }
        public void AddTempOperation()
        {
            VoiceSetting newOp = new VoiceSetting(false);
            VoiceSettingRows.Add(newOp);
        }
        private void CompileActionViewer(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;
            var codeWindow = new CodeWindow() {DataContext=new CodeWindowViewModel((VoiceSetting)btn.DataContext) };
            codeWindow.Show();
        }
        private Button ToggleCompileActionButtonInit(VoiceSetting obj)
        {
            var LogViewButton = new Button();
            LogViewButton.Click += CompileActionViewer;
            LogViewButton.Content = "Code";
            return LogViewButton;
        }
        private CheckBox IsActiveOperationCheckboxInit(VoiceSetting op)
        {
            var b = new CheckBox();
            b.IsChecked = op.IsActive;
            return b;
        }
        private void RemoveVoiceSetting(object sender, RoutedEventArgs e)
        {
            Button removeButton = (Button)sender;
            VoiceSetting removedOps = (VoiceSetting)removeButton.DataContext;
            VoiceSettingRows.Remove(removedOps);
            if (removedOps.IsSaved)
            {
                using (var DataSource = new HelpContext())
                {
                    DataSource.VoiceSettingTable.Attach(removedOps);
                    DataSource.VoiceSettingTable.Remove(removedOps);
                    DataSource.SaveChanges();
                }
            }
        }
        private void UpdateVoiceSetting(object sender, RoutedEventArgs e)
        {
            Button updateButton = (Button)sender;
            VoiceSetting updateHint = (VoiceSetting)updateButton.DataContext;
            List<Tag> assosiatedTags = new List<Tag>();
            if (updateHint.IsSaved)
            {
                using (var DataSource = new HelpContext())
                {
                    DataSource.VoiceSettingTable.Attach(updateHint);
                    DataSource.VoiceSettingTable.Update(updateHint);
                    DataSource.SaveChanges();
                }
            }
            else
            {
                using (var DataSource = new HelpContext())
                {
                    DataSource.VoiceSettingTable.Attach(updateHint);
                    DataSource.VoiceSettingTable.Add(updateHint);
                    DataSource.SaveChanges();
                    updateHint.IsSaved = true;
                }
            }
        }
        private Button UpdateButtonInit()
        {
            var b = new Button();
            b.Background = new SolidColorBrush() { Color = new Color(255, 34, 139, 34) };
            b.Content = "Add";
            b.Click += UpdateVoiceSetting;
            return b;
        }
        private Button RemoveButtonInit()
        {
            var b = new Button();
            b.Background = new SolidColorBrush() { Color = new Color(255, 80, 00, 20) };
            b.Content = "Remove";
            b.Click += RemoveVoiceSetting;
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
        public void TreeDataGridInit()
        {
            var TextColumnLength = new GridLength(1, GridUnitType.Star);
            var TemplateColumnLength = new GridLength(125, GridUnitType.Pixel);

            var EditOptions = new TextColumnOptions<VoiceSetting>
            {
                BeginEditGestures = BeginEditGestures.Tap,
                MinWidth = new GridLength(80, GridUnitType.Pixel)
            };
            TemplateColumn<VoiceSetting> IsActiveOperationColumn = new TemplateColumn<VoiceSetting>("Enabled", new FuncDataTemplate<VoiceSetting>((a, e) => IsActiveOperationCheckboxInit(a), supportsRecycling: true), width: TemplateColumnLength);
            TemplateColumn<VoiceSetting> ButtonColumn = new TemplateColumn<VoiceSetting>("", new FuncDataTemplate<VoiceSetting>((a, e) => ButtonsPanelInit(), supportsRecycling: true), width: TemplateColumnLength);
            TemplateColumn<VoiceSetting> CompileColumn = new TemplateColumn<VoiceSetting>("", new FuncDataTemplate<VoiceSetting>((a, e) => ToggleCompileActionButtonInit(a), supportsRecycling: true), width: TemplateColumnLength);
            TextColumn<VoiceSetting, string> DescriptionTextColumn = new TextColumn<VoiceSetting, string>("Description", x => x.Description, (r, v) => r.Description = v, options: EditOptions, width: TextColumnLength);
            TextColumn<VoiceSetting, string> CommandTextColumn = new TextColumn<VoiceSetting, string>("Voice Command", x => x.Command, (r, v) => r.Command = v, options: EditOptions, width: TextColumnLength);
            VoiceSettingGridData = new FlatTreeDataGridSource<VoiceSetting>(VoiceSettingRows)
            {
                Columns =
                    {
                        IsActiveOperationColumn,
                        DescriptionTextColumn,
                        CommandTextColumn,
                        CompileColumn,
                        ButtonColumn
                    },
            };
            VoiceSettingGridData.Selection = new TreeDataGridCellSelectionModel<VoiceSetting>(VoiceSettingGridData);
        }
        public void ShowAllOperations()
        {
            using (var DataSource = new HelpContext())
            {
                List<VoiceSetting> VoiceSettings = DataSource.VoiceSettingTable.ToList();
                VoiceSettingRows = new ObservableCollection<VoiceSetting>(VoiceSettings);
            }
            TreeDataGridInit();
        }
        public VoiceSettingViewModel(CancellationTokenSource cts)
        {
            VoiceSettingRows = new ObservableCollection<VoiceSetting>();
            TreeDataGridInit();
            ShowAllOperations();
        }
    }
}
