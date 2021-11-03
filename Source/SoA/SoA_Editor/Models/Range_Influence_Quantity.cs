using Caliburn.Micro;
using SOA_DataAccessLib;
using SoA_Editor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoA_Editor.Model
{
    class Range_Influence_Quantity : PropertyChangedBase
    {
        private Mtc_Range selectedQty;
        private List<Mtc_Range> infQtys;
        private string min;
        private string max;
        private string parameterRange;

        public Range_Influence_Quantity(List<Mtc_Range> options)
        {
            InfQtys = options;
            Min = "";
            Max = "";
        }

        public Mtc_Range SelectedQty
        {
            get { return selectedQty; }
            set
            {
                selectedQty = value;
                var range = selectedQty.Start.ValueString;
                ParameterRange = "";
                if (range != null || range != "")
                {
                    ParameterRange = string.Format("{0} to {1}", selectedQty.Start.ValueString, selectedQty.End.ValueString);
                }
                NotifyOfPropertyChange(() => SelectedQty);
            }
        }

        public List<Mtc_Range> InfQtys
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

        public string ParameterRange
        {
            get { return parameterRange; }
            set{ parameterRange = value; NotifyOfPropertyChange(() => ParameterRange); }
        }
    }
}
