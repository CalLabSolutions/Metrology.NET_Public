using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoAEditor.Models
{
    public class Technique_InputParameter : PropertyChangedBase
    {
        public Technique_InputParameter(string inputParam, string quantity)
        {
            InputParam = inputParam;
            Quantity = quantity;
        }

        public Technique_InputParameter()
        {

        }

        private string _InputParam;

        public string InputParam
        {
            get { return _InputParam; }
            set { _InputParam = value; NotifyOfPropertyChange(() => InputParam); }
        }

        private string _Quantity;

        public string Quantity
        {
            get { return _Quantity; }
            set { _Quantity = value; NotifyOfPropertyChange(() => Quantity); }
        }

    }
}
