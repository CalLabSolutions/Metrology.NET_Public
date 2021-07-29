using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoA_Editor.ViewModels
{
    class OutputDialogViewModel : Screen
    {

        public OutputDialogViewModel(string name = "", string min = "", string max = "", string testMin = "", string testMax = "")
        {
            OutputName = name;
            Min = min;
            Max = max;
            TestsMin = new() { "at", "before", "after", "not applicable" };
            TestsMax = new() { "at", "before", "after", "not applicable" };
            TestMin = testMin;
            TestMax = testMax;
        }

        #region Properties

        private string outputName;
        public string OutputName
        {
            get { return outputName; }
            set
            {
                outputName = value;
                NotifyOfPropertyChange(() => OutputName);
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

        #endregion
    }
}
