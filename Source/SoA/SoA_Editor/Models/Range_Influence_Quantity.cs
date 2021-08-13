using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoA_Editor.Model
{
    class Range_Influence_Quantity : PropertyChangedBase
    {
        private string infQtyName;
        private string[] infQtys;
        private string min;
        private string max;

        public Range_Influence_Quantity(string[] options)
        {
            this.infQtys = options;
        }

        public string InfQtyName
        {
            get { return infQtyName; }
            set { infQtyName = value; NotifyOfPropertyChange(() => InfQtyName); }
        }

        public string[] InfQtys
        {
            get { return infQtys; }
            set { infQtys = value; NotifyOfPropertyChange(() => InfQtys); }
        }

        public string Min
        {
            get { return min; }
            set { min = value; NotifyOfPropertyChange(() => Min); }
        }

        public string Max
        {
            get { return max; }
            set { max = value; NotifyOfPropertyChange(() => Max); }
        }
    }
}
