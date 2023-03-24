using Caliburn.Micro;

namespace SoA_Editor.Models
{
    public class TaxonomyInputParam : PropertyChangedBase
    {
        public TaxonomyInputParam(string param, string quantity, string optional)
        {
            Param = param;
            Quantity = quantity;
            Optional = optional;
        }

        public TaxonomyInputParam()
        {

        }

        private string _param;

        public string Param
        {
            get { return _param; }
            set
            {
                _param = value;
                NotifyOfPropertyChange(() => Param);
            }
        }

        private string _quantity;

        public string Quantity
        {
            get { return _quantity; }
            set
            {
                _quantity = value;
                NotifyOfPropertyChange(() => Quantity);
            }
        }

        private string _optional;

        public string Optional
        {
            get { return _optional; }
            set
            {
                _optional = value;
                NotifyOfPropertyChange(() => Optional);
            }
        }

    }
}
