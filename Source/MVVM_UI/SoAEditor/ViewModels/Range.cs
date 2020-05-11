using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoAEditor.ViewModels
{
    public class Range : PropertyChangedBase
    {
        private string rangeName = string.Empty;

        public string RangeName
        {
            get
            {
                return rangeName;
            }
            set
            {
                Set(ref rangeName, value);
                //rangeName = value;
                //OnPropertyChanged("BookName");
            }
        }

        public Range(string rangeName)
        {
            RangeName = rangeName;
        }
    }
}
