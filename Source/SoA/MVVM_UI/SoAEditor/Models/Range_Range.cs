using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoAEditor.Models
{
    public class Range_Range : PropertyChangedBase
    {
        public Range_Range(string range, string min, string max)
        {
            Range = range;
            Min = min;
            Max = max;
        }

        public Range_Range()
        {
        }

        private string _Range;

        public string Range
        {
            get { return _Range; }
            set { _Range = value; NotifyOfPropertyChange(() => Range); }
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
