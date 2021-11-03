using Caliburn.Micro;
using SOA_DataAccessLib;

namespace SoA_Editor.ViewModels
{
    internal class EditSingleRangeDialogViewModel : Screen
    {
        public EditSingleRangeDialogViewModel(Unc_Range range, string parameterRange, decimal min, decimal max)
        {
            Range = range;
            ParameterRange = parameterRange;
            Min = min;
            Max = max;
        }

        private Unc_Range range;

        public Unc_Range Range
        {
            get { return range; }
            set { range = value; NotifyOfPropertyChange(() => Range); }
        }

        private string error = "";

        public string Error
        {
            get { return error; }
            set { error = value; NotifyOfPropertyChange(() => Error); }
        }


        private string parameterRange;
        public string ParameterRange
        {
            get { return parameterRange; }
            set { parameterRange = value; NotifyOfPropertyChange(() => ParameterRange); }
        }

        private decimal min;

        public decimal Min
        {
            get { return min; }
            set { min = value; NotifyOfPropertyChange(() => min); }
        }

        private decimal max;

        public decimal Max
        {
            get { return max; }
            set { max = value; NotifyOfPropertyChange(() => max); }
        }
    }
}