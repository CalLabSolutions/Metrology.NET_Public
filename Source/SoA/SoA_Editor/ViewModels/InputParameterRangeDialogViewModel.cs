using Caliburn.Micro;
using SOA_DataAccessLib;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SoA_Editor.ViewModels
{
    internal class InputParameterRangeDialogViewModel : Screen
    {

        public InputParameterRangeDialogViewModel(Mtc_ParameterRanges inputParamRanges, Mtc_Parameters inputParams, string name = "", string min = "0", string max = "0", string testMin = "", string testMax = "", bool edit = false)
        {
            Edit = edit;
            ParamRangeNames = new();
            if (Edit)
            {
                ParamRangeNames.Add(inputParamRanges[name].name);
            }
            else
            {
                foreach (Mtc_Parameter param in inputParams)
                {
                    if (inputParamRanges[param.name] == null)
                    {
                        ParamRangeNames.Add(param.name);
                    }
                }
            }
            
            
            ParamRangeName = name;

            Min = min;
            Max = max;

            TestsMin = new() { "at", "before", "after", "not applicable" };
            TestsMax = new() { "at", "before", "after", "not applicable" };
            TestMin = testMin;
            TestMax = testMax;
        }
        
        #region Properties

        private string paramRangeName;

        public string ParamRangeName
        {
            get { return paramRangeName; }
            set
            {
                paramRangeName = value;
                NotifyOfPropertyChange(() => ParamRangeName);
            }
        }

        private ObservableCollection<string> paramRangeNames;

        public ObservableCollection<string> ParamRangeNames
        {
            get { return paramRangeNames; }
            set
            {
                paramRangeNames = value;
                NotifyOfPropertyChange(() => ParamRangeNames);
            }
        }

        private string min;

        public string Min
        {
            get { return min; }
            set
            {
                min = value;
                NotifyOfPropertyChange(() => Min);
            }
        }

        private string max;

        public string Max
        {
            get { return max; }
            set
            {
                max = value;
                NotifyOfPropertyChange(() => Max);
            }
        }

        private string testMin;

        public string TestMin
        {
            get { return testMin; }
            set
            {
                testMin = value;
                NotifyOfPropertyChange(() => TestMin);
            }
        }

        private ObservableCollection<string> testsMin;

        public ObservableCollection<string> TestsMin
        {
            get { return testsMin; }
            set
            {
                testsMin = value;
                NotifyOfPropertyChange(() => TestsMin);
            }
        }

        private string testMax;

        public string TestMax
        {
            get { return testMax; }
            set
            {
                testMax = value;
                NotifyOfPropertyChange(() => TestMax);
            }
        }

        private ObservableCollection<string> testsMax;

        public ObservableCollection<string> TestsMax
        {
            get { return testsMax; }
            set
            {
                testsMax = value;
                NotifyOfPropertyChange(() => TestsMax);
            }
        }

        private string error = "";

        public string Error
        {
            get { return error; }
            set
            {
                error = value;
                NotifyOfPropertyChange(() => Error);
            }
        }

        private bool edit;

        public bool Edit
        {
            get { return edit; }
            set
            {
                edit = value;
                NotifyOfPropertyChange(() => Edit);
            }
        }

        #endregion Properties
    }
}