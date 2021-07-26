using Caliburn.Micro;

namespace SoA_Editor.Models
{
    public class TaxonomyResult : PropertyChangedBase
    {

        private string name;
        private string quantity;

        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                NotifyOfPropertyChange(() => Name);
            }
        }

        public string Quantity
        {
            get { return quantity; }
            set
            {
                quantity = value;
                NotifyOfPropertyChange(() => Quantity);
            }
        }
    }
}
