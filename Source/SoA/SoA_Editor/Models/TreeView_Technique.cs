using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoA_Editor.ViewModels
{
    public class TreeView_Technique : PropertyChangedBase
    {
        private ObservableCollection<TreeView_Range> _ranges;

        public TreeView_Technique(string techniqueName)
        {
            TechniqueName = techniqueName;
            Ranges = new ObservableCollection<TreeView_Range>();
            //{
            //    new Range("range1"),
            //    new Range("range2")
            //};
        }

        public ObservableCollection<TreeView_Range> Ranges
        {
            get
            {
                return _ranges;
            }
            set
            {
                Set(ref _ranges, value);
                //_ranges = value;
                //OnPropertyChanged("Courses");
            }
        }

        public string TechniqueName
        {
            get;
            set;
        }
    }
}
