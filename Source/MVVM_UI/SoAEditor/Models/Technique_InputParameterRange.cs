using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoAEditor.Models
{
    public class Technique_InputParameterRange : PropertyChangedBase
    {

        public Technique_InputParameterRange(string inputParamRange, string min, string max)
        {
            InputParamRange = inputParamRange;
            Min = min;
            Max = max;
        }

        public Technique_InputParameterRange()
        {

        }


        private string _InputParamRange;

        public string InputParamRange
        {
            get { return _InputParamRange; }
            set { _InputParamRange = value; NotifyOfPropertyChange(() => InputParamRange); }
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

    }
}

