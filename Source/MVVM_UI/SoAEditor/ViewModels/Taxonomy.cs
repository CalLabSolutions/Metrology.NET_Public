using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoAEditor.ViewModels
{
    public class Taxonomy : PropertyChangedBase
    {
        private ObservableCollection<Technique> _techniques;

        public Taxonomy(string taxonomName)
        {
            TaxonomyName = taxonomName;
            Techniques = new ObservableCollection<Technique>();
            //{
            //    new Technique("technique1"),
            //    new Technique("technique2")
            //};
        }

        public ObservableCollection<Technique> Techniques
        {
            get
            {
                return _techniques;
            }
            set
            {
                Set(ref _techniques, value);
                //_techniques = value;
                //OnPropertyChanged("Courses");
            }
        }

        public string TaxonomyName
        {
            get;
            set;
        }
    }
}
