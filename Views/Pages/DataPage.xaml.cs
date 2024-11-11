using WMMT6_TOOLS.ViewModels.Pages;
using Wpf.Ui.Controls;

namespace WMMT6_TOOLS.Views.Pages
{
    public partial class DataPage : INavigableView<DataViewModel>
    {
        public DataViewModel ViewModel { get; }

        public DataPage(DataViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }
    }
}
