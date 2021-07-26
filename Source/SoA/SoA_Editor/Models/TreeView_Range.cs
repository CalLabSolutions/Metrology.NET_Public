using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoA_Editor.ViewModels
{
    public class TreeView_Range : PropertyChangedBase
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

        public TreeView_Range(string rangeName)
        {
            RangeName = rangeName;
        }
    }
}
