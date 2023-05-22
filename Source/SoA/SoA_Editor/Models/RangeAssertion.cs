using Caliburn.Micro;

namespace SoA_Editor.Models
{
    public class RangeAssertion : PropertyChangedBase
    {
        private string name;
        private string values;

        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                NotifyOfPropertyChange(() => Name);
            }
        }

        public string Values
        {
            get { return values; }
            set
            {
                values = value;
                NotifyOfPropertyChange(() => Values);
            }
        }
    }
}