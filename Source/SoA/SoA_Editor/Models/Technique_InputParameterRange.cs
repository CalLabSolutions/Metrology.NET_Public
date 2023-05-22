using Caliburn.Micro;
using SOA_DataAccessLib;

namespace SoA_Editor.Models
{
    public class Technique_InputParameterRange : PropertyChangedBase
    {
        public Technique_InputParameterRange(string inputParamRange, string min, string max, string testMin, string testMax)
        {
            InputParamRange = inputParamRange;
            Min = min;
            Max = max;
            TestMin = testMin;
            TestMax = testMax;
        }

        private string _InputParamRange;

        public string InputParamRange
        {
            get { return _InputParamRange; }
            set
            {
                _InputParamRange = value;
                NotifyOfPropertyChange(() => InputParamRange);
            }
        }

        private string _Min;

        public string Min
        {
            get { return _Min; }
            set
            {
                _Min = value;
                NotifyOfPropertyChange(() => Min);
            }
        }

        private string _Max;

        public string Max
        {
            get { return _Max; }
            set
            {
                _Max = value;
                NotifyOfPropertyChange(() => Max);
            }
        }

        private string _TestMax;

        public string TestMax
        {
            get { return _TestMax; }
            set
            {
                _TestMax = value;
                NotifyOfPropertyChange(() => TestMax);
            }
        }

        private string _TestMin;

        public string TestMin
        {
            get { return _TestMin; }
            set
            {
                _TestMin = value;
                NotifyOfPropertyChange(() => TestMin);
            }
        }        
    }
}