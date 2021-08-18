using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoA_Editor.Models
{
    public class Unc_Range_Min_Max
    {
        private string min;
        private string max;
        public string Min
        {
            get { return min; }
            set { min = value; }
        }

        public string Max
        {
            get { return max; }
            set { max = value; }
        }
    }
}
