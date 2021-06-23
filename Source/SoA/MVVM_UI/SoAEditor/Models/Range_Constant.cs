using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoAEditor.Models
{
    public class Range_Constant : PropertyChangedBase
    {
        public Range_Constant(string constName, string value)
        {
            ConstName = constName;           
            Value = value;
        }

        public Range_Constant()
        {
        }

        private string _ConstName;

        public string ConstName
        {
            get { return _ConstName; }
            set { _ConstName = value; NotifyOfPropertyChange(() => ConstName); }
        }


        private string _Value;

        public string Value
        {
            get { return _Value; }
            set { _Value = value; NotifyOfPropertyChange(() => Value); }
        }

    }
}
