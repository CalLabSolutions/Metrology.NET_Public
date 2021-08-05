using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Windows.Data;
using soa_1_03.models;

namespace soa_1_03.viewModels
{
    public class vmCmc : Inpc
    {
        #region Private Fields
        private mTaxonomy _currentTaxonomy;
        private ObservableCollection<mTaxonomy> _masterTaxonomy;
        private ObservableCollection<mTaxonomy> _userTaxonomy;
        private ObservableCollection<mTaxonomy> _filteredTaxonomy;
        private mClient _vmClient;
        private List<string> _actions;
        #endregion

        #region Constructor
        public vmCmc()
        {
            masterTaxonomy = new ObservableCollection<mTaxonomy>();
            userTaxonomy = new ObservableCollection<mTaxonomy>();
            filteredTaxonomy = new ObservableCollection<mTaxonomy>();
            currentTaxonomy = new mTaxonomy();
            actions = new List<string> { "Measure", "Source" };
        }
        #endregion

        #region Public Properties
        public DataTable rngTable { get; set; }

        public ObservableCollection<string> pxRangeHeaders;

        public ObservableCollection<mRange> pxRanges;

        public CollectionView currentFilterTaxonomy { get; set; }

        public CollectionView cvUserTaxonomy { get; set; }

        public mTaxonomy currentTaxonomy
        {
            get { return _currentTaxonomy; }
            set
            {
                if (value != _currentTaxonomy)
                {
                    _currentTaxonomy = value;
                    CreateRange();
                    OnPropertyChanged("currentTaxonomy");
                }
            }
        }

        public ObservableCollection<mTaxonomy> masterTaxonomy
        {
            get { return _masterTaxonomy; }
            set
            {
                if (value != _masterTaxonomy)
                {
                    _masterTaxonomy = value;
                    OnPropertyChanged("masterTaxonomy");
                }
            }
        }

        public ObservableCollection<mTaxonomy> userTaxonomy
        {
            get { return _userTaxonomy; }
            set
            {
                if (value != _userTaxonomy)
                {
                    _userTaxonomy = value;
                    OnPropertyChanged("userTaxonomy");
                }
            }
        }

        public ObservableCollection<mTaxonomy> filteredTaxonomy
        {
            get { return _filteredTaxonomy; }
            set
            {
                if (value != _filteredTaxonomy)
                {
                    _filteredTaxonomy = value;
                    OnPropertyChanged("filteredTaxonomy");
                }
            }
        }

        public mClient vmClient
        {
            get { return _vmClient; }
            set
            {
                if (value != _vmClient)
                {
                    _vmClient = value;
                    OnPropertyChanged("vmClient");
                }
            }
        }

        public List<string> actions
        {
            get { return _actions; }
            set
            {
                if (value != _actions)
                {
                    _actions = value;
                    OnPropertyChanged("actions");
                }
            }
        }
        #endregion

        #region Methods

        private void CreateDyRange()
        {
            if (currentTaxonomy.dyRanges.Count > 0) { currentTaxonomy.dyRanges.Clear(); }
            if (currentTaxonomy.pxRangeHeaders.Count > 0)
            {
                List<string> minMax = new List<string>() { "Min", "Max" };
                foreach (string s in currentTaxonomy.pxRangeHeaders)
                {
                    foreach (string m in minMax)
                    {
                        string p = string.Format("{0} {1}", s, m);
                        var x = new ExpandoObject() as IDictionary<string, Object>;
                        //x.p = double.NaN;
                        x.Add(p, double.NaN);
                        currentTaxonomy.dyRanges.Add(x);
                    }
                }
            }
            CreateRangesTable();
        }

        private void CreateRangesTable()
        {
            DataTable table = new DataTable();
            List<string> minMax = new List<string>() { "Min", "Max" };
            double[] values = new double[currentTaxonomy.pxRangeHeaders.Count + 1];
            List<double> dValues = new List<double>();
            int i = 0;
            foreach (string s in currentTaxonomy.pxRangeHeaders)
            {
                string nl = s.Replace(" ", Environment.NewLine);
                table.Columns.Add(nl);
                dValues.Add(i);
                values[i] = i;
                i++;
            }
            i = 0;
            DataRow row = table.NewRow();
            foreach (string s in currentTaxonomy.pxRangeHeaders)
            {
                string nl = s.Replace(" ", Environment.NewLine);
                row[nl] = i;
                i++;
            }
            table.Rows.Add(row);
            rngTable = table;
        }

        private void CreateRange()
        {
            if (!currentTaxonomy.optionalParams.Any() && !currentTaxonomy.requiredParams.Any()) { return; }

            currentTaxonomy.range.Clear();
            currentTaxonomy.ranges.Clear();
            currentTaxonomy.pxRangeHeaders.Clear();
            if (currentTaxonomy.requiredParams != null)
            {
                foreach (mRequiredParams p in currentTaxonomy.requiredParams)
                {
                    currentTaxonomy.range[p.parameter + " Min"] = 2.2;
                    currentTaxonomy.range[p.parameter + " Max"] = 3.3;
                }
            }

            if (currentTaxonomy.optionalParams != null)
            {
                foreach (mOptionalParams p in currentTaxonomy.optionalParams)
                {
                    currentTaxonomy.range[p.parameter + " Min"] = 4.4;
                    currentTaxonomy.range[p.parameter + " Max"] = 5.5;
                }
            }

            currentTaxonomy.range["Uncertainty"] = 6.6;
            currentTaxonomy.ranges.Add(currentTaxonomy.range);

            foreach (KeyValuePair<string, double> kvp in currentTaxonomy.ranges[0])
            {
                currentTaxonomy.pxRangeHeaders.Add(kvp.Key);
            }

            CreateDyRange();
        }
        #endregion
    }
}
