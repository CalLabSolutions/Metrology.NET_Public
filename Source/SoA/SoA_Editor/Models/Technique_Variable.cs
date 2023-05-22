using Caliburn.Micro;

namespace SoA_Editor.Models
{
    public class Technique_Variable : PropertyChangedBase
    {
        public Technique_Variable(string value, string type)
        {
            Value = value;
            Type = type;
        }        

        private string _Value;

        public string Value
        {
            get { return _Value; }
            set
            {
                _Value = value;
                NotifyOfPropertyChange(() => Value);
            }
        }

        private string _Type;

        public string Type
        {
            get { return _Type; }
            set
            {
                _Type = value;
                NotifyOfPropertyChange(() => Type);
            }
        }
    }
}
