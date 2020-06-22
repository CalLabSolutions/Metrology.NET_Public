using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoAEditor.Models
{
    public class Technique_Variable : PropertyChangedBase
    {
        public Technique_Variable(string value)
        {
            Value = value;
        }

        public Technique_Variable()
        {

        }

        private string _Value;

        public string Value
        {
            get { return _Value; }
            set { _Value = value; NotifyOfPropertyChange(() => Value); }
        }
    }
}
