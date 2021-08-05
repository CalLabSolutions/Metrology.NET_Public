using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoAEditor.ViewModels
{
    public class TreeView_Taxonomy : PropertyChangedBase
    {
        private ObservableCollection<TreeView_Technique> _techniques;

        public TreeView_Taxonomy(string taxonomName)
        {
            TaxonomyName = taxonomName;
            Techniques = new ObservableCollection<TreeView_Technique>();
            //{
            //    new Technique("technique1"),
            //    new Technique("technique2")
            //};
        }

        public ObservableCollection<TreeView_Technique> Techniques
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
