using MT_Editor.Converters;
using MT_Editor.ViewModels;
using System.Linq;
using System.Windows.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace MT_Editor.Views
{
    public sealed partial class AddEditView : UserControl
    {
        public AddEditView()
        {
            this.InitializeComponent();
            DataContext = new AddEditViewModel();
        }

        private void Measure_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var details = (AddEditViewModel)DataContext;
            if (details.Types == Types.Measure)
            {
                Measure.IsChecked = true;
            }
        }

        private void MeasureRatio_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var details = (AddEditViewModel)DataContext;
            if (details.Types == Types.MeasureRatio)
            {
                MeasureRatio.IsChecked = true;
            }
        }

        private void SourceRatio_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var details = (AddEditViewModel)DataContext;
            if (details.Types == Types.SourceRatio)
            {
                SourceRatio.IsChecked = true;
            }
        }

        private void Source_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var details = (AddEditViewModel)DataContext;
            if (details.Types == Types.Source)
            {
                Source.IsChecked = true;
            }
        }

        private void Quantities_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var details = (AddEditViewModel)DataContext;
            if (details.SelectedQuantity != null)
            {
                var quantitiy = details.Quantities.FirstOrDefault(q => q.QuantitiyName.Equals(details.SelectedQuantity.QuantitiyName));
                Quantities.SelectedItem = quantitiy;
            }
        }
    }
}