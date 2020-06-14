using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoAEditor.Models;
using System.Collections.ObjectModel;

namespace SoAEditor.ViewModels
{
    public class RangeViewModel: Screen
    {
        public RangeViewModel()
        {
            Ranges = new ObservableCollection<Range_Range>();
            Constants = new ObservableCollection<Range_Constant>();
            Formulas = new ObservableCollection<Range_Formula>();
        }

        private string _RangeName;

        public string RangeName
        {
            get { return _RangeName; }
            set { _RangeName = value; }
        }

        private string _CalculatedValue;

        public string CalculatedValue
        {
            get { return _CalculatedValue; }
            set { _CalculatedValue = value; }
        }


        private ObservableCollection<Range_Range> _Ranges;

        public ObservableCollection<Range_Range> Ranges
        {
            get
            {
                return _Ranges;
            }
            set
            {
                Set(ref _Ranges, value);
            }
        }

        private ObservableCollection<Range_Constant> _Constants;

        public ObservableCollection<Range_Constant> Constants
        {
            get
            {
                return _Constants;
            }
            set
            {
                Set(ref _Constants, value);
            }
        }

        private ObservableCollection<Range_Formula> _Formulas;

        public ObservableCollection<Range_Formula> Formulas
        {
            get
            {
                return _Formulas;
            }
            set
            {
                Set(ref _Formulas, value);
            }
        }
    }
}
