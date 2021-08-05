using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoAEditor.Models
{
    public class MeasurementParameter
    {

        private string _parameterName;

        public MeasurementParameter(string aName)
        {
            ParameterName = aName;
        }

        public string ParameterName
        {
            get { return _parameterName; }
            set { _parameterName = value; }
        }


    }
}
