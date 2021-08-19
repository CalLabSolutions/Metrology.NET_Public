using Caliburn.Micro;
using System.Collections.ObjectModel;
using SoA_Editor.Models;

namespace SoA_Editor.ViewModels
{
    public class TaxonomyViewModel : Screen
    {

        private string _name;

        public string TaxonName
        {
            get { return _name; }
            set { _name = value; NotifyOfPropertyChange(() => TaxonName); }
        }
        
        private ObservableCollection<TaxonomyResult> _resultQuant;

        public ObservableCollection<TaxonomyResult> ResultQuant
        {
            get { return _resultQuant; }
            set
            {
                Set(ref _resultQuant, value);
            }
        }       

        private string _definition;

        public string Definition
        {
            get { return _definition; }
            set { _definition = value; NotifyOfPropertyChange(() => Definition); }
        }

        private ObservableCollection<TaxonomyInputParam> _inputParams;

        public ObservableCollection<TaxonomyInputParam> InputParams
        {
            get
            {
                return _inputParams;
            }
            set
            {
                Set(ref _inputParams, value);
            }
        }

        public TaxonomyViewModel()
        {
            InputParams = new ObservableCollection<TaxonomyInputParam>();
            ResultQuant = new ObservableCollection<TaxonomyResult>();
        }
    }
}
