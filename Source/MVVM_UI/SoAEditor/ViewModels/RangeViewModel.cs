using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoAEditor.Models;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows.Controls;

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
            updateValues();
            CalculatedValue = calculatedResult;
        }


        private string calculatedResult;


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

        private string _activeHierarchy;

        public string activeHierarchy
        {
            get { return _activeHierarchy; }
            set { Set(ref _activeHierarchy, value); }
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

        private List<KeyValuePair<string, string>> constList_KeyValuePair;
        private List<KeyValuePair<string, string>> exprVarList_KeyValuePair;
        private string formulaExpression;


        public void SelectedRowsChangeEvent(ActionExecutionContext e)
        {

            //list of constants
            int arrayLength = ((DataRowView)((object[])((SelectionChangedEventArgs)e.EventArgs).AddedItems)[0]).Row.ItemArray.Length;
            string constsStr = ((DataRowView)((object[])((SelectionChangedEventArgs)e.EventArgs).AddedItems)[0]).Row.ItemArray[arrayLength-1].ToString();

            constList_KeyValuePair = convertStr_To_KeyValuePair(constsStr);

            //list of variables
            //IList<ExpressionVariable> exprVarList = ((SoAEditor.ViewModels.RangeViewModel)e.Target).ExprVars;

            //formula expression
            formulaExpression = ((RangeViewModel)e.Target).Formula;

            updateValues();

            //foreach (var addedRow in e.AddedRows)
            //{
            //    _selectedRows.Add(addedRow as RowViewModel);
            //}

            //foreach (var removedRow in e.RemovedRows)
            //{
            //    _selectedRows.Remove(removedRow as RowViewModel);
            //}
        }

        private void updateValues()
        {

            exprVarList_KeyValuePair = getVariableListAsKeyValuePair();

            //check if there's a null value
            foreach (KeyValuePair<string, string> pair in constList_KeyValuePair)
            {
                if (pair.Value == null || pair.Value == "")
                {
                    calculatedResult = null;
                    return;
                }
                    
            }
            foreach (KeyValuePair<string, string> pair in exprVarList_KeyValuePair)
            {
                if (pair.Value == null || pair.Value == "")
                {
                    calculatedResult = null;
                    return;
                }
            }           


            string injectedValuesInExpr = injectVarsInExpr(constList_KeyValuePair, exprVarList_KeyValuePair, formulaExpression);

            //calculated result
            calculatedResult = Convert.ToDouble(new DataTable().Compute(injectedValuesInExpr, null)).ToString();
        }



        private List<KeyValuePair<string, string>> convertStr_To_KeyValuePair(string constStr)
        {
            List<KeyValuePair<string, string>> temp = new List<KeyValuePair<string, string>>();

            string[] lines = constStr.Split(new[] { "\n" }, StringSplitOptions.None);

            for (int i = 0; i < lines.Length - 1; ++i)
            {
                string key = lines[i].Substring(0, lines[i].IndexOf('=')).Trim();
                string value = lines[i].Substring(lines[i].IndexOf('=') + 1).Trim();
                temp.Insert(i, new KeyValuePair<string, string>(key, value));
            }

            return temp;
        }

        private List<KeyValuePair<string, string>> getVariableListAsKeyValuePair()
        {
            List<KeyValuePair<string, string>> temp = new List<KeyValuePair<string, string>>();

            for (int i = 0; i < ExprVars.Count(); i++)
            {
                temp.Insert(i, new KeyValuePair<string, string>(ExprVars[i].Name, ExprVars[i].Value));
            }
            
            return temp;
        }

        private string injectVarsInExpr(List<KeyValuePair<string, string>> constPairs, List<KeyValuePair<string, string>> varPairs, string expr)
        {
            //string temp="";

            foreach (KeyValuePair<string, string> pair in constPairs)
            {
                expr = expr.Replace(pair.Key, pair.Value);
            }

            foreach (KeyValuePair<string, string> pair in varPairs)
            {
                expr = expr.Replace(pair.Key, pair.Value);
            }


            return expr;
        }



        private ObservableCollection<ExpressionVariable> _ExprVars;
        public ObservableCollection<ExpressionVariable> ExprVars
        {
            get
            {
                return _ExprVars;
            }
            set
            {
                Set(ref _ExprVars, value);
            }
        }

        private List<string> exprConstList = new List<string>();



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
