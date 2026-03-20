using Avalonia.Controls;

namespace voicio.Views
{
    public partial class ScriptWindow : Window
    {
        public ScriptWindow()
        {
            InitializeComponent();
            DataGrid grid = GetGrid();
        }
        private DataGrid GetGrid()
        {
            DataGrid grid = this.FindControl<DataGrid>("mgrid");
            grid.SelectionMode = DataGridSelectionMode.Single;
            return grid;
        }
    }
}
