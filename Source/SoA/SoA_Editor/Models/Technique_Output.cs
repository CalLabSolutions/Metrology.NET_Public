using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoA_Editor.Models
{
    public class Technique_Output : PropertyChangedBase
    {
        public Technique_Output(string output, string min, string max, string testMin, string testMax)
        {
            Output = output;
            Min = min;
            Max = max;
            TestMin = testMin;
            TestMax = testMax;
        }

        private string _Output;

        public string Output
        {
            get { return _Output; }
            set { _Output = value; NotifyOfPropertyChange(() => Output); }
        }

        private string _Min;

        public string Min
        {
            get { return _Min; }
            set { _Min = value; NotifyOfPropertyChange(() => Min); }
        }

        private string _Max;

        public string Max
        {
            get { return _Max; }
            set { _Max = value; NotifyOfPropertyChange(() => Max); }
        }

        private string _TestMin;

        public string TestMin
        {
            get { return _TestMin; }
            set { _TestMin = value; NotifyOfPropertyChange(() => TestMin); }
        }

        private string _TestMax;

        public string TestMax
        {
            get { return _TestMax; }
            set { _TestMax = value; NotifyOfPropertyChange(() => TestMax); }
        }
    }
}
