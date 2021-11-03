using Caliburn.Micro;
using System.Collections.ObjectModel;

namespace SoA_Editor.Models
{
    public class Assertion : PropertyChangedBase
    {
        public Assertion()
        {
            values = new();
            name = "";
            selectedValue = "";
        }
        
        private string name;

        public string Name
        {
            get { return name; }
            set { name = value; NotifyOfPropertyChange(() => Name); }
        }

        private ObservableCollection<string> values;

        public ObservableCollection<string> Values
        {
            get { return values; }
            set { values = value; NotifyOfPropertyChange(() => Values); }
        }

        private string selectedValue;

        public string SelectedValue
        {
            get { return selectedValue; }
            set { selectedValue = value; NotifyOfPropertyChange(() => SelectedValue); }
        }
    }
}