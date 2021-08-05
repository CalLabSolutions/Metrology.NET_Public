using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoA_Editor.Models;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows.Controls;
using System.Diagnostics;
using SOA_DataAccessLib;

namespace SoA_Editor.ViewModels
{
    public class RangeViewModel: Screen
    {

        // public vars needed for the calculation
        public Unc_Template template;
        public string functionName;
        public Mtc_CMCUncertainty uncertainty;
        public List<string> assertionNodeValues;

        public RangeViewModel()
        {
            assertionNodeValues = new();
        }

        // private var for the calculation
        private string calculatedResult;


        // Calculate selected row
        public void calcButton()
        {
            updateValues();
            CalculatedValue = calculatedResult;
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

        private string _activeHierarchy;

        public string activeHierarchy
        {
            get { return _activeHierarchy; }
            set { Set(ref _activeHierarchy, value); }
        }
        
        private DataRowView row;
        
        public DataRowView Row
        {
            get { return row; }
            set
            {
                if (value == null) return;

                row = value;
                UpdateFormula(row.Row);
                NotifyOfPropertyChange(() => Row);
            }
        }
        
        private string _Formula;

        public string Formula
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
            //IList<ExpressionVariable> exprVarList = ((SoA_Editor.ViewModels.RangeViewModel)e.Target).ExprVars;

            //formula expression
            formulaExpression = ((RangeViewModel)e.Target).Formula;

            updateValues();
           
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



        private void UpdateFormula(DataRow row)
        {
            var data = row.ItemArray;
            var table = row.Table;

            // build our column to value list
            Dictionary<string, string> rowData = new();
            int index = 0;
            foreach (DataColumn col in table.Columns)
            {
                rowData.Add(col.ColumnName, data[index].ToString());
                index++;
            }

            // See if we have values we need to pair up with their assertionName
            List<string> searchNames = new();
            if (assertionNodeValues.Count > 0)
            {
                foreach (string searchValue in assertionNodeValues)
                {
                    searchNames = template.getCMCFunctionAssertionNames(functionName).ToList();
                    foreach (string name in searchNames)
                    {
                        var values = template.getCMCFunctionAssertionValues(functionName, name);
                        foreach (string value in values)
                        {
                            if (value == searchValue)
                            {
                                rowData.Add(name, value);
                                
                            }
                        }
                    }
                }
            }

            // lets find the right case now
            searchNames = template.getCMCFunctionAssertionNames(functionName).ToList();
            List<Unc_Case> cases = new();
            foreach (string name in searchNames)
            {
                var value = rowData[name];
                
                foreach (Unc_Case _case in template.CMCUncertaintyFunctions[0].Cases)
                {
                    if (_case.Assertions[name].Value == value)
                    {
                        cases.Add(_case);
                    }
                }
            }
            Debug.WriteLine(cases.Count());
        }
    }
}
