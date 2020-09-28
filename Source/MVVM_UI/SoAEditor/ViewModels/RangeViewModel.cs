using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoAEditor.Models;
using System.Collections.ObjectModel;
using System.Data;

namespace SoAEditor.ViewModels
{
    public class RangeViewModel: Screen
    {
        /*
        private String _SelectedSubRange;

        public String SelectedSubRange
        {
            get { return _SelectedSubRange; }
            set
            {
                if (_SelectedSubRange != value)
                {
                    FreqSubRanges.Add("frequency 60, 60");
                    FreqSubRanges.Add("frequency 400, 400");

                    _SelectedSubRange = value;
                    NotifyOfPropertyChange();
                }
            }
        }


        private String _SelectedFreqSubRange;

        public String SelectedFreqSubRange
        {
            get { return _SelectedFreqSubRange; }
            set
            {
                if (_SelectedFreqSubRange != value)
                {
                    NominalSubRanges.Add("nominal 0, 11");
                    NominalSubRanges.Add("nominal 11, 110");

                    _SelectedFreqSubRange = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        private String _SelectedNominalSubRange;

        public String SelectedNominalSubRange
        {
            get { return _SelectedNominalSubRange; }
            set
            {
                if (_SelectedNominalSubRange != value)
                {
                    ConstSubRanges.Add("k_nominal 0.0001");
                    ConstSubRanges.Add("k_range 0.0002");

                    _SelectedNominalSubRange = value;
                    NotifyOfPropertyChange();
                }
            }
        }


        private String _SelectedConstSubRange;

        public String SelectedConstSubRange
        {
            get { return _SelectedConstSubRange; }
            set
            {
                if (_SelectedConstSubRange != value)
                {


                    _SelectedConstSubRange = value;
                    NotifyOfPropertyChange();
                }
            }
        }
        */

        public RangeViewModel()
        {

            /*
            SubRanges = new ObservableCollection<String>();
            FreqSubRanges = new ObservableCollection<String>();
            NominalSubRanges = new ObservableCollection<String>();
            ConstSubRanges = new ObservableCollection<String>();
            */


            //Ranges = new ObservableCollection<Range_Range>();
            //Constants = new ObservableCollection<Range_Constant>();
            //Formulas = new ObservableCollection<Range_Formula>();
        }



        public void calcButton()
        {
            CalculatedValue = "";
        }



        private string _CalculatedValue;

        public string CalculatedValue
        {
            get { return _CalculatedValue; }
            set { Set(ref _CalculatedValue, value); }
        }


        private DataTable _RangeGrid;
        public DataTable RangeGrid
        {
            get
            {
                return _RangeGrid;
            }
            set
            {
                Set(ref _RangeGrid, value);
            }
        }


        //private ObservableCollection<Range_Formula> _RangeGrid;

        //public ObservableCollection<Range_Formula> RangeGrid
        //{
        //    get
        //    {
        //        return _RangeGrid;
        //    }
        //    set
        //    {
        //        Set(ref _RangeGrid, value);
        //    }
        //}


        private String _Formula;

        public String Formula
        {
            get
            {
                return _Formula;
            }
            set
            {
                Set(ref _Formula, value);
            }
        }


        /*
        private string _RangeName;

        public string RangeName
        {
            get { return _RangeName; }
            set { _RangeName = value; }
        }


        private ObservableCollection<String> _SubRanges;

        public ObservableCollection<String> SubRanges
        {
            get
            {
                return _SubRanges;
            }
            set
            {
                Set(ref _SubRanges, value);
            }
        }

        private ObservableCollection<String> _Freq_SubRanges;

        public ObservableCollection<String> FreqSubRanges
        {
            get
            {
                return _Freq_SubRanges;
            }
            set
            {
                Set(ref _Freq_SubRanges, value);
            }
        }

        private ObservableCollection<String> _Nominal_SubRanges;

        public ObservableCollection<String> NominalSubRanges
        {
            get
            {
                return _Nominal_SubRanges;
            }
            set
            {
                Set(ref _Nominal_SubRanges, value);
            }
        }


        private ObservableCollection<String> _Const_SubRanges;

        public ObservableCollection<String> ConstSubRanges
        {
            get
            {
                return _Const_SubRanges;
            }
            set
            {
                Set(ref _Const_SubRanges, value);
            }
        }
        */



        //private ObservableCollection<Range_Range> _Ranges;

        //public ObservableCollection<Range_Range> Ranges
        //{
        //    get
        //    {
        //        return _Ranges;
        //    }
        //    set
        //    {
        //        Set(ref _Ranges, value);
        //    }
        //}

        //private ObservableCollection<Range_Constant> _Constants;

        //public ObservableCollection<Range_Constant> Constants
        //{
        //    get
        //    {
        //        return _Constants;
        //    }
        //    set
        //    {
        //        Set(ref _Constants, value);
        //    }
        //}

        //private ObservableCollection<Range_Formula> _Formulas;

        //public ObservableCollection<Range_Formula> Formulas
        //{
        //    get
        //    {
        //        return _Formulas;
        //    }
        //    set
        //    {
        //        Set(ref _Formulas, value);
        //    }
        //}
    }
}
