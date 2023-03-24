using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoA_Editor.ViewModels
{
    class AssertionDialogViewModel : Screen
    {
        public AssertionDialogViewModel(string name = "")
        {
            AssertionName = name;
        }

        private string assertionName;
        public string AssertionName
        {
            get { return assertionName; }
            set
            {
                assertionName = value;
                NotifyOfPropertyChange(() => AssertionName);
            }
        }

        private string error;
        public string Error
        {
            get { return error; }
            set
            {
                error = value;
                NotifyOfPropertyChange(() => Error);
            }
        }

    }
}
