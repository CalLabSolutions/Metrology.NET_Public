using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoA_Editor.Models
{
    public class ExpressionVariable : PropertyChangedBase
    {
        public ExpressionVariable(string name)
        {
            Name = name;
        }

        private string _Name;
        public string Name
        {
            get
            {
                return _Name;
            }

            set
            {
                Set(ref _Name, value);
            }
        }

        private string _Value;
        public string Value
        {
            get
            {
                return _Value;
            }

            set
            {
                Set(ref _Value, value);
            }
        }

        private string min;
        public string Min
        {
            get
            {
                return min;
            }

            set
            {
                Set(ref min, value);
            }
        }

        private string max;
        public string Max
        {
            get
            {
                return max;
            }

            set
            {
                Set(ref max, value);
            }
        }
    }
}
