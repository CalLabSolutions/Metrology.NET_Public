using System.Collections.Generic;

namespace SoA_Editor.Models
{
    public class Unc_Range
    {
        public Unc_Range()
        {
            Constants = new();
        }

        private List<Range_Constant> _Constants;

        public List<Range_Constant> Constants
        {
            get { return _Constants; }
            set { _Constants = value; }
        }

        private string _Min;

        public string Min
        {
            get { return _Min; }
            set { _Min = value; }
        }

        private string _Max;

        public string Max
        {
            get { return _Max; }
            set { _Max = value; }
        }
    }
}