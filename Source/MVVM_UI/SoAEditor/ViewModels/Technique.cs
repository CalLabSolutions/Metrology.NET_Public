using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoAEditor.ViewModels
{
    public class Technique : PropertyChangedBase
    {
        private ObservableCollection<Range> _ranges;

        public Technique(string techniqueName)
        {
            TechniqueName = techniqueName;
            Ranges = new ObservableCollection<Range>()
            {
                new Range("range1"),
                new Range("range2")
            };
        }

        public ObservableCollection<Range> Ranges
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
